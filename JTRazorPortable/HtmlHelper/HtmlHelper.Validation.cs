using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTRazorPortable
{
    public partial class HtmlHelper
    {
        public MvcHtmlString ValidationMessage(string name)
        {
            return ValidationMessage(name, null, null);
        }

        public MvcHtmlString ValidationMessage(string name, string message)
        {
            return ValidationMessage(name, message, (IDictionary<string, object>)null);
        }

        public MvcHtmlString ValidationMessage(string name, object htmlAttributes)
        {
            return ValidationMessage(name, null, TypeHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public MvcHtmlString ValidationMessage(string name, IDictionary<string, object> htmlAttributes)
        {
            return ValidationMessage(name, null, htmlAttributes);
        }

        public MvcHtmlString ValidationMessage(string name, string message, object htmlAttributes)
        {
            return ValidationMessage(name, message, TypeHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public MvcHtmlString ValidationMessage(string name, string message, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Argument cannot be null or empty: name");
            }
            return BuildValidationMessage(name, message, htmlAttributes);
        }

        private MvcHtmlString BuildValidationMessage(string name, string message, IDictionary<string, object> htmlAttributes)
        {
            var modelState = ModelState[name];
            ModelErrorCollection errors = null;
            if (modelState != null)
            {
                errors = modelState.Errors;
            }
            bool hasError = errors != null && errors.Any();
            if (!hasError)
            {
                // If unobtrusive validation is enabled, we need to generate an empty span with the "val-for" attribute"
                return null;
            }
            else
            {
                string error = null;
                if (hasError)
                {
                    error = message ?? errors.First().ErrorMessage;
                }

                TagBuilder tagBuilder = new TagBuilder("span") { InnerHtml = Encode(error) };
                tagBuilder.MergeAttributes(htmlAttributes);
                tagBuilder.AddCssClass(hasError ? ValidationMessageCssClassName : ValidationMessageValidCssClassName);
                return tagBuilder.ToHtmlString(TagRenderMode.Normal);
            }
        }

        public MvcHtmlString ValidationSummary()
        {
            return BuildValidationSummary(message: null, excludeFieldErrors: false, htmlAttributes: (IDictionary<string, object>)null);
        }

        public MvcHtmlString ValidationSummary(string message)
        {
            return BuildValidationSummary(message: message, excludeFieldErrors: false, htmlAttributes: (IDictionary<string, object>)null);
        }

        public MvcHtmlString ValidationSummary(bool excludeFieldErrors)
        {
            return ValidationSummary(message: null, excludeFieldErrors: excludeFieldErrors, htmlAttributes: (IDictionary<string, object>)null);
        }

        public MvcHtmlString ValidationSummary(object htmlAttributes)
        {
            return ValidationSummary(message: null, excludeFieldErrors: false, htmlAttributes: htmlAttributes);
        }

        public MvcHtmlString ValidationSummary(IDictionary<string, object> htmlAttributes)
        {
            return ValidationSummary(message: null, excludeFieldErrors: false, htmlAttributes: htmlAttributes);
        }

        public MvcHtmlString ValidationSummary(string message, object htmlAttributes)
        {
            return ValidationSummary(message, excludeFieldErrors: false, htmlAttributes: htmlAttributes);
        }

        public MvcHtmlString ValidationSummary(string message, IDictionary<string, object> htmlAttributes)
        {
            return ValidationSummary(message, excludeFieldErrors: false, htmlAttributes: htmlAttributes);
        }

        public MvcHtmlString ValidationSummary(string message, bool excludeFieldErrors, object htmlAttributes)
        {
            return ValidationSummary(message, excludeFieldErrors, TypeHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public MvcHtmlString ValidationSummary(string message, bool excludeFieldErrors, IDictionary<string, object> htmlAttributes)
        {
            return BuildValidationSummary(message, excludeFieldErrors, htmlAttributes);
        }

        public MvcHtmlString ValidationSummary(bool excludePropertyErrors, string message)
        {
            return BuildValidationSummary(message, excludePropertyErrors, null);
        }

        private MvcHtmlString BuildValidationSummary(string message, bool excludeFieldErrors, IDictionary<string, object> htmlAttributes)
        {
            ModelErrorCollection errors = null;
            if (excludeFieldErrors)
            {
                // Review: Is there a better way to share the form field name between this and ModelStateDictionary?
                var formModelState = ModelState[ModelStateDictionary.FormFieldKey];
                if (formModelState != null)
                {
                    errors = formModelState.Errors;
                }
            }
            else
            {
                errors = new ModelErrorCollection();
                foreach (KeyValuePair<string, ModelState> kvp in ModelState)
                {
                    errors.AddRange(kvp.Value.Errors);
                }
            }

            bool hasErrors = errors != null && errors.Any();
            if (!hasErrors && excludeFieldErrors)
            {
                // If no errors are found and we do not have unobtrusive validation enabled or if the summary is not meant to display field errors, don't generate the summary.
                return null;
            }
            else
            {
                TagBuilder tagBuilder = new TagBuilder("div");
                tagBuilder.MergeAttributes(htmlAttributes);
                tagBuilder.AddCssClass(hasErrors ? ValidationSummaryCssClassName : ValidationSummaryValidCssClassName);
                if (!excludeFieldErrors)
                {
                    tagBuilder.MergeAttribute("data-valmsg-summary", "true");
                }

                StringBuilder builder = new StringBuilder();
                if (message != null)
                {
                    builder.Append("<span>");
                    builder.Append(Encode(message));
                    builder.AppendLine("</span>");
                }
                builder.AppendLine("<ul>");
                foreach (var error in errors)
                {
                    builder.Append("<li>");
                    builder.Append(Encode(error.ErrorMessage));
                    builder.AppendLine("</li>");
                }
                builder.Append("</ul>");

                tagBuilder.InnerHtml = builder.ToString();
                return tagBuilder.ToHtmlString(TagRenderMode.Normal);
            }
        }
    }
}
