using System.Collections.Generic;

namespace HAW_Tool.HAW
{
    class WeekHelper
    {
        public static readonly Dictionary<string, int> DOW = new Dictionary<string, int>();
        public static readonly Dictionary<int, string> WD = new Dictionary<int, string>();
        
        static WeekHelper()
        {
            DOW["mo"] = 0;
            DOW["di"] = 1;
            DOW["mi"] = 2;
            DOW["do"] = 3;
            DOW["fr"] = 4;
            DOW["sa"] = 5;
            DOW["so"] = 6;

            WD[0] = "Montag";
            WD[1] = "Dienstag";
            WD[2] = "Mittwoch";
            WD[3] = "Donnerstag";
            WD[4] = "Freitag";
            WD[5] = "Samstag";
            WD[6] = "Sonntag";
        }
    }
}
