using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTRazorPortable;

namespace JTRazorPortable
{
    // All controller shoud inherit from this class.
    public class ControllerBase
    {
        // The current url, including the controller URL path, action, and arguments.
        public string CurrentUrl { get; set; }
        // The hybrid web view.
        public MVCManager MVCManager { get; set; }
        // The name of the controller in a URL.
        private string _ControllerUrlName;
        // The URL path of the controller (without an action name and arguments).
        private string _ControllerUrlPath;
        // Url helper.
        private UrlHelper _Url;
        // Model state.
        private ModelStateDictionary _ModelState;

        public ControllerBase()
        {
            _Url = null;
            _ModelState = null;
        }

        public ControllerBase(string name)
        {
            _ControllerUrlName = name;
            _ControllerUrlPath = "hybrid://" + name;
            _Url = null;
            _ModelState = null;
        }

        public virtual void Initialize()
        {
            _Url = null;
            _ModelState = null;
        }

        public string ControllerUrlName
        {
            get
            {
                return _ControllerUrlName;
            }
            set
            {
                _ControllerUrlName = value;
                _ControllerUrlPath = "hybrid://" + _ControllerUrlName;
            }
        }

        public string ControllerUrlPath
        {
            get
            {
                return _ControllerUrlPath;
            }
            set
            {
                _ControllerUrlPath = value;
            }
        }

        public UrlHelper Url
        {
            get
            {
                if (_Url == null)
                    _Url = new UrlHelper();

                return _Url;
            }
            set
            {
                _Url = value;
            }
        }

        public ModelStateDictionary ModelState
        {
            get
            {
                if (_ModelState == null)
                    _ModelState = new ModelStateDictionary();

                return _ModelState;
            }
            set
            {
                _ModelState = value;
            }
        }

        // Mimic the MVC controller functions.
        // These pass data back to the MVCManager that connect them to
        // the next page.

        public ActionResult View(string viewName, object model)
        {
            ViewBase view = MVCManager.GetViewWithModel(viewName, model);
            ActionResult actionResult = new ActionResult(view, model);
            return actionResult;
        }

        public ActionResult Redirect(string url)
        {
            ActionResult actionResult = new ActionResult(url);
            return actionResult;
        }

        public ActionResult RedirectToAction(string actionName)
        {
            ActionResult actionResult = new ActionResult(actionName, ControllerUrlName);
            return actionResult;
        }

        public ActionResult RedirectToAction(string actionName, object routeValues)
        {
            ActionResult actionResult = new ActionResult(actionName, ControllerUrlName, routeValues);
            return actionResult;
        }

        public ActionResult RedirectToAction(string actionName, string controllerName)
        {
            ActionResult actionResult = new ActionResult(actionName, controllerName);
            return actionResult;
        }

        public ActionResult RedirectToAction(string actionName, string controllerName, object routeValues)
        {
            ActionResult actionResult = new ActionResult(actionName, controllerName, routeValues);
            return actionResult;
        }

        public JsonResult Json(object data)
        {
            return Json(data, null /* contentType */, null /* contentEncoding */, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Json(object data, string contentType)
        {
            return Json(data, contentType, null /* contentEncoding */, JsonRequestBehavior.DenyGet);
        }

        public virtual JsonResult Json(object data, string contentType, Encoding contentEncoding)
        {
            return Json(data, contentType, contentEncoding, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Json(object data, JsonRequestBehavior behavior)
        {
            return Json(data, null /* contentType */, null /* contentEncoding */, behavior);
        }

        public JsonResult Json(object data, string contentType, JsonRequestBehavior behavior)
        {
            return Json(data, contentType, null /* contentEncoding */, behavior);
        }

        public virtual JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }

        public ActionResult Back()
        {
            ActionResult actionResult = new ActionResult(ActionType.Back);
            return actionResult;
        }

        public ActionResult Forward()
        {
            ActionResult actionResult = new ActionResult(ActionType.Forward);
            return actionResult;
        }

        public void Action(ActionResult actionResult)
        {
            MVCManager.HandleAction(actionResult);
        }

        public ActionResult Error(string message = null)
        {
            if (String.IsNullOrEmpty(message))
                message = "Unspecified error.";

            return View("Error", message);
        }
    }
}
