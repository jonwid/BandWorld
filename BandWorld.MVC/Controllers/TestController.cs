using System;
using BandWorld.MVC.Models;
using JTRazorPortable;

namespace BandWorld.MVC.Controllers
{
	public class TestController : ControllerCommon
	{

		public TestController() : base("Test")
		{
		}

		// By MVC convenion only, we have a default page entry point.
		public ActionResult Index()
		{
			return Test("My text.");
		}

		// Our main introduction page.
		public ActionResult Test(string text = null)
		{
			var model = new MainViewModel();
			if (String.IsNullOrEmpty(text))
				text = "(empty)";
			model.Text = text;
			return View("Test", model);
		}

		// The post handler for our main introduction page.
		// For now we need to use a different name so we append "Post" by convention.
		public ActionResult TestPost(FormCollection form, string command)
		{
			string text = form["textControl"];
			return RedirectToAction("Test", new { text });
		}

		public ActionResult TestAjax(string stringValue)
		{
			if (stringValue == null)
				stringValue = "(null)";
			return View("_TestAjax", stringValue);
		}

		private class AjaxTest
		{
			public string StringMember { get; set; }

			public AjaxTest(string stringValue)
			{
				StringMember = stringValue;
			}
		}

		public JsonResult TestAjaxJson(string stringValue)
		{
			if (stringValue == null)
				stringValue = "(null)";
			AjaxTest value = new AjaxTest(stringValue);
			return Json(value);
		}

		public ActionResult About()
		{
			return View("About", null);
		}
	}
}
