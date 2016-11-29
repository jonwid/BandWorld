using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTRazorPortable;

namespace JTRazorPortable
{
    public enum ActionType
    {
        View,
        Redirect,
        Back,
        Forward
    };

    public class ActionResult
    {
        public ActionType Action { get; set; }
        public ViewBase View { get; set; }
        public object Model { get; set; }
        public string RedirectUrl { get; set; }
        public static UrlHelper Url = new UrlHelper();
        private string _ActionName;

        public ActionResult(ActionType actionType, ViewBase view, object model, string redirectUrl)
        {
            Action = actionType;
            View = view;
            Model = model;
            RedirectUrl = redirectUrl;
            _ActionName = null;
        }

        public ActionResult(ViewBase view, object model)
        {
            Action = ActionType.View;
            View = view;
            Model = model;
            RedirectUrl = null;
            _ActionName = null;
        }

        public ActionResult(string redirectUrl)
        {
            Action = ActionType.Redirect;
            View = null;
            Model = null;
            RedirectUrl = redirectUrl;
            _ActionName = null;
        }

        public ActionResult(string actionName, string controllerName)
        {
            Action = ActionType.Redirect;
            View = null;
            Model = null;
            RedirectUrl = Url.Action(actionName, controllerName, null, String.Empty, String.Empty);
            _ActionName = actionName;
        }

        public ActionResult(string actionName, string controllerName, object routeValues)
        {
            Action = ActionType.Redirect;
            View = null;
            Model = null;
            RedirectUrl = Url.Action(actionName, controllerName, routeValues, String.Empty, String.Empty);
            _ActionName = actionName;
        }

        public ActionResult(ActionType actionType)
        {
            Action = actionType;
            View = null;
            Model = null;
            RedirectUrl = null;
            _ActionName = null;
        }

        public string ActionName
        {
            get
            {
                if (String.IsNullOrEmpty(_ActionName))
                {
                    if (!String.IsNullOrEmpty(RedirectUrl))
                        _ActionName = RedirectUrl;
                    else if (View != null)
                        _ActionName = View.ViewName;
                    else
                        _ActionName = String.Empty;
                }
                return _ActionName;
            }
        }
    }
}
