using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using Site = DotnetSpider.Core.Site;
using System.Linq;

namespace DotnetSpider.Extension.Processor
{
    public interface IEntityProcessor
    {
        IEntityDefine EntityDefine { get; }
    }

    public class EntityProcessor<T> : BasePageProcessor, IEntityProcessor where T : ISpiderEntity
    {
        public IEntityExtractor<T> Extractor { get; }

        public IEntityDefine EntityDefine => Extractor?.EntityDefine;

        public EntityProcessor(Site site, DataHandler<T> dataHandler = null, string tableName = null)
        {
            Site = site;
            Extractor = new EntityExtractor<T>(dataHandler, tableName);

            if (Extractor.EntityDefine.TargetUrlsSelectors != null && Extractor.EntityDefine.TargetUrlsSelectors.Count > 0)
            {
                foreach (var targetUrlsSelector in Extractor.EntityDefine.TargetUrlsSelectors)
                {
                    //if (targetUrlsSelector.XPaths == null && targetUrlsSelector.Patterns == null)

                    var patterns = targetUrlsSelector.Patterns?.Select(x => x?.Trim()).Distinct().ToArray();
                    var xpaths = targetUrlsSelector.XPaths?.Select(x => x?.Trim()).Distinct().ToList();
                    if (xpaths == null && patterns == null)
                    {
                        throw new SpiderException("Region xpath and patterns should not be null both.");
                    }
                    if (xpaths != null && xpaths.Count > 0)
                    {
                        //targetUrlsSelector.XPaths = new string[] { };
                        foreach (var xpath in xpaths)
                        {
                            AddTargetUrlExtractor(xpath, patterns);
                        }

                    }
                    else
                    {
                        if (patterns != null && patterns.Length > 0)
                        {
                            AddTargetUrlExtractor(null, patterns);
                        }

                    }                 
                }
            }
        }

        protected override void Handle(Page page)
        {
            List<T> list = Extractor.Extract(page);

            if (Extractor.DataHandler != null)
            {
                list = Extractor.DataHandler.Handle(list, page);
            }

            if (list == null || list.Count == 0)
            {
                return;
            }

            page.AddResultItem(Extractor.Name, list);
        }
    }
}
