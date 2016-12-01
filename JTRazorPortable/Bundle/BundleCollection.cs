using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTRazorPortable
{
    public class BundleCollection
    {
        public Dictionary<string, WebBundle> Bundles;

        public BundleCollection()
        {
            Bundles = new Dictionary<string, WebBundle>();
        }

        public void Add(WebBundle bundle)
        {
            Bundles.Add(bundle.TildeUrl, bundle);
        }

        public WebBundle Find(string tildeUrl)
        {
            WebBundle bundle = null;

            if (Bundles.TryGetValue(tildeUrl, out bundle))
                return bundle;

            return null;
        }
    }
}
