using System;
using System.IO;
using System.Reflection;
using System.Text;
using JTRazorPortable;

namespace JTRazorPortable
{
	public partial class HtmlHelper
	{
		public IHtmlString Label(string labelText, object htmlAttributes) {
			return Label(labelText, labelFor: "", htmlAttributes:htmlAttributes);
		}

		public IHtmlString Label(string labelText, string labelFor = "", object htmlAttributes = null) {
			return new HtmlString(string.Format ("<label for=\"{0}\"{1}>{2}</label>", 
				labelFor,
				GenerateHtmlAttributes (htmlAttributes),
				labelText));
		}
	}
}

