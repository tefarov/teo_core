using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding.Language
{
    public abstract class Argument
    {
        public abstract string Description { get; }
        public override string ToString()
        {
            return "Argument:" + this.Description;
        }
    }
    public class ArgumentKeyword : Argument
    {
        string NAM;

        public override string Description => NAM;

        public ArgumentKeyword(string name) { NAM = name; }
    }
    public class ArgumentAssignment : Argument
    {
        string NAM;

        public override string Description => NAM + Language.ChAssign;

        public ArgumentAssignment(string name) { NAM = name; }
    }
    public class ArgumentVariable : Argument
    {
        string NAM;

        public override string Description => Language.ChVariable + NAM;

        public ArgumentVariable(string name) { NAM = name; }
    }
    public class ArgumentString : Argument
    {
        char DEL;
        string VAL;

        public override string Description => DEL + VAL + DEL;

        public ArgumentString(string value, char delimiter)
        {
            VAL = value;
            DEL = delimiter;
        }
        
    }
}
