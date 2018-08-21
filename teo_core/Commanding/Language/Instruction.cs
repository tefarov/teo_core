using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding.Language
{
    public class Instruction
    {
        readonly Argument[] AARG;
        public Instruction(List<Argument> args)
        {
            AARG = args.NotNull().ToArray(args.Count);
        }
        public Instruction(params Argument[] args)
        {
            AARG = args.NotNull();
        }


    }
}
