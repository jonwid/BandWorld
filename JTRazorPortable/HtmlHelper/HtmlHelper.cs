using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using JTRazorPortable;

namespace JTRazorPortable
{
	public partial class HtmlHelper {
		private TextWriter _writer;
        public static readonly string ValidationInputCssClassName = "input-validation-error";
        public static readonly string ValidationInputValidCssClassName = "input-validation-valid";
        public static readonly string ValidationMessageCssClassName = "field-validation-error";
        public static readonly string ValidationMessageValidCssClassName = "field-validation-valid";
        public static readonly string ValidationSummaryCssClassName = "validation-summary-errors";
        public static readonly string ValidationSummaryValidCssClassName = "validation-summary-valid";

        public HtmlHelper(TextWriter writer, ModelStateDictionary modelState) {
			_writer = writer;
            ModelState = modelState;
		}

        public ModelStateDictionary ModelState { get; set; }

        public IHtmlString Raw(string value) {
			return new HtmlString (value);
		}

        public string Encode(object value)
        {
            return Encode(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public string Encode(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            else
            {
                return HttpUtility.HtmlEncode(value);
            }
        }

        private string GenerateHtmlAttributes(object htmlAttributes) {
			var attrs = new StringBuilder ();
			if (htmlAttributes != null) {
				foreach (var property in htmlAttributes.GetType ().GetProperties()) 
					attrs.AppendFormat (@" {0}=""{1}""", property.Name.Replace('_', '-'), property.GetGetMethod().Invoke (htmlAttributes, null));
			}
			return attrs.ToString ();
		}

        private string GenerateHtmlAttributes(IDictionary<string, object> htmlAttributes)
        {
            var attrs = new StringBuilder();
            if (htmlAttributes != null)
            {
                foreach (KeyValuePair<string, object> kvp in htmlAttributes)
                    attrs.AppendFormat(@" {0}=""{1}""", kvp.Key.Replace('_', '-'), kvp.Value.ToString());
            }
            return attrs.ToString();
        }

        public RouteValueDictionary AnonymousObjectToHtmlAttributes(object htmlAttributes)
        {
            RouteValueDictionary result = new RouteValueDictionary();

            if (htmlAttributes != null)
            {
                foreach (PropertyInfo property in htmlAttributes.GetType().GetProperties())
                    result.Add(property.Name, property.GetValue(htmlAttributes, null));
            }

            return result;
        }

        public TextWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        public ViewBase ViewContext
        {
            get
            {
                return MVCManager.Global.CurrentView;
            }
        }

        public MVCManager MVCManager
        {
            get
            {
                return MVCManager.Global;
            }
        }
    }
}

