using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Input;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Net;

namespace LittleHelpers
{
    public class ProgressEventArgs : EventArgs
    {
        public int Progress { get; set; }
    }

    public enum UmlautConvertDirection
    {
        FromCrossWordFormat,
        ToCrossWordFormat
    }

    public static class Helper
    {
        public static bool HasInternetConnection
        {
            get
            {
                try
                {
                    int desc;
                    bool state = InternetGetConnectedState(out desc, 0);
                    
                    WebClient tCnt = new WebClient();
                    tCnt.DownloadString("http://haw.seveq.de");

                    return state;
                }
                catch { return false; }
            }
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int Description, int Reserved);

        public static string ConvertEncoding(this string Value, Encoding From, Encoding To)
        {
            byte[] tBuff = From.GetBytes(Value);
            return To.GetString(tBuff);
        }

        public static string ConvertUmlauts(this string Value, UmlautConvertDirection dir)
        {
            switch (dir)
            {
                case UmlautConvertDirection.FromCrossWordFormat:
                    {
                        return Value
                            .Replace("AE", "Ä")
                            .Replace("ae", "ä")
                            .Replace("OE", "Ö")
                            .Replace("oe", "ö")
                            .Replace("UE", "Ü")
                            .Replace("ue", "ü")
                            .Replace("sz", "ß");
                    }
                case UmlautConvertDirection.ToCrossWordFormat:
                    {
                        return Value
                            .Replace("Ä", "AE")
                            .Replace("ä", "ae")
                            .Replace("Ö", "OE")
                            .Replace("ö", "oe")
                            .Replace("Ü", "UE")
                            .Replace("ü", "ue")
                            .Replace("ß", "sz");
                    }
            }

            return Value;
        }



        public static IEnumerable<T> GetEmpty<T>()
        {
            return new T[0];
        }

        public static double DistanceTo(this Point a, Point b)
        {
            double x_dist = Math.Abs(a.X - b.X), y_dist = Math.Abs(a.Y - b.Y);
            double x_quad = Math.Pow(x_dist, 2.0D), y_quad = Math.Pow(y_dist, 2.0D);
            double dist = Math.Sqrt(x_quad + y_quad);

            return dist;
        }

        public static Point Substract(this Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static bool IsNewerThan(this Version tMe, Version tYou)
        {
            return (tYou.Major > tMe.Major || tYou.Minor > tMe.Minor || tYou.Build > tMe.Build);
        }

        public static Size Scale(this Size Sz, double value)
        {
            return new Size(Sz.Width * value, Sz.Height * value);
        }

        private static bool? _isInDesignMode;

        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running in Blend
        /// or Visual Studio).
        /// </summary>
        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
#if SILVERLIGHT
            _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)DependencyPropertyDescriptor
                        .FromProperty(prop, typeof(FrameworkElement))
                        .Metadata.DefaultValue;
#endif
                }

                return _isInDesignMode.Value;
            }
        }

        public static bool HasCustomAttribute(this MemberInfo prop, Type Attrib)
        {
            var attribs = from Attribute p in prop.GetCustomAttributes(true)
                          where p.GetType() == Attrib
                          select p;

            return (attribs.Count() > 0);
        }

        public static string FilledString(char c, int count)
        {
            StringBuilder tBld = new StringBuilder();
            for (int i = 0; i < count; i++) tBld.Append(c);
            return tBld.ToString();
        }


        public static Color ConvertColor(string ColorCode)
        {
            System.Drawing.Color tCol = System.Drawing.ColorTranslator.FromHtml(ColorCode);
            return System.Windows.Media.Color.FromArgb(tCol.A, tCol.R, tCol.G, tCol.B);
        }

        public static string ConvertColor(Color Color)
        {
            System.Drawing.Color tCol = System.Drawing.Color.FromArgb(Color.A, Color.R, Color.G, Color.B);
            return System.Drawing.ColorTranslator.ToHtml(tCol);
        }


        public static object MakeInstance(this Type From, params object[] ConstructorArguments)
        {
            Type[] tTypes = (from p in ConstructorArguments select p.GetType()).ToArray<Type>();
            ConstructorInfo tInfo = From.GetConstructor(tTypes);
            if (tInfo == null) return null;

            object tRet = tInfo.Invoke(ConstructorArguments);
            return tRet;
        }

        public static bool ItemIsBinaryActivated(this char c, char[] Bitmask, char[] CharArray)
        {
            int index = Array.IndexOf(CharArray, c);
            if (index < 0) throw new ArgumentOutOfRangeException("Value for c is not part of CharArray");
            return (Bitmask[index] == '1');
        }

        public static string Combine(string Delimiter, params object[] Objects)
        {
            if (Objects == null || Objects.Length <= 0) return String.Empty;
            List<string> tValues = new List<string>();

            tValues.AddRange(from p in Objects where p != null && !(p is Array) select p.ToString());

            IEnumerable<object> tObjectArrays = from p in Objects where p is Array && p != null select p;
            foreach (Array tArr in tObjectArrays)
            {
                foreach (object tObj in tArr) tValues.Add(Combine(Delimiter, tObj));
            }

            return String.Join(Delimiter, tValues.ToArray());
        }

        public static T As<T>(this string Value)
        {
            return (T)Enum.Parse(typeof(T), Value);
        }

        public static RegistryKey OpenOrCreate(this RegistryKey Me, string Key)
        {
            RegistryKey newKey = Me.OpenSubKey(Key, true);
            if (newKey == null) newKey = Me.CreateSubKey(Key, RegistryKeyPermissionCheck.ReadWriteSubTree);
            return newKey;
        }

        public static void InstallCommand(ref RoutedCommand Command, string Name, Type OwnerType, ExecutedRoutedEventHandler OnCommand, CanExecuteRoutedEventHandler OnCanExecute)
        {
            Command = new RoutedCommand(Name, OwnerType);
            CommandManager.RegisterClassCommandBinding(OwnerType, new CommandBinding(Command, OnCommand, OnCanExecute));
        }

        public static string EncapsulateIn(this string Value, char Character)
        {
            return String.Format("{0}{1}{0}", Character, Value);
        }

        public static IEnumerable<T> GetChildren<T>(this DependencyObject Vis) where T : DependencyObject
        {
            if (Vis == null) return new T[0];
            List<T> tObjects = new List<T>();

            int tChildren = VisualTreeHelper.GetChildrenCount(Vis);
            for (int i = 0; i < tChildren; i++)
            {
                DependencyObject tObj = VisualTreeHelper.GetChild(Vis, i);
                tObjects.AddRange(tObj.GetChildren<T>());

                if (tObj is T)
                {
                    T tObjAsT = (T)tObj;
                    tObjects.Add(tObjAsT);
                }
            }

            return tObjects;
        }

        public static T GetLogicalParent<T>(this DependencyObject Control) where T : DependencyObject
        {
            DependencyObject tParent = LogicalTreeHelper.GetParent(Control);
            if (tParent is T) return tParent as T;
            if (tParent == null) return null;
            return tParent.GetLogicalParent<T>();
        }

        public static T GetParent<T>(this DependencyObject Control) where T : DependencyObject
        {
            DependencyObject tParent = VisualTreeHelper.GetParent(Control);
            if (tParent is T) return tParent as T;
            if (tParent == null) return null;
            return tParent.GetParent<T>();
        }

        public static Window GetWindow(this DependencyObject Control)
        {
            DependencyObject tParent = VisualTreeHelper.GetParent(Control);
            if (tParent is Window) return tParent as Window;
            if (tParent == null) throw new InvalidOperationException("There is no Window in the visual ancestors path");
            return tParent.GetWindow();
        }
    }
}
