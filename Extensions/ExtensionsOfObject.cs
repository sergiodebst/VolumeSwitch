using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeSwitch
{
    public static class ExtensionsOfObject
    {
        public static bool IsEqual(this object v1, object v2)
        {
            if (v1 == null & v2 == null)
                return true;
            else if (v1 == null | v2 == null)
                return false;
            else if (v1.GetType().IsClass != v2.GetType().IsClass)
                return false;
            else if (v1.GetType().IsClass && !(v1 is string))
                return v1 == v2;
            else
                return v1.Equals(v2);
        }

    }
}
