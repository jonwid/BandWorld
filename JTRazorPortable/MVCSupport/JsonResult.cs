using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JTRazorPortable
{
    public enum JsonRequestBehavior
    {
        AllowGet,
        DenyGet,
    }

    public class JsonResult
    {
        public JsonResult()
        {
            JsonRequestBehavior = JsonRequestBehavior.DenyGet;
            _JsonData = null;
        }

        public Encoding ContentEncoding { get; set; }

        public string ContentType { get; set; }

        public object Data { get; set; }

        public JsonRequestBehavior JsonRequestBehavior { get; set; }

        public int? MaxJsonLength { get; set; }

        public int? RecursionLimit { get; set; }

        private string _JsonData;

#if NOT_PORTABLE
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
                String.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(MvcResources.JsonRequest_GetNotAllowed);
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data != null)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                if (MaxJsonLength.HasValue)
                {
                    serializer.MaxJsonLength = MaxJsonLength.Value;
                }
                if (RecursionLimit.HasValue)
                {
                    serializer.RecursionLimit = RecursionLimit.Value;
                }
                response.Write(serializer.Serialize(Data));
            }
        }
#else
        public string JsonData
        {
            get
            {
                if (_JsonData != null)
                    return _JsonData;
                string returnJson = JsonConvert.SerializeObject(Data);
                return returnJson;
            }
            set
            {
                _JsonData = value;
            }
        }
#endif
    }
}
