using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using FFXIVAPP.Memory.Core;

namespace EncounterNS
{

    public class LearningHelper : AbilityController, IEncounter
    {
        private class learningAbilities
        {
            public string abilityName = "";
            public string mobName = "";
            public string logAbilityName = "";
            public string regexLogLine = "";
            public bool hasTarget = false;
            public int count = 0;
            public DateTime lastUsed = DateTime.MinValue;
        }

        private class lineDetails
        {
            public learningAbilities ability = null;
            public string mobName = "";
            public string logLine = "";
            public double bossHPPct = 0;
            public int session = -1;
            public DateTime sessionStart = DateTime.Now;
            public DateTime logTime = DateTime.Now;
        }

        private class phaseInfo
        {
            public int phaseStartLine = -1;
            
        }

        private string lastSQLError = "";

        string mdbConnectionStringBase = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=";
        string mdbConnectionStringAlternateBase = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=";
        OleDbConnection database;





        // these are always in english, since they're sent by the plugin directly
        private Regex targetMarkerRegex = new Regex("^(?<name>.*) places a target marker over (?<target>.+) of type (?<type>.+)\\.$");
        private Regex readiesRegex = new Regex("^(?<name>.+) readies (?<ability>.+)\\.$");


        // these could be in other languages
        private Regex suffersRegex = new Regex("^(?<target>.+) suffers the effect of (?<ability>.+)\\.$");
        private Regex usesRegex = new Regex("^(?<name>.+) (?<uses>uses|casts) (?<ability>.+)\\.$");
        private Regex startsUsingRegex = new Regex("^(?<name>.+) starts using (?<ability>.*) \\((?<abilityID>.+)\\) on (?<target>.+)\\.$");
        private Regex castingRegex = new Regex("^(?<name>.+) begins casting (?<ability>.+)\\.$");
        private Regex nameStripperRegex = new Regex("[^a-zA-Z0-9]");
        private Regex filenameStripperRegex = new Regex("[^a-zA-Z0-9 -]|^The ");

        private Dictionary<string, Dictionary<string, learningAbilities>> abilityList = new Dictionary<string, Dictionary<string, learningAbilities>>();
        private Dictionary<string, List<lineDetails>> abilityTimedOrder = new Dictionary<string, List<lineDetails>>();
        private List<lineDetails> abilityTimedOrderAll = new List<lineDetails>();

        private Dictionary<int, Dictionary<string, List<lineDetails>>> abilitySessionMobLines = new Dictionary<int, Dictionary<string, List<lineDetails>>>();
        private Dictionary<int, List<lineDetails>> abilitySessionMobLinesAll = new Dictionary<int, List<lineDetails>>();

        private Dictionary<int, Dictionary<int, int>> sessionPatterns = new Dictionary<int, Dictionary<int, int>>();

        private Dictionary<int, Dictionary<int, phaseInfo>> learningPhases = new Dictionary<int, Dictionary<int, phaseInfo>>();

        private Dictionary<string, ActorEntity> allEnemies = new Dictionary<string, ActorEntity>();

        string baseBossName = "";
        string safeBossName = "";
        string NSBossName = "";

        int session = 0;

        int bossHealth = 0;

        DateTime learningStarted = DateTime.Now;

        public void onStartEncounter()
        {
            phases[1] = new Phase();

            learningStarted = DateTime.Now;
        }


        public void onEndEncounter()
        {
            // any encounter shorter than 1 minute probably doesn't need learning info
            if ((DateTime.Now - learningStarted).Duration() < TimeSpan.FromMinutes(1))
            {
                return;
            }


            // Encounter ended. Use all info collected to create a sample encounter to be filled in by a real person.
            baseBossName = bossName;
            safeBossName = filenameStripperRegex.Replace(bossName, "");
            NSBossName = nameStripperRegex.Replace(bossName, "");

            


            if (!Directory.Exists(EncounterController.encounterZonePath + "\\Auto Helper\\"))
            {
                Directory.CreateDirectory(EncounterController.encounterZonePath + "\\Auto Helper\\");
            }

            connectToDatabase();



            writeClassFile();


            saveSessionData();






        }


        public void onTick()
        {

        }

        public void saveSessionData()
        {

            OleDbCommand command = new OleDbCommand("select MAX(sessionId) as sessionId from abilityUses", database);

            DataRowCollection sessionCol = query(command);

            if (sessionCol != null && sessionCol.Count > 0)
            {
                try
                {
                    session = (int)sessionCol[0]["sessionId"];
                    session++;
                }
                catch
                {
                    session = 0;
                }
            }
            else
            {
                session = 0;
            }

            lastSQLError = "";


            foreach (KeyValuePair<string, Dictionary<string, learningAbilities>> abilList in abilityList)
            {
                foreach (KeyValuePair<string, learningAbilities> abil in abilityList[abilList.Key])
                {
                    string logLineRegex = abil.Value.regexLogLine;


                    command = new OleDbCommand("INSERT INTO abilities (abilityName, mobName, logAbilityName, logLineRegex, hasTarget) VALUES (?, ?, ?, ?, ?)", database);
                    //command = new OleDbCommand("INSERT INTO abilities (abilityName, mobName, logAbilityName, logLineRegex, hasTarget) VALUES (@abilityName, @mobName, @logAbilityName, @logLineRegex, @hasTarget)", database);


                    command.Parameters.AddWithValue("abilityName", abil.Value.abilityName);
                    command.Parameters.AddWithValue("mobName", abilList.Key);
                    command.Parameters.AddWithValue("logAbilityName", abil.Key);
                    command.Parameters.AddWithValue("logLineRegex", abil.Value.regexLogLine);
                    command.Parameters.AddWithValue("hasTarget", abil.Value.hasTarget);

                    execute(command);


                }
            }

            if (lastSQLError != "")
            {
                //debug("SQL error: " + lastSQLError, DBMErrorLevel.FineTimings);
            }


            foreach (lineDetails abil in abilityTimedOrderAll)
            {
                //command = new OleDbCommand("INSERT INTO abilityUses (abilityName, sessionId, sessionStart, mobName, logLine, logTime, bossHPPct) VALUES (@abilityName, @sessionId, @sessionStart, @mobName, @logLine, @logTime, @bossHPPct)", database);

                command = new OleDbCommand("INSERT INTO abilityUses (abilityName, sessionId, sessionStart, mobName, logLine, logTime, bossHPPct) VALUES (?, ?, ?, ?, ?, ?, ?)", database);

                //command = new OleDbCommand("INSERT INTO abilityUses (abilityName) VALUES (?)", database);
                //command = new OleDbCommand("INSERT INTO abilityUses (sessionId) VALUES (?)", database);
                //command = new OleDbCommand("INSERT INTO abilityUses (sessionStart) VALUES (?)", database);
                //command = new OleDbCommand("INSERT INTO abilityUses (mobName) VALUES (?)", database);
                //command = new OleDbCommand("INSERT INTO abilityUses (logLine) VALUES (?)", database);
                //command = new OleDbCommand("INSERT INTO abilityUses (logTime) VALUES (?)", database);
                //command = new OleDbCommand("INSERT INTO abilityUses (bossHPPct) VALUES (?)", database);

                command.Parameters.AddWithValue("abilityName", abil.ability.abilityName);
                command.Parameters.AddWithValue("sessionId", session);
                command.Parameters.AddWithValue("sessionStart", (double)EncounterController.started.Ticks);
                command.Parameters.AddWithValue("mobName", abil.mobName);
                command.Parameters.AddWithValue("logLine", abil.logLine);
                command.Parameters.AddWithValue("logTime", (double)abil.logTime.Ticks);
                command.Parameters.AddWithValue("bossHPPct", abil.bossHPPct);

                execute(command);

            }



            if (lastSQLError != "")
            {
                debug("SQL error: " + lastSQLError, DBMErrorLevel.FineTimings);
            }


        }


        public void readInAllData()
        {
            abilityList.Clear();
            abilityTimedOrder.Clear();
            abilityTimedOrderAll.Clear();

            OleDbCommand command = new OleDbCommand("select * from abilities order by mobName, abilityName", database);

            DataRowCollection abilitiesResult = query(command);

            foreach (DataRow row in abilitiesResult)
            {
                learningAbilities abil = new learningAbilities();

                abil.abilityName = (string)row["abilityName"];
                abil.mobName = (string)row["mobName"];
                abil.logAbilityName = (string)row["logAbilityName"];
                abil.regexLogLine = (string)row["logLineRegex"];
                abil.hasTarget = (bool)row["hasTarget"];

                if (!abilityList.ContainsKey(abil.mobName))
                {
                    abilityList[abil.mobName] = new Dictionary<string,learningAbilities>();
                }

                abilityList[abil.mobName][abil.abilityName] = abil;
            }




            command = new OleDbCommand("select * from abilityUses order by sessionId, logTime, id", database);

            DataRowCollection logLinesResult = query(command);

            foreach (DataRow row in logLinesResult)
            {
                lineDetails line = new lineDetails();

                line.mobName = (string)row["mobName"];
                line.ability = abilityList[line.mobName][(string)row["abilityName"]];
                line.session = (int)row["sessionId"];
                line.sessionStart = new DateTime(((long)((double)row["sessionStart"])));
                line.logLine = (string)row["logLine"];
                line.logTime = new DateTime(((long)((double)row["logTime"])));
                line.bossHPPct = (double)row["bossHPPct"];

                if (!abilitySessionMobLines.ContainsKey(line.session))
                {
                    abilitySessionMobLines[line.session] = new Dictionary<string, List<lineDetails>>();
                }
                if (!abilitySessionMobLines[line.session].ContainsKey(line.mobName))
                {
                    abilitySessionMobLines[line.session][line.mobName] = new List<lineDetails>();
                }

                if (!abilitySessionMobLinesAll.ContainsKey(line.session))
                {
                    abilitySessionMobLinesAll[line.session] = new List<lineDetails>();
                }

                abilitySessionMobLines[line.session][line.mobName].Add(line);
                abilitySessionMobLinesAll[line.session].Add(line);
            }
        }

        public void findPhase(int phaseNum)
        {
            foreach (KeyValuePair<int, Dictionary<string, List<lineDetails>>> sessionLines in abilitySessionMobLines)
            {
                if (!learningPhases.ContainsKey(sessionLines.Key))
                {
                    learningPhases[sessionLines.Key] = new Dictionary<int, phaseInfo>();
                }

                if (!learningPhases[sessionLines.Key].ContainsKey(phaseNum))
                {
                    learningPhases[sessionLines.Key][phaseNum] = new phaseInfo();
                }

                if (phaseNum <= 1)
                {
                    learningPhases[sessionLines.Key][phaseNum].phaseStartLine = 0;
                }
            }

            for (int testline = 0; testline < 1000; testline++)
			{
			    

                foreach (KeyValuePair<int, Dictionary<string, List<lineDetails>>> sessionLines in abilitySessionMobLines)
                {
                    if (!sessionLines.Value.ContainsKey(bossName))
                    {
                        continue;
                    }

                    int sessionLine = testline + learningPhases[sessionLines.Key][phaseNum].phaseStartLine;


                    //sessionLines.Value[bossName]

                    if (phaseNum <= 1)
                    {
                        learningPhases[sessionLines.Key][phaseNum].phaseStartLine = 1;
                    }
                }
			}
        }

        public void analyzeData()
        {
            readInAllData();

            foreach (KeyValuePair<int, Dictionary<string, List<lineDetails>>> sessionLines in abilitySessionMobLines)
	        {
		        if (!sessionLines.Value.ContainsKey(bossName))
                {
                    continue;
                }

                learningPhases[sessionLines.Key] = new Dictionary<int, phaseInfo>();

                learningPhases[sessionLines.Key][1] = new phaseInfo();

                learningPhases[sessionLines.Key][1].phaseStartLine = 1;

	        }





            foreach (KeyValuePair<int, Dictionary<string, List<lineDetails>>> sessionLines in abilitySessionMobLines)
            {
                if (!sessionLines.Value.ContainsKey(bossName))
                {
                    continue;
                }

                learningPhases[sessionLines.Key] = new Dictionary<int, phaseInfo>();

                learningPhases[sessionLines.Key][1] = new phaseInfo();

                learningPhases[sessionLines.Key][1].phaseStartLine = 1;

                List<lineDetails> bossLines = sessionLines.Value[bossName];

                for (int startLine = 0; startLine < bossLines.Count; startLine++)
                {
                    List<lineDetails> patternList = new List<lineDetails>();

                    // add all lines starting at this line until the end
                    for (int i = startLine; i < bossLines.Count; i++)
                    {
                        patternList.Add(bossLines[i]);
                    }

                    // repeatedly narrow the pattern until it can't be narrowed any further.
                    while (narrowPattern(ref patternList))
                    {
                        // don't need to do anything here.
                    }

                    //sessionPatterns[sessionLines.Key][startLine]
                }
            }
        }


        private bool narrowPattern(ref List<lineDetails> patternList)
        {
            bool changedPattern = false;

            // first check if the entire thing is one big pattern by eliminating lines near the end if they match the start
            for (int i = patternList.Count - 1; i >= 0; i--)
            {
                bool match = true;

                for (int j = i; j < patternList.Count; j++)
                {
                    int baseIdx = j - i;

                    if (patternList[baseIdx].ability.abilityName != patternList[j].ability.abilityName)
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    // Everything from i to the end of the pattern matches the start of the pattern, so we have a repeat. Remove the repeats.
                    patternList.RemoveRange(i, patternList.Count - i);
                    changedPattern = true;
                }
            }

            return changedPattern;
        }


        public void writeClassFile()
        {

            DateTime lastTime = EncounterController.started;
            TimeSpan timeDiff = TimeSpan.Zero;

            string basepath = EncounterController.encounterZonePath + "\\Auto Helper\\" + safeBossName;
            int num = 1;

            while (File.Exists(basepath + "." + num + ".cs"))
            {
                num++;
            }
            
            string output = "using System;" + Environment.NewLine;
            output += "using System.Collections.Generic;" + Environment.NewLine;
            output += "using System.Linq;" + Environment.NewLine;
            output += "using System.Text;" + Environment.NewLine;
            output += "using System.Windows;" + Environment.NewLine;
            output += "using FFXIVDBM.Plugin;" + Environment.NewLine;
            output += "using FFXIVAPP.Common.Core.Memory;" + Environment.NewLine;
            output += "using System.Text.RegularExpressions;" + Environment.NewLine;
            output += "" + Environment.NewLine;
            output += "namespace " + NSBossName + "NS" + num + Environment.NewLine;
            output += "{" + Environment.NewLine;
            output += "    " + Environment.NewLine;
            output += "    public class MainEncounterLogic : AbilityController, IEncounter" + Environment.NewLine;
            output += "    {" + Environment.NewLine;
            output += "        public void onStartEncounter()" + Environment.NewLine;
            output += "        {" + Environment.NewLine;
            output += "            // bossName is needed if you want health based phase swaps" + Environment.NewLine;
            output += "            bossName = \"" + bossName + "\";" + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            int phaseNum = 1; // for convenience" + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            // Boss's abilities" + Environment.NewLine;

            if (!abilityList.ContainsKey(bossName))
            {
                foreach (KeyValuePair<string, Dictionary<string, learningAbilities>> abilList in abilityList)
                {
                    if (abilList.Key.ToLower().Contains("the " + bossName.ToLower()))
                    {
                        bossName = abilList.Key;
                    }

                }
            }

            if (abilityList.ContainsKey(bossName))
            {
                foreach (KeyValuePair<string, learningAbilities> abil in abilityList[bossName])
                {
                    string abilityName = abil.Value.abilityName;

                    abil.Value.lastUsed -= TimeSpan.FromDays(365);

                    output += "            RotationAbility " + abilityName + " = new RotationAbility(); // " + abil.Key + Environment.NewLine;
                    output += "            " + abilityName + ".announceWarning = true; " + Environment.NewLine;
                    output += "            " + abilityName + ".match = new Regex(@\"" + abil.Value.regexLogLine.Replace("\"", "\\\"" ) + "\");" + Environment.NewLine;
                    output += "            " + abilityName + ".warningMessage = @\"" + abil.Value.logAbilityName + "\"; " + Environment.NewLine;
                    output += "            " + Environment.NewLine;
                }


                output += "            // Boss ability rotation" + Environment.NewLine;
                output += "            // You'll need to split these up into phases" + Environment.NewLine;
                output += "            // And separate out any timed moves which aren't part of a rotation" + Environment.NewLine;
                output += "            // For now we'll assume they're all part of phase 1" + Environment.NewLine;
                output += "            " + Environment.NewLine;
                output += "            phaseNum = 1;" + Environment.NewLine;
                output += "            phases[phaseNum] = new Phase();" + Environment.NewLine;
                output += "            " + Environment.NewLine;
                output += "            // You can use one of the following two methods for determining the end of a phase." + Environment.NewLine;
                output += "            // Just choose which is appropriate to the encounter and uncomment/modify to fit" + Environment.NewLine;
                output += "            //phases[phaseNum].phaseEndHP = 90;" + Environment.NewLine;
                output += "            //phases[phaseNum].phaseEndRegex = new Regex(@\"Titan uses Geocrush\");" + Environment.NewLine;
                output += "            " + Environment.NewLine;

                lastTime = EncounterController.started;
                timeDiff = TimeSpan.Zero;

                foreach (lineDetails abil in abilityTimedOrder[bossName])
                {
                    if (abil.logTime - abil.ability.lastUsed < TimeSpan.FromSeconds(10))
                    {
                        continue;
                    }

                    abil.ability.lastUsed = abil.logTime;


                    timeDiff = (abil.logTime - lastTime).Duration();

                    lastTime = abil.logTime;

                    output += "            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(" + timeDiff.TotalSeconds + "), " + abil.ability.abilityName + ");      // " + abil.logTime.Hour + ":" + abil.logTime.Minute + ":" + abil.logTime.Second + ":" + abil.logTime.Millisecond + " @ " + abil.bossHPPct + "%" + Environment.NewLine;
                }


                output += "            " + Environment.NewLine;
                output += "            " + Environment.NewLine;
                output += "            // Here's phase 2 to get you started. Just move any of the 'AddRotation' lines from above down below the 'phaseEndRegex' area, and they'll be part of phase 2" + Environment.NewLine;
                output += "            " + Environment.NewLine;
                output += "            phaseNum = 2;" + Environment.NewLine;
                output += "            phases[phaseNum] = new Phase();" + Environment.NewLine;
                output += "            " + Environment.NewLine;
                output += "            // You can use one of the following two methods for determining the end of a phase." + Environment.NewLine;
                output += "            // Just choose which is appropriate to the encounter and uncomment/modify to fit" + Environment.NewLine;
                output += "            //phases[phaseNum].phaseEndHP = 75;" + Environment.NewLine;
                output += "            //phases[phaseNum].phaseEndRegex = new Regex(@\"Titan uses Geocrush\");" + Environment.NewLine;
                output += "            " + Environment.NewLine;

            }


            output += "            " + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            // Next is the same as everything above, but including all abilities used by all non-players, " + Environment.NewLine;
            output += "            // in case you need something used by something other than the boss" + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            " + Environment.NewLine;



            output += "            // This ability list may contain duplicates. Please double check before using." + Environment.NewLine;

            foreach (KeyValuePair<string, Dictionary<string, learningAbilities>> abilList in abilityList)
            {
                foreach (KeyValuePair<string, learningAbilities> abil in abilityList[abilList.Key])
                {
                    string abilityName = abil.Value.abilityName;

                    abil.Value.lastUsed -= TimeSpan.FromDays(365);

                    output += "            RotationAbility " + abilityName + " = new RotationAbility(); // " + abil.Key + Environment.NewLine;
                    output += "            " + abilityName + ".announceWarning = true; " + Environment.NewLine;
                    output += "            " + abilityName + ".match = new Regex(@\"" + abil.Value.regexLogLine.Replace("\"", "\\\"") + "\");" + Environment.NewLine;
                    output += "            " + abilityName + ".warningMessage = @\"" + abil.Value.logAbilityName + "\"; " + Environment.NewLine;
                    output += "            " + Environment.NewLine;
                }
            }


            output += "            // You'll need to split these up into phases" + Environment.NewLine;
            output += "            // And separate out any timed moves which aren't part of a rotation" + Environment.NewLine;
            output += "            // For now we'll assume they're all part of phase 1" + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            phaseNum = 1;" + Environment.NewLine;
            output += "            phases[phaseNum] = new Phase();" + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            // You can use one of the following two methods for determining the end of a phase." + Environment.NewLine;
            output += "            // Just choose which is appropriate to the encounter and uncomment/modify to fit" + Environment.NewLine;
            output += "            //phases[phaseNum].phaseEndHP = 90;" + Environment.NewLine;
            output += "            //phases[phaseNum].phaseEndRegex = new Regex(\"Titan uses Geocrush\");" + Environment.NewLine;
            output += "            " + Environment.NewLine;

            lastTime = EncounterController.started;
            timeDiff = TimeSpan.Zero;

            foreach (lineDetails abil in abilityTimedOrderAll)
            {
                if (abil.logTime - abil.ability.lastUsed < TimeSpan.FromSeconds(10))
                {
                    continue;
                }

                abil.ability.lastUsed = abil.logTime;


                timeDiff = (abil.logTime - lastTime).Duration();

                lastTime = abil.logTime;

                output += "            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(" + timeDiff.TotalSeconds + "), " + abil.ability.abilityName + ");      // " + abil.logTime.Hour + ":" + abil.logTime.Minute + ":" + abil.logTime.Second + ":" + abil.logTime.Millisecond + " @ " + abil.bossHPPct + "%" + Environment.NewLine;
            }


            output += "            " + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            // Here's phase 2 to get you started. Just move any of the 'AddRotation' lines from above down below the 'phaseEndRegex' area, and they'll be part of phase 2" + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            phaseNum = 2;" + Environment.NewLine;
            output += "            phases[phaseNum] = new Phase();" + Environment.NewLine;
            output += "            " + Environment.NewLine;
            output += "            // You can use one of the following two methods for determining the end of a phase." + Environment.NewLine;
            output += "            // Just choose which is appropriate to the encounter and uncomment/modify to fit" + Environment.NewLine;
            output += "            //phases[phaseNum].phaseEndHP = 75;" + Environment.NewLine;
            output += "            //phases[phaseNum].phaseEndRegex = new Regex(\"Titan uses Geocrush\");" + Environment.NewLine;
            output += "            " + Environment.NewLine;

            
            output += @"

        }


        public void onEndEncounter()
        {

        }

        public void onMobAdded(ActorEntity mob)
        {

        }

        public void onMobRemoved(ActorEntity mob)
        {

        }

        public void onMobAgro(ActorEntity mob)
        {

        }

        public void onAgroRemoved(ActorEntity mob)
        {

        }

        public void onNewChatLine(string line)
        {

        }

        public void onTick()
        {

        }
    }
}
";

            File.WriteAllText(basepath + "." + num + ".cs", output);
             
        }




        public void onMobAgro(ActorEntity mob)
        {
            if (mob.HPMax > bossHealth && !EncounterController.ignoreMobs.Contains(mob.Name))
            {
                bossName = mob.Name;
                bossHealth = mob.HPMax;
            }

            allEnemies[mob.Name] = mob;

        }

        public void onMobRemoved(ActorEntity mob)
        {

        }


        public void onNewChatLine(string line)
        {

            string name = "";
            string ability = "";
            string otherTarget = "";
            string regexLogLine = "";
            bool hasTarget = false;
            bool uses = false;
            


            Match m = usesRegex.Match(line);

            if (m.Success)
            {
                uses = m.Groups["uses"].Value == "uses";
                name = m.Groups["name"].Value;
                ability = m.Groups["ability"].Value;

                if (uses)
                {
                    regexLogLine = Regex.Escape(" uses " + ability + ".");
                }
                else
                {
                    regexLogLine = Regex.Escape(" casts " + ability + ".");
                }
            }
            else
            {

                m = readiesRegex.Match(line);

                if (m.Success)
                {
                    name = m.Groups["name"].Value;
                    ability = m.Groups["ability"].Value;
                    
                    regexLogLine = Regex.Escape(" readies " + ability + ".");
                }
                else
                {


                    m = startsUsingRegex.Match(line);

                    if (m.Success)
                    {
                        hasTarget = true;
                        
                        name = m.Groups["name"].Value;
                        ability = m.Groups["ability"].Value;
                        string abilityID = m.Groups["ability"].Value;
                        otherTarget = m.Groups["target"].Value;

                        regexLogLine = Regex.Escape(" starts using ") + (ability == "" ? "(.*)" : Regex.Escape(ability)) + Regex.Escape(" (" + abilityID + ") on ") + "(.+)" + Regex.Escape(".");

                        if (ability == "")
                        {
                            ability = abilityID;
                        }
                    }
                    else
                    {
                        m = castingRegex.Match(line);

                        if (m.Success)
                        {
                            name = m.Groups["name"].Value;
                            ability = m.Groups["ability"].Value;

                            regexLogLine = Regex.Escape(" begins casting " + ability + ".");
                        }
                        else
                        {


                            m = suffersRegex.Match(line);

                            if (m.Success)
                            {
                                name = bossName;
                                ability = m.Groups["ability"].Value;
                                otherTarget = m.Groups["target"].Value;

                                regexLogLine = Regex.Escape(" suffers the effect of " + ability + ".");

                                bool foundPerson = false;
                                foreach (var KVP in EncounterController.pcEntities)
                                {
                                    ActorEntity pe = KVP.Value;

                                    if (pe.Name == otherTarget)
                                    {
                                        foundPerson = true;
                                    }
                                }

                                if (!foundPerson || 
                                    ability == "Pacification" || 
                                    ability == "Weakness" ||
                                    ability == "Brink of Death" ||
                                    ability == "Walking Dead" || 
                                    ability == "Holmgang")
                                {
                                    return;
                                }

                            }
                            else
                            {
                                m = targetMarkerRegex.Match(line);

                                if (m.Success)
                                {
                                    hasTarget = true;

                                    name = m.Groups["name"].Value;
                                    ability = "Target Marker " + m.Groups["type"].Value;
                                    otherTarget = m.Groups["target"].Value;

                                    if (name == "")
                                    {
                                        name = ability;
                                    }

                                    regexLogLine = Regex.Escape(" places a target marker over ") +"(.+)" + Regex.Escape(" of type " + m.Groups["type"].Value + ".");
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }


            if (!m.Success)
            {
                return;
            }

                
            // ignore pets
            if (name.Contains("-Egi") || name == "Eos" || name == "Selene" || name == "Topaz Carbuncle" || name == "Emerald Carbuncle")
            {
                return;
            }
            // ignore players
            foreach (var KVP in EncounterController.pcEntities)
            {
                ActorEntity pe = KVP.Value;
                if (pe.Name == name)
                {
                    return;
                }
            }

            if (!abilityList.ContainsKey(name) || abilityList[name] == null)
            {
                abilityList[name] = new Dictionary<string, learningAbilities>();
            }
            if (!abilityList[name].ContainsKey(ability) || abilityList[name][ability] == null)
            {
                abilityList[name][ability] = new learningAbilities();
                abilityList[name][ability].logAbilityName = ability;
                abilityList[name][ability].mobName = name;
                abilityList[name][ability].abilityName = nameStripperRegex.Replace(ability, "");
                abilityList[name][ability].count = 0;
                abilityList[name][ability].hasTarget = hasTarget;
                abilityList[name][ability].regexLogLine = regexLogLine;
            }
            if (!abilityTimedOrder.ContainsKey(name) || abilityTimedOrder[name] == null)
            {
                abilityTimedOrder[name] = new List<lineDetails>();
            }

            if (DateTime.Now - abilityList[name][ability].lastUsed < TimeSpan.FromSeconds(10))
            {
                //return;
            }

            abilityList[name][ability].count++;
            abilityList[name][ability].lastUsed = DateTime.Now;

            lineDetails tmpLine = new lineDetails();

            tmpLine.ability = abilityList[name][ability];
            try
            {
                tmpLine.bossHPPct = ((double)bossEntity.HPPercent) * 100.0d;
            }
            catch
            {
                debug("LearningHelper bossEntity is null");
            }
            tmpLine.logLine = line;
            tmpLine.logTime = DateTime.Now;
            tmpLine.mobName = name;
            tmpLine.session = -1;
            tmpLine.sessionStart = EncounterController.started;

            abilityTimedOrder[name].Add(tmpLine);
            abilityTimedOrderAll.Add(tmpLine);
                
            
        }

        public void onMobAdded(ActorEntity mob)
        {

        }


        public void onAgroRemoved(ActorEntity mob)
        {

        }


        #region utility
        private int getInsertID()
        {
            int insertID = 0;
            using (OleDbCommand SQLQuery = new OleDbCommand("SELECT @@IDENTITY"))
            {
                SQLQuery.Connection = database;


                try
                {
                    object tmp = SQLQuery.ExecuteScalar();

                    string tmpStr = tmp.ToString();

                    int.TryParse(tmpStr, out insertID);
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            return insertID;
        }


        public DataRowCollection query(OleDbCommand SQLQuery)
        {
            try
            {

                DataTable data = null;
                OleDbDataAdapter dataAdapter = null;
                SQLQuery.Connection = database;

                using (data = new DataTable())
                {
                    using (dataAdapter = new OleDbDataAdapter(SQLQuery))
                    {
                        dataAdapter.Fill(data);
                    }

                    lastSQLError = "";

                    return data.Rows;
                }
            }
            catch (Exception ex)
            {
                lastSQLError = "SQL Error: " + ex.Message + " Query: " + SQLQuery.CommandText;
            }


            return null;
        }

        public int execute(OleDbCommand SQLQuery)
        {
            try
            {

                //DataTable data = null;
                //OleDbDataAdapter dataAdapter = null;
                SQLQuery.Connection = database;

                lastSQLError = "";

                return SQLQuery.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                lastSQLError = "SQL Error: " + ex.Message + " Query: " + SQLQuery.CommandText;
            }


            return 0;
        }

        public string getDatabasePath()
        {
            return EncounterController.encounterZonePath + "\\Auto Helper\\" + safeBossName + ".mdb";
        }

        private void connectToDatabase()
        {
            try
            {
                if (!File.Exists(getDatabasePath()))
                {

                    // Use a late bound COM object to create a new catalog. This is so we avoid an interop assembly. 
                    Type catType = Type.GetTypeFromProgID("ADOX.Catalog");
                    object o = Activator.CreateInstance(catType);

                    
                    try
                    {
                        catType.InvokeMember("Create", System.Reflection.BindingFlags.InvokeMethod, null, o, new object[] { mdbConnectionStringBase + getDatabasePath() });

                    }
                    catch
                    {
                        try
                        {
                            catType.InvokeMember("Create", System.Reflection.BindingFlags.InvokeMethod, null, o, new object[] { mdbConnectionStringAlternateBase + getDatabasePath() });
                        }
                        catch (Exception ex2)
                        {
                            debug("Create Database error 1: " + ex2.Message, DBMErrorLevel.FineTimings);

                            Marshal.ReleaseComObject(o);
                            return;
                        }
                    }

                    try
                    {
                        Marshal.ReleaseComObject(o);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                debug("Create Database error 2: " + ex.Message, DBMErrorLevel.FineTimings);
                return;
            }

            try
            {
                database = new OleDbConnection(mdbConnectionStringBase + getDatabasePath());
                database.Open();

            }
            catch
            {
                try
                {
                    database = new OleDbConnection(mdbConnectionStringAlternateBase + getDatabasePath());
                    database.Open();
                }
                catch (Exception ex)
                {
                    debug("Database error: " + ex.Message, DBMErrorLevel.FineTimings);
                    return;
                }
            }

            analyzeAndUpdateDatabase();

        }

        private void createTableIfNotExists(string tableName)
        {
            OleDbCommand SQLQuery;
            bool tableExists = false;
            DataTable dt = database.GetSchema("tables");

            foreach (DataRow row in dt.Rows)
            {
                if (row["TABLE_NAME"].ToString() == tableName)
                {
                    tableExists = true;
                    break;
                }
            }

            if (!tableExists)
            {
                using (SQLQuery = new OleDbCommand("create table " + tableName))
                {
                    execute(SQLQuery);
                }
            }
        }

        private void analyzeAndUpdateAbilitiesTable()
        {
            OleDbCommand SQLQuery;

            createTableIfNotExists("abilities");

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilities ADD COLUMN abilityName varchar(100) NOT NULL"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilities ADD COLUMN mobName varchar(100) NOT NULL"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilities ADD COLUMN logAbilityName varchar(100)"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilities ADD COLUMN logLineRegex varchar(250)"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilities ADD COLUMN hasTarget YesNo"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilities ADD CONSTRAINT pk_MobAbility PRIMARY KEY (abilityName,mobName)"))
            {
                execute(SQLQuery);
            }


        }

        private void analyzeAndUpdateAbilityUsesTable()
        {
            OleDbCommand SQLQuery;


            createTableIfNotExists("abilityUses");

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilityUses ADD id AUTOINCREMENT PRIMARY KEY"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilityUses ADD COLUMN abilityName varchar(100)"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilityUses ADD COLUMN sessionId Long"))
            {
                execute(SQLQuery);
            }
            using (SQLQuery = new OleDbCommand("ALTER TABLE abilityUses ADD COLUMN sessionStart Double"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilityUses ADD COLUMN mobName varchar(100)"))
            {
                execute(SQLQuery);
            }



            using (SQLQuery = new OleDbCommand("ALTER TABLE abilityUses ADD COLUMN logLine varchar(250)"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilityUses ADD COLUMN logTime Double"))
            {
                execute(SQLQuery);
            }

            using (SQLQuery = new OleDbCommand("ALTER TABLE abilityUses ADD COLUMN bossHPPct DOUBLE"))
            {
                execute(SQLQuery);
            }
        }


        private void analyzeAndUpdateDatabase()
        {
            //analyzeAndUpdateInstanceRewardsTable();
            //analyzeAndUpdateInstanceStatsTable();
            analyzeAndUpdateAbilityUsesTable();
            analyzeAndUpdateAbilitiesTable();
        }

        #endregion


    }
}