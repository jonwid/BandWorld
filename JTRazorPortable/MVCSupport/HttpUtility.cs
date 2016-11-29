// 
// System.Web.HttpUtility
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace JTRazorPortable
{
	public sealed class HttpUtility
	{
		sealed class HttpQSCollection : Dictionary<string, string>
		{
			public override string ToString ()
			{
				int count = Count;
				if (count == 0)
					return "";
				StringBuilder sb = new StringBuilder ();
				var keys = this.Keys;
				foreach (var key in this.Keys) {
					sb.AppendFormat ("{0}={1}&", key, this [key]);
				}
				if (sb.Length > 0)
					sb.Length--;
				return sb.ToString ();
			}
		}

		public HttpUtility ()
		{
		}

		public static Dictionary<string, string> ParseQueryString (string query)
		{
			return ParseQueryString (query, Encoding.UTF8);
		}

		public static Dictionary<string, string> ParseQueryString (string query, Encoding encoding)
		{
			if (query == null)
				throw new ArgumentNullException ("query");
			if (encoding == null)
				throw new ArgumentNullException ("encoding");
			if (query.Length == 0 || (query.Length == 1 && query [0] == '?'))
				return new HttpQSCollection ();
			if (query [0] == '?')
				query = query.Substring (1);

			var result = new HttpQSCollection ();
			ParseQueryString (query, encoding, result);
			return result;
		}

		internal static void ParseQueryString (string query, Encoding encoding, Dictionary<string, string> result)
		{
			if (query.Length == 0)
				return;

			string decoded = HtmlDecode(query);
			int decodedLength = decoded.Length;
			int namePos = 0;
			bool first = true;
			while (namePos <= decodedLength) {
				int valuePos = -1, valueEnd = -1;
				for (int q = namePos; q < decodedLength; q++) {
					if (valuePos == -1 && decoded [q] == '=') {
						valuePos = q + 1;
					} else if (decoded [q] == '&') {
						valueEnd = q;
						break;
					}
				}

				if (first) {
					first = false;
					if (decoded [namePos] == '?')
						namePos++;
				}

				string name, value;
				if (valuePos == -1) {
					name = null;
					valuePos = namePos;
				} else {
					name = UrlDecode (decoded.Substring (namePos, valuePos - namePos - 1));
				}
				if (valueEnd < 0) {
					namePos = -1;
					valueEnd = decoded.Length;
				} else {
					namePos = valueEnd + 1;
				}
				value = UrlDecode (decoded.Substring (valuePos, valueEnd - valuePos));

                string tmp;
                if (result.TryGetValue(name, out tmp))
                    result[name] = tmp + value;
                else
    				result.Add (name, value);
				if (namePos == -1)
					break;
			}
		}

        public static string UrlEncode(string str)
        {
            if (str != null)
            {
                if (str != null)
                {
                    str = str.Replace("|", "%7C");
                    str = str.Replace("\"", "%22");
                    str = str.Replace("<", "%3C");
                    str = str.Replace(">", "%3E");
                    str = str.Replace("(", "%28");
                    str = str.Replace(")", "%29");
                    //str = str.Replace("%", "%25");
                    str = str.Replace("&", "%26");
                    str = str.Replace("/", "%2F");
                    str = str.Replace("=", "%3D");
                    str = str.Replace("?", "%3F");
                    str = str.Replace(":", "%3A");
                    str = str.Replace("'", "%27");
                    str = str.Replace(" ", "%20");
                    str = str.Replace("\t", "%09");
                    str = str.Replace("\r", "%0D");
                    str = str.Replace("\n", "%0A");
                    str = str.Replace("ā", "%u0101");
                    str = str.Replace("ō", "%u014D");
                    str = str.Replace("ū", "%u016B");
                    str = str.Replace("ē", "%u0113");
                    str = str.Replace("ī", "%u012B");
                    str = str.Replace("Ā", "%u0100");
                    str = str.Replace("Ō", "%u014C");
                    str = str.Replace("Ū", "%u016A");
                    str = str.Replace("Ē", "%u0112");
                    str = str.Replace("Ī", "%u012A");
                }
            }

            return str;
        }

        public static string UrlDecode(string str)
        {
            if (str != null)
            {
                string lastStr;

                do
                {
                    lastStr = str;
                    str = str.Replace("%7C", "|");
                    str = str.Replace("%22", "\"");
                    str = str.Replace("%3C", "<");
                    str = str.Replace("%3E", ">");
                    str = str.Replace("%28", "(");
                    str = str.Replace("%29", ")");
                    str = str.Replace("%25", "%");
                    str = str.Replace("%26", "&");
                    str = str.Replace("%2F", "/");
                    str = str.Replace("%3D", "=");
                    str = str.Replace("%3F", "?");
                    str = str.Replace("%3A", ":");
                    str = str.Replace("%27", "'");
                    str = str.Replace("%20", " ");
                    str = str.Replace("%09", "\t");
                    str = str.Replace("%0D", "\r");
                    str = str.Replace("%0A", "\n");
                    str = str.Replace("%u0101", "ā");
                    str = str.Replace("%u014D", "ō");
                    str = str.Replace("%u016B", "ū");
                    str = str.Replace("%u0113", "ē");
                    str = str.Replace("%u012B", "ī");
                    str = str.Replace("%u0100", "Ā");
                    str = str.Replace("%u014C", "Ō");
                    str = str.Replace("%u016A", "Ū");
                    str = str.Replace("%u0112", "Ē");
                    str = str.Replace("%u012A", "Ī");
                }
                while (str != lastStr);
            }

            return str;
        }

        public static string HtmlEncode(string str)
        {
            if (str != null)
            {
                str = str.Replace("&", "&amp;");
                str = str.Replace("<", "&lt;");
                str = str.Replace(">", "&gt;");
                str = str.Replace("\"", "&quot;");
                str = str.Replace("\r", "%0D");
                str = str.Replace("\n", "%0A");
            }

            return str;
        }

        public static void HtmlEncode(string value, TextWriter output)
        {
            output.Write(HtmlEncode(value));
        }

        public static string HtmlDecode(string str)
        {
            if (str != null)
            {
                string lastStr;

                do
                {
                    lastStr = str;
                    str = str.Replace("&amp;", "&");
                    str = str.Replace("&lt;", "<");
                    str = str.Replace("&gt;", ">");
                    str = str.Replace("&quot;", "\"");
                    str = str.Replace("%0D", "\r");
                    str = str.Replace("%0A", "\n");
                }
                while (str != lastStr);
            }

            return str;
        }

        public static string HtmlAttributeEncode(string str)
        {
            if (str != null)
            {
                str = str.Replace("\"", "&quot;");
                str = str.Replace("\r", "%0D");
                str = str.Replace("\n", "%0A");
            }

            return str;
        }

        public static void HtmlAttributeEncode(string str, TextWriter output)
        {
            output.Write(HtmlAttributeEncode(str));
        }

        public static void HtmlAttributeEncodeInternal(string str, TextWriter output)
        {
            output.Write(HtmlAttributeEncode(str));
        }

        public static string HtmlAttributeDecode(string str)
        {
            if (str != null)
            {
                string lastStr;

                do
                {
                    lastStr = str;
                    str = str.Replace("&quot;", "\"");
                    str = str.Replace("%0D", "\r");
                    str = str.Replace("%0A", "\n");
                }
                while (str != lastStr);
            }

            return str;
        }

        public static string JavascriptEncode(string str)
        {
            if (str != null)
            {
                str = str.Replace("'", "\\'");
                str = str.Replace("\"", "\\\"");
                str = str.Replace("\r\n", "\\n");
            }

            return str;
        }

        public static string UrlPathEncode(string str)
        {
            if (str == null)
            {
                return null;
            }

            // recurse in case there is a query string
            int i = str.IndexOf('?');
            if (i >= 0)
            {
                return UrlPathEncode(str.Substring(0, i)) + str.Substring(i);
            }

            // encode DBCS characters and spaces only
            return UrlEncodeSpaces(UrlEncodeNonAscii(str, Encoding.UTF8));
        }

        //  Helper to encode the non-ASCII url characters only
        public static string UrlEncodeNonAscii(string str, Encoding e)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            if (e == null)
            {
                e = Encoding.UTF8;
            }
            byte[] bytes = e.GetBytes(str);
            bytes = UrlEncodeBytesToBytesInternalNonAscii(bytes, 0, bytes.Length, false);
#if NOT_PORTABLE
            return Encoding.ASCII.GetString(bytes);
#else
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
#endif
        }

        //  Helper to encode spaces only
        public static string UrlEncodeSpaces(string str)
        {
            if (str != null && str.IndexOf(' ') >= 0)
            {
                str = str.Replace(" ", "%20");
            }
            return str;
        }

        public static byte[] UrlEncodeBytesToBytesInternalNonAscii(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
        {
            int cNonAscii = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                if (IsNonAsciiByte(bytes[offset + i]))
                {
                    cNonAscii++;
                }
            }

            // nothing to expand?
            if (!alwaysCreateReturnValue && cNonAscii == 0)
            {
                return bytes;
            }

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cNonAscii * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];

                if (IsNonAsciiByte(b))
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
                else
                {
                    expandedBytes[pos++] = b;
                }
            }

            return expandedBytes;
        }

        public static bool IsNonAsciiByte(byte b)
        {
            return (b >= 0x7F || b < 0x20);
        }

        public static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + (int)'0');
            }
            else
            {
                return (char)(n - 10 + (int)'a');
            }
        }

        // Set of safe chars, from RFC 1738.4 minus '+'
        public static bool IsSafe(char ch)
        {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
            {
                return true;
            }

            switch (ch)
            {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '\'':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }
    }
}
