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
    public partial class Language : IFactory<Input, IBatchable>
    {
        
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
                src = env.VariableGet<string>(asrc.Value, false);
            else if (asrc.Value == lg.KwAsk)
                src = new GetterAskConsole(lg.Tx_AskText);
            else
                src = new GetterValue<string>() { Value = asrc.Value };

            if (adst.IsVariable) dst = env[adst.Value].Value as ISetter<string>;
            else
                throw new ArgumentException("Второй аргумент должен быть переменной");

            if (dst == null) throw new InvalidCastException("Неизвестный способ отображения");

            return new CmdTransfer<string>(src, dst) { Text = hdr };
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
