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
        #region Property Bindings
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
        public int phase
        {
            get
            {
                return _phase;
            }
        }
        #endregion

        public Dictionary<int,Phase> phases;

        public List<TriggeredAbility> timedAbilities = new List<TriggeredAbility>();

        public string bossName = "";
        public ActorEntity bossEntity = null;
        public double bossHPPercent = 100;


        private int _phase = 1;

        private DateTime nextRotationTime = DateTime.Now;
        private RotationAbility nextRotationAbility = null;

        private Dictionary<TimeSpan, RotationAbility>.Enumerator nextRotationEnumerator;

        private bool _inController = false;


        public AbilityController()
        {
            _inController = true;

            phases = new Dictionary<int, Phase>();

            _inController = false;
        }


        public bool inController()
        {
            return _inController;
        }

        public bool bossCheck(ActorEntity mob)
        {
            _inController = true;

            if (mob.Name.Trim() == bossName.Trim())
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
            setPhase(_phase + 1);
        }


        
        public void setPhase(int phaseNum)
        {
            _inController = true;

            TimeSpan totalTime = TimeSpan.Zero;

            _phase = phaseNum;

            if (phases.ContainsKey(_phase))
            {

                phases[_phase].phaseStarted = DateTime.Now + phases[_phase].phaseStartDelay;

                if (phases[_phase].rotationAbilities.Any())
                {
                    nextRotationEnumerator = phases[_phase].rotationAbilitiesWarningTime.GetEnumerator();
                    nextRotationEnumerator.MoveNext();

                    nextRotationTime = phases[_phase].phaseStarted + nextRotationEnumerator.Current.Key;
                    nextRotationAbility = nextRotationEnumerator.Current.Value;


                    totalTime = phases[_phase].rotationAbilities.Last().Key;
                }

                phases[_phase].phaseLength = totalTime;

                if (phases[_phase].onPhaseStart != null)
                {
                    phases[_phase].onPhaseStart.start();
                }

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


            if (bossEntity != null)
            {
                if (phases.ContainsKey(_phase))
                {
                    bossHPPercent = ((double)bossEntity.HPPercent) * 100.0d;

                    if (bossHPPercent < phases[_phase].phaseEndHP)
                    {
                        nextPhase();
                    }

                    
                }
            }

            

            if (nextRotationTime <= DateTime.Now)
            {
                processRotationAbility();
            }

            if (phases.ContainsKey(_phase))
            {
                testTimedAbilityTicks(phases[_phase].timedAbilities);
            }

            testTimedAbilityTicks(timedAbilities);

            _inController = false;
            tickWorking = false;
        }

        private void testTimedAbilityTicks(List<TriggeredAbility> timedAbils)
        {
            foreach (TriggeredAbility abil in timedAbils)
            {
                testTimedAbilityDelayedMatch(abil);
                testTimedAbilityTimer(abil);
                testTimedAbilityHealth(abil);
            }
        }

        private void testTimedAbilityDelayedMatch(TriggeredAbility abil)
        {
            bool delayed = abil.timerStartedAt > DateTime.Now - abil.collectMultipleLinesFor;

            if (abil.announceWarning && abil.warningDelayStarted && !delayed)
            {
                abil.warningDelayStarted = false;
                string message = parseMessage(abil, abil.matchMessage);

                tts(message);
            }
        }
            
        private void testTimedAbilityTimer(TriggeredAbility abil)
        {
            if (abil.timerStarted)
            {
                if (abil.nextAbilityTime <= DateTime.Now)
                {
                    abil.timerStarted = false;

                    if (abil.announceWarning)
                    {
                        string message = parseMessage(abil, abil.warningMessage);

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
        private void testTimedAbilityHealth(TriggeredAbility abil)
        {
            if (abil.healthTriggerAt >= 0 && bossHPPercent < abil.healthTriggerAt && !abil.healthWarningAlreadyTriggered)
            {
                abil.healthWarningAlreadyTriggered = true;

                if (abil.announceWarning)
                {
                    string message = parseMessage(abil, abil.healthMessage);

                    tts(message);
                }

                if (abil.healthCallback != null)
                {
                    _inController = false;
                    abil.healthCallback(abil);
                    _inController = true;
                }
            }
        }


        private string parseMessage(Ability abil, string message)
        {
            Regex rgx;

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

            return message;
        }



        private void processRotationAbility()
        {
            Regex rgx;

            if (phases.ContainsKey(_phase))
            {
                if (phases[_phase].rotationAbilities.Any())
                {
                    string debugOut = nextRotationAbility.warningMessage + " warning time. ";
                    if (nextRotationAbility.announceWarning)
                    {
                        string message = parseMessage(nextRotationAbility, nextRotationAbility.warningMessage);

                        tts(message);
                    }

                    RotationAbility tmpAbil = nextRotationAbility;



                    if (!nextRotationEnumerator.MoveNext())
                    {
                        nextRotationEnumerator = phases[_phase].rotationAbilitiesWarningTime.GetEnumerator();
                        nextRotationEnumerator.MoveNext();

                        phases[_phase].phaseStarted = phases[_phase].phaseStarted + phases[_phase].phaseLength;
                    }

                    nextRotationTime = phases[_phase].phaseStarted + nextRotationEnumerator.Current.Key;
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


        private void testTimedAbilityRegex(TriggeredAbility abil, string line)
        {
            if (abil != null && abil.matchRegex && abil.match != null)
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

        private void testTimedAbilityRegex(List<TriggeredAbility> timedAbils, string line)
        {
            foreach (TriggeredAbility abil in timedAbils)
            {
                testTimedAbilityRegex(abil, line);
            }
        }

        private void testRotationAbilityRegex(string line)
        {
            foreach (KeyValuePair<TimeSpan, RotationAbility> abil in phases[_phase].rotationAbilities)
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
                            DateTime abilThisRotationsTime = phases[_phase].phaseStarted + abil.Key;
                            DateTime abilLastRotationsTime = abilThisRotationsTime - phases[_phase].phaseLength;
                            DateTime abilNextRotationsTime = abilThisRotationsTime + phases[_phase].phaseLength;

                            TimeSpan timeBetweenNowAndThisRotationsTime = (DateTime.Now - abilThisRotationsTime).Duration();
                            TimeSpan timeUntilNextRotationsAbil = (abilNextRotationsTime - DateTime.Now).Duration();
                            TimeSpan timeSinceLastRotationsAbil = (DateTime.Now - abilLastRotationsTime).Duration();

                            EncounterController.debug(abil.Value.warningMessage + " matched (" + timeSinceLastRotationsAbil + ", " + timeBetweenNowAndThisRotationsTime + ", " + timeUntilNextRotationsAbil + "), phaseStarted: " + phases[_phase].phaseStarted + " nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);

                            if (timeUntilNextRotationsAbil < timeSinceLastRotationsAbil) // already passed this rotation, don't need to check last rotation times
                            {
                                if (timeUntilNextRotationsAbil < timeBetweenNowAndThisRotationsTime)
                                {
                                    if (timeUntilNextRotationsAbil <= TimeSpan.FromSeconds(10) || abil.Value.uniqueInPhase)
                                    {
                                        phases[_phase].phaseStarted += timeUntilNextRotationsAbil;
                                        nextRotationTime += timeUntilNextRotationsAbil;

                                        EncounterController.debug(" - timeUntilNextRotationsAbil closest, new phaseStarted: " + phases[_phase].phaseStarted + " new nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);

                                    }
                                }
                                else
                                {
                                    if (timeBetweenNowAndThisRotationsTime <= TimeSpan.FromSeconds(10) || abil.Value.uniqueInPhase)
                                    {
                                        phases[_phase].phaseStarted += timeBetweenNowAndThisRotationsTime;
                                        nextRotationTime += timeBetweenNowAndThisRotationsTime;

                                        EncounterController.debug(" - timeBetweenNowAndThisRotationsTime closest, new phaseStarted: " + phases[_phase].phaseStarted + " new nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);
                                    }
                                }
                            }
                            else // haven't passed rotation this rotation, check previous phase time and this rotation, ignore next rotation
                            {
                                if (timeSinceLastRotationsAbil < timeBetweenNowAndThisRotationsTime)
                                {
                                    if (timeSinceLastRotationsAbil <= TimeSpan.FromSeconds(10) || abil.Value.uniqueInPhase)
                                    {
                                        phases[_phase].phaseStarted -= timeSinceLastRotationsAbil;
                                        nextRotationTime -= timeSinceLastRotationsAbil;

                                        EncounterController.debug(" - timeSinceLastRotationsAbil closest, new phaseStarted: " + phases[_phase].phaseStarted + " new nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);
                                    }
                                }
                                else
                                {
                                    if (timeBetweenNowAndThisRotationsTime <= TimeSpan.FromSeconds(10) || abil.Value.uniqueInPhase)
                                    {
                                        phases[_phase].phaseStarted -= timeBetweenNowAndThisRotationsTime;
                                        nextRotationTime -= timeBetweenNowAndThisRotationsTime;

                                        EncounterController.debug(" - timeBetweenNowAndThisRotationsTime closest, new phaseStarted: " + phases[_phase].phaseStarted + " new nextRotationTime: " + nextRotationTime, DBMErrorLevel.FineTimings);
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

            if (phases.ContainsKey(_phase))
            {
                if (phases[_phase].phaseEndRegex != null && phases[_phase].phaseEndRegex.IsMatch(line))
                {
                    // save the regex to use to trigger the next phase's onPhaseStart
                    Regex endRegex = phases[_phase].phaseEndRegex;

                    nextPhase();
                    
                    if (phases[_phase].onPhaseStart != null)
                    {
                        phases[_phase].onPhaseStart.match = endRegex;

                        phases[_phase].timedAbilities.Add(phases[_phase].onPhaseStart);
                    }
                }
            }
            if (phases.ContainsKey(_phase)) // test again, since we may have just switched phases
            {
                testTimedAbilityRegex(phases[_phase].timedAbilities, line);
                testTimedAbilityRegex(timedAbilities, line);

                testRotationAbilityRegex(line);
            }

            _inController = false;
        }

        public void delayRotation(TimeSpan amount)
        {
            phases[_phase].phaseStarted += amount;
            nextRotationTime += amount;
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
