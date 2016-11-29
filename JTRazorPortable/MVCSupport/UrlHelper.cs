using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JTRazorPortable
{
	public class UrlHelper
	{
		public UrlHelper ()
		{
		}

        public UrlHelper(object context)
        {
        }

        public string Action(string actionName, object routeValues) {
			return Action(actionName, controllerName: null, routeValues: routeValues);
		}

		public string Encode(string url) {
			return HttpUtility.UrlEncode (url);
		}

        public string Content(string contentPath) {
            //ASP.NET MVC calls this -> return GenerateContentUrl(contentPath, RequestContext.HttpContext);
            return ContentStatic(contentPath);
		}

        public static string ContentStatic(string contentPath)
        {
            if (String.IsNullOrEmpty(contentPath))
                return String.Empty;

            if (contentPath.StartsWith("~"))
                contentPath = contentPath.Substring(1);

            if (contentPath.StartsWith("/"))
                contentPath = contentPath.Substring(1);

            return contentPath;
        }

        public static string ActionUrlStatic(string url)
        {
            if (String.IsNullOrEmpty(url))
                return url;

            string scheme = ViewBase.UrlScheme;

            if (url.StartsWith("/"))
                return scheme + url.Substring(1);

            return url;
        }

        public string ActionUrl(string url)
        {
            return ActionUrlStatic(url);
        }

        public string Action(
			string actionName, 
			string controllerName = "", 
			object routeValues = null, 
			string scheme = "", 
			string hostName = "") {

			if (String.IsNullOrEmpty(scheme))
				scheme = ViewBase.UrlScheme;

			var qs = GenerateQueryString (routeValues);
			if (qs.Length > 0)
				qs = "?" + qs;

			return string.Format ("{0}{1}{2}{3}{4}", 
				scheme,
				String.IsNullOrEmpty(hostName) ? String.Empty : hostName + ".",
				String.IsNullOrEmpty(controllerName) ? String.Empty : controllerName + "/",
				actionName, 
				qs);
		}

		public static string GenerateQueryString(object routeValues = null) {
			if (routeValues == null)
				return String.Empty;

			var qs = new StringBuilder ();
            foreach (var property in routeValues.GetType().GetProperties())
            {
                string name = property.Name;
                object value = property.GetGetMethod().Invoke(routeValues, null);
                string encodedValue = EncodeQueryValue(value != null ? value.ToString() : String.Empty);
                qs.AppendFormat("&{0}={1}", name, encodedValue);
            }

			if (qs.Length == 0)
				return String.Empty;

			return qs.ToString (1, qs.Length - 1);
		}

        public static string GenerateQueryString(RouteValueDictionary routeValues)
        {
            if (routeValues == null)
                return String.Empty;

            var qs = new StringBuilder();
            foreach (KeyValuePair<string, object> kvp in routeValues)
            {
                string name = kvp.Key.Replace('_', '-');
                object value = kvp.Value;
                string encodedValue = EncodeQueryValue(value != null ? value.ToString() : String.Empty);
                qs.AppendFormat("&{0}={1}", name, encodedValue);
            }

            if (qs.Length == 0)
                return String.Empty;

            return qs.ToString(1, qs.Length - 1);
        }

        public static string EncodeQueryValue(string str)
        {
            if (str != null)
            {
                str = str.Replace("<", "&lt;");
                str = str.Replace(">", "&gt;");
                str = str.Replace("\"", "&quot;");
                //str = str.Replace("&", "&amp;");
                //str = str.Replace("?", "&#63;");
                str = str.Replace("&", "%26");
                str = str.Replace("?", "%3F");
                str = str.Replace("!", "%21");
                str = str.Replace("\r", "%0D");
                str = str.Replace("\n", "%0A");
                str = str.Replace(" ", "%20");
            }

            return str;
        }
    }
}

