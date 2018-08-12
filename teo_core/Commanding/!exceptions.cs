using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding
{
    public class ExceptionRuntime : Exception
    {
        public bool IsCritical = false;

        public ExceptionRuntime(string message, bool iscritical = false) : base(message)
        {
            this.IsCritical = iscritical;
        }
    }
    public class ExceptionParsing : Exception
    {
        public ExceptionParsing(Exception ex, Input line, int linenumber) : base("Parsing exception on line " + linenumber.ToString() + ": " + line.ToString()) { }
    }
}
