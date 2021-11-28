using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Merger.core;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.IO;

namespace Merger
{
    public class MergerCatalog
    {
        private static Lazy<MergerCatalog> _instance =
            new Lazy<MergerCatalog>(() => new MergerCatalog());


        public static MergerCatalog Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        [ImportMany(typeof(BaseMerger))]
        private IEnumerable<BaseMerger> mergers = null;

        private MergerCatalog()
        {
            mergers = null;
            var catalog = new AggregateCatalog();
            string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            catalog.Catalogs.Add(new DirectoryCatalog(location));
            using (var container = new CompositionContainer(catalog))
            {
                container.ComposeParts(this);
            }
        }

        public IEnumerable<BaseMerger> FindAllMergers()
        {
            return mergers;
        }

        public BaseMerger FindSpecialNameMergers(string methodName)
        {
            if (mergers == null)
                return null;
            List<BaseMerger> rtn =  mergers.Where(x => x.MethodName == methodName).ToList();
            if (rtn == null || rtn.Count == 0)
                return null;
            return rtn[0];
        }
    }
}
