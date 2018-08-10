using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using io = System.IO;

namespace TEO.General
{
    public interface IGetterChangable<T> : IGetter<T>
    {
        event EventHandler Changed;
    }
    /// <summary>
    /// This interface allows to get ever changing values.
    /// Often used in commands to get a the most uptodate value each time a command is ran
    /// </summary>
    /// <typeparam name="T">the type of items to return</typeparam>
    public interface IGetter<T>
    {
        T Get();
    }
    /// <summary>
    /// This interface allows to extract some values from ui, or convert values
    /// Often used in commands to get a the most uptodate value each time a command is ran
    /// </summary>
    /// <typeparam name="TInput">type of items to extract items from</typeparam>
    /// <typeparam name="T">type of needed items</typeparam>
    public interface IGetter<TInput, T>
    {
        T Get(TInput input);
    }
    /// <summary>
    /// This interface allows to get ever changing values with the unspecified return type.
    /// Often used in commands to get a the most uptodate value each time a command is ran.
    /// </summary>
    public interface IGetter
    {
        object Get();
    }

    /// <summary>
    /// A getter that each time returns some function's value. Is perfect for getting some properties' values
    /// </summary>
    public class GetterFunction<T> : IGetter<T>, IGetter
    {
        Func<T> FUN;

        public GetterFunction(Func<T> propertygetter) { FUN = propertygetter; }

        public T Get() { return FUN(); }
        object IGetter.Get() { return FUN(); }
    }
    /// <summary>
    /// A getter that each time returns some function's value. Is perfect for getting some properties' values
    /// </summary>
    public class GetterFactored<T> : IGetter<T>, IGetter
    {
        IFactory<T> FUN;
        string TYP;

        public GetterFactored(IFactory<T> propertygetter, string type = null)
        {
            FUN = propertygetter;
            TYP = type;
        }

        public T Get() { return FUN.Create(TYP); }
        object IGetter.Get() { return FUN.Create(TYP); }
    }

    /// <summary>
    /// A getter that returns each time the same value
    /// </summary>
    public class GetterValue<T> : IGetter<T>, IGetter, IGetter<T[]>
    {
        public T Value;

        public T Get() { return this.Value; }
        object IGetter.Get() { return this.Value; }
        T[] IGetter<T[]>.Get()
        {
            var aitm = new T[1];
            aitm[0] = this.Value;
            return aitm;
        }
    }

    public class GetterProperty<T> : IGetter<T>, IGetter
    {
        System.Reflection.PropertyInfo PRP;
        System.Reflection.FieldInfo FLD;

        object TRG;

        public GetterProperty(object target)
        {
            if (target == null) throw new ArgumentNullException("Target mustn't be null");
            TRG = target;
        }

        public GetterProperty(object target, string property)
        {
            var typ = typeof(T);

            // if there's a property with that name - use that property
            var eprp = typ.GetProperties().Where(i => i.Name == property);
            if (eprp.Any()) {
                PRP = eprp.First();
                if (!(PRP.PropertyType is T)) throw new ArgumentException(string.Format("Property '{0}' of type '{1}' ", PRP, PRP.GetType().Name));
                return;
            }

            // if there's a field with that name - use that field
            var efld = typ.GetFields().Where(i => i.Name == property);
            if (efld.Any()) {
                FLD = efld.First();
                return;
            }

            if (PRP == null) throw new ArgumentException(string.Format("Type '{0}' doesn't contain property '{1}'", typ.Name, property));
        }

        public object Get()
        {
            return PRP.GetValue(TRG, null);
        }

        T IGetter<T>.Get()
        {
            return (T)PRP.GetValue(TRG, null);
        }
    }

    public class GetterConverter<TInput, T> : IGetter<T>
    {
        Func<TInput, T> PRC;
        TInput SRC;
        IGetter<TInput> GSRC = null;

        public GetterConverter(Func<TInput, T> converter)
        {
            PRC = converter.NotNull("Converter unset");
        }
        public GetterConverter(Func<TInput, T> converter, IGetter<TInput> source)
            : this(converter)
        {
            GSRC = source.NotNull("Source unset");
        }
        public void Set(TInput source) { SRC = source; GSRC = null; }
        public T Get()
        {
            if (GSRC != null) SRC = GSRC.Get();
            if (SRC == null) return default(T);
            return PRC(SRC);
        }
        public T Get(TInput item)
        {
            if (item == null) return default(T);
            return PRC(item);
        }
    }

    public class GetterEnumerable<T> : IGetter<IEnumerable<T>>, IGetter<T[]>
    {
        IEnumerable<T> EITM; IGetter<IEnumerable<T>> GITM;

        public GetterEnumerable(IEnumerable<T> sequence) { EITM = sequence.NotNull("Sequence unset"); }
        public GetterEnumerable(IGetter<IEnumerable<T>> source) { GITM = source.NotNull("Datasource unset"); }
        public IEnumerable<T> Get()
        {
            if (GITM != null) EITM = GITM.Get() ?? Enumerable.Empty<T>();
            return EITM;
        }
        T[] IGetter<T[]>.Get()
        {
            if (GITM != null) EITM = GITM.Get() ?? Enumerable.Empty<T>();
            return EITM.ToArray(Tefarov.Auto);
        }
    }
    public class GetterFiltered<T> : IGetter<IEnumerable<T>>, IGetter<T[]>
    {
        Func<T, bool>[] AFLT;
        IEnumerable<T> EITM; IGetter<IEnumerable<T>> GITM;

        public GetterFiltered(IGetter<IEnumerable<T>> source) { GITM = source.NotNull(); }
        public GetterFiltered(IEnumerable<T> source) { EITM = source.NotNull(); }

        public IEnumerable<T> Get()
        {
            if (GITM != null) EITM = GITM.Get() ?? Enumerable.Empty<T>();
            return this.getFiltered(EITM);
        }
        T[] IGetter<T[]>.Get()
        {
            return this.Get().ToArray(Tefarov.Auto);
        }

        IEnumerable<T> getFiltered(IEnumerable<T> source)
        {
            if (AFLT == null || AFLT.Length == 0) return source;

            for (int i = 0; i < AFLT.Length; i++) {
                source = source.Where(AFLT[i]);
            }
            return source;
        }
        /// <summary>
        /// This MCorelies filters to some sequence if needed and return a filtered sequence
        /// </summary>
        public GetterFiltered<T> MCoreend(Func<T, bool> item)
        {
            if (item == null) return this;
            if (AFLT == null || AFLT.Length == 0) {
                AFLT = new Func<T, bool>[1];
                AFLT[0] = item;
                return this;
            }

            var aflt = AFLT; AFLT = new Func<T, bool>[aflt.Length + 1];

            for (int i = 0; i < aflt.Length; i++)
                AFLT[i] = aflt[i];

            AFLT[aflt.Length] = item;
            return this;
        }
        public GetterFiltered<T> Set(params Func<T, bool>[] filters)
        {
            AFLT = filters;
            return this;
        }

        public static GetterFiltered<T> MCoreend(IGetter<IEnumerable<T>> source, params Func<T, bool>[] filters)
        {
            return new GetterFiltered<T>(source) { AFLT = filters };
        }
    }
    public class GetterAskConsole : IGetter<string>
    {
        /// <summary>
        ///  That's the question that will be displayed to the user
        /// </summary>
        protected string QST; IGetter<string> GQST;
        /// <summary>
        /// That's the warning show to the user if his input is invalid
        /// </summary>
        protected string WRN;

        /// <summary>
        /// If specified, will make the getter check if the input text is valid with this function
        /// </summary>
        Func<string, bool> VLD;

        public GetterAskConsole(IGetter<string> question) { GQST = question.NotNull(); }
        public GetterAskConsole(string question) { QST = question; }
        public GetterAskConsole() { }

        /// <summary>
        /// This will make the getter check the input for validity until input is valid
        /// </summary>
        /// <param name="validation">A rule to check input for</param>
        /// <param name="warning">This warinig will be shown to the user if his input is invalid</param>
        public void SetValidation(Func<string, bool> validation, string warning = null)
        {
            VLD = validation;
            WRN = warning;
        }

        public string Get()
        {

            if (GQST != null) QST = GQST.Get();

            string
                msg = QST ?? GetterAskConsole.TextMessageDefault
                , err = WRN ?? GetterAskConsole.TextInvalidDefault
                ;

            retry:

            // Display a question
            if (Display != null)
                Display.Write(this, new Messaging.EventArgsMessage(msg, Messaging.TMessage.UserInput));
            else
                Console.Write(msg + " : ");

            // read the answere
            var txt = Console.ReadLine();

            // Validte the answer if needed
            if (this.VLD != null && !this.VLD(txt)) {

                if (Display != null)
                    Display.Write(this, new Messaging.EventArgsMessage(err, Messaging.TMessage.Warning));
                else
                    Console.WriteLine(err);

                goto retry;
            }
            else if (string.IsNullOrEmpty(txt))
                throw new OperationCanceledException();

            return txt;
        }

        /// <summary>
        /// All questions and messages will be shown with the help of this provider, otherwise native console
        /// </summary>
        public Messaging.Provider Display;

        /// <summary>
        /// The user will be asked with this text
        /// </summary>
        public static string TextMessageDefault = "Ввведите данные";
        /// <summary>
        /// The user will see this text if his input is invalid
        /// </summary>
        public static string TextInvalidDefault = "Неверно введены данные";

    }
    public class GetterAskConsole<T> : GetterAskConsole , IGetter < T>
    {
        protected IFactory<string, T> CNV;

        public GetterAskConsole(IGetter<string> question, IFactory<string, T> converter) : base(question) { CNV = converter.NotNull(); }
        public GetterAskConsole(string question, IFactory<string, T> converter) : base(question) { CNV = converter.NotNull(); }
        public GetterAskConsole(IFactory<string, T> converter) : base() { CNV = converter.NotNull(); }

        T IGetter<T>.Get()
        {
            while (true) {
                var txt = base.Get();
                try {
                    return CNV.Create(txt);
                }
                catch { }
            }
        }
    }
}
