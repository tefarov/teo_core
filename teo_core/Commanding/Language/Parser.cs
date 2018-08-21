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
        int LineNumber = 0;

        instruction[] AINS;

        List<Input> LINP = new List<Input>();
        List<Argument> LARG = new List<Argument>();

        public readonly ISet<string>
            FilesUsed = new HashSet<string>()    // files used in parsing, needed to prevent once-files to be used repeatedly
            ;

        protected readonly Dictionary<string, IFactory<Input, IBatchable>> DFBAB = new Dictionary<string, IFactory<Input, IBatchable>>();

        public Parser(Environment.Environment environment)
        {
            int i = -1;
            AINS = new instruction[6];

            AINS[++i] = new ins_variable(this);
            AINS[++i] = new ins_string(this, ins_string.ModeSimple);
            AINS[++i] = new ins_string(this, ins_string.ModeComplex);
            AINS[++i] = new ins_number(this);
            AINS[++i] = new ins_keyword(this);

        }
        public IBatch Parse(IGetter<string> gtext, string description = null )
        {
            var txt = gtext.Get();
            var enm = txt.AsEnumerable().GetEnumerator();
            // currently active instruction-parser
            instruction ins = null;
            char c;

            while (enm.MoveNext()) {
                c = enm.Current;

                // if there's any instruction, just proceed to appending a character to instruction
                if (ins != null) { }
                // if there's no active instruction, and the character is a whitespace,
                // no need to parse it, jsut proceed to next chracter
                else if (char.IsWhiteSpace(c)) continue;
                // this means we have no currently active instruction
                // but we found some reasonable character, and should try to define with the help of that charcter
                // what kind of instruction to activate
                else if ((ins = get_instruction(ref c)) == null)
                    throw new ExceptionParsing("Invalid character at :" + SBD.ToString() + c, this.LineNumber);

                // THE PROBLEM !!!
                // WHENEVER WE EXIT AN INS, THE INVALID CHARACTER REMAINS UNPARSED
                // WHENEVER WE REACH THE END OF THE LINE IN A COMMENT AND PROBABLY KEWORD WE DON'T KNOW THAT
                // probably we should insist on closing the qoutes of a string not to let it finish on close
                // and implement ins_end.finish to ins_comment.finish (don't like this)
                if (!ins.TryAppend(ref c)) {
                    ins.Finish();
                    ins = null;
                }
            }

            if (ins != null) {
                ins.Finish();
                ins = null;
            }

            
            return new BatchSequential();
        }

        bool endswith(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            if (SBD.Length < text.Length) return false;

            int i = text.Length, j = SBD.Length;

            while (i >= 0)
                if (text[--i] != SBD[--j]) return false;

            return true;
        }
        /// <summary>
        /// Will remove last symobols from the stringbuilder.
        /// WARNING!!! It doesn't check the text it removes, just calcs the length to remove lengths 
        /// </summary>
        void remove_last(string text)
        {
            int l = text.Length;
            int s = SBD.Length - l;

            SBD.Remove(s, l);
        }

        /// <summary>
        /// This tries to define the needed instruction by the first character.
        /// Returns an instruction or null if not defined
        /// </summary>
        /// <param name="c">The character that describes the instruction needed</param>
        instruction get_instruction(ref char c)
        {
            for (int i = 0; i < AINS.Length; i++)
                if (AINS[i].IsStart(ref c)) return AINS[i];

            return null;
        }
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

        #region Parser.instructions
        abstract class instruction
        {
            protected Parser PRT;
            protected instruction(Parser parent) { PRT = parent.NotNull(); }
            public abstract bool TryAppend(ref char c);
            public abstract bool IsStart(ref char c);
            public abstract void Finish();
        }
        class ins_keyword : instruction
        {
            /// <summary>
            /// This tells that a specified keyword is an assignment keyword,
            /// that descibes some argument's name i.g. position:32 or value:"some text"
            /// </summary>
            bool ASG = false;
            
            public ins_keyword(Parser parent) : base(parent) { }
            public override bool TryAppend(ref char c)
            {
                if (char.IsLetter(c)) { }
                else if (char.IsDigit(c)) { }
                else if (Language.SymbolsKeyword.Contains(c)) { }
                else if (c == Language.ChAssign) {
                    ASG = true;
                    return false;
                }
                else if (char.IsWhiteSpace(c)) return false;
                else
                    throw new InvalidOperationException("Invalid character at keyword: " + PRT.SBD.ToString() + c);

                PRT.SBD.Append(c);
                return true;
            }
            public override bool IsStart(ref char c)
            {
                if (char.IsLetter(c)) return true;
                else if (Language.SymbolsKeyword.Contains(c)) return true;

                return false;
            }
            public override void Finish()
            {
                // if we are processing an assignment keyword
                if (ASG)
                    PRT.LARG.Add(new ArgumentAssignment(PRT.SBD.ToString()));
                else
                    PRT.LARG.Add(new ArgumentKeyword(PRT.SBD.ToString()));

                PRT.SBD.Clear();
            }
        }
        class ins_variable : instruction
        {
            /// <summary>
            /// Is Started processing
            /// </summary>
            bool IST = false;
            
            public ins_variable(Parser parent) : base(parent) { }
            public override bool TryAppend(ref char c)
            {
                if (!IST) {
                    // set is started to true
                    if (c == Language.ChVariable) return IST = true;
                    // this shouldn't happen, because that's when the first character of the sequence is not a dollar
                    throw new InvalidOperationException("Invalid character at variable: " + PRT.SBD.ToString() + c);
                }

                if (char.IsLetterOrDigit(c)) { }
                else if (Language.SymbolsVariables.Contains(c)) { }
                else if (char.IsWhiteSpace(c)) return false;
                else
                    throw new InvalidOperationException("Invalid character at variable: " + PRT.SBD.ToString() + c);

                PRT.SBD.Append(c);
                return true;
            }
            public override bool IsStart(ref char c) { return c == Language.ChVariable; }
            public override void Finish()
            {
                PRT.LARG.Add(new ArgumentVariable(PRT.SBD.ToString()));

                PRT.SBD.Clear();
                IST = false;
            }
        }
        class ins_string : instruction
        {
            public const byte
                ModeNone = 0
                , ModeSimple = 1
                , ModeComplex = 2
                ;

            /// <summary>
            /// A character at the beginning and the end of a string
            /// </summary>
            char CHR = '\'';

            byte MOD = 0;

            /// <summary>
            /// Is started
            /// </summary>
            bool IST = false;
            /// <summary>
            /// Is escape
            /// </summary>
            bool IES = false;

            public ins_string(Parser parent, byte mode) : base(parent)
            {
                MOD = mode;
                if (MOD == ins_string.ModeSimple)
                    CHR = '\'';
                else if (MOD == ins_string.ModeComplex)
                    CHR = '"';
                else
                    throw new ArgumentException("Unknown string-instruction mode: " + MOD);
            }

            public override bool TryAppend(ref char c)
            {
                // CHECK SPECIAL PARSING-MODES : escape, end, etc
                // the sequence hasn't yet started
                if (!IST) {
                    // set is started to true
                    if (c == CHR) return IST = true;
                    // this shouldn't happen, because that's wen the first character of the sequence is not a quote
                    throw new InvalidOperationException("Invalid character at string: " + PRT.SBD.ToString() + c);
                }
                // previous character was an escape-character
                else if (IES) {
                    IES = false; // switch escape-mode off

                    if (c == 'r')
                        PRT.SBD.Append('\r');
                    else if (c == 'n')
                        PRT.SBD.Append('\n');
                    else if (c == 't')
                        PRT.SBD.Append('\t');
                    else
                        PRT.SBD.Append(c);

                    return true;
                }
                // this character is an escape character - just switch-escape mode on
                else if (c == Language.ChEscapeC) return IES = true;
                else if (c == CHR) {
                    // the parsing sequence is ended
                    return false;
                }

                // THIS APPENDS A NEW CHARACTER TO THE STRING
                PRT.SBD.Append(c);

                // THIS CHEKCS MORE SPECIAL MODES : newline
                // if the string is a simple mode, newline means the end of the string
                if (MOD == ins_string.ModeSimple && PRT.endswith(System.Environment.NewLine)) {
                    PRT.remove_last(System.Environment.NewLine);
                    return false;
                }

                // This happens when we added char to a string in a normal mode
                return true;
            }
            public override bool IsStart(ref char c)
            {
                if (IST) throw new InvalidOperationException("ins_string is already parsing something");
                return c == CHR;
            }
            public override void Finish()
            {
                PRT.LARG.Add(new ArgumentString(PRT.SBD.ToString(), CHR));

                PRT.SBD.Clear();
                IST = false;
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
        class ins_end : instruction
        {
            expectation EXP;

            public ins_end(Parser parent) : base(parent) { }
            public override bool IsStart(ref char c)
            {
                if (c == Language.ChLinebrk) {
                    EXP = new expectation(Language.ChLinebrk);
                    return true;
                }
                else if (c == Language.StNewLine[0]) {
                    EXP = new expectation(Language.StNewLine);
                    return true;
                }

                return false;
            }
            public override bool TryAppend(ref char c)
            {
                return EXP.TryAppend(ref c);
            }
            public override void Finish()
            {
                // it's empty or usless, because we are processing a linebreak or smth
                PRT.SBD.Clear();
                // may be we are processing an empty line
                if (!PRT.LARG.Any()) return;

                var itm = new Instruction();

            }
        }
        class ins_comment : instruction {
            bool IST = false;

            public ins_comment(Parser parent) : base(parent) { }
            public override bool TryAppend(ref char c)
            {
                if (!IST) {
                    // set is started to true
                    if (c == Language.ChComment) return IST = true;
                    // this shouldn't happen, because that's wen the first character of the sequence is not a quote
                    throw new InvalidOperationException("Invalid character at comment: " + PRT.SBD.ToString() + c);
                }

                // THIS APPENDS A NEW CHARACTER TO THE STRING
                PRT.SBD.Append(c);

                // THIS CHEKCS MORE SPECIAL MODES : newline
                // newline means the end of the comment
                if (PRT.endswith(System.Environment.NewLine))
                    return false;
                

                // This happens when we added char to a string in a normal mode
                return true;
            }
            public override bool IsStart(ref char c)
            {
                if (IST) throw new InvalidOperationException("ins_comment is already parsing something");

                if (c == Language.ChComment) return true;

                return false;
            }
            public override void Finish()
            {
                PRT.SBD.Clear();
                IST = false;
            }
        }

        class expectation
        {
            char[] ACHR;
            int CUR = 0;
            public expectation(string value) { ACHR = value.ToCharArray().NotNull(); }
            public expectation(params char[] value) { ACHR = value.NotNull(); }

            /// <summary>
            /// This implements instruction.TryAppend(c): checks a character and iterates to the next char specified in constructor.
            /// Will throw an error if there's an unexpected character.
            /// Will return false, if we succesfully reached the end of the expectation.
            /// Will return true if the checked character is the expected one, but there are some characters to be checked.
            /// </summary>
            /// <param name="c">A character to be checked</param>
            /// <exception cref="ArgumentOutOfRangeException"/>
            /// <exception cref="ArgumentException"/>
            public bool TryAppend(ref char c)
            {
                // check if we are still within the expected range
                if (CUR < 0 || CUR >= ACHR.Length) throw new ArgumentOutOfRangeException("Expected is out of range");
                // check if the current character is the expted one
                if (ACHR[CUR] != c) throw new ArgumentException("Unexpected character: " + c);
                // this happens if we found the expected character
                // and now we check if there are any characters left to check
                if (++CUR >= ACHR.Length) return false; // this means no characters left
                // this means that we found an expected character and there are still characters left
                return true;
            }
        }
        #endregion
    }
}
