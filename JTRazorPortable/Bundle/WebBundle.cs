using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTRazorPortable;

namespace JTRazorPortable
{
    public class WebBundle
    {
        public string TildeUrl { get; set; }
        public List<string> Includes { get; set; }

        public WebBundle(string tildeUrl)
        {
            TildeUrl = tildeUrl;
            Includes = new List<string>();
        }

        public WebBundle Include(params string[] virtualPaths)
        {
            if (virtualPaths != null)
            {
                foreach (string virtualPath in virtualPaths)
                {
                    string path = virtualPath;

                    // Map to a web-page-friendly path.
                    path = UrlHelper.ContentStatic(path);

                    Includes.Add(path);
                }
            }

            return this;
        }

        public virtual void Render(StringBuilder sb)
        {
        }
    }
}
