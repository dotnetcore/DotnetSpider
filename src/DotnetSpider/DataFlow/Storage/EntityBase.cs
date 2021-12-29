using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.DataFlow.Storage
{
    public interface IEntity
    {
        TableMetadata GetTableMetadata();
    }

    /// <summary>
    /// 实体基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EntityBase<T> : IEntity where T : class, new()
    {
        private readonly Lazy<TableMetadata> _tableMetadata = new();

        /// <summary>
        /// 获取实体的表元数据
        /// </summary>
        /// <returns></returns>
        TableMetadata IEntity.GetTableMetadata()
        {
            Configure();

            var type = GetType();

            var schema = type.GetCustomAttributes(typeof(Schema), false).FirstOrDefault();
            if (schema != null)
            {
                _tableMetadata.Value.Schema = (Schema) schema;
                if (string.IsNullOrWhiteSpace(_tableMetadata.Value.Schema.Table))
                {
                    _tableMetadata.Value.Schema = new Schema(_tableMetadata.Value.Schema.Database, type.Name);
                }
            }
            else
            {
                _tableMetadata.Value.Schema = new Schema(null, type.Name);
            }

            var properties = type.GetProperties().Where(x => x.CanRead && x.CanWrite).ToList();

            foreach (var property in properties)
            {
                var column = new Column
                {
                    PropertyInfo = property,
                    Name = property.Name,
                    Type = property.PropertyType.Name,
                    Required = property.GetCustomAttributes(typeof(RequiredAttribute), false).Any()
                };

                var stringLength =
                    (StringLengthAttribute) property.GetCustomAttributes(typeof(StringLengthAttribute), false)
                        .FirstOrDefault();
                if (stringLength != null)
                {
                    column.Length = stringLength.MaximumLength;
                }

                _tableMetadata.Value.Columns.Add(property.Name, column);
            }

            // 如果未设置主键, 但实体中有名为 Id 的属性, 则默认把 Id 作为主键
            if (_tableMetadata.Value.Primary == null || _tableMetadata.Value.Primary.Count == 0)
            {
                var primary = properties.FirstOrDefault(x => x.Name.ToLower() == "id");
                if (primary != null)
                {
                    _tableMetadata.Value.Primary = new HashSet<string> {primary.Name};
                }
            }

            _tableMetadata.Value.TypeName = type.FullName;

            // 如果有主键，但没有设置更新字段，则完全更新
            if (_tableMetadata.Value.Primary != null && _tableMetadata.Value.Primary.Count > 0 &&
                !_tableMetadata.Value.HasUpdateColumns)
            {
                var columns = _tableMetadata.Value.Columns.Select(x => x.Key).ToList();
                foreach (var primary in _tableMetadata.Value.Primary)
                {
                    columns.Remove(primary);
                }

                _tableMetadata.Value.Updates = new HashSet<string>(columns);
            }

            return _tableMetadata.Value;
        }

        protected virtual void Configure()
        {
        }

        protected T HasKey(Expression<Func<T, object>> expression)
        {
            expression.NotNull(nameof(expression));
            var columns = GetColumns(expression);
            if (columns == null || columns.Count == 0)
            {
                throw new SpiderException("主键不能为空");
            }

            _tableMetadata.Value.Primary = new HashSet<string>(columns);
            return this as T;
        }

        protected T HasIndex(Expression<Func<T, object>> expression, bool isUnique = false)
        {
            expression.NotNull(nameof(expression));

            var columns = GetColumns(expression);

            if (columns == null || columns.Count == 0)
            {
                throw new SpiderException("索引列不能为空");
            }

            _tableMetadata.Value.Indexes.Add(new IndexMetadata(columns.ToArray(), isUnique));
            return this as T;
        }

        protected T ConfigureUpdateColumns(Expression<Func<T, object>> expression)
        {
            expression.NotNull(nameof(expression));
            var columns = GetColumns(expression);
            _tableMetadata.Value.Updates = columns;
            return this as T;
        }

        private HashSet<string> GetColumns(Expression<Func<T, object>> expression)
        {
            expression.NotNull(nameof(expression));
            var nodeType = expression.Body.NodeType;
            var columns = new HashSet<string>();
            switch (nodeType)
            {
                case ExpressionType.New:
                {
                    var body = (NewExpression) expression.Body;
                    foreach (var argument in body.Arguments)
                    {
                        var memberExpression = (MemberExpression) argument;
                        columns.Add(memberExpression.Member.Name);
                    }

                    if (columns.Count != body.Arguments.Count)
                    {
                        throw new SpiderException("表达式不正确");
                    }

                    break;
                }

                case ExpressionType.MemberAccess:
                {
                    var memberExpression = (MemberExpression) expression.Body;
                    columns.Add(memberExpression.Member.Name);
                    break;
                }

                case ExpressionType.Convert:
                {
                    var body = (UnaryExpression) expression.Body;
                    var memberExpression = (MemberExpression) body.Operand;
                    columns.Add(memberExpression.Member.Name);
                    break;
                }

                default:
                {
                    throw new SpiderException("表达式不正确");
                }
            }

            return columns;
        }
    }
}