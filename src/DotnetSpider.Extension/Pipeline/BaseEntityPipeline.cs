using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityPipeline : IEntityPipeline
	{
		public ISpider Spider { get; protected set; }

		public virtual void Dispose()
		{
		}

		public abstract void InitiEntity(EntityMetadata metadata);

        /// <summary>
        /// Create new instance, copy property values and initialize it by entity metadata. the default implementation is simply calling InitEntity method.
        /// When there are multiple entities, every original pipeline instance is somehow sort of a "template", for there should be 
        /// every single pipelines created for every single entity definition. That's why we have this method.
        /// Please reference to the Run method of EntitySpider class where this method is called. theoretically all the sub class of this class should override
        /// this method.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns>the new created instance.</returns>
        public virtual BaseEntityPipeline CreateNewByInitEntity(EntityMetadata metadata)
        {
            InitiEntity(metadata);
            return this;
        }

		public virtual void InitPipeline(ISpider spider)
		{
			Spider = spider;
		}

		public abstract void Process(List<JObject> datas);
	}
}
