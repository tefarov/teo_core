using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.General.Configurating
{
    /// <summary>
    /// This interface helps automating such configurations as window-gonfiguration or some other objects constantly created and destroyed.
    /// </summary>
    public interface IConfigurable
    {
        IConfigurator GetConfigurator();
    }
}
