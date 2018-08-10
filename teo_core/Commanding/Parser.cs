using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;
using lg = TEO.Commanding.Language.Language;

namespace TEO.Commanding
{
    public class Parser
    {
        const byte
            BYT_STTNULL = 0
            , BYT_STTOPND = 1
            , BYT_STTCLSD = 2
            ;
        
        StringBuilder SBD = new StringBuilder(); // instruction-builder: 
        List<string> SINS = new List<string>();  // list of instructions
        iInstruction CUR = new ins_variant();

        public Environment.Environment Environment;

        public readonly ISet<string>
            FilesUsed = new HashSet<string>()    // files used, needed to prevent once-files to be used
            ;

        protected // a dictionary of command-factories that are used to parse the input to batchables
                Dictionary<string, IFactory<Input, IBatchable>> DFBAB = new Dictionary<string, IFactory<Input, IBatchable>>();

        public Parser(Environment.Environment environment)
        {
            // Language elements
            DFBAB["break"] = new FactorySingleton<Input, IBatchable>(new BatchBreaker());

            // Keywords
            DFBAB["use"] = new Language.Use(this, environment);
            DFBAB["run"] = new Language.Run(this, environment);

            // Commands
            DFBAB["exe"] = new lg(environment, lg.Create_Exe);
            DFBAB["set"] = new lg(environment, lg.Create_Set);
            DFBAB["display"] = new lg(environment, lg.Create_Display);
            DFBAB["http"] = new lg(environment, Web.CmdRequestHttp.Create);
        }

        public IBatch Parse(IGetter<string> source, string description = null )
        {
            SBD.Clear(); SINS.Clear(); CUR = new ins_variant();

            var src = source.Get();
            if (string.IsNullOrEmpty(src)) goto finalize;

            // This will store a filepath in order to prevent multiple calls for once-only files
            if (source is GetterTextFile)
                FilesUsed.Add(((GetterTextFile)source).Filepath);

            char c;
            var echr = src.AsEnumerable().GetEnumerator();
            while (echr.MoveNext()) {
                c = echr.Current;
                this.append(c);
            }
            this.append('\r'); // this closes the current value-parsers and any other processing

            finalize:
            var ainp = SINS.Select(x => new Input(ref x)).ToArray(SINS.Count);
            var abab = ainp.Select(x => this.parse(x)).ToArrayR(ainp.Length);

            var bch = new BatchSequential() { Text = description };
            return bch.Append(abab);
        }
        void append(char c)
        {
            bool isretry = false;
            retry:

            // this means that the current instruction will append the current character
            // and no other processing needed
            if (CUR.TryAppend(ref c)) {
                if (c == '\\') SBD.Append('\\');
                SBD.Append(c);
                return;
            }

            // the current character is not appended to instruction
            // but the instruction is still active and later might append some characters
            if (CUR.IsActive)
                return;

            if (isretry)
                throw new InvalidOperationException("Infinite loop while parsing data");

            // the instruction has fullfilled it's mission
            // and now we need some other instruction
            if (c == lg.ChComment) { CUR = new ins_comment(); return; }
            else if (c == lg.ChQuotSim || c == lg.ChQuotCpx) { CUR = new ins_string(c); SBD.Append(c); return; }
            else if (c == lg.ChLinebrk) { linebreak(); return; }
            else if (c == lg.ChLineend) { linebreak(); return; }
            else if (CUR is ins_variant) throw new InvalidOperationException("We found nonsense: current instruction is ins_variant and we need ins_variant");

            CUR = new ins_variant(); isretry = true;
            goto retry;

            void linebreak()
            {
                var ins = SBD.ToString();
                if (!string.IsNullOrEmpty(ins) && !string.IsNullOrEmpty(ins = ins.Trim()))
                    SINS.Add(ins);

                SBD.Clear();
                CUR = new ins_variant();
            }
        }

        IBatchable parse(Input command)
        {
            var key = command.Command.ToLowerInvariant();

            if (DFBAB.ContainsKey(key))
                return DFBAB[key].Create(command);

            return new Command1 { Text = command.ToString() };
        }

        #region iInstruction - parsing instructions
        interface iInstruction
        {
            /// <summary>
            /// This checks if this character should be appended to the instruction
            /// </summary>
            bool TryAppend(ref char c);
            /// <summary>
            /// If set to false tells, that the current instruction should be replaced with some other instruction,
            /// because the current instruction has reached the end of it's lifecycle
            /// </summary>
            bool IsActive { get; }

        }
        struct ins_variant : iInstruction
        {
            /// <summary>
            /// Current state.
            /// 0 - not started. 2 - started not finished. 3 - started and finished
            /// </summary>
            byte STT;
            bool isEscape;
            public bool IsActive => STT < BYT_STTCLSD;

            public bool TryAppend(ref char c)
            {
                // this tells that there's a whitespace character in front of the instruction and they are useless and should be omited
                //if (STT == BYT_STTNULL && __ASPC.Contains(c)) return false;

                if (STT == BYT_STTNULL) STT++;

                if (c == lg.ChLineend) return (STT++ < 0); // assigns 2 and returns false

                if (this.isEscape) {
                    if (c == 'r') c = '\r';
                    else if (c == 'n') c = '\n';
                    else if (c == 't') c = '\t';

                    return !(this.isEscape = false); // assigns false and returns true
                }
                
                if (c == lg.ChComment) return (STT++ < 0); // assigns 2 and returns false
                if (c == lg.ChLinebrk) return (STT++ < 0); // assigns 2 and returns false
                if (c == lg.ChEscapeC) return (this.isEscape = true); // assigns true and returns true
                if (c == lg.ChQuotSim) return (STT++ < 0); // assigns 2 and returns false
                if (c == lg.ChQuotCpx) return (STT++ < 0); // assigns 2 and returns false

                return true;
            }
        }
        struct ins_string: iInstruction
        {
            bool isComplex, isEscape;
            byte STT;
            public bool IsActive => STT < BYT_STTCLSD;

            public ins_string(char c)
            {
                this.isComplex = false;
                this.isEscape = false;

                if (c == lg.ChQuotSim) this.isComplex = false;
                else if (c == lg.ChQuotCpx) this.isComplex = true;
                else throw new InvalidOperationException("Quoted string may not start with a char: " + c);

                STT = BYT_STTOPND; // assigns state and returns true
            }

            public bool TryAppend(ref char c)
            {
                // what's after this instruction happens only if a quote is opened
                if (STT == BYT_STTCLSD)
                    return false;

                // prvious char was an escape-sequence starter, so we process that escape-sequence
                else if (this.isEscape) {
                    if (c == 'r') c = '\r';
                    else if (c == 'n') c = '\n';
                    else if (c == 't') c = '\t';

                    return !(this.isEscape = false); // assigns false and returns true
                }
                // this char tells that we should start an escape sequence
                else if (c == lg.ChEscapeC)
                    return !(this.isEscape = true);  // assigns true and returns false

                // we are processing a complex string
                else if (this.isComplex) {
                    if (c == lg.ChQuotCpx) STT++;    // we say that we need to append this character, but the next one should be omitted
                }
                // we are processing a simple string
                else if (!this.isComplex) {
                    if (c == lg.ChQuotSim) STT++;         // we say that we need to append this character, but the next one should be omitted
                    else if (c == lg.ChLineend) return (STT++ < 0);  // assigns 2 and returns false
                }

                return true;
            }
        }
        struct ins_comment : iInstruction
        {
            /// <summary>
            /// Current state.
            /// 0 - not started. 1 - started not finished. 2 - started and finished
            /// </summary>
            byte STT;
            public bool IsActive => STT < BYT_STTCLSD;

            public bool TryAppend(ref char c)
            {
                if (STT == BYT_STTNULL) STT++;
                if (c == lg.ChLineend) STT++;

                return false;
            }
        }
        #endregion

        /// <summary>
        /// Helps creating a singleton factory out of a command
        /// </summary>
        /// <param name="item">The command to be used as a singleton return item</param>
        protected IFactory<Input, IBatchable> get_singleton(ICommand item)
        {
            return new FactorySingleton<Input, IBatchable>(item);
        }
        /// <summary>
        /// Helps creating a singleton factory out of an action
        /// </summary>
        /// <param name="item">The action to be used as a singleton return command</param>
        protected IFactory<Input, IBatchable> get_singleton(Action item)
        {
            var cmd = new CmdSimple(item);
            return new FactorySingleton<Input, IBatchable>(cmd);
        }
    }
}
