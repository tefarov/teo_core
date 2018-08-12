using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

using TEO.General;
using lg = TEO.Commanding.Language.Language;

namespace TEO.Commanding
{
    public class Input
    {
        /// <summary>
        /// Use this value in TrySet functions to tell that id is not specified.
        /// This is needed if you want for some argument to be spcified only explicitly .. i.g.  arg:="somevalue"
        /// and do not want for it to have some speciific location 1,2,3 etc
        /// </summary>
        public const int IdUnspecified = -1;

        Arg[] AARG;

        public readonly string Command;

        public Input(ref string command)
        {
            var buf = __SBD; // buffer: this is used to create an argument-text. we add characters to it until we understand that the argument's is full
            var sarg = __SARG; // this just contains all the parsed arguments

            bool
                isq = false         // text in quotes are being input
                , isx = false       // escape character occured
                ;

            string cmd = null;
            int len = STR_ARGASSG.Length;

            var echr = command.AsEnumerable().GetEnumerator();
            while (echr.MoveNext()) {
                var val = echr.Current;

                // previous element was an escape-character, so we just Append this char to the string
                if (isx) {
                    buf.Append(val);
                    isx = false;
                }
                // we found an escape-symbol, start escape-sequence processing
                else if (val == lg.ChEscapeC) { isx = true; }
                // we are processing some long string and found a quote, so that's the end, we should flush a buffer
                else if (val == lg.ChQuotCpx && isq) {
                    isq = false;
                    flush_string(cmd, buf.ToString());
                }
                // we found a quote and should start a long-string processing, however, the prviously input text, might be an-assignment
                // so then we should save the assignment-data to a variable, to link it later to a long-string
                // and yet, starting a quote, tells that the prviously input text is a standalone-argument
                else if (val == lg.ChQuotCpx) {
                    isq = true;

                    // nothing has been previously input
                    if (buf.Length == 0) { }
                    // whatever was entered previously it may not contain any assignment
                    else if (buf.Length < len) {
                        flush_buffer();
                    }
                    // oh yes, there was an assignment
                    else if (buf.ToString().EndsWith(STR_ARGASSG)) {
                        cmd = buf.ToString();
                        cmd = cmd.Substring(0, cmd.Length - len);
                        buf.Clear();
                    }
                    // something has been previously input, bu that's not assignment, so it's processed as a separate argument
                    else {
                        flush_buffer();
                    }

                }
                // if we are processing a long-string, we just save all the characters as-is
                else if (isq) {
                    buf.Append(val);
                }
                // some argument-separator has been found, so we flush a buffer
                else if (lg.Spaces.Contains(val)) {
                    flush_buffer();
                }
                else if (lg.Operators.Contains(val)) {
                    flush_buffer();
                    sarg.Add(new Arg(sarg.Count, null, val.ToString()));
                }
                // any other symbol is found, wea just add it to buffer
                else {
                    buf.Append(val);
                }
            }

            flush_buffer();
            flush_set();

            if (AARG != null && AARG.Length > 0) this.Command = AARG[0].Value;

            this.Arguments = (AARG ?? Enumerable.Empty<Arg>()).Where(x => x.Id > 0);
            this.ArgumentsUnnamed = this.Arguments.Where(x => string.IsNullOrEmpty(x.Name));

            // This just looks what text is in the buffer, and creates some argument out of it
            void flush_buffer()
            {
                if (buf.Length > 0) {
                    sarg.Add(Input.getArgument(sarg.Count, buf.ToString()));
                    buf.Clear();
                }
            }
            void flush_string(string name, string value)
            {
                if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(value)) {
                    sarg.Add(new Arg(sarg.Count, name, value));
                    buf.Clear();
                }
            }
            // to conserve resources we store arguments in an array, not a set.
            // this flushes a static set to the array
            void flush_set()
            {
                AARG = sarg.ToArray(sarg.Count);
                sarg.Clear();
            }
        }
        static Arg getArgument(int id, string value)
        {
            string nam = string.Empty, val;

            int pos = 0, len = STR_ARGASSG.Length;
            if ((pos = value.IndexOf(STR_ARGASSG)) > 0) {
                nam = value.Substring(0, pos);
                val = value.Substring(pos + len);
            }
            else if (pos == 0)
                val = value.Substring(pos + len - 1);
            else
                val = value;

            return new Arg(id, nam, val);
        }

        public Arg this[int id]
        {
            get
            {
                if (id < 0 || id >= AARG.Length) throw new ArgumentOutOfRangeException("Input contains no argument: " + id);
                return AARG[id];
            }
        }
        public Arg this[string id]
        {
            get
            {
                id = id.ToLowerInvariant();
                for (int i = 0; i < AARG.Length; i++) {
                    if (AARG[i].Name.ToLowerInvariant() == id) return AARG[i];
                }
                throw new ArgumentOutOfRangeException("Input contains no argument: " + id);
            }
        }

        public bool Contains(string name)
        {
            name = name.ToLowerInvariant();
            for (int i = 0; i < AARG.Length; i++) {
                if (string.IsNullOrEmpty(AARG[i].Name)) continue;
                if (AARG[i].Name.ToLowerInvariant() == name) return true;
            }
            return false;
        }
        public bool Contains(int id)
        {
            return (id >= 0 && id < AARG.Length);
        }
        /// <summary>
        /// This tells if the input has any arguments
        /// </summary>
        public bool Any()
        {
            return AARG != null && AARG.Length > 0;
        }
        public bool Any(string value)
        {
            if (AARG == null || AARG.Length < 1) return false;

            value = value.ToLowerInvariant();
            for (int i = 0; i < AARG.Length; i++) {
                if (AARG[i].Value.ToLowerInvariant() == value) return true;
            }
            return false;
        }

        public bool TrySet(ref string value, int id = Input.IdUnspecified, string name = null)
        {
            string val;

            if (id != Input.IdUnspecified && this.Contains(id))
                val = this[id].Value;
            else if (!string.IsNullOrEmpty(name) && this.Contains(name))
                val = this[name].Value;
            else
                return false;

            if (string.IsNullOrEmpty(val)) return false;

            value = val;
            return true;
        }
        public bool TrySet(ref decimal value, int id = Input.IdUnspecified, string name = null)
        {
            string txt;

            if (id != Input.IdUnspecified && this.Contains(id))
                txt = this[id].Value;
            else if (!string.IsNullOrEmpty(name) && this.Contains(name))
                txt = this[name].Value;
            else
                return false;

            if (string.IsNullOrEmpty(txt)) return false;
            else if (decimal.TryParse(txt, out decimal val)) {
                value = val;
                return true;
            }

            return false;
        }
        public bool TrySet(ref DateTime value, int id = Input.IdUnspecified, string name = null)
        {
            string txt;

            if (id != Input.IdUnspecified && this.Contains(id))
                txt = this[id].Value;
            else if (!string.IsNullOrEmpty(name) && this.Contains(name))
                txt = this[name].Value;
            else
                return false;

            if (string.IsNullOrEmpty(txt)) return false;
            else if (DateTime.TryParse(txt, out DateTime val)) {
                value = val;
                return true;
            }

            return false;
        }
        public bool TrySet(ref bool value, int id = Input.IdUnspecified, string name = null)
        {
            string txt;

            if (id != Input.IdUnspecified && this.Contains(id))
                txt = this[id].Value;
            else if (!string.IsNullOrEmpty(name) && this.Contains(name))
                txt = this[name].Value;
            else
                return false;

            if (string.IsNullOrEmpty(txt)) return false;
            else if (bool.TryParse(txt, out bool val)) {
                value = val;
                return true;
            }
            else if (txt == Language.Language.TxTrue || txt == Language.Language.TxTrue2) {
                value = true;
                return true;
            }
            else if (txt == Language.Language.TxFalse || txt == Language.Language.TxFalse2) {
                value = false;
                return true;
            }
            else if (txt == name) {
                value = true;
                return true;
            }

            return false;
        }

        public bool TrySet(out Arg value, int id = Input.IdUnspecified, string name = null)
        {
            if (id != Input.IdUnspecified && this.Contains(id))
                value = this[id];
            else if (!string.IsNullOrEmpty(name) && this.Contains(name))
                value = this[name];
            else {
                value = new Arg();
                return false;
            }

            return true;
        }

        public bool TrySet(Arg item, ref string value)
        {
            if (string.IsNullOrWhiteSpace(item.Value)) return false;

            value = item.Value;
            return true;
        }
        public bool TrySet(Arg item, ref DateTime value)
        {
            DateTime val;

            if (string.IsNullOrWhiteSpace(item.Value)) return false;
            if (!DateTime.TryParse(item.Value, out val)) return false;

            value = val;
            return true;
        }
        public bool TrySet(Arg item, ref decimal value)
        {
            decimal val;

            if (string.IsNullOrWhiteSpace(item.Value)) return false;
            if (!decimal.TryParse(item.Value, out val)) return false;

            value = val;
            return true;
        }

        public IGetter<string> GetGetter_STR(Environment.Environment env, ref Arg val, string askquestion = null)
        {
            if (val.IsVariable)
                return env.VariableGet<string>(val.Value, false);
            else if (val.Value == lg.KwAsk)
                return new GetterAskConsole(askquestion);

            return new GetterValue<string>() { Value = val.Value };
        }
        public IGetter<int> GetGetter_INT(Environment.Environment env, ref Arg val, string askquestion = null)
        {
            if (val.IsVariable)
                return env.VariableGet<int>(val.Value, false);
            else if (val.Value == lg.KwAsk)
                return new GetterAskConsole<int>(askquestion, new FactoryFunction<string, int>(int.Parse));

            if (int.TryParse(val.Value, out var value))
                return new GetterValue<int>() { Value = value };

            throw new InvalidCastException("Аргумент '" + val.ToString() + "' не может быть превращён в число");
        }
        public IGetter<decimal> GetGetter_DEC(Environment.Environment env, ref Arg val, string askquestion = null)
        {
            if (val.IsVariable)
                return env.VariableGet<decimal>(val.Value, false);
            else if (val.Value == lg.KwAsk)
                return new GetterAskConsole<decimal>(askquestion, new FactoryFunction<string, decimal>(decimal.Parse));

            if (decimal.TryParse(val.Value, out var value))
                return new GetterValue<decimal>() { Value = value };

            throw new InvalidCastException("Аргумент '" + val.ToString() + "' не может быть превращён в число");
        }

        /// <summary>
        /// All the arguments, except the one with index 0. The 0-argument is a command argument
        /// </summary>
        public readonly IEnumerable<Arg> Arguments;
        /// <summary>
        /// Arguments, who's names are omitted
        /// </summary>
        public readonly IEnumerable<Arg> ArgumentsUnnamed;

        class reader : IEnumerable<Input>, IGetter<string[]>
        {
            const char CHR_COMMENT = '#';

            public readonly string Filepath;
            string[] ALIN;

            public reader(string filepath)
            {
                this.Filepath = filepath.NotNull("Filepath undefined");
                this.Reset();
            }

            public IEnumerator<Input> GetEnumerator() { return new enumerator(this); }
            IEnumerator IEnumerable.GetEnumerator() { return new enumerator(this); }
            public string[] Get() { return ALIN; }

            public void Reset()
            {
                var fil = FactoryFiles_old.ToFilepath(this.Filepath);

                List<string> slin = new List<string>();

                using (var reader = new System.IO.StreamReader(fil)) {
                    string line;
                    while ((line = reader.ReadLine()) != null) {
                        // get rid of comments
                        if (line.Contains(CHR_COMMENT))
                            line = line.Substring(0, line.IndexOf(CHR_COMMENT));

                        line = line.Trim();
                        if (!string.IsNullOrEmpty(line))
                            slin.Add(line);
                    }
                }
                this.ALIN = slin.ToArray(slin.Count);
            }
        }
        class enumerator : IEnumerator<Input>
        {
            IGetter<string[]> SRC;
            Input ITM;
            string[] ACMD;
            long CUR = 0;

            public enumerator(IGetter<string[]> source)
            {
                SRC = source.NotNull();
                this.Reset();
            }

            public Input Current => ITM;
            object IEnumerator.Current => ITM;

            public void Dispose() { SRC = null; }
            public void Reset()
            {
                CUR = -1;
                if (SRC != null) ACMD = SRC.Get();
                ITM = null;
            }
            public bool MoveNext()
            {
                if (ACMD == null) return false;
                if (CUR >= ACMD.Length) return false;
                if (++CUR >= ACMD.Length) return false;

                ITM = new Input(ref ACMD[CUR]);

                return true;
            }
        }

        public override string ToString()
        {
            return AARG.Select(x => x.ToString()).Concat(" ");
        }


        const string
            STR_ARGASSG = "::"
            ;

        static StringBuilder __SBD = new StringBuilder();
        static HashSet<Arg> __SARG = new HashSet<Arg>();
        
        public static IEnumerable<Input> Read(string filepath)
        {
            return new reader(filepath);
        }
    }
    public struct Arg
    {
        public readonly string Name, Value;
        public readonly int Id;
        public readonly bool IsVariable;

        public Arg(int id, string name, string value)
        {
            this.Id = id;
            this.Name = name;
            this.Value = value;
            this.IsVariable = false;

            if (string.IsNullOrEmpty(value)) { }
            else if (value[0] == lg.ChVariable) {
                this.IsVariable = true;
                this.Value = value.Substring(1);
            }
        }
        public bool IsEmpty
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Name)) return false;
                if (!string.IsNullOrEmpty(this.Value)) return false;

                return true;
            }
        }

        public override string ToString()
        {
            if (this.IsEmpty) return "-";

            string fmt = "{1}"; // unnamed format
            if (this.IsVariable)
                fmt = lg.ChVariable + fmt;
            else if (this.Value.IndexOfAny(lg.Spaces) > 0) 
                fmt = lg.ChQuotCpx + fmt + lg.ChQuotCpx;

            if (!string.IsNullOrEmpty(this.Name)) fmt = "{0}::" + fmt;

            return string.Format(fmt, this.Name, this.Value);
        }

        public IGetter<T> GetGetter<T>(Environment.Environment environment, bool isnullable = false)
        {
            if (this.IsVariable)
                return environment.VariableGet<T>(this.Value, isnullable);

            throw new ArgumentException("На позиции " + this.Id.ToString() + " " + this.Name + " должна быть переменная");
        }
        public IGetter<T> GetGetter<T>(Environment.Environment environment, Func<string, T> converter, bool isnullable = false)
        {
            if (this.IsVariable)
                return environment.VariableGet<T>(this.Value, isnullable);

            var val = converter(this.Value);
            return new GetterValue<T>() { Value = val };
        }
        public IGetter<string> GetGetter(Environment.Environment environment, bool isnullable = false)
        {
            if (this.IsVariable)
                return environment.VariableGet<string>(this.Value, isnullable);

            return new GetterValue<string>() { Value = this.Value };
        }

        public static Func<string, int> ConverterInt = x => int.Parse(x);
        public static Func<string, decimal> ConverterDec = x => decimal.Parse(x);
    }
}
