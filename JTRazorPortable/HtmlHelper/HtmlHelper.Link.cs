using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JTRazorPortable
{
    public static class LinkExtensions
    {
        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, null, null);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, object routeValues)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, TypeHelper.ObjectToDictionary(routeValues), null);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, object routeValues, object htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, TypeHelper.ObjectToDictionary(routeValues), TypeHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, routeValues, null);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, routeValues, htmlAttributes);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName)
        {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, null, null);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes)
        {
            var qs = UrlHelper.GenerateQueryString(routeValues);
            if (qs.Length > 0)
                qs = "?" + qs;

            if (String.IsNullOrEmpty(controllerName))
                controllerName = htmlHelper.MVCManager.CurrentControllerGroup;

            return new MvcHtmlString(string.Format("<a href=\"{0}{1}{2}{3}\"{4}>{5}</a>",
                ViewBase.UrlScheme,
                string.IsNullOrEmpty(controllerName) ? String.Empty : controllerName + "/",
                actionName,
                qs,
                GenerateHtmlAttributes(htmlAttributes),
                linkText));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            var qs = UrlHelper.GenerateQueryString(routeValues);
            if (qs.Length > 0)
                qs = "?" + qs;

            if (String.IsNullOrEmpty(controllerName))
                controllerName = htmlHelper.MVCManager.CurrentControllerGroup;

            return new MvcHtmlString(string.Format("<a href=\"{0}{1}{2}{3}\"{4}>{5}</a>",
                ViewBase.UrlScheme,
                string.IsNullOrEmpty(controllerName) ? String.Empty : controllerName + "/",
                actionName,
                qs,
                GenerateHtmlAttributes(htmlAttributes),
                linkText));
        }

        public static string GenerateHtmlAttributes(object htmlAttributes)
        {
            var attrs = new StringBuilder();
            if (htmlAttributes != null)
            {
                foreach (var property in htmlAttributes.GetType().GetProperties())
                    attrs.AppendFormat(@" {0}=""{1}""", property.Name.Replace('_', '-'), property.GetGetMethod().Invoke(htmlAttributes, null));
            }
            return attrs.ToString();
        }

        private static string GenerateHtmlAttributes(IDictionary<string, object> htmlAttributes)
        {
            var attrs = new StringBuilder();
            if (htmlAttributes != null)
            {
                foreach (KeyValuePair<string, object> kvp in htmlAttributes)
                    attrs.AppendFormat(@" {0}=""{1}""", kvp.Key.Replace('_', '-'), kvp.Value.ToString());
            }
            return attrs.ToString();
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, protocol, hostName, fragment, TypeHelper.ObjectToDictionary(routeValues), TypeHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, object routeValues)
        {
            return RouteLink(htmlHelper, linkText, TypeHelper.ObjectToDictionary(routeValues));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, RouteValueDictionary routeValues)
        {
            return RouteLink(htmlHelper, linkText, routeValues, null);
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName)
        {
            return RouteLink(htmlHelper, linkText, routeName, (object)null /* routeValues */);
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, object routeValues)
        {
            return RouteLink(htmlHelper, linkText, routeName, TypeHelper.ObjectToDictionary(routeValues));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, RouteValueDictionary routeValues)
        {
            return RouteLink(htmlHelper, linkText, routeName, routeValues, null);
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, object routeValues, object htmlAttributes)
        {
            return RouteLink(htmlHelper, linkText, TypeHelper.ObjectToDictionary(routeValues), TypeHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            return RouteLink(htmlHelper, linkText, null /* routeName */, routeValues, htmlAttributes);
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, object routeValues, object htmlAttributes)
        {
            return RouteLink(htmlHelper, linkText, routeName, TypeHelper.ObjectToDictionary(routeValues), TypeHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            var qs = UrlHelper.GenerateQueryString(routeValues);
            if (qs.Length > 0)
                qs = "?" + qs;

            return new MvcHtmlString(string.Format("<a href=\"{0}{1}{2}\"{3}>{4}</a>",
                ViewBase.UrlScheme,
                routeName,
                qs,
                GenerateHtmlAttributes(htmlAttributes),
                linkText));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes)
        {
            return RouteLink(htmlHelper, linkText, routeName, protocol, hostName, fragment, TypeHelper.ObjectToDictionary(routeValues), TypeHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, string linkText, string routeName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            var qs = UrlHelper.GenerateQueryString(routeValues);
            if (qs.Length > 0)
                qs = "?" + qs;

            return new MvcHtmlString(string.Format("<a href=\"{0}{1}{2}\"{3}>{4}</a>",
                ViewBase.UrlScheme,
                routeName,
                qs,
                GenerateHtmlAttributes(htmlAttributes),
                linkText));
        }
    }
}
