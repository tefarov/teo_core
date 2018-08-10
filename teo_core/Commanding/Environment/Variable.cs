using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding.Environment
{
    public class Variable
    {
        public readonly string Name;
        public object Value;

        public Variable(string name) { this.Name = name.NotNull(); }
    }
}
