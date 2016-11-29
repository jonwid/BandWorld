using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTRazorPortable
{
    public static class StringUtilities
    {
        public static string GetFileName(string filePath)
        {
            string fileName = filePath;
            char[] delimiters = { '/', '\\' };
            string[] parts = filePath.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if ((parts != null) && (parts.Count() != 0))
                fileName = parts[parts.Count() - 1];
            int offset = fileName.IndexOf('?');
            if (offset > 0)
                fileName = fileName.Substring(0, offset);
            return fileName;
        }

        public static string GetBaseFileName(string filePath)
        {
            string fileName = GetFileName(filePath);
            if (String.IsNullOrEmpty(fileName))
                return fileName;
            int offset = fileName.LastIndexOf('.');
            if (offset > 0)
                fileName = fileName.Substring(0, offset);
            return fileName;
        }
    }
}
