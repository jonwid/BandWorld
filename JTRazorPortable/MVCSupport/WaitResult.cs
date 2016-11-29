using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTRazorPortable
{
    public class WaitResult
    {
        public string ReturnUrl { get; set; }

        public WaitResult(string returnUrl)
        {
            ReturnUrl = returnUrl;
        }
    }
}
