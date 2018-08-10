using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;

namespace TEO.Commanding.Environment
{
    public class Environment
    {
        SortedDictionary<string, Variable> DVAR = new SortedDictionary<string, Variable>();

        public Variable this[string id]
        {
            get
            {
                if (DVAR.ContainsKey(id)) return DVAR[id];

                var val = new Variable(id);
                DVAR[id] = val;
                return val;
            }
            set
            {
                if (string.IsNullOrEmpty(id)) return;
                else if (value == null) {
                    if (DVAR.ContainsKey(id)) DVAR.Remove(id);
                }
                else if (value.Name != id)
                    DVAR[id] = new Variable(id) { Value = value.Value };
                else
                    DVAR[id] = value;
            }
        }

        public void Set(string id, object value)
        {
            if (string.IsNullOrEmpty(id)) return;
            else if (value == null) {
                if (DVAR.ContainsKey(id)) DVAR.Remove(id);
            }
            else if (DVAR.ContainsKey(id))
                DVAR[id].Value = value;
            else
                DVAR[id] = new Variable(id) { Value = value };
        }

        public IGetter<T> GetGetter<T>(string name)
        {
            return new controller_value<T>(this, name);
        }
        public ISetter<T> GetSetter<T>(string name)
        {
            return new controller_value<T>(this, name);
        }

        /// <summary>
        /// Items of this class control some specific variable. Thay may get it or set it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class controller_value<T> : IGetter<T>, ISetter < T>
        {
            readonly string NAM;
            readonly Environment PRT;

            public controller_value(Environment parent, string name)
            {
                NAM = name.NotNull();
                PRT = parent.NotNull();
            }
            public T Get()
            {
                var val = PRT[NAM].Value;

                if (val == null) return default(T);
                if (!(val is T)) {
                    if (typeof(T) == typeof(string))
                        val = val.ToString();
                    else
                        return default(T);
                }

                return (T)val;
            }

            public void Set(T item)
            {
                if (!PRT.DVAR.ContainsKey(NAM))
                    PRT.DVAR.Add(NAM, new Variable(NAM) { Value = item });
                else
                    PRT.DVAR[NAM].Value = item;
            }
        }
    }
}
