using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;

namespace TEO.General
{
    /// <summary>
    /// These items are used to map items from one value to another.
    /// </summary>
    public class Mapper : IFactory < string >
    {
        public readonly string Filepath;

        Dictionary<string, string> DVAL = new Dictionary<string, string>();

        public string this[string id]
        {
            get
            {
                if (string.IsNullOrEmpty(id)) return null;

                id = id.ToLowerInvariant();
                if (!DVAL.ContainsKey(id)) return null;
                return DVAL[id];
            }
        }

        string IFactory<string>.Create(string id)
        {
            return this[id];
        }

        public bool TrySet(string id, ref string value)
        {
            if (string.IsNullOrEmpty(id)) return false;

            id = id.ToLowerInvariant();
            if (!DVAL.ContainsKey(id)) return false;
            value = DVAL[id];

            return true;
        }
    }
}
