using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVDBM.Plugin
{
    public delegate void MatchCallback(Ability self, Match match);
    public delegate void WarningCallback(Ability self);

    public class Ability
    {
        public TimeSpan warningTime = TimeSpan.FromSeconds(5);

        public bool announceWarning = true;
        public string warningMessage = "";
        public string matchMessage = "";


        public bool matchRegex = true;
        public Regex match = null;
        public Dictionary<int,Dictionary<string, string>> lastMatch = new Dictionary<int,Dictionary<string,string>>();
        public DateTime lastMatched = DateTime.MinValue;

        public MatchCallback matchCallback = null;
        public WarningCallback warningCallback = null;

        public static Ability Blank()
        {
            return new Ability();
        }
    }
}
