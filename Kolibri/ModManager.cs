using System;
using System.Collections;
using System.Collections.Generic;

namespace Kolibri
{
    public class ModManager : IEnumerable<Mod>
    {
        private List<Mod> _mods = new List<Mod>();

        public void AddMod(Mod mod)
        {
            if (mod == null)
            {
                throw new ArgumentNullException(nameof(mod));
            }
            _mods.Add(mod);
        }

        public IEnumerator<Mod> GetEnumerator()
        {
            return _mods.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
