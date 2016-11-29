using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTRazorPortable;

namespace BandWorld.MVC.Helpers
{
    public static class HtmlExtensions
    {
        // Mode helpers.
        public static bool IsOrientationPortrait(this HtmlHelper Html)
        {
            return MVCManager.Global.HybridView.IsOrientationPortrait();
        }

        public static bool IsOrientationLandscape(this HtmlHelper Html)
        {
            return !MVCManager.Global.HybridView.IsOrientationPortrait();
        }

        // String helpers.

        public static void BR(this HtmlHelper Html)
        {
            HtmlTextWriter writer = new HtmlTextWriter(Html.Writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Br);
            writer.RenderEndTag();
            writer.WriteLine();
        }

        public static string X(this HtmlHelper Html, string text)
        {
            if (text == null)
                text = String.Empty;
            // Maybe translate here.
            return text;
        }

        public static MvcHtmlString S(this HtmlHelper Html, string text)
        {
            return new MvcHtmlString(Html.X(text));
        }

        public static char[] LineBreak = new char[] { '\n' };

        public static string TextWithLineBreaks(this HtmlHelper Html, string text)
        {
            if (String.IsNullOrEmpty(text))
                return "";

            if (text.Contains("\n"))
            {
                string returnValue = "";
                string[] lines = text.Split(LineBreak, StringSplitOptions.None);

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine.Length == 0)
                        returnValue += "<div class=\"lbe\"></div>\n";
                    else
                        returnValue += "<div class=\"lb\">" + trimmedLine + "</div>\n";
                }

                return returnValue;
            }
            else
                return text;
        }

        public static void MessageDiv(this HtmlHelper Html, string message, string error)
        {
            HtmlTextWriter writer = new HtmlTextWriter(Html.ViewContext.Writer);

            if (!String.IsNullOrEmpty(message))
            {
                message = Html.X(message);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "confirmedmessage");
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "messageDiv");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.WriteLine();
                writer.Write(TextWithLineBreaks(Html, message));
                writer.WriteLine();
                writer.RenderEndTag();  // Div
                writer.WriteLine();
            }

            if (!String.IsNullOrEmpty(error))
            {
                message = Html.X(error);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "error");
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "errorDiv");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.WriteLine();
                writer.Write(TextWithLineBreaks(Html, error));
                writer.WriteLine();
                writer.RenderEndTag();  // Div
                writer.WriteLine();
            }
        }

        public static MvcHtmlString ActionLinkWithImage(
            this HtmlHelper htmlHelper,
            string linkImageName,
            string linkText,
            string actionName,
            string controllerName,
            Object routeValues)
        {
            MvcHtmlString aLink = LinkExtensions.ActionLink(htmlHelper, linkText, actionName, controllerName, routeValues,
                new { Style = "font-size: .9em; white-space: nowrap;" });
            var Url = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            string encodedLinkText = HttpUtility.HtmlEncode(linkText);
            string img = "";
            if (!String.IsNullOrEmpty(linkImageName))
                img = "<img src=\"" + Url.Content("~/Content/Images/" + linkImageName) + "\" alt=\"" + encodedLinkText
                    + "\" border=\"0\" align=\"middle\" />";
            aLink = MvcHtmlString.Create(aLink.ToString().Replace(">" + encodedLinkText + "<", ">" + img + encodedLinkText + "<") + " ");
            return aLink;
        }
    }
}