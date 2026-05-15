using System.Collections.Generic;
using System.Linq;

namespace RubbishPot.Core
{
    public class Potion
    {
        private List<IPotionModule> _modules;

        public Potion(List<IPotionModule> modules)
        {
            _modules = modules;
        }
        
        public T GetModule<T>() where T : IPotionModule
        {
            return _modules.OfType<T>().FirstOrDefault();
        }
    }
}
