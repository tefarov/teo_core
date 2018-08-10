using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

using TEO.General;

namespace TEO.General.Configurating
{
    /// <summary>
    /// This type of configurators use Registry to configure items of type <typeparamref name="TItem"/>
    /// </summary>
    /// <typeparam name="TItem">Type of items to be configured</typeparam>
    public class ConfiguratorRegistry<TItem> : IConfigurator where TItem : class
    {
        TItem ITM; 
        string PTH;
        /// <summary>
        /// This tells if the configurates falls with Error when there's no needed folder in the registry
        /// </summary>
        public bool IsEmptyRegsitryCritical = false;

        /// <summary>
        /// Initializes a new instance of Configurator
        /// </summary>
        /// <param name="path">relative to the Registry\CurrentUser\Software\</param>
        /// <param name="item">item to be configured</param>
        public ConfiguratorRegistry(string path, TItem item)
        {
            ITM = item.NotNull();
            PTH = path.NotNull("Path invalid");
        }

        public void Configure()
        {
            var cu = Registry.CurrentUser.OpenSubKey("Software");

            var key = cu.OpenSubKey(PTH);
            if (key == null) {
                if (this.IsEmptyRegsitryCritical)
                    throw new InvalidOperationException("We may not load config from nonexisting registry");
                else
                    return;
            }
            
            foreach (var prp in this.getProperties())
            {
                if (!prp.CanWrite) continue;
                var val = key.GetValue(prp.Name);
                if (val == null) continue;

                var typ = prp.PropertyType;
                if (typ == typeof(string) || typ == typeof(int))
                    prp.SetValue(ITM, val, null);
                else if (typ == typeof(double))
                {
                    double dbl;
                    if (!double.TryParse((string)val, out dbl)) continue;
                    prp.SetValue(ITM, dbl, null);
                }
                else if (typ == typeof(bool))
                {
                    bool itm = (string)val == "True";
                    prp.SetValue(ITM, itm, null);
                }
                else if (typ == typeof(byte[]))
                {
                    prp.SetValue(ITM, val, null);
                }
            }
        }
        public void Serialize()
        {
            var cu = Registry.CurrentUser.OpenSubKey("Software", true);
            var key = cu.OpenSubKey(PTH, true);

            if (key != null) cu.DeleteSubKey(PTH);
            key = cu.CreateSubKey(PTH);

            foreach (var prp in this.getProperties())
            {
                if (!prp.CanRead) continue;

                var val = prp.GetValue(ITM, null);
                if (val != null)
                    key.SetValue(prp.Name, val);
            }
        }

        /// <summary>
        /// The properties, that are specified in the implementation-class ( all the properties but the one specified in this abstract class )
        /// </summary>
        protected IEnumerable<System.Reflection.PropertyInfo> getProperties()
        {
            // we need to record all the properties but the ones, specified in this abstract class
            // so we select the property names of this abstract class
            var typ = typeof(TItem);
            return typ.GetProperties();
        }
    }
}
