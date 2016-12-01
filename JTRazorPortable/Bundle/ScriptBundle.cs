using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTRazorPortable
{
    public class ScriptBundle : WebBundle
    {
        public ScriptBundle(string tildeUrl) : base(tildeUrl)
        {
        }

        public override void Render(StringBuilder sb)
        {
            foreach (string include in Includes)
                sb.AppendLine("<script src=\"" + include + "\" type=\"text/javascript\"></script>");
        }
    }
}
