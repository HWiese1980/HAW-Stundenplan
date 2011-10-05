#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

#endregion

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
        private static bool? _isInDesignMode;

        public static bool HasInternetConnection
        {
            get
            {
                try
                {
                    int desc;
                    bool state = InternetGetConnectedState(out desc, 0);

                    var tCnt = new WebClient();
                    tCnt.DownloadString("http://haw.seveq.de");

                    return state;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///   Gets a value indicating whether the control is in design mode (running in Blend
        ///   or Visual Studio).
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
                    DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool) DependencyPropertyDescriptor
                                     .FromProperty(prop, typeof (FrameworkElement))
                                     .Metadata.DefaultValue;
#endif
                }

                return _isInDesignMode.Value;
            }
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int description, int reserved);

        public static string ConvertEncoding(this string value, Encoding @from, Encoding to)
        {
            byte[] tBuff = @from.GetBytes(value);
            return to.GetString(tBuff);
        }

        public static string ConvertUmlauts(this string value, UmlautConvertDirection dir)
        {
            switch (dir)
            {
                case UmlautConvertDirection.FromCrossWordFormat:
                    {
                        return value
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
                        return value
                            .Replace("Ä", "AE")
                            .Replace("ä", "ae")
                            .Replace("Ö", "OE")
                            .Replace("ö", "oe")
                            .Replace("Ü", "UE")
                            .Replace("ü", "ue")
                            .Replace("ß", "sz");
                    }
            }

            return value;
        }


        public static IEnumerable<T> GetEmpty<T>()
        {
            return new T[0];
        }

        public static double DistanceTo(this Point a, Point b)
        {
            double xDist = Math.Abs(a.X - b.X), yDist = Math.Abs(a.Y - b.Y);
            double xQuad = Math.Pow(xDist, 2.0D), yQuad = Math.Pow(yDist, 2.0D);
            double dist = Math.Sqrt(xQuad + yQuad);

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

        public static Size Scale(this Size sz, double value)
        {
            return new Size(sz.Width*value, sz.Height*value);
        }

        public static bool HasCustomAttribute(this MemberInfo prop, Type attrib)
        {
            var attribs = from Attribute p in prop.GetCustomAttributes(true)
                                             where p.GetType() == attrib
                                             select p;

            return (attribs.Count() > 0);
        }

        public static string FilledString(char c, int count)
        {
            var tBld = new StringBuilder();
            for (int i = 0; i < count; i++) tBld.Append(c);
            return tBld.ToString();
        }


        public static Color ConvertColor(string colorCode)
        {
            System.Drawing.Color tCol = ColorTranslator.FromHtml(colorCode);
            return Color.FromArgb(tCol.A, tCol.R, tCol.G, tCol.B);
        }

        public static string ConvertColor(Color color)
        {
            System.Drawing.Color tCol = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            return ColorTranslator.ToHtml(tCol);
        }


        public static object MakeInstance(this Type @from, params object[] constructorArguments)
        {
            Type[] tTypes = (from p in constructorArguments select p.GetType()).ToArray<Type>();
            ConstructorInfo tInfo = @from.GetConstructor(tTypes);
            if (tInfo == null) return null;

            object tRet = tInfo.Invoke(constructorArguments);
            return tRet;
        }

        public static bool ItemIsBinaryActivated(this char c, char[] bitmask, char[] charArray)
        {
            var index = Array.IndexOf(charArray, c);
            if (index < 0) throw new ArgumentOutOfRangeException("c");
            return (bitmask[index] == '1');
        }

        public static string Combine(string delimiter, params object[] objects)
        {
            if (objects == null || objects.Length <= 0) return String.Empty;
            var tValues = new List<string>();

            tValues.AddRange(from p in objects where p != null && !(p is Array) select p.ToString());

            IEnumerable<object> tObjectArrays = from p in objects where p is Array && p != null select p;
            foreach (Array tArr in tObjectArrays)
            {
                foreach (var tObj in tArr) tValues.Add(Combine(delimiter, tObj));
            }

            return String.Join(delimiter, tValues.ToArray());
        }

        public static T As<T>(this string value)
        {
            return (T) Enum.Parse(typeof (T), value);
        }

        public static RegistryKey OpenOrCreate(this RegistryKey me, string key)
        {
            RegistryKey newKey = me.OpenSubKey(key, true);
            if (newKey == null) newKey = me.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree);
            return newKey;
        }

        public static void InstallCommand(ref RoutedCommand command, string name, Type ownerType,
                                          ExecutedRoutedEventHandler onCommand,
                                          CanExecuteRoutedEventHandler onCanExecute)
        {
            command = new RoutedCommand(name, ownerType);
            CommandManager.RegisterClassCommandBinding(ownerType, new CommandBinding(command, onCommand, onCanExecute));
        }

        public static string EncapsulateIn(this string value, char character)
        {
            return String.Format("{0}{1}{0}", character, value);
        }

        public static IEnumerable<T> GetChildren<T>(this DependencyObject vis) where T : DependencyObject
        {
            if (vis == null) return new T[0];
            var tObjects = new List<T>();

            int tChildren = VisualTreeHelper.GetChildrenCount(vis);
            for (int i = 0; i < tChildren; i++)
            {
                DependencyObject tObj = VisualTreeHelper.GetChild(vis, i);
                tObjects.AddRange(tObj.GetChildren<T>());

                if (tObj is T)
                {
                    var tObjAsT = (T) tObj;
                    tObjects.Add(tObjAsT);
                }
            }

            return tObjects;
        }

        public static T GetLogicalParent<T>(this DependencyObject control) where T : DependencyObject
        {
            DependencyObject tParent = LogicalTreeHelper.GetParent(control);
            if (tParent is T) return tParent as T;
            if (tParent == null) return null;
            return tParent.GetLogicalParent<T>();
        }

        public static T GetParent<T>(this DependencyObject control) where T : DependencyObject
        {
            DependencyObject tParent = VisualTreeHelper.GetParent(control);
            if (tParent is T) return tParent as T;
            if (tParent == null) return null;
            return tParent.GetParent<T>();
        }

        public static Window GetWindow(this DependencyObject control)
        {
            DependencyObject tParent = VisualTreeHelper.GetParent(control);
            if (tParent is Window) return tParent as Window;
            if (tParent == null) throw new InvalidOperationException("There is no Window in the visual ancestors path");
            return tParent.GetWindow();
        }
    }
}