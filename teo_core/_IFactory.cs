using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEO
{
    public interface IFactory<T>
    {
        T Create(string type = null);
    }
    /// <summary>
    /// Converter-like factory that creates <typeparamref name="T2"/> out of <typeparamref name="T1"/>
    /// </summary>
    /// <typeparam name="T1">Source-items' type</typeparam>
    /// <typeparam name="T2">Type of items created</typeparam>
    public interface IFactory<T1, T2>
    {
        T2 Create(T1 item);
    }

    public class FactorySimple<T> : IFactory<T>
    {
        Func<string, T> CRT;

        public FactorySimple(Func<string, T> procedure)
        {
            if (procedure == null) throw new ArgumentNullException("Procedure must be set");
            CRT = procedure;
        }

        public T Create(string type = null) { return CRT(type); }
    }
    /// <summary>
    /// This factory will return the same item every time
    /// </summary>
    /// <typeparam name="T1">Type of items to be used as input</typeparam>
    /// <typeparam name="T2">Type of items to be returned</typeparam>
    public class FactorySingleton<T1, T2> : IFactory<T1, T2> 
    {
        readonly T2 ITM;
        public FactorySingleton(T2 item) { ITM = item; }
        public T2 Create(T1 item)
        {
            return ITM;
        }
    }
    public class FactoryFunction<T1, T2> : IFactory<T1, T2>
    {
        public delegate T2 Converter(T1 item);
        Converter CNV;
        public FactoryFunction(Converter converter) { CNV = converter.NotNull(); }
        public T2 Create(T1 item)
        {
            return CNV(item);
        }
    }
}
