using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding.Language
{
    public class Parser
    {
        readonly StringBuilder SBD = new StringBuilder();

        instruction[] AINS;

        List<Input> LINP = new List<Input>();
        List<Argument> LARG = new List<Argument>();

        public Parser()
        {
            int i = -1;
            AINS = new instruction[3];

            AINS[++i] = new ins_variable(this);
            AINS[++i] = new ins_string_simple(this);
            AINS[++i] = new ins_string_complex(this);
            AINS[++i] = new ins_number(this);
            AINS[++i] = new ins_keyword(this);

        }
        public IBatch Parse(IGetter<string> gtext, string description)
        {
            int line = 1;

            var txt = gtext.Get();
            var enm = txt.AsEnumerable().GetEnumerator();
            instruction ins = null;

            while (enm.MoveNext()) {
                var c = enm.Current;

                if (ins != null) { }
                else if (char.IsWhiteSpace(c)) continue;
                else if ((ins = getinstruction(ref c)) == null)
                    throw new ExceptionParsing("Invalid character at :" + SBD.ToString() + c, line);

                if (ins.TryAppend(ref c)) {

                }

            }
        }

        bool endswith(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            if (SBD.Length < text.Length) return false;

            int i = text.Length, j = SBD.Length;

            while (i >= 0)
                if (text[i] != SBD[j]) return false;

            return true;
        }
        /// <summary>
        /// Will remove last symobols from the stringbuilder.
        /// WARNING!!! It doesn't check the text it removes, uses lengths 
        /// </summary>
        /// <param name="text"></param>
        void remove_last(string text) {
            int l = text.Length;
            int s = SBD.Length - l;

            SBD.Remove(s, l);
        }

        instruction getinstruction(ref char c)
        {
            for (int i = 0; i < AINS.Length; i++)
                if (AINS[i].IsStart(ref c)) return AINS[i];

            return null;
        }

        abstract class instruction
        {
            protected Parser PRT;
            protected instruction(Parser parent) { PRT = parent.NotNull(); }
            public abstract bool TryAppend(ref char c);
            public abstract bool IsStart(ref char c);
            public abstract void Finish();
        }
        class ins_keyword : instruction {
            public ins_keyword(Parser parent) : base(parent) { }
            public override bool TryAppend(ref char c)
            {
                throw new NotImplementedException();
            }
            public override bool IsStart(ref char c)
            {
                if (char.IsLetter(c)) return true;
                if (Language.SymbolsKeyword.Contains(c)) return true;

                return false;
            }
            public override void Finish()
            {
                throw new NotImplementedException();
            }
        }
        class ins_variable : instruction
        {
            public ins_variable(Parser parent) : base(parent) { }
            public override bool TryAppend(ref char c)
            {
                throw new NotImplementedException();
            }
            public override bool IsStart(ref char c) { return c == Language.ChVariable; }
        }
        class ins_string_simple : instruction
        {
            /// <summary>
            /// Is started
            /// </summary>
            bool IST = false;
            /// <summary>
            /// Is escape
            /// </summary>
            bool ISE = false;

            public ins_string_simple(Parser parent) : base(parent) { }
            public override bool TryAppend(ref char c)
            {
                // the sequence hasn't yet started
                if (!IST) {

                }


                else if (c == Language.ChQuotSim) {
                    // the parsing sequence has started : set isstarted to true
                    if (!IST) return IST = true;
                    // the parsing sequence is ended
                    return false;
                }
                


                

                if (PRT.endswith(System.Environment.NewLine)) {
                    PRT.remove_last(System.Environment.NewLine);
                    return false;
                }

                return true;
            }
            public override bool IsStart(ref char c) { return c == Language.ChQuotSim; }
            public override void Finish()
            {
                IST = false;
            }
        }
        
        class ins_string_complex : instruction {
            public ins_string_complex(Parser parent) : base(parent) { }
            public override bool TryAppend(ref char c)
            {
                throw new NotImplementedException();
            }
            public override bool IsStart(ref char c) { return c == Language.ChQuotCpx; }
            public override void Finish()
            {
                throw new NotImplementedException();
            }
        }
        class ins_number : instruction
        {
            public ins_number(Parser parent) : base(parent) { }
            public override bool TryAppend(ref char c)
            {
                throw new NotImplementedException();
            }
            public override bool IsStart(ref char c)
            {
                if (char.IsDigit(c)) return true;
                if (c == ',' || c == '.') return true;

                return false;
            }
            public override void Finish()
            {
                throw new NotImplementedException();
            }
        }
    }
}
