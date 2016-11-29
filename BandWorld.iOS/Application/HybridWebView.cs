using System;
using System.IO;
using System.Text;
using Foundation;
using UIKit;
using JTRazorPortable;

namespace BandWorld.iOS
{
    class HybridWebView : IHybridWebView
    {
        UIWebView webView;
        MVCManager MVCManager;
        JavaScriptInterop interop;
        bool isOrientationPortrait;
        int inLoadCount;
        string currentUrl;

        public HybridWebView(UIWebView uiWebView)
        {
            webView = uiWebView;
            inLoadCount = 0;
            isOrientationPortrait = true;

            // Enable JavaScript to C# call.
            interop = new JavaScriptInterop();

            Initialize(uiWebView);
        }

        public void Initialize(UIWebView uiWebView)
        {
            webView = uiWebView;
        }

        public void SetMVCManager(MVCManager mvcManager)
        {
            MVCManager = mvcManager;
            interop.SetMVCManager(mvcManager);
        }

#region IHybridWebView implementation

        public void LoadHtmlString(string url, string html)
        {
            string datapath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            NSUrl newUrl = new NSUrl(datapath, true);
            currentUrl = url;
            webView.LoadHtmlString(html, newUrl);
        }

        public void LoadHtmlFile(string fileName)
        {
            string path = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), fileName);
            byte[] htmlBytes = File.ReadAllBytes(path);
            string htmlString = Encoding.UTF8.GetString(htmlBytes);
            currentUrl = "file://" + fileName;
            var newUrl = new NSUrl(currentUrl);
            webView.LoadHtmlString(htmlString, newUrl);
        }

        public string EvaluateJavascript(string script)
        {
            var newUrl = new NSUrl(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), true);
            currentUrl = "javascript:" + script;
            webView.LoadHtmlString(currentUrl, newUrl);
            return "";
        }

        public bool HandleRequest(string testUrl)
        {
            string url = testUrl;
            bool handled = false;

            if (String.IsNullOrEmpty(testUrl))
                url = currentUrl;

            if (inLoadCount == 0)
            {
                // Android uses '+' instead of "%20" to encode spaces.
                url = url.Replace("+", " ");
                url = url.Replace("%2543", "+");
                url = url.Replace("%26%2343%3B", "+");

                handled = MVCManager.GoToPage(url);

                if (handled)
                    inLoadCount++;
            }
            else
                inLoadCount--;

            return handled;
        }

        public string BasePath { get; }

        public bool IsOrientationPortrait()
        {
            return isOrientationPortrait;
        }

        #endregion

    }
}