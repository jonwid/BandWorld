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
	}
}