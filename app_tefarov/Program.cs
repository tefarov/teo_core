using System;  
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO;
using TEO.Commanding;
using mg = TEO.General.Messaging;

namespace APP
{
    class Program
    {
        static void Main(string[] args)
        {
            var cor = Program.Core;
            var dsp = cor.Display;
            var env = cor.Environment;
            Console.WriteLine();

            cor.BusConsole = new mg.BusConsole(dsp);
            cor.BusMsg = new mg.BusMsgBox(dsp) { Header = "TEO-Application" };

            dsp.Write("Program initialized", mg.TMessage.CommandResult);

            //var prs = new APP.Commanding.Parser(env);
            var prs = new APP.Commanding.Parser(env);
            var prc = new Processor(env) { Display = dsp };
            var ask = new GetterAskConsole("app") { Display = dsp };

            var earg = args.Where(x => x.Length > 1 && x.StartsWith("/"));
            var ecmd = args.Where(x => x.Length > 1 && x.StartsWith("-"));
            var efil = cor.GetMacros("ini");
#if DEBUG
            efil = efil.Concat(cor.GetMacros("debug"));
#endif
            efil = efil.Concat(args.Except(earg).Except(ecmd));

            earg = earg.Select(x => x.Substring(1));
            ecmd = ecmd.Select(x => x.Substring(1));
            
            cor.GetConfigurator(earg).Configure();

            var enm = ecmd.GetEnumerator();
            while (enm.MoveNext()) process_cmd(enm.Current);

            enm = efil.GetEnumerator();
            while (enm.MoveNext()) process_fil(enm.Current);

            while (Program.Continue) {

                try {
                    Console.WriteLine();
                    var cmd = prs.Parse(ask);
                    prc.Execute(cmd);
                }
                catch (OperationCanceledException) {
                    // dsp.Write("..", mg.TMessage.CommandHeader);
                }
                catch (ExceptionRuntime ex) {
                    dsp.Write(ex, ex.IsCritical ? mg.TMessage.ExceptionWorkflow : mg.TMessage.Warning);
                }
                catch (Exception ex) {
                    dsp.Write(ex, mg.TMessage.ExceptionCritical);
                }
            }
            
            void process_cmd(string arg) { throw new NotImplementedException("Processing command-arguments not yet implemented"); }
            void process_fil(string file)
            {
                dsp.Write(file, mg.TMessage.CommandHeader);

                IBatch bch = null;

                try {
                    bch = prs.Parse(cor.GFileContent_TXT(file), description: file);
                }
                catch (Exception ex) {
                    dsp.Write(ex, position: "Batch-parsing");
                    return;
                }

                prc.Execute(bch);
            }
        }

        public static bool Continue = true;

        public static Core Core = new Core("cfg.main");
    }
}
