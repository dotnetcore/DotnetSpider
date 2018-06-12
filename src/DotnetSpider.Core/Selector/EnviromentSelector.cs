﻿using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
    /// <summary>
    /// 环境变量值查询, 在Request对象中, 可以存入一些初始字典供查询
    /// 还可以查询如: 当天时间等
    /// 此类不需要具体实现, 仅作为标识使用
    /// </summary>
    public class EnviromentSelector : ISelector
    {
        /// <summary>
        /// 查询的键值
        /// </summary>
        public string Field { get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="field">查询的键值</param>
        public EnviromentSelector(string field)
        {
            Field = field;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public dynamic Select(dynamic text)
        {
            throw new NotSupportedException("EnviromentSelector does not support SelectList method now.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> SelectList(dynamic text)
        {
            throw new NotSupportedException("EnviromentSelector does not support SelectList method now.");
        }
    }
}
