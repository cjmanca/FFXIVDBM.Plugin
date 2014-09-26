using FFXIVAPP.Common.Core.Memory;
using FFXIVAPP.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVDBM.Plugin
{
    public class AbilityController
    {
        public static List<ActorEntity> agroList
        {
            get
            {
                return EncounterController.agroList;
            }
        }


        public static List<ActorEntity> mobList
        {
            get
            {
                return EncounterController.mobList;
            }
        }
        public static List<ActorEntity> npcList
        {
            get
            {
                return EncounterController.npcList;
            }
        }

        public static List<ActorEntity> pcEntities
        {
            get
            {
                return EncounterController.pcEntities;
            }
        }

        public static List<PartyEntity> partyList
        {
            get
            {
                return EncounterController.partyList;
            }
        }

        public static PlayerEntity playerEntity
        {
            get
            {
                return EncounterController.playerEntity;
            }
        }

        public static ActorEntity CurrentUser
        {
            get
            {
                return EncounterController.CurrentUser;
            }
        }

        public static ZoneHelper.MapInfo zone
        {
            get
            {
                return EncounterController.zone;
            }
        }

        public Dictionary<int,Phase> phases;
        public int phase = 1;

        public List<TriggeredAbility> timedAbilities = new List<TriggeredAbility>();

        public DateTime nextRotationTime = DateTime.Now;
        public RotationAbility nextRotationAbility = null;

        public Dictionary<TimeSpan, RotationAbility>.Enumerator nextRotationEnumerator;

        public string bossName = "";
        public ActorEntity bossEntity = null;


        private bool _inController = false;


        public bool inController()
        {
            return _inController;
        }

        public AbilityController()
        {
            _inController = true;

            phases = new Dictionary<int, Phase>();

            _inController = false;
        }

        public bool bossCheck(ActorEntity mob)
        {
            _inController = true;

            if (mob.Name == bossName)
            {
                bossEntity = mob;

                _inController = false;
                return true;
            }

            _inController = false;
            return false;
        }

        public void nextPhase()
        {
            setPhase(phase + 1);
        }


        
        public void setPhase(int phaseNum)
        {
            _inController = true;

            TimeSpan totalTime = TimeSpan.Zero;

            phase = phaseNum;

            if (phases.ContainsKey(phase))
            {

                phases[phase].phaseStarted = DateTime.Now + phases[phase].phaseStartDelay;

                if (phases[phase].rotationAbilities.Any())
                {
                    nextRotationEnumerator = phases[phase].rotationAbilitiesWarningTime.GetEnumerator();
                    nextRotationEnumerator.MoveNext();

                    nextRotationTime = phases[phase].phaseStarted + nextRotationEnumerator.Current.Key;
                    nextRotationAbility = nextRotationEnumerator.Current.Value;


                    totalTime = phases[phase].rotationAbilities.Last().Key;
                }

                phases[phase].phaseLength = totalTime;
            }

            _inController = false;
        }

        

        private bool tickWorking = false;

        public void tick()
        {
            if (tickWorking)
            {
                return;
            }

            tickWorking = true;
            _inController = true;

            foreach (ActorEntity ent in EncounterController.agroList)
            {
                if (bossCheck(ent))
                {
                    if (phases.ContainsKey(phase))
                    {
                        if ((((double)ent.HPPercent) * 100.0d) < phases[phase].phaseEndHP)
                        {
                            nextPhase();
                        }
                    }
                }
            }
            

            if (nextRotationTime <= DateTime.Now)
            {
                processRotationAbility();
            }

            if (phases.ContainsKey(phase))
            {
                testTimedAbilityTicks(phases[phase].timedAbilities);
            }

            testTimedAbilityTicks(timedAbilities);

            _inController = false;
            tickWorking = false;
        }

        private void testTimedAbilityTicks(List<TriggeredAbility> timedAbils)
        {
            Regex rgx;

            foreach (TriggeredAbility abil in timedAbils)
            {
                bool delayed = abil.timerStartedAt > DateTime.Now - abil.collectMultipleLinesFor;

                if (abil.announceWarning && abil.warningDelayStarted && !delayed)
                {
                    string message = abil.matchMessage;

                    abil.warningDelayStarted = false;

                    if (abil.lastMatch.Count > 0)
                    {
                        foreach (KeyValuePair<int, Dictionary<string, string>> collections in abil.lastMatch)
                        {
                            foreach (KeyValuePair<string, string> group in collections.Value)
                            {
                                rgx = new Regex("\\$\\{" + Regex.Escape(group.Key) + "\\." + Regex.Escape(collections.Key.ToString()) + "\\}");

                                if (rgx.IsMatch(message))
                                {
                                    message = rgx.Replace(message, group.Value);
                                }
                            }
                        }

                        foreach (KeyValuePair<int, Dictionary<string, string>> collections in abil.lastMatch)
                        {
                            foreach (KeyValuePair<string, string> group in collections.Value)
                            {
                                rgx = new Regex("\\$" + Regex.Escape(group.Key));
                                message = rgx.Replace(message, group.Value);

                                rgx = new Regex("\\$\\{" + Regex.Escape(group.Key) + "\\}");
                                message = rgx.Replace(message, group.Value);
                            }
                        }
                    }

                    rgx = new Regex("\\$\\{[^\\}]+\\}");
                    message = rgx.Replace(message, "");
                    rgx = new Regex("\\$[^ ]+");
                    message = rgx.Replace(message, "");

                    tts(message);
                }


                if (abil.timerStarted)
                {
                    if (abil.nextAbilityTime <= DateTime.Now)
                    {
                        abil.timerStarted = false;

                        if (abil.announceWarning)
                        {
                            string message = abil.warningMessage;

                            if (abil.lastMatch.Count > 0)
                            {
                                foreach (KeyValuePair<int, Dictionary<string, string>> collections in abil.lastMatch)
                                {
                                    foreach (KeyValuePair<string, string> group in collections.Value)
                                    {
                                        rgx = new Regex("\\$\\{" + Regex.Escape(group.Key) + "\\." + Regex.Escape(collections.Key.ToString()) + "\\}");

                                        if (rgx.IsMatch(message))
                                        {
                                            message = rgx.Replace(message, group.Value);
                                        }
                                    }
                                }

                                foreach (KeyValuePair<int, Dictionary<string, string>> collections in abil.lastMatch)
                                {
                                    foreach (KeyValuePair<string, string> group in collections.Value)
                                    {
                                        rgx = new Regex("\\$" + Regex.Escape(group.Key));
                                        message = rgx.Replace(message, group.Value);

                                        rgx = new Regex("\\$\\{" + Regex.Escape(group.Key) + "\\}");
                                        message = rgx.Replace(message, group.Value);
                                    }
                                }
                            }

                            rgx = new Regex("\\$\\{[^\\}]+\\}");
                            message = rgx.Replace(message, "");
                            rgx = new Regex("\\$[^ ]+");
                            message = rgx.Replace(message, "");

                            tts(message);
                        }

                        if (abil.warningCallback != null)
                        {
                            _inController = false;
                            abil.warningCallback(abil);
                            _inController = true;
                        }


                        if (abil.timerAutoRestart)
                        {
                            abil.start(false);
                        }
                    }
                }
            }
        }

        private void processRotationAbility()
        {
            Regex rgx;

            if (phases.ContainsKey(phase))
            {
                if (phases[phase].rotationAbilities.Any())
                {
                    string debugOut = nextRotationAbility.warningMessage + " warning time. ";
                    if (nextRotationAbility.announceWarning)
                    {
                        string message = nextRotationAbility.warningMessage;

                        if (nextRotationAbility.lastMatch.Count > 0)
                        {
                            foreach (KeyValuePair<int, Dictionary<string, string>> collections in nextRotationAbility.lastMatch)
                            {
                                foreach (KeyValuePair<string, string> group in collections.Value)
                                {
                                    rgx = new Regex("\\$\\{" + Regex.Escape(group.Key) + "\\." + Regex.Escape(collections.Key.ToString()) + "\\}");

                                    if (rgx.IsMatch(message))
                                    {
                                        message = rgx.Replace(message, group.Value);
                                    }
                                }
                            }

                            foreach (KeyValuePair<int, Dictionary<string, string>> collections in nextRotationAbility.lastMatch)
                            {
                                foreach (KeyValuePair<string, string> group in collections.Value)
                                {
                                    rgx = new Regex("\\$" + Regex.Escape(group.Key));
                                    message = rgx.Replace(message, group.Value);

                                    rgx = new Regex("\\$\\{" + Regex.Escape(group.Key) + "\\}");
                                    message = rgx.Replace(message, group.Value);
                                }
                            }
                        }

                        rgx = new Regex("\\$\\{[^\\}]+\\}");
                        message = rgx.Replace(message, "");
                        rgx = new Regex("\\$[^ ]+");
                        message = rgx.Replace(message, "");


                        tts(message);
                    }

                    RotationAbility tmpAbil = nextRotationAbility;



                    if (!nextRotationEnumerator.MoveNext())
                    {
                        nextRotationEnumerator = phases[phase].rotationAbilitiesWarningTime.GetEnumerator();
                        nextRotationEnumerator.MoveNext();

                        phases[phase].phaseStarted = phases[phase].phaseStarted + phases[phase].phaseLength;
                    }

                    nextRotationTime = phases[phase].phaseStarted + nextRotationEnumerator.Current.Key;
                    nextRotationAbility = nextRotationEnumerator.Current.Value;

                    debugOut += "nextRotationTime: " + nextRotationTime + " nextRotationAbility: " + nextRotationAbility.warningMessage;

                    EncounterController.debug(debugOut, DBMErrorLevel.FineTimings);

                    // Need to call this after moving to the next rotation
                    // otherwise the callback may swap phases, and we'd then skip the first rotation in the next phase
                    if (tmpAbil.warningCallback != null)
                    {
                        _inController = false;
                        tmpAbil.warningCallback(tmpAbil);
                        _inController = true;
                    }

                }
            }
        }

        public void endEncounter()
        {
            _inController = true;

            _inController = false;
        }


        private void testTimedAbilityRegex(List<TriggeredAbility> timedAbils, string line)
        {
            Regex rgx = null;
            string message;

            foreach (TriggeredAbility abil in timedAbils)
            {
                if (abil.matchRegex && abil.match != null)
                {
                    Match m = abil.match.Match(line);

                    if (m.Success)
                    {
                        bool delayed = abil.timerStartedAt > DateTime.Now - abil.collectMultipleLinesFor;
                        abil.warningDelayStarted = true;

                        if (abil.collectMultipleLinesFor == TimeSpan.Zero || !delayed)
                        {
                            abil.lastMatch.Clear();
                        }

                        int lineNumber = abil.lastMatch.Count + 1;

                        abil.lastMatch[lineNumber] = new Dictionary<string, string>();

                        string[] groupNames = abil.match.GetGroupNames();

                        foreach (string groupName in groupNames)
                        {
                            abil.lastMatch[lineNumber][groupName] = m.Groups[groupName].Value;
                        }



                        if (abil.matchCallback != null)
                        {
                            _inController = false;
                            abil.matchCallback(abil, m);
                            _inController = true;
                        }

                        abil.start();
                    }
                }
            }
        }

        private void testRotationAbilityRegex(string line)
        {
            foreach (KeyValuePair<TimeSpan, RotationAbility> abil in phases[phase].rotationAbilities)
            {
                if (abil.Value.matchRegex && abil.Value.match != null)
                {
                    Match m = abil.Value.match.Match(line);

                    if (m.Success)
                    {
                        abil.Value.lastMatch.Clear();

                        int lineNumber = abil.Value.lastMatch.Count + 1;

                        abil.Value.lastMatch[lineNumber] = new Dictionary<string, string>();

                        string[] groupNames = abil.Value.match.GetGroupNames();

                        foreach (string groupName in groupNames)
                        {
                            abil.Value.lastMatch[lineNumber][groupName] = m.Groups[groupName].Value;
                        }


                        // Since we matched a rotation ability, we need to check the current time against when the timer was supposed to trigger, 
                        // and adjust our timer accordingly so that it doesn't desync.

                        // Some abilities will match multiple times in a row, sometimes one or two seconds apart. Make sure we only process the first.
                        TimeSpan timeSinceLastMatched = (DateTime.Now - abil.Value.lastMatched).Duration();

                        if (timeSinceLastMatched < TimeSpan.FromSeconds(10))
                        {
                            EncounterController.debug(abil.Value.warningMessage + " matched multiple times too fast.", DBMErrorLevel.FineTimings);
                        }
                        else
                        {
                            abil.Value.lastMatched = DateTime.Now;


                            // First, figure out which point in the rotation is closest to our current time
                            DateTime abilThisRotationsTime = phases[phase].phaseStarted + abil.Key;
                            DateTime abilLastRotationsTime = abilThisRotationsTime - phases[phase].phaseLength;
                            DateTime abilNextRotationsTime = abilThisRotationsTime + phases[phase].phaseLength;

                            TimeSpan timeBetweenNowAndThisRotationsTime = (DateTime.Now - abilThisRotationsTime).Duration();
                            TimeSpan timeUntilNextRotationsAbil = (abilNextRotationsTime - DateTime.Now).Duration();
                            TimeSpan timeSinceLastRotationsAbil = (DateTime.Now - abilLastRotationsTime).Duration();

                            EncounterController.debug(abil.Value.warningMessage + " matched (" + timeSinceLastRotationsAbil + ", " + timeBetweenNowAndThisRotationsTime + ", " + timeUntilNextRotationsAbil + "), phaseStarted: " + phases[phase].phaseStarted + " nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);

                            if (timeUntilNextRotationsAbil < timeSinceLastRotationsAbil) // already passed this rotation, don't need to check last rotation times
                            {
                                if (timeUntilNextRotationsAbil < timeBetweenNowAndThisRotationsTime)
                                {
                                    if (timeUntilNextRotationsAbil <= TimeSpan.FromSeconds(10))
                                    {
                                        phases[phase].phaseStarted += timeUntilNextRotationsAbil;
                                        nextRotationTime += timeUntilNextRotationsAbil;

                                        EncounterController.debug(" - timeUntilNextRotationsAbil closest, new phaseStarted: " + phases[phase].phaseStarted + " new nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);

                                    }
                                }
                                else
                                {
                                    if (timeBetweenNowAndThisRotationsTime <= TimeSpan.FromSeconds(10))
                                    {
                                        phases[phase].phaseStarted += timeBetweenNowAndThisRotationsTime;
                                        nextRotationTime += timeBetweenNowAndThisRotationsTime;

                                        EncounterController.debug(" - timeBetweenNowAndThisRotationsTime closest, new phaseStarted: " + phases[phase].phaseStarted + " new nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);
                                    }
                                }
                            }
                            else // haven't passed rotation this rotation, check previous phase time and this rotation, ignore next rotation
                            {
                                if (timeSinceLastRotationsAbil < timeBetweenNowAndThisRotationsTime)
                                {
                                    if (timeSinceLastRotationsAbil <= TimeSpan.FromSeconds(10))
                                    {
                                        phases[phase].phaseStarted -= timeSinceLastRotationsAbil;
                                        nextRotationTime -= timeSinceLastRotationsAbil;

                                        EncounterController.debug(" - timeSinceLastRotationsAbil closest, new phaseStarted: " + phases[phase].phaseStarted + " new nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);
                                    }
                                }
                                else
                                {
                                    if (timeBetweenNowAndThisRotationsTime <= TimeSpan.FromSeconds(10))
                                    {
                                        phases[phase].phaseStarted -= timeBetweenNowAndThisRotationsTime;
                                        nextRotationTime -= timeBetweenNowAndThisRotationsTime;

                                        EncounterController.debug(" - timeBetweenNowAndThisRotationsTime closest, new phaseStarted: " + phases[phase].phaseStarted + " new nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);
                                    }
                                }
                            }
                        }

                        if (abil.Value.matchCallback != null)
                        {
                            _inController = false;
                            abil.Value.matchCallback(abil.Value, m);
                            _inController = true;
                        }
                    }
                }
            }
        }



        public void processChatLine(string line)
        {
            _inController = true;

            if (phases.ContainsKey(phase))
            {
                if (phases[phase].phaseEndRegex != null && phases[phase].phaseEndRegex.IsMatch(line))
                {
                    nextPhase();
                }
            }
            if (phases.ContainsKey(phase)) // test again, since we may have just switched phases
            {
                testTimedAbilityRegex(phases[phase].timedAbilities, line);
                testTimedAbilityRegex(timedAbilities, line);

                testRotationAbilityRegex(line);
            }

            _inController = false;
        }




        public static void tts(string toRead)
        {

            EncounterController.tts(toRead);

        }

        public static void debug(string message, DBMErrorLevel level = DBMErrorLevel.EncounterErrors, Exception ex = null)
        {
            EncounterController.debug(message, level, ex);
        }
    }
}
