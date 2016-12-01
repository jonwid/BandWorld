using System;
using System.Threading;
using Foundation;
using UIKit;
using JTRazorPortable;
using BandWorld.MVC.Controllers;
using BandWorld.MVC.Application;
using BandWorld.MVC;

namespace BandWorld.iOS
{
	public partial class WebViewController : UIViewController
	{
		// ApplicationData settings - adjust them here.

		// The application name.
		string applicationName = "Band World";
		// Master administrator user name.
		string masterAdministratorUserName = "jonwid";
		// Can enable some development options, such as initializing the file system each time.
		bool isDevelopmentVersion = true;
		// Where the mobile version gets its content.
		string serviceUrl = "http://www.fixme.com";
		// The base directory for the platform data files.
		string basePlatformDirectory;
		// The tilde URL where the content is.
		string contentTildeUrl = "~/Content";
		// By changing this, we can cause the file system to be initialized.
		string mediaVersion = "1";
		// Copy resources check flag.
		bool copyResourcesCheck =
#if DEBUG
				true;
#else
                false;
#endif

		// Some main pointers.
		ApplicationDataPlatform applicationData;
		public static WebViewController Global;

		// Implementation data.
		public static UIWebView webView;
		static HybridWebView hybridWebView;
		public static MVCManager mvcManager;
		public static bool initialized = false;

		static bool UserInterfaceIdiomIsPhone
		{
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public WebViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (!initialized)
			{
				System.Reflection.Assembly assembly = typeof(TestController).Assembly;

				// Where our stuff ends up on the phone.
				basePlatformDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

				// Set up map to file.
				ApplicationData.SetUpMapToFile(basePlatformDirectory);

				// Create application data object.
				applicationData = new ApplicationDataPlatform(
					applicationName,
					masterAdministratorUserName,
					isDevelopmentVersion,
					serviceUrl,
					basePlatformDirectory,
					contentTildeUrl,
					mediaVersion);

				webView = WebView;

				// Intercept URL loading to handle native calls from browser
				WebView.ShouldStartLoad += HandleShouldStartLoad;

				// Create our hybrid WebView wrapper.
				hybridWebView = new HybridWebView(WebView);

				// Create our MVC manager.  This manages the interation between the
				// models, controllers, and views.
				mvcManager = new MVCManager(hybridWebView);
				mvcManager.PreHandleRequest = applicationData.PreHandleRequest;
				mvcManager.PostHandleRequest = applicationData.PostHandleRequest;

				mvcManager.DispatchToUIThread = DispatchToUI;

				// Save pointer to main activity.
				Global = this;

				// Prevent recreating everything on an orientation change or restart.
				initialized = true;

				// Display a splash pages.
				ResourceManager.EnsureResource(
					typeof(TestController).Assembly,
					basePlatformDirectory + "/Content/Splash.html",
					"Content.Splash.html",
					true);
				mvcManager.HybridView.LoadHtmlFile("Content/Splash.html");

				// Do this as a thread so we don't block.
				applicationData.RunAsThread(
					threadOp =>
					{
						// Copy resources to expected places.
						ResourceManager.EnsureResources(
							typeof(TestController).Assembly,
							basePlatformDirectory,
							ApplicationData.IsDevelopmentVersion,
							copyResourcesCheck);
					},
					continueOp =>
					{
						// Enter the common start up code for our app.
						// It should register the controllers, views, and display the
						// startup page.
						DispatchToUI(callback => BandWorldCommonApp.StartUp(mvcManager));
					}
				);
			}
			else
			{
				// Come here if just rotating the device or restarting.

				webView = WebView;

				// Intercept URL loading to handle native calls from browser
				WebView.ShouldStartLoad += HandleShouldStartLoad;

				// Reset our hybrid WebView wrapper.
				hybridWebView.Initialize(webView);

				// Restart the app, going to the current view.
				BandWorldCommonApp.Restart(mvcManager);
			}
		}

		private static void DispatchToUI(WaitCallback thunk)
		{
			Global.InvokeOnMainThread(() => thunk(null));
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		bool HandleShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			string url = request.Url.AbsoluteString;
			if ((url.IndexOf("ajax:") < 0) && (url.IndexOf("call:") < 0))
			{
				int offsetHybrid = url.IndexOf("hybrid:");

				if (offsetHybrid != -1)
					url = url.Substring(offsetHybrid);
			}

			if (!url.EndsWith("/"))
				hybridWebView.HandleRequest(url);

			return true;
		}
	}
}

