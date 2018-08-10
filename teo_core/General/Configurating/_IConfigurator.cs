using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.General.Configurating
{
    public interface IConfigurator
    {
        void Configure();
        void Serialize();
    }
}
