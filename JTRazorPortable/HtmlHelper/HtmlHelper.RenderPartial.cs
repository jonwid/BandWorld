using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTRazorPortable
{
    public partial class HtmlHelper
    {
        // Renders the partial view with the parent's view data and model
        public void RenderPartial(string partialViewName)
        {
            MVCManager.Global.RenderPartialImplementation(partialViewName, null);
        }

        // Renders the partial view with an empty view data and the given model
        public void RenderPartial(string partialViewName, object model)
        {
            MVCManager.Global.RenderPartialImplementation(partialViewName, model);
        }

        // Renders the partial view with the parent's view data and model
        public IHtmlString Partial(string partialViewName)
        {
            string pageHtml = MVCManager.Global.PartialImplementation(partialViewName, null);
            return new HtmlString(pageHtml);
        }

        // Renders the partial view with an empty view data and the given model
        public IHtmlString Partial(string partialViewName, object model)
        {
            string pageHtml = MVCManager.Global.PartialImplementation(partialViewName, model);
            return new HtmlString(pageHtml);
        }
    }
}
