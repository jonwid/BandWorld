using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JTRazorPortable;

namespace JTRazorPortable
{
    public delegate bool UrlInterceptDelegate(string Url);
    public delegate void DispatchToUIDelegate(WaitCallback callback);

    // This class is for a singleton that manages all our controllers and views.
    public class MVCManager
    {
        public IHybridWebView HybridView;
        private string defaultControllerName;
        public Dictionary<string, ControllerBase> Controllers { get; private set; }
        public ControllerBase CurrentController { get; private set; }
        public Dictionary<string, ViewBase> Views { get; private set; }
        public ViewBase CurrentView { get; private set; }
        public string CurrentControllerGroup { get; private set; }
        public string CurrentControllerAction { get; private set; }
        public WaitResult CurrentWaitResult { get; set; }
        public string CurrentWaitUrl { get; set; }
        public BundleCollection Bundles { get; set; }
        public Dictionary<string, string> Sections { get; private set; }
        public Dictionary<string, RouteValueDictionary> FormQueryValues { get; private set; }
        public List<string> UrlStack { get; private set; }
        public int UrlStackIndex { get; private set; }
        public int UrlStackMaxCount { get; set; }

        // Callbacks
        public UrlInterceptDelegate PreHandleRequest;
        public UrlInterceptDelegate PostHandleRequest;
        public DispatchToUIDelegate DispatchToUIThread;

        // Global static pointer to this.
        public static MVCManager Global;

        public MVCManager(IHybridWebView hybridView)
        {
            Controllers = new Dictionary<string, ControllerBase>(StringComparer.OrdinalIgnoreCase);
            CurrentController = null;
            Views = new Dictionary<string, ViewBase>(StringComparer.OrdinalIgnoreCase);
            CurrentView = null;
            defaultControllerName = String.Empty;
            Bundles = new BundleCollection();
            Sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            FormQueryValues = new Dictionary<string, RouteValueDictionary>();
            UrlStack = new List<string>();
            UrlStackIndex = 0;
            UrlStackMaxCount = 100;
            PreHandleRequest = null;
            PostHandleRequest = null;
            DispatchToUIThread = null;
            HybridView = hybridView;
            hybridView.SetMVCManager(this);
            Global = this;
        }

        public void RegisterController(ControllerBase controller)
        {
            string controllerName = controller.ControllerUrlName;

            controller.MVCManager = this;

            if (!Controllers.ContainsKey(controllerName))
            {
                Controllers.Add(controllerName, controller);
                if (defaultControllerName == String.Empty)
                    defaultControllerName = controllerName;
            }
        }

        public void RegisterController<T>(string controllerName) where T : ControllerBase, new()
        {
            T controller = new T() { ControllerUrlName = controllerName };
            RegisterController(controller);
        }

        public void SetDefaultController(string controllerName)
        {
            defaultControllerName = controllerName;
        }

        public void RegisterView(ViewBase view)
        {
            string viewName = view.ViewName;

            view.MVCManager = this;

            if (!Views.ContainsKey(viewName))
                Views.Add(viewName, view);
        }

        public void RegisterView<T>(string viewName) where T : ViewBase, new()
        {
            T view = new T() { ViewName = viewName };
            RegisterView(view);
        }

        public ViewBase FindView(string viewName)
        {
            ViewBase view = null;

            if (!Views.TryGetValue(viewName, out view))
                view = null;

            return view;
        }

        public ViewBase GetViewWithModel(string viewName, object model)
        {
            ViewBase view = FindView(viewName);

            if (view != null)
                view.SetModel(model);

            return view;
        }

        public WebBundle FindBundle(string path)
        {
            WebBundle bundle = Bundles.Find(path);
            return bundle;
        }

        public string GetSection(string name)
        {
            string sectionHtml = null;

            if (!Sections.TryGetValue(name, out sectionHtml))
                sectionHtml = null;

            return sectionHtml;
        }

        // If a section is already defined, just concatenate it.
        public void AddSection(string name, string sectionHtml)
        {
            if (Sections.ContainsKey(name))
                Sections[name] += sectionHtml;
            else
                Sections.Add(name, sectionHtml);
        }

        public void DeleteAllSections()
        {
            Sections.Clear();
        }

        public RouteValueDictionary GetFormQueryValues(string name)
        {
            RouteValueDictionary values = null;

            if (!FormQueryValues.TryGetValue(name, out values))
                values = null;

            return values;
        }

        // If a form is already registered, just overwrite it.
        public void AddFormQueryValues(string name, RouteValueDictionary values)
        {
            if (FormQueryValues.ContainsKey(name))
                FormQueryValues[name] = values;
            else
                FormQueryValues.Add(name, values);
        }

        public void DeleteAllFormQueryValues()
        {
            FormQueryValues.Clear();
        }

        public string CurrentUrl
        {
            get
            {
                if (UrlStackIndex > 0)
                    return UrlStack[UrlStackIndex - 1];

                return null;
            }
            set
            {
                if (UrlStackIndex < UrlStack.Count)
                    UrlStack[UrlStackIndex] = value;
                else
                    PushUrl(value);
            }
        }

        public string CurrentUrlNoQuery
        {
            get
            {
                string url = StripQuery(CurrentUrl);
                return url;
            }
        }

        public bool CanGoBack()
        {
            if (UrlStackIndex > 1)
                return true;
            return false;
        }

        public bool GoBack()
        {
            if (CanGoBack())
            {
                PopUrl();
                string url = PeekUrl();
                return HandleRequest(url);
            }

            return false;
        }

        public bool CanGoForward()
        {
            if (UrlStackIndex < UrlStack.Count)
                return true;
            return false;
        }

        public bool GoForward()
        {
            if (CanGoForward())
            {
                UrlStackIndex++;
                string url = PeekUrl();
                return HandleRequest(url);
            }

            return false;
        }

        public void PushUrl(string url)
        {
            if (UrlStackIndex < UrlStack.Count)
            {
                //while (UrlStack.Count > UrlStackIndex)
                //    UrlStack.RemoveAt(UrlStack.Count - 1);

                UrlStack.Insert(UrlStackIndex, url);
                UrlStackIndex++;
            }
            else
            {
                if (UrlStack.Count >= UrlStackMaxCount)
                {
                    UrlStack.RemoveAt(0);
                    UrlStackIndex--;
                }

                UrlStack.Add(url);
                UrlStackIndex++;
            }
        }

        public string PopUrl()
        {
            string url = null;
            if (UrlStackIndex > 0)
            {
                UrlStackIndex--;
                url = UrlStack[UrlStackIndex];
            }
            return url;
        }

        public string PopAndDeleteUrl()
        {
            string url = null;
            if (UrlStackIndex > 0)
            {
                UrlStackIndex--;
                url = UrlStack[UrlStackIndex];
                UrlStack.RemoveAt(UrlStackIndex);
            }
            return url;
        }

        public string PeekUrl()
        {
            if (UrlStackIndex > 0)
                return UrlStack[UrlStackIndex - 1];
            return null;
        }

        public string PeekBackUrl()
        {
            if (UrlStackIndex > 1)
                return UrlStack[UrlStackIndex - 2];
            return null;
        }

        public bool GoToPage(string url)
        {
            PushUrl(url);
            return HandleRequest(url);
        }

        public bool Refresh()
        {
            if (UrlStackIndex > 0)
                return HandleRequest(UrlStack[UrlStackIndex - 1]);
            return false;
        }

        public bool RefreshNoQuery()
        {
            if (UrlStackIndex > 0)
                return HandleRequest(StripQuery(UrlStack[UrlStackIndex - 1]));
            return false;
        }

        public static string StripQuery(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                int offset = url.IndexOf('?');
                if (offset > 0)
                    url = url.Substring(0, offset);
            }

            return url;
        }

        public bool Redirect(string url)
        {
            if (UrlStackIndex > 0)
            {
                if ((UrlStackIndex > 1) && (UrlStack[UrlStackIndex - 2] == url))
                {
                    UrlStackIndex--;
                    while (UrlStack.Count > UrlStackIndex)
                        UrlStack.RemoveAt(UrlStack.Count - 1);
                }
                else
                    UrlStack[UrlStackIndex - 1] = url;
            }
            return HandleRequest(url);
        }

        public bool GoToPreviousPage()
        {
            string url = null;
            if (UrlStackIndex > 0)
            {
                UrlStackIndex--;
                url = UrlStack[UrlStackIndex - 1];
            }
            if (!String.IsNullOrEmpty(url))
                return HandleRequest(url);
            return true;
        }

        private static char[] pathSeps = { '/', '\\' };
        private static char[] commaSeps = { ',' };

        public bool HandleRequest(string url)
        {
            string controllerName;
            string actionName;
            ControllerBase controller;
            MethodInfo method;
            object[] paramsIn;
            bool returnValue;

            if (!ProcessUrl(
                    url,
                    out controllerName,
                    out actionName,
                    out controller,
                    out method,
                    out paramsIn,
                    out returnValue))
                return returnValue;

            // Use DispatchToUIThread as a flag to determine if we use a separate thread.
            if (DispatchToUIThread != null)
            {
                ThreadPool.QueueUserWorkItem(
                    // Call controller action method and process result.
                    threadOp => InvokeController(controller, paramsIn, controllerName, actionName, url, method)
                );
            }
            else
            {
                // Call controller action method and proces result on current thread.
                InvokeController(controller, paramsIn, controllerName, actionName, url, method);
            }

            return true;
        }

        public bool ProcessUrl(
            string url,
            out string controllerName,
            out string actionName,
            out ControllerBase controller,
            out MethodInfo method,
            out object[] paramsIn,
            out bool handleRequestReturnValue)
        {
            string formQueryActionName;
            string queryString;

            paramsIn = null;

            if (ProcessControllerAction(
                    url,
                    out controllerName,
                    out actionName,
                    out formQueryActionName,
                    out queryString,
                    out controller,
                    out method,
                    out handleRequestReturnValue))
            {
                if (!ProcessArguments(method, queryString, formQueryActionName, out paramsIn))
                    return false;
            }
            else
                return false;

            return handleRequestReturnValue;
        }

        public bool ProcessControllerAction(
            string url,
            out string controllerName,
            out string actionName,
            out string formQueryActionName,
            out string queryString,
            out ControllerBase controller,
            out MethodInfo method,
            out bool handleRequestReturnValue)
        {
            // If the URL is not our own custom scheme, just let the webView load the URL as usual
            var scheme = ViewBase.UrlScheme;

            controllerName = String.Empty;
            actionName = String.Empty;
            formQueryActionName = String.Empty;
            queryString = String.Empty;
            controller = null;
            method = null;
            handleRequestReturnValue = false;

            if (!url.StartsWith(scheme))
                return false;

            if (url == scheme)
                return false;

            string path = url.Substring(scheme.Length);

            handleRequestReturnValue = true;

            int queryIndex = path.IndexOf('?');

            if (queryIndex >= 0)
            {
                actionName = path.Substring(0, queryIndex);
                queryString = path.Substring(queryIndex + 1);
            }
            else
                actionName = path;

            controllerName = defaultControllerName;

            if (actionName.Contains("/"))
            {
                var parts = actionName.Split(pathSeps, StringSplitOptions.RemoveEmptyEntries);
                controllerName = parts[0];
                actionName = parts[1];
            }

            formQueryActionName = actionName;

            // Clear sections from previous view.
            DeleteAllSections();

            if (!Controllers.TryGetValue(controllerName, out controller))
            {
                HandleError("Controller not registered: " + controllerName);
                return false;
            }

            controller.Initialize();

            method = controller.GetType().GetRuntimeMethod(actionName);

            // If there is no post function, try to redirect to the GET function of the controller.
            if (method == null)
            {
                if (actionName.EndsWith("Post"))
                {
                    string alternateActionName = actionName.Substring(0, actionName.Length - 4);
                    method = controller.GetType().GetRuntimeMethod(alternateActionName);
                    if (method != null)
                    {
                        url = url.Replace(
                            scheme + controllerName + "/" + actionName,
                            scheme + controllerName + "/" + alternateActionName);
                        actionName = alternateActionName;
                    }
                }
            }

            if (method == null)
            {
                HandleError("Can't find controller function " + actionName);
                return false;
            }

            return true;
        }

        public bool ProcessArguments(
            MethodInfo method,
            string queryString,
            string formQueryActionName,
            out object[] paramsIn)
        {
            ParameterInfo[] methodParams = method.GetParameters();
            Dictionary<string, string> parameters = null;
            bool gotQueryValues = false;
            RouteValueDictionary queryValues = null;
            int index = 0;

            paramsIn = null;

            try
            {
                parameters = HttpUtility.ParseQueryString(queryString);
                paramsIn = new object[methodParams.Length];

                foreach (ParameterInfo p in methodParams)
                {
                    string parameterValueString;
                    object parameterValue = null;

                    if ((p.Attributes & ParameterAttributes.HasDefault) != 0)
                        parameterValue = p.DefaultValue;

                    if (parameters != null)
                    {
                        if (parameters.TryGetValue(p.Name, out parameterValueString))
                        {
                            if (!String.IsNullOrEmpty(parameterValueString))
                                parameterValue = Convert.ChangeType(parameterValueString, p.ParameterType, CultureInfo.InvariantCulture);
                        }
                        else if (p.Name == "form")
                            parameterValue = new FormCollection(parameters);
                        else if (!gotQueryValues || (queryValues != null))
                        {
                            object value;

                            if (!gotQueryValues)
                            {
                                queryValues = GetFormQueryValues(formQueryActionName);
                                gotQueryValues = true;
                            }

                            if (queryValues != null)
                            {
                                if (queryValues.TryGetValue(p.Name, out value))
                                    parameterValue = value;
                            }
                        }
                    }

                    paramsIn[index] = parameterValue;
                    index++;
                }
            }
            catch (Exception exception)
            {
                // Render error view.
                HandleError(exception.Message);

                return false;
            }

            return true;
        }

        public void InvokeController(ControllerBase controller, object[] paramsIn,
            string controllerName, string actionName, string url, MethodInfo method)
        {
            object actionReturnValue = null;

            controller.CurrentUrl = url;

            if (PreHandleRequest != null)
            {
                if (!PreHandleRequest(url))
                    return;
            }

            try
            {
                // Invoke controller function
                actionReturnValue = method.Invoke(controller, paramsIn);
            }
            catch (Exception exception)
            {
                // Render error view.
                HandleError(exception.Message);
                return;
            }

            // Process return result.
            ProcessActionReturn(controller, actionReturnValue, controllerName, actionName, url);
        }

        public void ProcessActionReturn(
            ControllerBase controller,
            object actionReturnValue,
            string controllerName,
            string actionName,
            string url)
        {
            if (actionReturnValue == null)
                HandleError("Controller function must return a value: " + actionName);
            else if (actionReturnValue is ActionResult)
            {
                ActionResult actionResult = actionReturnValue as ActionResult;
                switch (actionResult.Action)
                {
                    case ActionType.View:
                        {
                            ViewBase view = actionResult.View;
                            if (view != null)
                            {
                                ControllerBase saveController = CurrentController;
                                string saveControllerGroup = CurrentControllerGroup;
                                string saveControllerAction = CurrentControllerAction;
                                string saveControllerUrl = controller.CurrentUrl;

                                CurrentController = controller;
                                CurrentControllerGroup = controllerName;
                                CurrentControllerAction = actionName;
                                controller.CurrentUrl = url;
                                CurrentWaitUrl = url;

                                try
                                {
                                    RenderViewToWebView(view, url);
                                }
                                catch (Exception exception)
                                {
                                    // Render error view.
                                    HandleError(exception.Message);
                                }
                                finally
                                {
                                    // Restore "current" controller state.
                                    CurrentController = saveController;
                                    CurrentControllerGroup = saveControllerGroup;
                                    CurrentControllerAction = saveControllerAction;
                                    controller.CurrentUrl = saveControllerUrl;
                                }

                                return; // Because we will do the PostHandleRequest callback in the UI thread.
                            }
                            else
                                HandleError(
                                    "Couldn't find view for controller->action: " + controllerName + "->" + actionName);
                        }
                        break;
                    case ActionType.Redirect:
                        Dispatch(cb => Redirect(actionResult.RedirectUrl));
                        break;
                    case ActionType.Back:
                        Dispatch(cb =>
                            {
                                PopAndDeleteUrl();
                                GoBack();
                            }
                        );
                        break;
                    case ActionType.Forward:
                        Dispatch(cb =>
                            {
                                PopAndDeleteUrl();
                                GoForward();
                            }
                        );
                        break;
                    default:
                        Dispatch(cb =>
                            {
                                HandleError(
                                    "Unexpected action type for controller->action: "
                                        + controllerName + "->" + actionName + ": " + actionResult.Action.ToString());
                            }
                        );
                        break;
                }
            }
            else if (actionReturnValue is JsonResult)
            {
                HandleError(
                    "Unexpected controller function return type of JsonResult for controller->action: "
                        + controllerName + "->" + actionName);
            }
            else if (actionReturnValue is WaitResult)
            {
                CurrentWaitResult = actionReturnValue as WaitResult;
            }
            else
                throw new Exception("Unsupported action return object type.");

            if (PostHandleRequest != null)
                PostHandleRequest(url);
        }

        public void Dispatch(WaitCallback callback)
        {
            if (DispatchToUIThread != null)
            {
                DispatchToUIThread(callback);
            }
            else
                callback(null);
        }

        public virtual void RunAsThread(WaitCallback threadOp, WaitCallback continueOp)
        {
            ThreadPool.QueueUserWorkItem(o => SubThread(threadOp, continueOp));
        }

        public void SubThread(WaitCallback threadOp, WaitCallback continueOp)
        {
            threadOp(null);
            continueOp(null);
        }

        public void HandleAction(ActionResult actionResult)
        {
            switch (actionResult.Action)
            {
                case ActionType.View:
                    {
                        CurrentView = actionResult.View;
                        if (CurrentView != null)
                            RenderViewToWebView(CurrentView, CurrentWaitUrl);
                        else
                            HandleError("Couldn't find view for action: " + actionResult.ActionName);
                    }
                    break;
                case ActionType.Redirect:
                    CurrentController = null;
                    CurrentView = null;
                    Redirect(actionResult.RedirectUrl);
                    break;
                case ActionType.Back:
                    PopAndDeleteUrl();
                    GoBack();
                    break;
                case ActionType.Forward:
                    PopAndDeleteUrl();
                    GoForward();
                    break;
                default:
                    HandleError("Unexpected action type for controller action "
                        + actionResult.ActionName + ": " + actionResult.Action.ToString());
                    break;
            }
        }

        private class AjaxRequest
        {
            public string url;
            public string type;
            public JObject data;
            public string complete;

            public AjaxRequest()
            {
                this.url = null;
                this.type = null;
                this.data = null;
                this.complete = null;
            }
        }

        private class AjaxReturn
        {
            public string responseText;
            public string responseJSON;

            public AjaxReturn(string returnType, string returnValue)
            {
                switch (returnType)
                {
                    default:
                    case "responseText":
                        responseText = returnValue;
                        break;
                    case "responseJSON":
                        responseJSON = returnValue;
                        break;
                }
            }
        }

        public string HandleAjaxRequest(string jsonObject)
        {
            AjaxRequest obj = JsonConvert.DeserializeObject<AjaxRequest>(jsonObject);
            RouteValueDictionary values = null;
            if (obj.data != null)
            {
                values = new RouteValueDictionary();
                foreach (JProperty property in obj.data.Properties())
                {
                    Type type = null;
                    JTokenType tokenType = property.Value.Type;
                    switch (tokenType)
                    {
                        case JTokenType.None:
                            type = null;
                            break;
                        case JTokenType.Object:
                            type = typeof(object);
                            break;
                        case JTokenType.Array:
                            // This is hackish.  We hope the array is a list of strings.
                            type = typeof(List<string>);
                            break;
                        case JTokenType.Constructor:
                            type = null;
                            break;
                        case JTokenType.Property:
                            type = null;
                            break;
                        case JTokenType.Comment:
                            type = typeof(string);
                            break;
                        case JTokenType.Integer:
                            type = typeof(int);
                            break;
                        case JTokenType.Float:
                            type = typeof(float);
                            break;
                        case JTokenType.String:
                            type = typeof(string);
                            break;
                        case JTokenType.Boolean:
                            type = typeof(bool);
                            break;
                        case JTokenType.Null:
                            type = null;
                            break;
                        case JTokenType.Undefined:
                            type = null;
                            break;
                        case JTokenType.Date:
                            type = typeof(DateTime);
                            break;
                        case JTokenType.Raw:
                            type = typeof(byte[]);
                            break;
                        case JTokenType.Bytes:
                            type = typeof(byte[]);
                            break;
                        case JTokenType.Guid:
                            type = typeof(Guid);
                            break;
                        case JTokenType.Uri:
                            type = typeof(string);
                            break;
                        case JTokenType.TimeSpan:
                            type = typeof(TimeSpan);
                            break;
                        default:
                            throw new Exception("JavaScriptInterop.Ajax needs support for type: " + tokenType.ToString());
                    }
                    values.Add(property.Name, property.Value.ToObject(type));
                }
            }
            string returnType;
            string returnValue = HandleAjax(obj.url, obj.type, values, out returnType);
            if (returnValue != null)
            {
                AjaxReturn ajaxReturn = new AjaxReturn(returnType, returnValue);
                string returnJson = JsonConvert.SerializeObject(ajaxReturn);
                return returnJson;
            }
            return null;
        }

        public string HandleAjax(string url, string type, RouteValueDictionary parameters,
            out string returnType /* "responseText" or "responseJSON" */)
        {
            string controllerName;
            string actionName;
            ControllerBase controller;
            MethodInfo method;
            object[] paramsIn;
            bool returnValue;
            string ajaxReturn = null;

            returnType = "responseText";

            if (!ProcessAjaxUrl(
                    url,
                    parameters,
                    out controllerName,
                    out actionName,
                    out controller,
                    out method,
                    out paramsIn,
                    out returnValue))
                return null;

            ControllerBase saveController = null;
            string saveControllerGroup = null;
            string saveControllerAction = null;
            string saveControllerUrl = null;

            try
            {
                saveController = CurrentController;
                saveControllerGroup = CurrentControllerGroup;
                saveControllerAction = CurrentControllerAction;
                saveControllerUrl = controller.CurrentUrl;

                CurrentControllerGroup = controllerName;
                CurrentControllerAction = actionName;
                controller.CurrentUrl = url;

                object actionReturnValue = method.Invoke(controller, paramsIn);

                if (actionReturnValue != null)
                {
                    if (actionReturnValue is ActionResult)
                    {
                        ActionResult actionResult = actionReturnValue as ActionResult;
                        switch (actionResult.Action)
                        {
                            case ActionType.View:
                                {
                                    ViewBase view = actionResult.View;
                                    if (view != null)
                                    {
                                        ajaxReturn = RenderViewToString(view);
                                        returnType = "responseText";
                                    }
                                    else
                                        HandleError("Couldn't find view for action: " + actionName);
                                }
                                break;
                            default:
                                HandleError("Controller functions for Ajax can only return view action results: " + actionName);
                                break;
                        }
                    }
                    else if (actionReturnValue is JsonResult)
                    {
                        JsonResult jsonResult = actionReturnValue as JsonResult;
                        ajaxReturn = jsonResult.JsonData;
                        returnType = "responseJSON";
                    }
                    else
                        HandleError("Unexpected Ajax return value type: " + actionReturnValue.GetType().Name);
                }
            }
            catch (Exception exception)
            {
                // Render error view.
                HandleError(exception.Message);
            }
            finally
            {
                // Restore "current" controller state.
                CurrentController = saveController;
                CurrentControllerGroup = saveControllerGroup;
                CurrentControllerAction = saveControllerAction;
                controller.CurrentUrl = saveControllerUrl;
            }

            return ajaxReturn;
        }

        public bool ProcessAjaxUrl(
            string url,
            RouteValueDictionary parameters,
            out string controllerName,
            out string actionName,
            out ControllerBase controller,
            out MethodInfo method,
            out object[] paramsIn,
            out bool handleRequestReturnValue)
        {
            string formQueryActionName;
            string queryString;

            paramsIn = null;

            if (ProcessControllerAction(
                    url,
                    out controllerName,
                    out actionName,
                    out formQueryActionName,
                    out queryString,
                    out controller,
                    out method,
                    out handleRequestReturnValue))
            {
                if (!String.IsNullOrEmpty(queryString))
                {
                    Dictionary<string, string> rawParameters = HttpUtility.ParseQueryString(queryString);

                    if (parameters == null)
                        parameters = new RouteValueDictionary();

                    foreach (KeyValuePair<string, string> kvp in rawParameters)
                    {
                        object testValue;

                        if (!parameters.TryGetValue(kvp.Key, out testValue))
                            parameters.Add(kvp.Key, kvp.Value);
                    }
                }

                if (!ProcessAjaxArguments(method, parameters, formQueryActionName, out paramsIn))
                    return false;
            }

            return true;
        }

        public bool ProcessAjaxArguments(
            MethodInfo method,
            RouteValueDictionary parameters,
            string formQueryActionName,
            out object[] paramsIn)
        {
            ParameterInfo[] methodParams = method.GetParameters();
            int index = 0;

            paramsIn = null;

            try
            {
                paramsIn = new object[methodParams.Length];

                foreach (ParameterInfo p in methodParams)
                {
                    object parameterValue = null;

                    if ((p.Attributes & ParameterAttributes.HasDefault) != 0)
                        parameterValue = p.DefaultValue;

                    if (parameters != null)
                    {
                        if (!parameters.TryGetValue(p.Name, out parameterValue))
                            parameterValue = null;
                    }

                    paramsIn[index] = parameterValue;
                    index++;
                }
            }
            catch (Exception exception)
            {
                // Render error view.
                HandleError(exception.Message);

                return false;
            }

            return true;
        }

        public void HandleError(string message)
        {
            ViewBase view = FindView("Error");
            var scheme = ViewBase.UrlScheme;
            string url = scheme + "Error";
            if (view != null)
            {
                view.SetModel(message);
                RenderViewToWebView(view, url);
            }
            else
            {
                string pageHtml = "<html><page>Error</page></html>";
                LoadHtmlString_Dispatch(url, pageHtml);
            }
        }

        protected bool RenderViewToWebView(ViewBase view, string url)
        {
            if (view != null)
            {
                string pageHtml = RenderViewToString(view);
                LoadHtmlString_Dispatch(url, pageHtml);
            }

            return true;
        }

        public void LoadHtmlString_Dispatch(string url, string pageHtml)
        {
            if (DispatchToUIThread != null)
            {
                DispatchToUIThread(
                    callback => LoadHtmlString(url, pageHtml)
                );
            }
            else
                LoadHtmlString(url, pageHtml);
        }

        public void LoadHtmlString(string url, string pageHtml)
        {
            HybridView.LoadHtmlString(url, pageHtml);

            if (PostHandleRequest != null)
                PostHandleRequest(url);
        }

        protected string RenderViewToString(ViewBase view)
        {
            string pageHtml = null;

            if (view != null)
            {
                object model = view.GetModel();
                ViewBase saveView = CurrentView;
                CurrentView = view;
                // The view gets rendered here.
                pageHtml = view.GenerateString();
                string layoutName = view.LayoutName;
                if (!String.IsNullOrEmpty(layoutName))
                {
                    ViewBase layoutView = GetViewWithModel(layoutName, model);
                    if (layoutView == null)
                        throw new Exception("Can't find layout view: " + view.Layout);
                    CurrentView = layoutView;
                    string layoutHtml = layoutView.GenerateString();
                    pageHtml = layoutHtml.Replace(ViewBase.RenderBodyPlaceholder, pageHtml);
                }
                pageHtml = RenderSections(pageHtml);
                CurrentView = saveView;
            }

            return pageHtml;
        }

        protected string RenderSections(string pageHtml)
        {
            int offset = 0;
            // Handle sections here.
            while ((offset = pageHtml.IndexOf(ViewBase.RenderSectionPlaceholder, offset)) != -1)
            {
                int argsOffset = offset + ViewBase.RenderSectionPlaceholder.Length;
                int end = pageHtml.IndexOf(")", argsOffset);

                if (end != -1)
                {
                    end++;
                    string args = pageHtml.Substring(argsOffset + 1, (end - argsOffset) - 2);
                    pageHtml = pageHtml.Remove(offset, end - offset);
                    string[] argParts = args.Split(commaSeps);
                    string sectionName = argParts[0].Trim();
                    string requiredString = argParts[1].Trim();
                    bool required = (requiredString == "true" ? true : false);
                    string sectionHtml = GetSection(sectionName);
                    if (!String.IsNullOrEmpty(sectionHtml))
                    {
                        pageHtml = pageHtml.Insert(offset, sectionHtml);
                        offset += sectionHtml.Length;
                    }
                }
                else
                    pageHtml = pageHtml.Remove(offset, argsOffset);
            }

            return pageHtml;
        }

        public IHtmlString Render(params string[] paths)
        {
            StringBuilder sb = new StringBuilder();

            if (paths != null)
            {
                foreach (string path in paths)
                {
                    WebBundle bundle = FindBundle(path);

                    if (bundle != null)
                        bundle.Render(sb);
                }
            }

            return new HtmlString(sb.ToString());
        }

        // Implement sub-view rendering.
        // Note: It would be natural to put support for sections here, but because MVC doesn't support it
        // we won't either.
        public void RenderPartialImplementation(string partialViewName, object model)
        {
            if (String.IsNullOrEmpty(partialViewName))
                return;

            if (model == null)
                model = CurrentView.GetModel();

            string[] parts = partialViewName.Split(pathSeps);
            string viewName;

            if (parts.Length != 0)
                viewName = parts[parts.Length - 1];
            else
                viewName = partialViewName;

            ViewBase saveView = CurrentView;
            TextWriter saveWriter = saveView.Writer;
            object saveModel = saveView.GetModel();

            ViewBase view = GetViewWithModel(viewName, model);

            if (view == null)
            {
                saveView.SetModel(saveModel);
                throw new Exception("View not registered: " + viewName);
            }

            CurrentView = view;
            view.Generate(saveView.Writer);
            CurrentView = saveView;
            saveView.Writer = saveWriter;
            saveView.SetModel(saveModel);
        }

        // Implement sub-view rendering, returning the HTML directly.
        public string PartialImplementation(string partialViewName, object model)
        {
            if (String.IsNullOrEmpty(partialViewName))
                return String.Empty;

            if (model == null)
                model = CurrentView.GetModel();

            string[] parts = partialViewName.Split(pathSeps);
            string viewName;

            if (parts.Length != 0)
                viewName = parts[parts.Length - 1];
            else
                viewName = partialViewName;

            ViewBase view = GetViewWithModel(viewName, model);

            if (view == null)
                throw new Exception("View not registered: " + viewName);

            ViewBase saveView = CurrentView;
            CurrentView = view;
            string pageHtml = view.GenerateString();
            CurrentView = saveView;

            return pageHtml;
        }
    }
}
