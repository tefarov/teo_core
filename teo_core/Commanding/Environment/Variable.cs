using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding.Environment
{
    public class Variable
    {
        public readonly string Name;
        public object Value;

        public Variable(string name)
        {
            this.Name = name.NotNull();
        }

        /// <summary>
        /// Will try to convert a value of this variable to a specified type
        /// </summary>
        /// <typeparam name="T">Type of the Variable.Value to convert to</typeparam>
        /// <param name="isnullable">If set to true will return null/default if values is not of a specified type</param>
        /// <returns></returns>
        public T Convert<T>(bool isnullable = false)
        {
            if (this.Value is T) return (T)this.Value;
            if (isnullable) return default(T);

            throw new InvalidCastException("Невозможно преобразовать переменную '" + this.Name + "' в тип " + typeof(T).Name);
        }
    }
}
