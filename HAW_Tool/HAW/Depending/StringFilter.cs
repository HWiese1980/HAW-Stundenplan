using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HAW_Tool.HAW.Depending
{
    public class StringFilter
    {
        private readonly Regex _rgx;
        public string Name { get; private set; }

        public bool FilterMatches(string line)
        {
            return _rgx.IsMatch(line);
        }
        
        // object _threadSafety = new object();
        public IDictionary<string, string> GetValues(string line)
        {
            // lock(_threadSafety)
            {
                if (!_rgx.IsMatch(line)) return null;

                var tmatch = _rgx.Match(line);
                var ret = new Dictionary<string, string>();

                // Console.WriteLine("-------- matching string filter {0} -----------------------", this.Name);
                for (int g = 0; g < tmatch.Groups.Count; g++)
                {
                    var tgrp = tmatch.Groups[g];
                    string name = _rgx.GroupNameFromNumber(g);
                    // Console.WriteLine("| Group {0} -> Value: {1}", name, tgrp.Value);
                    ret.Add(name, tgrp.Value);
                }
                // Console.WriteLine("-------------------------------------------------------------\n\n");
                return ret;
            }
        } 

        public StringFilter(string name, string expression)
        {
            _rgx = new Regex(expression);
            Name = name;
        }
    }
}
