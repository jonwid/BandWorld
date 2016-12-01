using System;

namespace JTRazorPortable
{
	public interface IHybridWebView
	{
		string BasePath { get; }
		void LoadHtmlString(string url, string html);
        void LoadHtmlFile(string fileName);
        string EvaluateJavascript(string script);
        void SetMVCManager(MVCManager mvcManager);
        bool IsOrientationPortrait();
	}
}