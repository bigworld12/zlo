using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZloGUILauncher
{
    public static class Helpers
    {
        public static T Find<T>(this ObservableCollection<T> Source , Predicate<T> predicate)
        {
            if (Source == null || predicate == null)
            {
                return default(T);
            }

            int count = Source.Count;
            for (int i = 0; i < count; i++)
            {
                var elem = Source[i];                
                if (predicate(elem))
                {
                    return elem;
                }
            }
            return default(T);
        }
        public static void Remove<T>(this ObservableCollection<T> Source , Predicate<T> predicate)
        {
            if (Source == null || predicate == null)
            {
                return;
            }
            var element = Source.Find(predicate);
            if (element == null)
            {
                return;
            }
            else
            {
                Source.Remove(element);
            }
        }
    }
}
