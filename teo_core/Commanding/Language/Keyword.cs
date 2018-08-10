using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;
using ev = TEO.Commanding.Environment;

namespace TEO.Commanding.Language
{
    class Keyword : IFactory<Input, IBatchable>
    {
        protected readonly ev.Environment ENV;

        public Keyword(ev.Environment parent)
        {
            ENV = parent.NotNull("Environment is essential for keywords");
        }

        public virtual IBatchable Create(Input item) { throw new NotImplementedException("Keyword logic must be overridden"); }
    }
}
