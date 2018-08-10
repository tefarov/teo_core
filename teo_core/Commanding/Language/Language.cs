using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;
using ev = TEO.Commanding.Environment;
using lg = TEO.Commanding.Language.Language;

namespace TEO.Commanding.Language
{
    /// <summary>
    /// Command-factory. It creates commands. Needed to store an Environment-variable and to pass it to commands
    /// </summary>
    public partial class Language: IFactory<Input, IBatchable>
    {
        const string
            STR_ASKTEXT = "Введите текст"
            , STR_ASKNUMB = "Введите число"
            ;

        public delegate IBatchable Creator(Input command, ev.Environment environment);

        readonly Creator CRT;
        readonly ev.Environment ENV;

        public Language(ev.Environment environment, Creator creator)
        {
            ENV = environment.NotNull();
            CRT = creator.NotNull();
        }

        public IBatchable Create(Input item)
        {
            return CRT(item, ENV);
        }

        public static IBatchable Create_Say(Input item, ev.Environment env)
        {
            IGetter<string> src;
            ISetter<string> dst;
            string hdr = null;

            Arg asrc, adst, ahdr;

            if (!item.TrySet(out asrc, 1)) throw new ArgumentException("Первый аргумент, который должен быть переменной отсутствует");
            if (!item.TrySet(out adst, 2)) adst = new Arg(2, null, "$console");  //throw new ArgumentException("Второй аргумент, который должен описывать способ отображения, отуствует");
            if (item.TrySet(out ahdr, 3, "header"))
                hdr = item.GetGetter_STR(env, ref ahdr).Get();
            
            if (asrc.IsVariable)
                src = env.GetGetter<string>(asrc.Value);
            else if (asrc.Value == lg.KwAsk)
                src = new GetterAskConsole(STR_ASKTEXT);
            else
                src = new GetterValue<string>() { Value = asrc.Value };

            if (adst.IsVariable) dst = env[adst.Value].Value as ISetter<string>;
            else
                throw new ArgumentException("Второй аргумент должен быть переменной");

            if (dst == null) throw new InvalidCastException("Неизвестный способ отображения");

            return new CmdTransfer<string>(src, dst) { Text = hdr };
        }

        public static IBatchable Create_Assign(Input item, ev.Environment env)
        {
            Arg avar, atyp, aval;

            if (!item.TrySet(out avar, 0)) throw new ArgumentException("Первый аргумент, который должен быть переменной отсутствует");
            if (!item.TrySet(out atyp, 2)) throw new ArgumentException("Второй аргумент, который должен быть типом переменной (string|int|float .. etc) отсутствует");
            if (!item.TrySet(out aval, 3)) throw new ArgumentException("Третий аргумент, который должен содержать значение|переменную отсутствует");

            if (!avar.IsVariable) throw new ArgumentException("Первый аргумент должен быть переменной (например: $foo)");
            if (atyp.IsVariable) throw new ArgumentException("Второй аргумент не может быть переменной");

            if (atyp.Value == lg.TpString) {
                var gval = item.GetGetter_STR(env, ref aval, STR_ASKTEXT);
                var sval = env.GetSetter<string>(avar.Value);

                return new CmdTransfer<string>(gval, sval);
            }
            else if (atyp.Value == lg.TpInt) {
                var gval = item.GetGetter_INT(env, ref aval, STR_ASKNUMB);
                var sval = env.GetSetter<int>(avar.Value);

                return new CmdTransfer<int>(gval, sval);
            }
            else if (atyp.Value == lg.TpDecimal) {
                var gval = item.GetGetter_DEC(env, ref aval, STR_ASKNUMB);
                var sval = env.GetSetter<decimal>(avar.Value);

                return new CmdTransfer<decimal>(gval, sval);
            }

            throw new ArgumentException("Неизвестный тип данных:" + atyp.Value);
        }

        public static IBatchable Create_Exe(Input item, ev.Environment env)
        {
            Arg afil;

            if (!item.Any()) return null;
            else if (item.TrySet(out afil, 1, "file")) { }
            else if (item.TrySet(out afil, name: "command")) { }
            else if (item.TrySet(out afil, name: "cmd")) { }
            else
                return null;

            var gfil = item.GetGetter_STR(env, ref afil, "Введите путь файла");

            return new CmdExecute(gfil);
        }
    }
}
