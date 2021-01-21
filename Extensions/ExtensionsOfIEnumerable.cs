using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeSwitch
{
    public static class ExtensionsOfIEnumerable
    {
        public static bool ContainsSame(this IEnumerable i1, IEnumerable i2)
        {
            if (i1 == null || i2 == null)
            {
                return false;
            }
            else
            {
                var list1 = (from object item in i1 select item).ToList();
                var list2 = (from object item in i2 select item).ToList();
                if (list1.Count != list2.Count)
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < list1.Count - 1; i++)
                    {
                        if (!list1[i].IsEqual(list2[i])) return false;
                    }
                    return true;
                }
            }
        }
    }
}
