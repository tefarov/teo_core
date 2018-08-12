using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;
using ev = TEO.Commanding.Environment;
using lg = TEO.Commanding.Language.Language;

namespace TEO.Commanding
{
    public class CmdAssignSet : ICommand
    {
        readonly static Dictionary<string, factory> __DFVAR = new Dictionary<string, factory>();

        readonly string NAM;
        readonly IGetter<object> GVAL;
        readonly Action<object> PRC;

        public TCommand Type => TCommand.Sequential;

        static CmdAssignSet()
        {
            __DFVAR[lg.TpString] = new factory();
            __DFVAR[lg.TpInt] = new factory_int();
            __DFVAR[lg.TpDecimal] = new factory_dec();
            __DFVAR[lg.TpFile] = new factory_file();
        }

        public CmdAssignSet(string name, IGetter<object> source, Action<object> processor)
        {
            NAM = name.NotNull();
            GVAL = source.NotNull();
            PRC = processor.NotNull();
        }
        public ExecuteResult Execute(Context context)
        {
            var env = context.Environment;

            PRC(GVAL.Get());

            return new ExecuteResult(true);
        }

        public static IBatchable Create_Assign(Input item, ev.Environment env)
        {
            Arg avar, atyp, aval;

            if (!item.TrySet(out avar, 0)) throw new ArgumentException("Первый аргумент, который должен быть переменной отсутствует");
            if (!item.TrySet(out aval, 1)) throw new ArgumentException("Второй аргумент, который должен содержать значение|переменную отсутствует");
            if (!item.TrySet(out atyp, 2)) throw new ArgumentException("Третий аргумент, который должен быть типом переменной (string|int|float .. etc) отсутствует");

            if (!avar.IsVariable) throw new ArgumentException("Первый аргумент должен быть переменной (например: $foo)");
            if (atyp.IsVariable) throw new ArgumentException("Второй аргумент не может быть переменной");

            if (!__DFVAR.ContainsKey(atyp.Value)) throw new ArgumentException("Неизвестный тип данных:" + atyp.Value);

            // This will get the getter that will extract some value from the specified value-argument at runtime
            // It might be a Variable-getter that will extract a value from some variable
            // It might be a value-getter or ask-getter
            var gval = __DFVAR[atyp.Value].Create(env, item, aval);

            // this action actually describes how to assign the value
            Action<object> prc = x => env.Set(avar.Value, gval.Get());

            return new CmdAssignSet(avar.Value, gval, prc);
        }

        public static IBatchable Create_Set(Input item, ev.Environment env)
        {
            Arg avar, aval;

            if (!item.TrySet(out avar, 0)) throw new ArgumentException("Первый аргумент, который должен быть переменной отсутствует");
            if (!item.TrySet(out aval, 2)) throw new ArgumentException("Второй аргумент, который должен содержать значение|переменную отсутствует");

            if (!avar.IsVariable) throw new ArgumentException("Первый аргумент должен быть переменной (например: $foo)");

            // this action actually describes how to set the value
            // the way of setting depends on the type of the variable's value
            Action<object> prc = x => {
                var vbl = env[avar.Value];
                var val = vbl.Value;

                if (val == null)
                    vbl.Value = x;
                else if (val is ev.FileText)
                    ((ev.FileText)val).Set(x.ToString());
                else
                    vbl.Value = x;
            };

            var gtxt = item.GetGetter_STR(env, ref aval);
            var gval = new GetterConverter<string, object>(x => x, gtxt);

            return new CmdAssignSet(avar.Value, gval, prc);
        }
        

        class factory
        {
            public virtual IGetter<object> Create(ev.Environment environment, Input input, Arg value)
            {
                var gval = input.GetGetter_STR(environment, ref value, lg.Tx_AskText);
                return new GetterConverter<string, object>(x => x, gval);
            }
        }
        class factory_int : factory
        {
            public override IGetter<object> Create(ev.Environment environment, Input input, Arg value)
            {
                var gval = input.GetGetter_INT(environment, ref value, lg.Tx_AskNumber);
                return new GetterConverter<int, object>(x => x, gval);
            }
        }
        class factory_dec : factory
        {
            public override IGetter<object> Create(ev.Environment environment, Input input, Arg value)
            {
                var gval = input.GetGetter_DEC(environment, ref value, lg.Tx_AskNumber);
                return new GetterConverter<decimal, object>(x => x, gval);
            }
        }
        class factory_file : factory
        {
            public override IGetter<object> Create(ev.Environment environment, Input input, Arg value)
            {
                Func<string, ev.FileText> converter = path => {
                    string pth = path;
                    if (environment.FFiles != null) pth = environment.FFiles.Create(pth);

                    return new ev.FileText(pth, false);
                };

                // this getter will the value of the argument by the time of execution
                // actually it should contain some filepath
                var garg = input.GetGetter_STR(environment, ref value, lg.Tx_AskPath);
                // this will convert the argument's value to the ev.FileText object
                return new GetterConverter<string, object>(converter, garg);
            }
        }
    }
}
