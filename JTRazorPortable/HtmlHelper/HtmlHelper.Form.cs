using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using JTRazorPortable;

namespace JTRazorPortable
{
    public enum FormMethod { Get, Post };

    public partial class HtmlHelper
    {
#if NOT_PORTABLE
        public MvcForm BeginForm(string actionName = "", string controllerName = "", object routeValues = null, FormMethod method = FormMethod.Get, object htmlAttributes = null) {
			var qs = UrlHelper.GenerateQueryString (routeValues);
			if (qs.Length > 0)
				qs = "?" + qs;

            //Hack: Append "Post" to action names, so we don't have to use the real post name in the form.
            if ((method == FormMethod.Post) && !String.IsNullOrEmpty(actionName) && !actionName.EndsWith("Post"))
                actionName += "Post";

            //Hack: There's a bug in webView/WebViewClient such that ShouldOverrideUrlLoading doesn't get
            // called for a POST.  But GET seems to work, so we just remap it to a GET here.
            method = FormMethod.Get;

			var form = String.Format ("<form action=\"{0}{1}{2}{3}\" method=\"{4}\"{5}>", 
				ViewBase.UrlScheme,
                String.IsNullOrEmpty (controllerName) ? String.Empty : controllerName + "/", 
                actionName, 
				qs, 
				method == FormMethod.Post ? "post" : "get",
				GenerateHtmlAttributes(htmlAttributes));
			_writer.Write (form);
			return new MvcForm (_writer, "form");
		}

        public MvcForm BeginForm(string actionName, string controllerName, FormMethod method, object htmlAttributes)
        {
            return BeginForm(actionName, controllerName, null, method, htmlAttributes);
        }

        public MvcForm BeginForm(string actionName, string controllerName)
        {
            return BeginForm(actionName, controllerName, null, method, htmlAttributes);
        }
#endif

        public MvcForm BeginForm()
        {
            return BeginForm(MVCManager.CurrentControllerAction, MVCManager.CurrentControllerGroup);
        }

        public MvcForm BeginForm(object routeValues)
        {
            return BeginForm(MVCManager.CurrentControllerAction, MVCManager.CurrentControllerGroup, TypeHelper.ObjectToDictionary(routeValues), FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(RouteValueDictionary routeValues)
        {
            return BeginForm(MVCManager.CurrentControllerAction, MVCManager.CurrentControllerGroup, routeValues, FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(), FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, object routeValues)
        {
            return BeginForm(actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return BeginForm(actionName, controllerName, routeValues, FormMethod.Post, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, FormMethod method)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(), method, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, object routeValues, FormMethod method)
        {
            return BeginForm(actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), method, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, RouteValueDictionary routeValues, FormMethod method)
        {
            return BeginForm(actionName, controllerName, routeValues, method, new RouteValueDictionary());
        }

        public MvcForm BeginForm(string actionName, string controllerName, FormMethod method, object htmlAttributes)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(), method, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public MvcForm BeginForm(string actionName, string controllerName, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            return BeginForm(actionName, controllerName, new RouteValueDictionary(), method, htmlAttributes);
        }

        public MvcForm BeginForm(string actionName, string controllerName, object routeValues, FormMethod method, object htmlAttributes)
        {
            return BeginForm(actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), method, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public MvcForm BeginForm(string actionName, string controllerName, RouteValueDictionary routeValues, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            var qs = UrlHelper.GenerateQueryString(routeValues);
            if (qs.Length > 0)
                qs = "?" + qs;

            //Hack: Append "Post" to action names, so we don't have to use the real post name in the form.
            if ((method == FormMethod.Post) && !String.IsNullOrEmpty(actionName) && !actionName.EndsWith("Post"))
                actionName += "Post";

            // The webView doesn't include the query values when the ShouldOverrideUrlLoading function is called,
            // so we save them to use in HandleRequest.
            MVCManager.AddFormQueryValues(actionName, routeValues);

            //Hack: There's a bug in webView/WebViewClient such that ShouldOverrideUrlLoading doesn't get
            // called for a POST.  But GET seems to work, so we just remap it to a GET here.
            method = FormMethod.Get;

            var form = String.Format("<form action=\"{0}{1}{2}{3}\" method=\"{4}\"{5}>",
                ViewBase.UrlScheme,
                String.IsNullOrEmpty(controllerName) ? String.Empty : controllerName + "/",
                actionName,
                qs,
                method == FormMethod.Post ? "post" : "get",
                GenerateHtmlAttributes(htmlAttributes));
            _writer.Write(form);
            return new MvcForm(_writer, "form");
        }

        public IHtmlString AntiForgeryToken()
        {
            return new MvcHtmlString("");
        }
    }
}

