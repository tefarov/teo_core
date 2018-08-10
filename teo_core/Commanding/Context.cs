using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.Commanding.Environment;

namespace TEO.Commanding
{
    public class Context
    {
        public readonly Environment.Environment Environment;
        public Context(Environment.Environment environment) { this.Environment = environment.NotNull(); }
    }
}
