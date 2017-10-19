using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using System.Linq;

namespace DotnetSpider.Extension
{
    public abstract class EntitySpider : CommonSpider
    {
        public EntitySpider() : this(new Site())
        {
        }

        public EntitySpider(Site site) : base(site)
        {
        }

        public EntitySpider(string name) : base(name)
        {
        }

        public EntitySpider(string name, Site site) : base(name, site)
        {
        }

        public void AddEntityType<T>(string tableName = null) where T : ISpiderEntity
        {
            AddEntityType<T>(null, tableName);
        }

        public void AddEntityType<T>(DataHandler<T> dataHandler) where T : ISpiderEntity
        {
            AddEntityType(dataHandler, null);
        }

        public void AddEntityType<T>(DataHandler<T> dataHandler, string tableName) where T : ISpiderEntity
        {
            CheckIfRunning();

            EntityProcessor<T> processor = new EntityProcessor<T>(Site, dataHandler, tableName);
            AddPageProcessor(processor);
        }

        protected override IPipeline GetDefaultPipeline()
        {
            return BaseEntityPipeline.GetPipelineFromAppConfig();
        }

        protected override void PreInitComponent(params string[] arguments)
        {
            base.PreInitComponent(arguments);

            if (arguments.Contains("skip"))
            {
                return;
            }

            foreach (var processor in PageProcessors)
            {
                if (processor is IEntityProcessor entityProcessor)
                {
                    foreach (var pipeline in Pipelines)
                    {
                        BaseEntityPipeline newPipeline = pipeline as BaseEntityPipeline;

                        newPipeline?.AddEntity(entityProcessor.EntityDefine);
                    }
                }
            }
        }
    }
}