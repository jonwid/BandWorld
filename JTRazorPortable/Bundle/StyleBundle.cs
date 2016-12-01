using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTRazorPortable
{
    public class StyleBundle : WebBundle
    {
        public StyleBundle(string tildeUrl) : base(tildeUrl)
        {
        }

        public override void Render(StringBuilder sb)
        {
            foreach (string include in Includes)
                sb.AppendLine("<link rel=\"stylesheet\" href=\"" + include + "\" />");
        }
    }
}
