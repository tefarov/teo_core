using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding.Language
{
    class Input
    {
        readonly Argument[] AARG;
        public Input(params Argument[] args)
        {
            AARG = args.NotNull("Input may not be empty");
        }
    }
}
