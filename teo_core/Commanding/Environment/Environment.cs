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

        public FactoryFiles FFiles;

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
        /// <summary>
        /// This returns a getter, that will return some variable's value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="isnullable">Will tell if the getter will return nulls if no data, or throw exceptions</param>
        /// <param name="extractor">This converter will try to convert the values to thw needed type</param>
        public IGetter<T> VariableGet<T>(string name, bool isnullable, Func<object, T> extractor = null)
        {
            return new controller_value<T>(this, name, isnullable) { Extractor = extractor };
        }
        /// <summary>
        /// This setter will make the Variable 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="isnullable"></param>
        /// <returns></returns>
        public ISetter<T> VariableSet<T>(string name, bool isnullable = false)
        {
            return new controller_value<T>(this, name, isnullable);
        }

        /// <summary>
        /// Items of this class control some specific variable. Thay may get it or set it
        /// </summary>
        class controller_value<T> : IGetter<T>, ISetter < T>
        {
            readonly string NAM;
            readonly Environment PRT;

            public Func<object, T> Extractor;
            public readonly bool IsNullable;

            public controller_value(Environment parent, string name, bool isnullable = false)
            {
                NAM = name.NotNull();
                PRT = parent.NotNull();
                this.IsNullable = isnullable;
            }
            public T Get()
            {
                var val = PRT[NAM].Value;

                if (val == null) return this.IsNullable ? default(T) : throw new ArgumentNullException("Переменной '" + NAM + "' не присвоено значение");
                if (val is IValuable) val = ((IValuable)val).Value;
                
                if (val is T)
                    return (T)val;
                else if (this.Extractor != null)
                    return this.Extractor(val);
                else if (typeof(T) == typeof(string)) {
                    val = val.ToString();
                    return (T)val;
                }

                if (this.IsNullable) return default(T);

                throw new InvalidCastException("Переменную '" + NAM + "' невозможно привести к типу " + typeof(T).Name);
            }

            public void Set(T item)
            {
                if (item == null && !this.IsNullable) throw new ArgumentNullException("Переменной  '" + NAM + "' невозможно установить пустое значение");

                if (!PRT.DVAR.ContainsKey(NAM))
                    PRT.DVAR.Add(NAM, new Variable(NAM) { Value = item });
                else
                    PRT.DVAR[NAM].Value = item;
            }
        }

        /// <summary>
        /// This function helps extracting some string from an object value
        /// </summary>
        /// <param name="value">A value to extract data from</param>
        public static string ExtractStr(object value)
        {
            if (value == null) { }
            else if (value is string)
                return (string)value;
            else if (value is IGetter<string>)
                return ((IGetter<string>)value).Get();

            return value.ToString();
        }
        /// <summary>
        /// This function helps extracting some int from an object value
        /// </summary>
        /// <param name="value">A value to extract data from</param>
        public static int ExtractInt(object value)
        {
            if (value == null) { }
            else if (value is int
                || value is byte
                || value is decimal
                ) {
                return (int)value;
            }
            else if (value is IGetter<int>) {
                return ((IGetter<int>)value).Get();
            }
            else if (value is string && int.TryParse((string)value, out var val)) {
                return val;
            }
            else if (value is IGetter<string>) {
                if (int.TryParse(((IGetter<string>)value).Get(), out var vl1)) {
                    return vl1;
                }
            }

            return default(int);
        }
    }
}
