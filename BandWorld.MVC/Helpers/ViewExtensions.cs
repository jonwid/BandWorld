using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using JTRazorPortable;

namespace BandWorld.MVC.Helpers
{
    public static class ViewBaseExtensions
    {
        // ViewBase extensions.
        public static string ReturnUrl(this ViewBase view)
        {
            if (view.MVCManager.PeekUrl() != null)
                return view.MVCManager.PeekUrl();

            return String.Empty;
        }

        public static string ThisUrl(this ViewBase view)
        {
            if (view.MVCManager.CurrentUrl != null)
                return view.MVCManager.CurrentUrl;

            return String.Empty;
        }

        public static string X(this ViewBase view, string text)
        {
            return view.Html.X(text);
        }

        public static MvcHtmlString S(this ViewBase view, string text)
        {
            return view.Html.S(text);
        }

        public static void MessageDiv(this ViewBase view, string message, string error)
        {
            view.Html.MessageDiv(message, error);
        }
    }
}