using JTRazorPortable;

namespace BandWorld.iOS
{
	public class JavaScriptInterop : object
	{
		MVCManager MVCManager;

		public JavaScriptInterop()
		{
			this.MVCManager = null;
		}

		public void SetMVCManager(MVCManager mvcManager)
		{
			MVCManager = mvcManager;
		}

		public string Ajax(string jsonObject)
		{
			return MVCManager.HandleAjaxRequest(jsonObject);
		}

		public string MyNativeCall(string arg1, string arg2)
		{
			return "MyNativeCall(" + arg1 + ", " + arg2 + ") called.";
		}
	}
}