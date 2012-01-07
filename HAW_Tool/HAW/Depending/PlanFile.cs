using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using LittleHelpers;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace HAW_Tool.HAW.Depending
{
    public class PlanFile : DependencyObject
    {
        private PlanFile()
        {
            SeminarGroups = new ObservableCollection<SeminarGroup>();
        }

        static PlanFile()
        {
            Instance = new PlanFile();
            LineFilters = new List<StringFilter>
                               {
                                   new StringFilter("version", @"\w+?\s+(?<Semester>.*?)\s*Vers\.\s*(?<Version>\d+?\.\d+?)\s+?vom\s+?(?<Datum>.+)"),
                                   new StringFilter("gruppe", @"Semestergruppe\s+(?<Gruppe>.*?)$"),
                                   new StringFilter("kw", @"^(?:\d+(?:-|,\s+|$))+$"),
                                   new StringFilter("veranstaltung", @"^(?<Code>.+?),(?<Tutor>.*?),(?<Room>.+?),(?<DayOfWeek>\w+?),(?<Start>\d{1,2}\:\d{1,2}),(?<End>\d{1,2}\:\d{1,2})$")
                               };

        }

        public SeminarGroup SelectedSeminarGroup
        {
            get { return (SeminarGroup)GetValue(SelectedSeminarGroupProperty); }
            set { SetValue(SelectedSeminarGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSeminarGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSeminarGroupProperty =
            DependencyProperty.Register("SelectedSeminarGroup", typeof(SeminarGroup), typeof(PlanFile), new UIPropertyMetadata(null, SeminarGroupChanged));

        static void SeminarGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("Seminar Group selected: {0}", ((SeminarGroup)e.NewValue).Name);
        }


        private static readonly List<StringFilter> LineFilters;

        public static PlanFile Instance { get; set; }

        public static event EventHandler<ValueEventArgs<int>> StatusProgressChanged;

        public static void OnStatusProgressChanged(int value)
        {
            var args = new ValueEventArgs<int> { Value = value };
            if (StatusProgressChanged != null) StatusProgressChanged(null, args);
        }

        public static event EventHandler<ValueEventArgs<string>> StatusMessageChanged;

        public static void OnStatusMessageChanged(string format, params object[] parameter)
        {
            var args = new ValueEventArgs<string> { Value = string.Format(format, parameter) };
            if (StatusMessageChanged != null) StatusMessageChanged(null, args);
        }

        internal void LoadSchedule(string schedule)
        {
            OnStatusMessageChanged("Lade {0}", schedule);
            if (!Uri.IsWellFormedUriString(schedule, UriKind.Absolute))
                return;

            try
            {
                string content = new WebClient().DownloadString(schedule);
                ParseContent(content);
            }
            catch (Exception exp)
            {
                OnStatusMessageChanged("Laden von {0} fehlgeschlagen: {1}", schedule.Substring(schedule.Length - 20),
                                       exp.Message);
                Debugger.Break();
            }
        }

        /*
         *                
         *      case "version":
                    {
                        string dateString = CleanDateString(match.Groups["Datum"].Value.Trim());

                        DateTime date = ParseDate(dateString);

                        _mCurrentYear = date.Year;
                        _mCurrentSemGroup = new XElement("semgrp",
                                                         new XAttribute("version", match.Groups["Version"].Value),
                                                         new XAttribute("lastupdate", match.Groups["Datum"].Value));
                        break;
                    }
                case "gruppe":
                    {
                        Group grp = match.Groups["Gruppe"];
                        _mCurrentSemGroup.Add(new XAttribute("name", grp.Value));
                        _mSemgroups.Add(_mCurrentSemGroup);
                        break;
                    }
                case "kw":
                    {
                        _mCurrentWeeks = ParseKW(match.Value);
                        _mCurrentSemGroup.Add(_mCurrentWeeks);
                        break;
                    }
                case "event":
                    {
                        foreach (XElement tWeek in _mCurrentWeeks)
                        {
                            tWeek.Add(CreateEventElement(match));
                        }
                        break;
                    }

         * 
         * 
         * 
         * 
         *             var tCode = new XElement("code",
                                     match.Groups["Code"].Value.ConvertUmlauts(UmlautConvertDirection.ToCrossWordFormat));
            var tTutor = new XElement("dozent", match.Groups["Tutor"].Value);
            var tRoom = new XElement("raum", match.Groups["Room"].Value);
            var tDay = new XElement("tag", match.Groups["DayOfWeek"].Value);
            var tFrom = new XElement("von", match.Groups["Start"].Value);
            var tTill = new XElement("bis", match.Groups["End"].Value);

            var tEvent = new XElement("event",
                                      tCode, tTutor, tRoom, tDay, tFrom, tTill);

            return tEvent;

         * */
        #region Parser Helper Methods
        public static string CleanDateString(string dateString)
        {
            var rgxClearDate = new Regex(@"\d{1,2}\.\d{1,2}\.\d{1,4}");
            dateString = rgxClearDate.Match(dateString).Value;
            return dateString;
        }

        private DateTime ParseDate(string cleanDate)
        {
            var dateFormats = new[]
                                  {
                                      "dd.MM.yyyy",
                                      "dd.MM.yy"
                                  };

            foreach (string dateFormat in dateFormats)
            {
                DateTime temp;
                const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
                if (DateTime.TryParseExact(cleanDate, dateFormat, CultureInfo.CurrentCulture, style, out temp))
                    return temp;
            }

            return DateTime.MinValue;
        }

        private IEnumerable<CalendarWeek> ParseKW(string kwLine, int year)
        {
            var tElm = new List<CalendarWeek>();

            IEnumerable<string> tWBlocks = from string p in kwLine.Split(',')
                                           select p.Trim();

            var tFromTo = new Regex(@"^(?<from>\d+)-(?<to>\d+)$");
            foreach (string tBlock in tWBlocks)
            {
                if (tFromTo.IsMatch(tBlock))
                {
                    Match tMatch = tFromTo.Match(tBlock);
                    int tStart = int.Parse(tMatch.Groups["from"].Value);
                    int tEnd = int.Parse(tMatch.Groups["to"].Value);

                    for (int i = tStart; i <= tEnd; i++)
                    {
                        var cw = new CalendarWeek { Week = i, Year = year };
                        tElm.Add(cw);
                    }
                }
                else
                {
                    var cw = new CalendarWeek { Week = int.Parse(tBlock), Year = year };
                    tElm.Add(cw);
                }
            }
            return tElm;
        }


        #endregion


        private void ParseContent(string content)
        {
            // Zeile für Zeile einlesen
            var sr = new StringReader(content);
            string line = "";

            SeminarGroup currentSeminarGroup = null;
            CalendarWeek[] currentWeeks = null;
            int currentYear = 0;

            while (null != (line = sr.ReadLine()))
            {
                var filter = GetLineFilter(line);
                if (filter == null) continue;

                var values = filter.GetValues(line);

                OnStatusMessageChanged("Filter {0} -> hat {1} Werte", filter.Name, values.Count);

                switch (filter.Name)
                {
                    case "version":
                        {
                            string cleanDate = CleanDateString(values["Datum"]);
                            var date = ParseDate(cleanDate);

                            currentYear = date.Year;
                            currentSeminarGroup = new SeminarGroup { Version = values["Version"], LastUpdated = date };

                            break;
                        }
                    case "gruppe":
                        {
                            if (currentSeminarGroup != null)
                            {
                                currentSeminarGroup.Name = values["Gruppe"];
                                SeminarGroups.Add(currentSeminarGroup);
                            }
                            break;
                        }
                    case "kw":
                        {
                            currentWeeks = ParseKW(values["0"], currentYear).ToArray();
                            foreach (var cw in currentWeeks)
                            {
                                cw.InitializeDays();
                                currentSeminarGroup.CalendarWeeks.Add(cw);
                            }
                            break;
                        }
                    case "veranstaltung":
                        {
                            if (currentWeeks != null)
                                foreach (var cw in currentWeeks)
                                {
                                    TimeSpan from = TimeSpan.MinValue, till = TimeSpan.MinValue;
                                    var s_dow = values["DayOfWeek"];
                                    var i_dow = Native.Helper.DaysOfWeek[s_dow] - 1;
                                    var day = cw.Days.Where(d => d.DOW == (DayOfWeek)(i_dow)).FirstOrDefault();

                                    if (day == null)
                                    {
                                        Debugger.Break();
                                    }

                                    try
                                    {
                                        from = TimeSpan.ParseExact(values["Start"], "g",
                                                                                                          CultureInfo.CurrentCulture);
                                        till = TimeSpan.ParseExact(values["End"], "g",
                                                                      CultureInfo.CurrentCulture);
                                    }
                                    catch
                                    {
                                        Debugger.Break();
                                    }

                                    var evt = new Event
                                                  {
                                                      Code = values["Code"],
                                                      Date = cw.GetDateOfWeekday(i_dow),
                                                      From =
                                                          from,
                                                      Till =
                                                          till,
                                                      Tutor = values["Tutor"],
                                                      DayOfWeek = i_dow,
                                                      Day = day,
                                                      CalendarWeek = cw.Week,
                                                      Room = values["Room"]
                                                  };

                                    evt.Day.Events.Add(evt);
                                    evt.Freeze();
                                }
                            break;
                        }
                }
            }

            OnStatusMessageChanged("Fertig");
        }

        private StringFilter GetLineFilter(string line)
        {
            var filter = LineFilters.Where(flt => flt.FilterMatches(line)).FirstOrDefault();
            return filter;
        }


        #region Daten

        public ObservableCollection<SeminarGroup> SeminarGroups { get; set; }

        #endregion
    }
}
