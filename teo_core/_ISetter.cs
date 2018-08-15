using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO
{
    public interface ISetter<T>
    {
        void Set(T item);
    }
}
