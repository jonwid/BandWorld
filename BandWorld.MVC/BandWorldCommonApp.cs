using JTRazorPortable;
using BandWorld.MVC.Controllers;

namespace BandWorld.MVC
{
	// Just a place to put some common stuff for our app.
	public static class BandWorldCommonApp
	{
		// This is our app's common entry point.
		public static void StartUp(MVCManager mvcManager)
		{
			// Register our controllers so we can find them by name.
			RegisterControllers(mvcManager);

			// Register our views so we can find them by name.
			RegisterViews(mvcManager);

			// Register our style and script bundles for @Styles and @Scripts directives.
			// This is just a convenience to make our views like in MVC.
			// You can just reference the CSS and script file directly if you prefer.
			RegisterBundles(mvcManager);

			// Start out with our main page.  Note the use of "hybrid:" in our URL.
			// Our URLs are in the form of: "hybrid:(controller prefix)/(controller function name)
			mvcManager.GoToPage("hybrid:Test/Index");
		}

		// Restart, such as when restarted or the orientation changes.
		public static void Restart(MVCManager mvcManager)
		{
			mvcManager.GoToPage(mvcManager.CurrentUrl);
		}

		// Register our controllers.
		public static void RegisterControllers(MVCManager mvcManager)
		{
			mvcManager.RegisterController<TestController>("Test");
		}

		// Register our views.
		public static void RegisterViews(MVCManager mvcManager)
		{
			// Test views.
			mvcManager.RegisterView<Test>("Test");
			mvcManager.RegisterView<About>("About");
			mvcManager.RegisterView<_SubView>("_SubView");
			mvcManager.RegisterView<_TestAjax>("_TestAjax");

			// Shared views.
			// By convention, we prefix layouts and subviews with "_".
			mvcManager.RegisterView<_Layout>("_Layout");
			mvcManager.RegisterView<_MainHeader>("_MainHeader");
			mvcManager.RegisterView<_MainFooter>("_MainFooter");
			// By default, this view will be displayed on errors.
			mvcManager.RegisterView<Error>("Error");
		}

		public static void RegisterBundles(MVCManager mvcManager)
		{
			BundleCollection bundles = mvcManager.Bundles;

			bundles.Add(
				new ScriptBundle("~/bundles/scripts").Include(
					"~/Scripts/jquery-1.10.2.min.js",
					"~/Scripts/bootstrap.min.js",
					"~/Scripts/portable-razor.js",
					"~/Scripts/myscript.js"));

			bundles.Add(
				new StyleBundle("~/Content/css").Include(
					"~/Content/bootstrap.min.css",
					"~/Content/site.css"));
		}
	}
}
