using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Core.Common;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
    public abstract class BaseEntityDbPipeline : BaseEntityPipeline
    {
        public string ConnectString { get; set; }
        public PipelineMode Mode { get; set; } = PipelineMode.Insert;

        protected abstract DbConnection CreateConnection();

        protected abstract string GetInsertSql();
        protected abstract string GetUpdateSql();
        protected abstract string GetCreateTableSql();
        protected abstract string GetCreateSchemaSql();
        protected abstract DbParameter CreateDbParameter();

        protected List<List<string>> Indexs { get; set; } = new List<List<string>>();
        protected List<List<string>> Uniques { get; set; } = new List<List<string>>();
        protected List<Field> Primary { get; set; } = new List<Field>();
        protected string AutoIncrement { get; set; }
        protected Schema Schema { get; set; }
        protected List<Field> Columns { get; set; } = new List<Field>();
        protected List<Field> UpdateColumns { get; set; } = new List<Field>();

        protected abstract string ConvertToDbType(string datatype);

        protected BaseEntityDbPipeline()
        {
        }

        protected BaseEntityDbPipeline(string connectString, PipelineMode mode = PipelineMode.Insert)
        {
            Mode = mode;
            ConnectString = connectString;
        }


        public override BaseEntityPipeline CreateNewByInitEntity(EntityMetadata metadata)
        {
            BaseEntityDbPipeline newObject = Activator.CreateInstance(this.GetType()) as BaseEntityDbPipeline;
            newObject.ConnectString = this.ConnectString;
            newObject.Mode = this.Mode;
            newObject.Spider = this.Spider;
            initEntity(metadata, newObject);
            return newObject;
        }

        public override void InitiEntity(EntityMetadata entityDefine)
        {
            initEntity(entityDefine, this);
        }

        private void initEntity(EntityMetadata entityDefine, BaseEntityDbPipeline newObject)
        {
            if (entityDefine.Schema == null)
                return;
            newObject.Schema = GenerateSchema(entityDefine.Schema);
            foreach (var f in entityDefine.Entity.Fields)
            {
                if (!string.IsNullOrEmpty(((Field)f).DataType))
                {
                    newObject.Columns.Add((Field)f);
                }
            }
            var primary = entityDefine.Primary;
            if (primary != null)
            {
                foreach (var p in primary)
                {
                    var col = newObject.Columns.FirstOrDefault(c => c.Name == p);
                    if (col == null)
                    {
                        throw new SpiderException("Columns set as primary is not a property of your entity.");
                    }
                    else
                    {
                        newObject.Primary.Add(col);
                    }
                }
            }

            if (newObject.Mode == PipelineMode.Update)
            {
                if (newObject.Primary == null || newObject.Primary.Count == 0)
                {
                    throw new SpiderException("Set Primary in the Indexex attribute.");
                }

                if (entityDefine.Updates != null && entityDefine.Updates.Length > 0)
                {
                    foreach (var column in entityDefine.Updates)
                    {
                        var col = newObject.Columns.FirstOrDefault(c => c.Name == column);
                        if (col == null)
                        {
                            throw new SpiderException("Columns set as update is not a property of your entity.");
                        }
                        else
                        {
                            newObject.UpdateColumns.Add(col);
                        }
                    }

                    newObject.UpdateColumns.RemoveAll(c => newObject.Primary.Contains(c));

                    if (newObject.UpdateColumns.Count == 0)
                    {
                        throw new SpiderException("Can't update primary key.");
                    }
                }
                else
                {
                    newObject.UpdateColumns = newObject.Columns;
                    newObject.UpdateColumns.RemoveAll(c => newObject.Primary.Contains(c));

                    if (newObject.UpdateColumns.Count == 0)
                    {
                        throw new SpiderException("Can't update primary key.");
                    }
                }
            }

            newObject.AutoIncrement = entityDefine.AutoIncrement;

            if (entityDefine.Indexes != null)
            {
                foreach (var index in entityDefine.Indexes)
                {
                    List<string> tmpIndex = new List<string>();
                    foreach (var i in index)
                    {
                        var col = newObject.Columns.FirstOrDefault(c => c.Name == i);
                        if (col == null)
                        {
                            throw new SpiderException("Columns set as index is not a property of your entity.");
                        }
                        else
                        {
                            tmpIndex.Add(col.Name);
                        }
                    }
                    if (tmpIndex.Count != 0)
                    {
                        newObject.Indexs.Add(tmpIndex);
                    }
                }
            }
            if (entityDefine.Uniques != null)
            {
                foreach (var unique in entityDefine.Uniques)
                {
                    List<string> tmpUnique = new List<string>();
                    foreach (var i in unique)
                    {
                        var col = newObject.Columns.FirstOrDefault(c => c.Name == i);
                        if (col == null)
                        {
                            throw new SpiderException("Columns set as unique is not a property of your entity.");
                        }
                        else
                        {
                            tmpUnique.Add(col.Name);
                        }
                    }
                    if (tmpUnique.Count != 0)
                    {
                        newObject.Uniques.Add(tmpUnique);
                    }
                }
            }
        }


        private Schema GenerateSchema(Schema schema)
        {
            switch (schema.Suffix)
            {
                case TableSuffix.FirstDayOfThisMonth:
                    {
                        schema.TableName += "_" + DateTimeUtils.FirstDayofThisMonth.ToString("yyyy_MM_dd");
                        break;
                    }
                case TableSuffix.Monday:
                    {
                        schema.TableName += "_" + DateTimeUtils.FirstDayofThisWeek.ToString("yyyy_MM_dd");
                        break;
                    }
                case TableSuffix.Today:
                    {
                        schema.TableName += "_" + DateTime.Now.ToString("yyyy_MM_dd");
                        break;
                    }
            }
            return schema;
        }

        public override void InitPipeline(ISpider spider)
        {
            base.InitPipeline(spider);

            if (Mode == PipelineMode.Update)
            {
                return;
            }
            if (Schema == null)
                return;
            NetworkCenter.Current.Execute("db-init", () =>
            {
                using (DbConnection conn = CreateConnection())
                {
                    conn.Open();
                    var command = conn.CreateCommand();
                    string schemaScript = GetCreateSchemaSql();
                    if (!string.IsNullOrEmpty(schemaScript))
                    {
                        command.CommandText = schemaScript;
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                    command.CommandText = GetCreateTableSql();
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            });
        }

        public override void Process(List<JObject> datas)
        {
            NetworkCenter.Current.Execute("pp-", () =>
            {
                switch (Mode)
                {
                    case PipelineMode.Insert:
                        {
                            using (var conn = CreateConnection())
                            {
                                var cmd = conn.CreateCommand();
                                cmd.CommandText = GetInsertSql();
                                cmd.CommandType = CommandType.Text;
                                conn.Open();

                                foreach (var data in datas)
                                {
                                    cmd.Parameters.Clear();

                                    List<DbParameter> parameters = new List<DbParameter>();
                                    foreach (var column in Columns)
                                    {
                                        var parameter = CreateDbParameter();
                                        parameter.ParameterName = $"@{column.Name}";
                                        parameter.Value = data.SelectToken($"{column.Name}")?.Value<string>();
                                        if (parameter.Value == null)
                                            parameter.Value = DBNull.Value;
                                        parameter.DbType = Convert(column.DataType);
                                        parameters.Add(parameter);
                                    }

                                    cmd.Parameters.AddRange(parameters.ToArray());
                                    cmd.ExecuteNonQuery();
                                }

                                conn.Close();
                            }
                            break;
                        }
                    case PipelineMode.Update:
                        {
                            using (var conn = CreateConnection())
                            {
                                var cmd = conn.CreateCommand();
                                cmd.CommandText = GetUpdateSql();
                                cmd.CommandType = CommandType.Text;
                                conn.Open();

                                foreach (var data in datas)
                                {
                                    cmd.Parameters.Clear();

                                    List<DbParameter> parameters = new List<DbParameter>();
                                    foreach (var column in UpdateColumns)
                                    {
                                        var parameter = CreateDbParameter();
                                        parameter.ParameterName = $"@{column.Name}";
                                        parameter.Value = data.SelectToken($"{column.Name}")?.Value<string>();
                                        parameter.DbType = Convert(column.DataType);
                                        parameters.Add(parameter);
                                    }

                                    foreach (var column in Primary)
                                    {
                                        var parameter = CreateDbParameter();
                                        parameter.ParameterName = $"@{column.Name}";
                                        parameter.Value = data.SelectToken($"{column.Name}")?.Value<string>();
                                        parameter.DbType = Convert(column.DataType);
                                        parameters.Add(parameter);
                                    }

                                    cmd.Parameters.AddRange(parameters.ToArray());
                                    cmd.ExecuteNonQuery();
                                }

                                conn.Close();
                            }
                            break;
                        }
                }

            });
        }

        private DbType Convert(string datatype)
        {
            if (string.IsNullOrEmpty(datatype))
            {
                throw new SpiderException("TYPE can not be null");
            }

            if (datatype.StartsWith("STRING,") || "TEXT" == datatype)
            {
                return DbType.String;
            }
            if ("BOOL" == datatype)
            {
                return DbType.Boolean;
            }

            if ("DATE" == datatype || "TIME" == datatype)
            {
                return DbType.DateTime;
            }

            throw new SpiderException("Unsport datatype: " + datatype);
        }
    }
}

