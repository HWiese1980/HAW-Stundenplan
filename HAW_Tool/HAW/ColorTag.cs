using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace HAW_Tool.HAW
{
    public class ColorTag
    {
        public static IEnumerable<LegendEntry> EventColors
        {
            get
            {
                ResourceDictionary tCodeColorsDictionary = (ResourceDictionary)Application.Current.FindResource("EventCodeColors");
                var tCodeColors = from string p in tCodeColorsDictionary.Keys
                                  orderby p ascending
                                  select new LegendEntry { Code = p, Color = (Color)tCodeColorsDictionary[p] };

                return tCodeColors;
            }
        }
    }
}
