using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTRazorPortable
{
    public class FormCollection : Dictionary<string, string>
    {
        public FormCollection()
        {
        }

        public FormCollection(int size) : base(size)
        {
        }

        public FormCollection(Dictionary<string, string> other) : base(other)
        {
        }

        public string[] AllKeys
        {
            get
            {
                List<string> keys = new List<string>();

                foreach (KeyValuePair<string, string> kvp in this)
                    keys.Add(kvp.Key);

                return keys.ToArray();
            }
        }

        public string[] AllValues
        {
            get
            {
                List<string> values = new List<string>();

                foreach (KeyValuePair<string, string> kvp in this)
                    values.Add(kvp.Value);

                return values.ToArray();
            }
        }

        public string Get(int index)
        {
            return AllValues[index];
        }

        public string GetKey(int index)
        {
            return AllKeys[index];
        }
    }
}
