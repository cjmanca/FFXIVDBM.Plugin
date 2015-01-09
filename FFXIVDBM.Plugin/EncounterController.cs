
using System;
using System.Linq;
using FFXIVAPP.IPluginInterface.Events;
using FFXIVAPP.Common.Core.Memory;
using FFXIVAPP.Common.Helpers;
using FFXIVDBM.Plugin.Utilities;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Text;
using System.Reflection;
using System.Speech.Synthesis;
using EncounterNS;
using FFXIVAPP.Common.Audio;
using NAudio.Wave;
using System.Threading;
using System.Diagnostics;
using FFXIVDBM.Plugin.Views;
using FFXIVDBM.Plugin.Properties;

namespace FFXIVDBM.Plugin
{
    public enum DBMErrorLevel
    {
        FineTimings = 0,
        Trace = 1,
        Notice = 2,
        EncounterInfo = 3,
        EncounterErrors = 4,
        EngineErrors = 5,
        None = 6
    }

    public class EncounterController
    {
        public static DBMErrorLevel errorLevel = DBMErrorLevel.Trace;

        public static int speechVolume = 50;

        private static bool useTestEncounter = false;

        #region Property Bindings
        private static List<ActorEntity> _agroList = new List<ActorEntity>();
        public static List<ActorEntity> agroList
        {
            get
            {
                return _agroList;
            }
            set
            {
                _agroList = value;
            }
        }


        private static List<ActorEntity> _mobList = new List<ActorEntity>();
        public static List<ActorEntity> mobList
        {
            get
            {
                return _mobList;
            }
            set
            {
                _mobList = value;
            }
        }


        private static List<ActorEntity> _npcList = new List<ActorEntity>();
        public static List<ActorEntity> npcList
        {
            get
            {
                return _npcList;
            }
            set
            {
                _npcList = value;
            }
        }


        private static List<ActorEntity> _pcEntities = new List<ActorEntity>();
        public static List<ActorEntity> pcEntities
        {
            get
            {
                return _pcEntities;
            }
            set
            {
                _pcEntities = value;
            }
        }

        private static List<PartyEntity> _partyList = new List<PartyEntity>();
        public static List<PartyEntity> partyList
        {
            get
            {
                return _partyList;
            }
            set
            {
                _partyList = value;
            }
        }

        private static PlayerEntity _playerEntity = null;
        public static PlayerEntity playerEntity
        {
            get
            {
                return _playerEntity;
            }
            set
            {
                _playerEntity = value;
            }
        }

        private static ActorEntity _currentUser = null;
        public static ActorEntity CurrentUser 
        { 
            get 
            { 
                return _currentUser;
            }
            set 
            {
                _currentUser = value;
            }
        }

        private static ZoneHelper.MapInfo _zone = null;
        public static ZoneHelper.MapInfo zone
        {
            get
            {
                return _zone;
            }
            set
            {
                if (_zone != null && value != null &&_zone.Index != value.Index)
                {
                    handleRemoveEntryName = "Zone change";
                    endEncounter();
                    handleRemoveEntryName = "";
                }
                _zone = value;
            }
        }

        #endregion

        private static IEncounter implementationClass = null;
        private static IEncounter learningHelperClass = null;

        private static List<ActorEntity> _mobCached = null;
        private static List<EnmityEntry> _enmityCached = null;
        private static System.Timers.Timer tickTimer = null;
        private static System.Timers.Timer enmityTimer = null;


        private static string debugLogPath = "";
        private static string encounterDebugLogPath = "";

        public static bool encounterStarted = false;

        public static DateTime started = DateTime.Now;

        public static string encounterZonePath = "";

        public static Thread ttsThread;

        public static Regex logStripper = new Regex(@" ⇒ ");


        public static List<VoiceThroughNetAudio> TTSQueue = new List<VoiceThroughNetAudio>();
        public static bool speaking = false;
        public static object speakingLock = new object();

        private class Replacements
        {
            public Replacements(Regex newRegex, string newReplacement)
            {
                regex = newRegex;
                replacement = newReplacement;
            }
            public Regex regex;
            public string replacement;
        }

        private static Dictionary<string, Replacements> youGainDict = new Dictionary<string, Replacements>();
        private static Dictionary<string, Replacements> youLoseDict = new Dictionary<string, Replacements>();
        private static Dictionary<string, Replacements> youSufferDict = new Dictionary<string, Replacements>();
        private static Dictionary<string, Replacements> youRecoverDict = new Dictionary<string, Replacements>();
        private static Dictionary<string, Regex> sealedOffDict = new Dictionary<string, Regex>();



        private static Replacements youGain = null;
        private static Replacements youLose = null;
        private static Replacements youSuffer = null;
        private static Replacements youRecover = null;
        private static Regex sealedOff = null;

        private static Regex testString = null;
        private static Regex endString = null;

        public static List<string> ignoreMobs;

        static bool inUpdate = false;
        static bool inTick = false;
        static bool setupDone = false;

        static int handleRemoveEntry = 0;
        static string handleRemoveEntryName = "";

        static object tickLock = new object();
        static object updateLock = new object();
        static object accessControl = new object();


        public static void initialize()
        {
            debugLogPath = Constants.BaseDirectory + "\\debug.log";

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainAssemblyResolve;


                _enmityCached = new List<EnmityEntry>();
                _agroList = new List<ActorEntity>();
                _mobCached = new List<ActorEntity>();



                youGainDict["English"] = new Replacements(new Regex(@"^( ⇒ )?You gain the effect of "), " gains the effect of ");
                youLoseDict["English"] = new Replacements(new Regex(@"^( ⇒ )?You lose the effect of "), " loses the effect of ");
                youSufferDict["English"] = new Replacements(new Regex(@"^( ⇒ )?You suffer the effect of "), " suffers the effect of ");
                youRecoverDict["English"] = new Replacements(new Regex(@"^( ⇒ )?You recover from the effect of "), " recovers from the effect of ");
                sealedOffDict["English"] = new Regex(@" will be sealed off in 15 seconds\!");

                // please send corrections for the replacements if needed
                youGainDict["French"] = new Replacements(new Regex(@"^( ⇒ )?Vous bénéficiez? de l'effet "), " bénéficiez de l'effet ");
                youLoseDict["French"] = new Replacements(new Regex(@"^( ⇒ )?Vous perd(ez?)? l'effet "), " perdez l'effet ");
                youSufferDict["French"] = new Replacements(new Regex(@"^( ⇒ )?Vous subi(t|ssez?) l'effet "), " subissez l'effet ");
                youRecoverDict["French"] = new Replacements(new Regex(@"^( ⇒ )?Vous (perd(ez?)?|ne subi(t|ssez?)) plus l'effet "), " perdez subissez plus l'effet ");
                sealedOffDict["French"] = new Regex(@" will be sealed off in 15 seconds\!"); // TODO: Find out the French translation for this

                // Japanese log lines already use your own name by the looks of things from parser plugin regex file
                // If this isn't correct, please tell me the proper lines to match
                youGainDict["Japanese"] = null;
                youLoseDict["Japanese"] = null;
                youSufferDict["Japanese"] = null;
                youRecoverDict["Japanese"] = null;
                sealedOffDict["Japanese"] = new Regex(@" will be sealed off in 15 seconds\!"); // TODO: Find out the Japanese translation for this

                // I need working regex strings for several of these for the German language version
                // please send corrections for the replacements if needed
                youGainDict["German"] = new Replacements(new Regex(@"^( ⇒ )?(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<target>.+) erh lt(st| den) Effekt von "), " erh lt(st| den) Effekt von ");
                youLoseDict["German"] = null; // new Replacements(new Regex(@""), "");
                youSufferDict["German"] = null; // new Replacements(new Regex(@""), "");
                youRecoverDict["German"] = null; // new Replacements(new Regex(@""), "");
                sealedOffDict["German"] = new Regex(@" will be sealed off in 15 seconds\!"); // TODO: Find out the Japanese translation for this


                ttsThread = new Thread(TTSThread);

                ttsThread.Start();

                // send ticks 10 times per second to the encounter controllers
                tickTimer = new System.Timers.Timer(50);
                tickTimer.Elapsed += tickTimerEvent;
                tickTimer.Start();

                // check for agro changes and update mob info twice per second
                enmityTimer = new System.Timers.Timer(500);
                enmityTimer.Elapsed += updateData;
                enmityTimer.Start();

            }
            catch (Exception e2)
            {
                debug("initialize", DBMErrorLevel.EngineErrors, e2);
            }
        }

        static void trySetup()
        {
            if (Constants.GameLanguage == null)
            {
                return;
            }

            try
            {
                if (youGainDict.ContainsKey(Constants.GameLanguage))
                {
                    youGain = youGainDict[Constants.GameLanguage];
                }
                if (youLoseDict.ContainsKey(Constants.GameLanguage))
                {
                    youLose = youLoseDict[Constants.GameLanguage];
                }
                if (youSufferDict.ContainsKey(Constants.GameLanguage))
                {
                    youSuffer = youSufferDict[Constants.GameLanguage];
                }
                if (youRecoverDict.ContainsKey(Constants.GameLanguage))
                {
                    youRecover = youRecoverDict[Constants.GameLanguage];
                }
                if (sealedOffDict.ContainsKey(Constants.GameLanguage))
                {
                    sealedOff = sealedOffDict[Constants.GameLanguage];
                }

                if (youGain == null)
                {
                    youGain = new Replacements(null, "");
                }
                if (youLose == null)
                {
                    youLose = new Replacements(null, "");
                }
                if (youSuffer == null)
                {
                    youSuffer = new Replacements(null, "");
                }
                if (youRecover == null)
                {
                    youRecover = new Replacements(null, "");
                }

                testString = new Regex(@"\!testing");
                endString = new Regex(@"\!end");

                try
                {
                    EncounterController.speechVolume = int.Parse(Settings.Default.VoiceVolume);
                }
                catch
                {
                    EncounterController.speechVolume = 100;
                }
                try
                {
                    EncounterController.errorLevel = (DBMErrorLevel)Enum.Parse(typeof(DBMErrorLevel), Settings.Default.DebugLogLevel);
                }
                catch
                {
                    EncounterController.errorLevel = DBMErrorLevel.EncounterInfo;
                }


                debug("DBM Ready, speech volume: " + speechVolume);

            }
            catch (Exception e2)
            {
                debug("trySetup", DBMErrorLevel.EngineErrors, e2);
            }
            setupDone = true;
        }


        static void checkForNewAgro()
        {
            List<uint> diff = null;

            try
            {
                // Check for new entries in the enmity list
                diff = playerEntity.EnmityEntries.Select(x => x.ID).Except(_enmityCached.Select(y => y.ID)).ToList();

                if (diff.Any())
                {
                    // loop through the new enmity entries
                    diff.ForEach(delegate(uint ID)
                    {
                        // get the actual ActorEntity object for this enmity entry
                        IEnumerable<ActorEntity> tmp = mobList.Where(x => x.ID == ID);

                        if (tmp.Any())
                        {
                            ActorEntity currentEntity = tmp.First();
                            if (currentEntity != null && currentEntity.IsValid && currentEntity.HPCurrent > 0 && currentEntity.ClaimedByID != 0 && currentEntity.ClaimedByID < 0xE0000000)
                            {
                                if (!_agroList.Any() && implementationClass == null && learningHelperClass == null)
                                {
                                    // This is the first enmity entry we found. Start the encounter timer.
                                    started = DateTime.Now;
                                }

                                // make sure it's our own copy so it won't mysteriously disappear or change without our knowledge
                                currentEntity = cloneActorEntity(currentEntity);

                                _agroList.Add(currentEntity);


                                if (implementationClass != null)
                                {
                                    // send the new mobs to the encounter script
                                    try
                                    {
                                        debug("Agroed Mob: " + currentEntity.Name, DBMErrorLevel.Trace);
                                        implementationClass.onMobAgro(currentEntity);
                                    }
                                    catch (Exception e2)
                                    {
                                        debug("onMobAgro", getEngineVsEncounter(implementationClass), e2);
                                    }
                                    try
                                    {
                                        implementationClass.bossCheck(currentEntity);
                                    }
                                    catch (Exception e2)
                                    {
                                        debug("bossCheck", getEngineVsEncounter(implementationClass), e2);
                                    }

                                }
                                else
                                {
                                    // No encounter script loaded. Lets see if we have one that matches this mob
                                    Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                                    string mobName = rgx.Replace(currentEntity.Name, "");

                                    string dir = getZoneDirectory();

                                    if (useTestEncounter)
                                    {
                                        IEncounter inst = (IEncounter)(new TestEncounter());
                                        implementationClass = inst;

                                        setupNewClass(inst);


                                        debug("TestEncounter started", DBMErrorLevel.Trace);
                                        debug("Agroed Mob: " + currentEntity.Name, DBMErrorLevel.Trace);
                                    }
                                    else
                                    {
                                        if (File.Exists(dir + @"\" + mobName + ".cs"))
                                        {
                                            // Found one, lets load the script
                                            loadClassFile(dir + @"\" + mobName + ".cs");
                                            debug("Agroed Mob: " + currentEntity.Name, DBMErrorLevel.Trace);
                                        }
                                    }
                                }


                                if (learningHelperClass != null)
                                {
                                    // send the new mobs to the learning helper
                                    try
                                    {
                                        learningHelperClass.onMobAgro(currentEntity);
                                    }
                                    catch (Exception e2)
                                    {
                                        debug("learningHelper onMobAgro", getEngineVsEncounter(learningHelperClass), e2);
                                    }
                                    try
                                    {
                                        learningHelperClass.bossCheck(currentEntity);
                                    }
                                    catch (Exception e2)
                                    {
                                        debug("learningHelper bossCheck", getEngineVsEncounter(learningHelperClass), e2);
                                    }
                                }
                                else
                                {
                                    refreshIgnoreMobsList();

                                    // Start learning helper for a new encounter
                                    LearningHelper abc = new LearningHelper();
                                    setupNewClass((IEncounter)abc);
                                    learningHelperClass = abc;
                                    debug("LearningHelper started (" + currentEntity.Name + ")", DBMErrorLevel.Trace);
                                }

                            }
                        }
                    });
                }
            }
            catch (Exception e2)
            {
                debug("updateData check for new agro", DBMErrorLevel.EngineErrors, e2);
            }
        }

        static void checkForNewMobs()
        {
            try
            {

                // Check for new entries in the mob list
                
                //var anonDiff = mobList.Select(i => new { NPCID1 = i.NPCID1, NPCID2 = i.NPCID2 }).Except(_mobCached.Select(y => new { NPCID1 = y.NPCID1, NPCID2 = y.NPCID2 })).ToList();
                List<uint> anonDiff = mobList.Select(i => i.ID).Except(_mobCached.Select(y => y.ID)).ToList();


                if (anonDiff.Any())
                {
                    // loop through the new mob entries
                    foreach (var ID in anonDiff)
                    {
                        // get the actual ActorEntity object for this mob entry
                        //IEnumerable<ActorEntity> currentEntityIEnum = mobList.Where(x => x.NPCID1 == ID.NPCID1 && x.NPCID2 == ID.NPCID2);
                        IEnumerable<ActorEntity> currentEntityIEnum = mobList.Where(x => x.ID == ID);

                        if (currentEntityIEnum.Any())
                        {
                            ActorEntity currentEntity = currentEntityIEnum.First();
                            if (currentEntity != null && currentEntity.IsValid && currentEntity.HPCurrent > 0)
                            {
                                // make sure it's our own copy so it won't mysteriously disappear or change without our knowledge
                                currentEntity = cloneActorEntity(currentEntity);

                                _mobCached.Add(currentEntity);

                                if (learningHelperClass != null)
                                {
                                    // send the new mobs to the learning helper
                                    try
                                    {
                                        learningHelperClass.onMobAdded(currentEntity);
                                    }
                                    catch (Exception e2)
                                    {
                                        debug("learningHelper onMobAdded", getEngineVsEncounter(learningHelperClass), e2);
                                    }
                                }

                                if (implementationClass != null)
                                {
                                    // send the new mobs to the encounter script
                                    try
                                    {
                                        debug("Added Mob: " + currentEntity.Name + " ID: " + currentEntity.ID + " NPCID1: " + currentEntity.NPCID1 + " NPCID2: " + currentEntity.NPCID2, DBMErrorLevel.Trace);
                                        implementationClass.onMobAdded(currentEntity);
                                    }
                                    catch (Exception e2)
                                    {
                                        debug("onMobAdded", getEngineVsEncounter(implementationClass), e2);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e2)
            {
                debug("updateData check for new mobs", DBMErrorLevel.EngineErrors, e2);
            }
        }

        static void checkForRemovedMobs()
        {
            try
            {
                // Duplicate the list first before looping through it, or we'll get exceptions when we try to remove entries from the original list
                List<ActorEntity> tmpList = _mobCached.ToList();

                tmpList.ForEach(delegate(ActorEntity cachedEntity)
                {
                    handleRemoveEntryName = cachedEntity.Name;
                    //IEnumerable<ActorEntity> enumTmpEnt = _mobList.Where(x => x.NPCID1 == cachedEntity.NPCID1 && x.NPCID2 == cachedEntity.NPCID2);
                    IEnumerable<ActorEntity> enumTmpEnt = mobList.Where(x => x.ID == cachedEntity.ID);

                    if (!enumTmpEnt.Any())
                    {
                        handleRemoveEntry = 1;
                        handleRemove(cachedEntity, null);
                    }
                    else
                    {
                        ActorEntity currentEntity = enumTmpEnt.First();
                        if (currentEntity == null)
                        {
                            handleRemoveEntry = 2;
                            handleRemove(cachedEntity, currentEntity);
                        }
                        else
                        {
                            if (!currentEntity.IsValid)
                            {
                                handleRemoveEntry = 3;
                                handleRemove(cachedEntity, currentEntity);
                            }
                            else
                            {
                                // remove if dead
                                if (currentEntity.HPCurrent <= 0)
                                {
                                    handleRemoveEntry = 5;
                                    handleRemove(cachedEntity, currentEntity);
                                }
                                else
                                {
                                    // update our copy with the new info
                                    copyActorEntity(currentEntity, ref cachedEntity);
                                }
                            }
                        }
                    }
                    handleRemoveEntry = 0;
                    handleRemoveEntryName = "";
                });



            }
            catch (Exception e2)
            {
                debug("updateData check mob removal", DBMErrorLevel.EngineErrors, e2);
            }

        }

        static void checkForRemovedAgro()
        {
            List<uint> newEnmityList = playerEntity.EnmityEntries.Select(x => x.ID).ToList();

            try
            {
                // Duplicate the list first before looping through it, or we'll get exceptions when we try to remove entries from the original list
                List<ActorEntity> tmpList = _agroList.ToList();

                tmpList.ForEach(delegate(ActorEntity cachedEntity)
                {
                    handleRemoveEntryName = cachedEntity.Name;
                    //IEnumerable<ActorEntity> enumTmpEnt = _mobList.Where(x => x.NPCID1 == cachedEntity.NPCID1 && x.NPCID2 == cachedEntity.NPCID2);
                    IEnumerable<ActorEntity> enumTmpEnt = _mobList.Where(x => x.ID == cachedEntity.ID);
                    if (!enumTmpEnt.Any())
                    {
                        handleRemoveEntry = 1;
                        handleRemoveAgro(cachedEntity, null);
                    }
                    else
                    {
                        ActorEntity currentEntity = enumTmpEnt.First();
                        if (currentEntity == null)
                        {
                            handleRemoveEntry = 2;
                            handleRemoveAgro(cachedEntity, currentEntity);
                        }
                        else
                        {
                            if (!currentEntity.IsValid)
                            {
                                handleRemoveEntry = 3;
                                handleRemoveAgro(cachedEntity, currentEntity);
                            }
                            else
                            {
                                if (currentEntity.ClaimedByID == 0 || currentEntity.ClaimedByID >= 0xE0000000)
                                //if ((!tmpActor.IsClaimed && tmpActor.ClaimedByID == 0))
                                {
                                    handleRemoveEntry = 4;
                                    handleRemoveAgro(cachedEntity, currentEntity);
                                }
                                else
                                {
                                    // remove if dead
                                    if (currentEntity.HPCurrent <= 0)
                                    {
                                        handleRemoveEntry = 5;
                                        handleRemoveAgro(cachedEntity, currentEntity);
                                    }
                                    else
                                    {
                                        // ClaimedByID seems unreliable sometimes, and IsClaimed never seems to work,
                                        // so instead, check to see if the mob is full health but NOT in the agroList anymore
                                        // Also make sure we're alive, since the agro list is temporarily empty when dead.
                                        if (!newEnmityList.Contains(currentEntity.ID) && currentEntity.HPCurrent >= currentEntity.HPMax && CurrentUser.HPCurrent > 0)
                                        {
                                            handleRemoveEntry = 6;
                                            handleRemoveAgro(cachedEntity, currentEntity);
                                        }
                                        else
                                        {
                                            // update our copy with the new info
                                            copyActorEntity(currentEntity, ref cachedEntity);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    handleRemoveEntry = 0;
                    handleRemoveEntryName = "";
                });



            }
            catch (Exception e2)
            {
                debug("updateData check agro mob removal", DBMErrorLevel.EngineErrors, e2);
            }

        }

        static void updateData(object sender, ElapsedEventArgs e)
        {
            List<uint> diff = new List<uint>();

            lock (updateLock)
            {
                if (inUpdate || playerEntity == null)
                {
                    return;
                }
                if (!setupDone)
                {
                    trySetup();
                    return;
                }
                // this ensures that we don't try to process more than one updateData at a time on slower computers. 
                inUpdate = true;
            }

            lock (accessControl)
            {

                checkForNewMobs();
                checkForNewAgro();
                checkForRemovedMobs();
                checkForRemovedAgro();

                try
                {
                    // cache the enmity list so we can compare next time to detect changes
                    _enmityCached = playerEntity.EnmityEntries.ToList();
                }
                catch (Exception e2)
                {
                    debug("updateData cache lists", DBMErrorLevel.EngineErrors, e2);
                }

            }
            inUpdate = false;
        }

        static void tickTimerEvent(object sender, ElapsedEventArgs e)
        {
            lock (tickLock)
            {
                if (inTick || playerEntity == null || mobList == null || !setupDone)
                {
                    return;
                }
                // this ensures that we don't try to process more than one tick at a time on slower computers. Not likely to happen though
                inTick = true;
            }

            lock (accessControl)
            {

                // call the tick events for both the learning helper and the encounter script controllers
                if (learningHelperClass != null)
                {
                    try
                    {
                        learningHelperClass.tick();
                        learningHelperClass.onTick();
                    }
                    catch (Exception e2)
                    {
                        debug("learningHelper tick", getEngineVsEncounter(learningHelperClass), e2);
                    }
                }

                if (implementationClass != null)
                {
                    try
                    {
                        implementationClass.tick();
                        implementationClass.onTick();
                    }
                    catch (Exception e2)
                    {
                        debug("tick", getEngineVsEncounter(implementationClass), e2);
                    }
                }
            }

            inTick = false;
        }


        public static void handleRemove(ActorEntity cachedEntity, ActorEntity currentEntity)
        {
            //debug("Removed Mob: " + currentEntity.Name + " (" + handleRemoveEntry + ", " + (currentEntity.IsValid ? "valid" : "not valid") + ", " + (currentEntity.IsClaimed ? "claimed" : "not claimed") + ", by id: " + currentEntity.ClaimedByID + ")", DBMErrorLevel.Trace);
                                       
            _mobCached.Remove(cachedEntity);

            if (learningHelperClass != null)
            {
                try
                {
                    learningHelperClass.onMobRemoved(cachedEntity);
                }
                catch (Exception e2)
                {
                    debug("learningHelper onMobRemoved", getEngineVsEncounter(learningHelperClass), e2);
                }
            }

            if (implementationClass != null)
            {
                try
                {
                    if (currentEntity == null)
                    {
                        debug("Removed Mob: " + cachedEntity.Name + " ID: " + cachedEntity.ID + " NPCID1: " + cachedEntity.NPCID1 + " NPCID2: " + cachedEntity.NPCID2 + " (" + handleRemoveEntry + ", null)", DBMErrorLevel.Trace);
                    }
                    else
                    {
                        debug("Removed Mob: " + currentEntity.Name + " ID: " + currentEntity.ID + " NPCID1: " + currentEntity.NPCID1 + " NPCID2: " + currentEntity.NPCID2 + " (" + handleRemoveEntry + ", " + (currentEntity.IsValid ? "valid" : "not valid") + ")", DBMErrorLevel.Trace);
                    }
                    implementationClass.onMobRemoved(cachedEntity);
                }
                catch (Exception e2)
                {
                    debug("onMobRemoved", getEngineVsEncounter(implementationClass), e2);
                }
            }
        }

        public static void handleRemoveAgro(ActorEntity cachedEntity, ActorEntity currentEntity)
        {
            if (currentEntity == null)
            {
                debug("Removed Agro: " + cachedEntity.Name + " (" + handleRemoveEntry + ", null)", DBMErrorLevel.Trace);
            }
            else
            {
                debug("Removed Agro: " + currentEntity.Name + " (" + handleRemoveEntry + ", " + (currentEntity.IsValid ? "valid" : "not valid") + ", " + (currentEntity.IsClaimed ? "claimed" : "not claimed") + ", by id: " + currentEntity.ClaimedByID + ")", DBMErrorLevel.Trace);
            }
            _agroList.Remove(cachedEntity);

            if (learningHelperClass != null)
            {
                try
                {
                    learningHelperClass.onAgroRemoved(cachedEntity);
                }
                catch (Exception e2)
                {
                    debug("learningHelper onAgroRemoved", getEngineVsEncounter(learningHelperClass), e2);
                }
            }

            if (implementationClass != null)
            {
                try
                {
                    implementationClass.onAgroRemoved(cachedEntity);
                }
                catch (Exception e2)
                {
                    debug("onAgroRemoved", getEngineVsEncounter(implementationClass), e2);
                }
            }


            if (encounterStarted && !_agroList.Any())
            {
                endEncounter();
            }
        }


        public static void endEncounter()
        {
            if (learningHelperClass != null)
            {
                try
                {
                    learningHelperClass.endEncounter();
                    learningHelperClass.onEndEncounter();
                }
                catch (Exception e2)
                {
                    debug("learningHelper endEncounter", getEngineVsEncounter(learningHelperClass), e2);
                }
                learningHelperClass = null;
            }

            if (implementationClass != null)
            {
                debug("endEncounter removed " + handleRemoveEntryName + "(" + handleRemoveEntry + ")", DBMErrorLevel.Trace);
                try
                {
                    implementationClass.endEncounter();
                    implementationClass.onEndEncounter();
                }
                catch (Exception e2)
                {
                    debug("endEncounter", getEngineVsEncounter(implementationClass), e2);
                }
                implementationClass = null;
            }
            _agroList.Clear();
            _enmityCached.Clear();
            _mobCached.Clear();
            encounterStarted = false;
        }

        public static void setupNewClass(IEncounter inst)
        {
            encounterStarted = true;
            try
            {
                inst.onStartEncounter();
            }
            catch (Exception e2)
            {
                debug("onStartEncounter", getEngineVsEncounter(inst), e2);
            }

            try
            {
                inst.setPhase(1);
            }
            catch (Exception e2)
            {
                debug("setPhase", getEngineVsEncounter(inst), e2);
            }


            foreach (ActorEntity tmpEnt in _agroList)
            {
                try
                {
                    inst.onMobAgro(tmpEnt);
                }
                catch (Exception e2)
                {
                    debug("onMobAgro", getEngineVsEncounter(inst), e2);
                }
                try
                {
                    inst.bossCheck(tmpEnt);
                }
                catch (Exception e2)
                {
                    debug("bossCheck", getEngineVsEncounter(inst), e2);
                }
            }

            foreach (ActorEntity tmpEnt in _mobCached)
            {
                try
                {
                    if (inst == implementationClass)
                    {
                        debug("Added Mob (Setup): " + tmpEnt.Name + " ID: " + tmpEnt.ID + " NPCID1: " + tmpEnt.NPCID1 + " NPCID2: " + tmpEnt.NPCID2, DBMErrorLevel.Trace);
                    }
                    inst.onMobAdded(tmpEnt);
                }
                catch (Exception e2)
                {
                    debug("onMobAdded", getEngineVsEncounter(inst), e2);
                }
            }
        }

        public static void OnNewChatLogEntry(object sender, ChatLogEntryEvent chatLogEntryEvent)
        {
            try
            {
                // delegate event from chat log, not required to subsribe
                // this updates 100 times a second and only sends a line when it gets a new one
                if (sender == null || chatLogEntryEvent == null || chatLogEntryEvent.ChatLogEntry == null || !setupDone)
                {
                    return;
                }

                string chatLogEntry = logStripper.Replace(chatLogEntryEvent.ChatLogEntry.Line, "");

                if (testString.IsMatch(chatLogEntry))
                {
                    if (!encounterStarted)
                    {
                        tts("DBM Ready");
                    }
                    else
                    {
                        foreach (ActorEntity pActor in _agroList)
                        {
                            debug("Dump Mob: " + pActor.Name + " (" + (pActor.IsValid ? "valid" : "not valid") + ", " + (pActor.IsClaimed ? "claimed" : "not claimed") + ", by id: " + pActor.ClaimedByID + ")", DBMErrorLevel.Trace);
                        }
                    }
                }


                if (!encounterStarted)
                {
                    return;
                }

                if (endString.IsMatch(chatLogEntry))
                {
                    foreach (ActorEntity pActor in _agroList)
                    {
                        handleRemoveEntry = 99;
                        handleRemoveAgro(pActor, pActor);
                    }
                }

                lock (accessControl)
                {

                    dispatchChatLogEntry(chatLogEntry);


                    if (youGain.regex != null && youGain.regex.IsMatch(chatLogEntry))
                    {
                        chatLogEntry = youGain.regex.Replace(chatLogEntry, playerEntity.Name + youGain.replacement);
                        dispatchChatLogEntry(chatLogEntry);
                    }
                    else if (youLose.regex != null && youLose.regex.IsMatch(chatLogEntry))
                    {
                        chatLogEntry = youLose.regex.Replace(chatLogEntry, playerEntity.Name + youLose.replacement);
                        dispatchChatLogEntry(chatLogEntry);
                    }
                    else if (youSuffer.regex != null && youSuffer.regex.IsMatch(chatLogEntry))
                    {
                        chatLogEntry = youSuffer.regex.Replace(chatLogEntry, playerEntity.Name + youSuffer.replacement);
                        dispatchChatLogEntry(chatLogEntry);
                    }
                    else if (youRecover.regex != null && youRecover.regex.IsMatch(chatLogEntry))
                    {
                        chatLogEntry = youRecover.regex.Replace(chatLogEntry, playerEntity.Name + youRecover.replacement);
                        dispatchChatLogEntry(chatLogEntry);
                    }

                    if (sealedOff != null && (DateTime.Now - started).Duration() > TimeSpan.FromSeconds(1) && sealedOff.IsMatch(chatLogEntry))
                    {
                        handleRemoveEntryName = "Sealed Off";
                        endEncounter();
                        handleRemoveEntryName = "";
                    }
                }
            }
            catch(Exception ex)
            {
                debug("OnNewChatLogEntry", DBMErrorLevel.EngineErrors, ex);
            }
        }

        public static void dispatchChatLogEntry(string chatLogEntry)
        {
            if (learningHelperClass != null)
            {
                try
                {
                    learningHelperClass.processChatLine(chatLogEntry);
                    learningHelperClass.onNewChatLine(chatLogEntry);
                }
                catch (Exception e2)
                {
                    debug("learningHelper processChatLine", getEngineVsEncounter(learningHelperClass), e2);
                }
            }

            if (implementationClass != null)
            {
                try
                {
                    implementationClass.processChatLine(chatLogEntry);
                    implementationClass.onNewChatLine(chatLogEntry);
                }
                catch (Exception e2)
                {
                    debug("processChatLine", getEngineVsEncounter(implementationClass), e2);
                }
            }
        }




        #region utility

        public static string getZoneDirectory()
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string zoneName = rgx.Replace(zone.English, "");

            string dir = Constants.BaseDirectory + @"\zones\" + Constants.GameLanguage + @"\" + zone.Index;

            encounterZonePath = dir;

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(dir + "\\" + zoneName + ".txt", zone.English);
            }

            return dir;
        }

        public static void refreshIgnoreMobsList()
        {
            ignoreMobs = new List<string>();

            string ignoreFile = getZoneDirectory() + Path.PathSeparator + "ignore.txt";

            if (File.Exists(ignoreFile))
            {
                string[] lines = File.ReadAllLines(ignoreFile);
                foreach (string line in lines)
                {
                    if (line.Trim().Length > 0)
                    {
                        ignoreMobs.Add(line.Trim());
                    }
                }
            }
        }

        private static DBMErrorLevel getEngineVsEncounter(IEncounter controller)
        {
            if (controller == null)
            {
                return DBMErrorLevel.EngineErrors;
            }
            return (controller.inController() ? DBMErrorLevel.EngineErrors : DBMErrorLevel.EncounterErrors);
        }

        public static void debug(string message, DBMErrorLevel level = DBMErrorLevel.EncounterErrors, Exception ex = null)
        {
            if (level >= errorLevel)
            {
                var timeStampColor = Settings.Default.TimeStampColor.ToString();
                DateTime now = DateTime.Now.ToUniversalTime();
                string debugInfo = "): ";
                string timeStamp = "[" + now.ToShortDateString() + " " + now.Hour + ":" + now.Minute + ":" + now.Second + ":" + now.Millisecond + "] ";
                string line = "";

                if (ex != null)
                {
                    StackTrace st = new StackTrace(ex, true);
                    // Get the top stack frame
                    StackFrame frame = st.GetFrame(0);

                    debugInfo = ":" + frame.GetFileName() + ":" + frame.GetMethod() + ":" + frame.GetFileLineNumber() + ":" + frame.GetFileColumnNumber() + ": " + ex.Message + "): ";
                }


                line = timeStamp + "(" + Enum.GetName(typeof(DBMErrorLevel),level) + debugInfo + message;

                File.AppendAllText(debugLogPath, line + Environment.NewLine);

                FFXIVAPP.Common.Constants.FD.AppendFlow(timeStamp, "", line, new[]
                {
                    timeStampColor, "#FFFFFF"
                }, MainView.View.ChatLogFD._FDR);
            }
        }

        public static void tts(string toRead)
        {

            try
            {

                lock (speakingLock)
                {
                    while (speaking)
                    {
                        // wait for previous speech to finish
                    }
                    speaking = true;
                }

                try
                {
                    SpeechSynthesizer m_speechSynth = new SpeechSynthesizer();
                    m_speechSynth.Volume = speechVolume;
                    //m_speechSynth.Rate = 2;

                    MemoryStream waveStream = new MemoryStream();
                    m_speechSynth.SetOutputToWaveStream(waveStream);
                    m_speechSynth.SpeakAsync(toRead);
                    m_speechSynth.SpeakCompleted += delegate
                    {
                        try
                        {
                            m_speechSynth.SetOutputToNull();
                            m_speechSynth.Dispose();
                            m_speechSynth = null;

                            speaking = false;

                            waveStream.Position = 0; // reset counter to start


                            VoiceThroughNetAudio netAudio = new VoiceThroughNetAudio(waveStream, "WAV");
                            //netAudio.Volume = 1;
                            /*
                            netAudio.PlaybackStop += delegate
                            {
                                try
                                {
                                    netAudio.Dispose();
                                }
                                catch { }
                            };
                            */

                            lock (TTSQueue)
                            {
                                TTSQueue.Add(netAudio);
                            }
                        }
                        catch(Exception ex)
                        {
                            debug("SpeakCompleted", DBMErrorLevel.EngineErrors, ex);
                        }
                    };
                }
                catch(Exception ex)
                {
                    debug("tts", DBMErrorLevel.EngineErrors, ex);
                }
            }
            catch
            {

            }


        }

        
        public static void TTSThread()
        {
            VoiceThroughNetAudio toPlay;

            while (true)
            {
                bool any = false;


                try
                {
                    lock (TTSQueue)
                    {
                            any = TTSQueue.Any();
                    }
                }
                catch (Exception ex)
                {
                    debug("TTSThread 1", DBMErrorLevel.EngineErrors, ex);
                }

                if (any) // something in queue
                {
                    try
                    {
                        //MessageBox.Show("test");
                        lock (TTSQueue)
                        {
                            if (TTSQueue.Any()) // could have changed between our locks
                            {
                                toPlay = TTSQueue.First();
                                TTSQueue.Remove(toPlay);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (toPlay.TotalDuration > TimeSpan.FromSeconds(0.25))
                        {
                            toPlay.Play();

                            DateTime started = DateTime.Now;

                            while (toPlay.TimePosition < toPlay.TotalDuration)
                            {
                                Thread.Sleep(50);
                                if ((DateTime.Now - started).Duration() > toPlay.TotalDuration + TimeSpan.FromSeconds(2))
                                {
                                    break;
                                }
                            }

                            toPlay.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        debug("TTSThread 2", DBMErrorLevel.EngineErrors, ex);
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }



        public static ActorEntity cloneActorEntity(ActorEntity toClone)
        {
            ActorEntity newEntity = new ActorEntity();

            copyActorEntity(toClone, ref newEntity);

            return newEntity;
        }

        public static void copyActorEntity(ActorEntity fromEntity, ref ActorEntity toEntity)
        {
            ActorEntity tmpEnt = fromEntity;
            ActorEntity tmpActor = toEntity;

            tmpActor.ActionStatus = tmpEnt.ActionStatus;
            tmpActor.CastingID = tmpEnt.CastingID;
            tmpActor.CastingProgress = tmpEnt.CastingProgress;
            tmpActor.CastingTargetID = tmpEnt.CastingTargetID;
            tmpActor.CastingTime = tmpEnt.CastingTime;
            tmpActor.ClaimedByID = tmpEnt.ClaimedByID;
            tmpActor.Coordinate = tmpEnt.Coordinate;
            tmpActor.CPCurrent = tmpEnt.CPCurrent;
            tmpActor.CPMax = tmpEnt.CPMax;
            tmpActor.Distance = tmpEnt.Distance;
            tmpActor.Fate = tmpEnt.Fate;
            tmpActor.GatheringInvisible = tmpEnt.GatheringInvisible;
            tmpActor.GatheringStatus = tmpEnt.GatheringStatus;
            tmpActor.GPCurrent = tmpEnt.GPCurrent;
            tmpActor.GPMax = tmpEnt.GPMax;
            tmpActor.GrandCompany = tmpEnt.GrandCompany;
            tmpActor.GrandCompanyRank = tmpEnt.GrandCompanyRank;
            tmpActor.Heading = tmpEnt.Heading;
            tmpActor.HPCurrent = tmpEnt.HPCurrent;
            tmpActor.HPMax = tmpEnt.HPMax;
            tmpActor.Icon = tmpEnt.Icon;
            tmpActor.ID = tmpEnt.ID;
            tmpActor.IsCasting = tmpEnt.IsCasting;
            tmpActor.IsGM = tmpEnt.IsGM;
            tmpActor.Job = tmpEnt.Job;
            tmpActor.Level = tmpEnt.Level;
            tmpActor.MapIndex = tmpEnt.MapIndex;
            tmpActor.ModelID = tmpEnt.ModelID;
            tmpActor.MPCurrent = tmpEnt.MPCurrent;
            tmpActor.MPMax = tmpEnt.MPMax;
            tmpActor.Name = tmpEnt.Name;
            tmpActor.NPCID1 = tmpEnt.NPCID1;
            tmpActor.NPCID2 = tmpEnt.NPCID2;
            tmpActor.OwnerID = tmpEnt.OwnerID;
            tmpActor.Race = tmpEnt.Race;
            tmpActor.Sex = tmpEnt.Sex;
            tmpActor.Status = tmpEnt.Status;
            tmpActor.TargetID = tmpEnt.TargetID;
            tmpActor.TargetType = tmpEnt.TargetType;
            tmpActor.Title = tmpEnt.Title;
            tmpActor.TPCurrent = tmpEnt.TPCurrent;
            tmpActor.TPMax = tmpEnt.TPMax;
            tmpActor.Type = tmpEnt.Type;
            tmpActor.X = tmpEnt.X;
            tmpActor.Y = tmpEnt.Y;
            tmpActor.Z = tmpEnt.Z;


            tmpActor.StatusEntries.Clear();

            foreach (StatusEntry se in tmpEnt.StatusEntries)
            {
                tmpActor.StatusEntries.Add(se);
            }

        }

        #endregion

        #region DynamicClassLoading

        public static void loadClassFile(string path)
        {
            try
            {

                string language = CSharpCodeProvider.GetLanguageFromExtension(
                                     Path.GetExtension(path));

                CodeDomProvider cdp = CodeDomProvider.CreateProvider(language);
                CompilerParameters cp = new CompilerParameters();

                cp.GenerateExecutable = false;
                cp.GenerateInMemory = true;

                cp.TreatWarningsAsErrors = false;
                cp.IncludeDebugInformation = false;

                string extAssembly;



                cp.ReferencedAssemblies.Add(typeof(HtmlAgilityPack.Crc32).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(MahApps.Metro.Accent).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly.Location);
                //cp.ReferencedAssemblies.Add(typeof(Newtonsoft.Json.ConstructorHandling).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(NLog.GlobalDiagnosticsContext).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Windows.Media.AdornerHitTestResult).Assembly.Location); // PresentationCore
                cp.ReferencedAssemblies.Add(typeof(System.Windows.Controls.AccessText).Assembly.Location); // PresentationFramework
                cp.ReferencedAssemblies.Add(typeof(System.Uri).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Collections.Generic.KeyNotFoundException).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Linq.Enumerable).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Text.Decoder).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Threading.Tasks.Task).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Windows.MessageBox).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.ComponentModel.AddingNewEventArgs).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Configuration.ApplicationScopedSettingAttribute).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Diagnostics.EventSchemaTraceListener).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Drawing.Color).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Windows.Interactivity.Behavior).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Xaml.AmbientPropertyValue).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Xml.XmlAttribute).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.Xml.Linq.Extensions).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(System.IO.FileFormatException).Assembly.Location);

                cp.ReferencedAssemblies.Add(typeof(FFXIVAPP.Common.Helpers.ZoneHelper).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(FFXIVAPP.Common.Core.Memory.StatusEntry).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(FFXIVAPP.IPluginInterface.IPlugin).Assembly.Location);



                extAssembly = Path.Combine(
                             Constants.AppDirectory,
                              "FFXIVAPP.Client.exe");
                cp.ReferencedAssemblies.Add(extAssembly);

                extAssembly = Path.Combine(
                             Constants.AppDirectory,
                              "FFXIVAPP.Common.dll");
                cp.ReferencedAssemblies.Add(extAssembly);

                extAssembly = Path.Combine(
                             Constants.AppDirectory,
                              "FFXIVAPP.IPluginInterface.dll");
                cp.ReferencedAssemblies.Add(extAssembly);


                extAssembly = Path.Combine(
                             Constants.BaseDirectory,
                              AssemblyHelper.Name + ".dll");
                cp.ReferencedAssemblies.Add(extAssembly);




                CompilerResults cr = cdp.CompileAssemblyFromFile(cp, path);

                if (cr.Errors.HasErrors)
                {
                    StringBuilder errors = new StringBuilder();
                    string filename = Path.GetFileName(path);
                    foreach (CompilerError err in cr.Errors)
                    {
                        errors.Append(string.Format("\r\n{0}({1},{2}): {3}: {4}",
                                    filename, err.Line, err.Column,
                                    err.ErrorNumber, err.ErrorText));
                    }
                    string str = "Error loading script\r\n" + errors.ToString();
                    throw new ApplicationException(str);
                }

                IEncounter inst = findEntryPoint(cr.CompiledAssembly);
                implementationClass = inst;
                setupNewClass(inst);
                debug("Encounter started (" + path + ")", DBMErrorLevel.Trace);

                if (learningHelperClass != null)
                {
                    try
                    {
                        learningHelperClass.endEncounter();
                        learningHelperClass.onEndEncounter();
                    }
                    catch (Exception e2)
                    {
                        debug("learningHelper processChatLine", getEngineVsEncounter(learningHelperClass), e2);
                    }
                    learningHelperClass = null;
                }

                try
                {
                    refreshIgnoreMobsList();

                    // Start learning helper for a new encounter
                    LearningHelper abc = new LearningHelper();
                    setupNewClass((IEncounter)abc);
                    learningHelperClass = abc;
                    debug("LearningHelper started", DBMErrorLevel.Trace);
                }
                catch (Exception e2)
                {
                    debug("learningHelper setupNewClass", getEngineVsEncounter(learningHelperClass), e2);
                }

            }
            catch (Exception ex)
            {
                debug("Error loading script (" + path + ")", DBMErrorLevel.EncounterErrors, ex);
            }

        }

        private static IEncounter findEntryPoint(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsClass || type.IsNotPublic) continue;
                Type[] interfaces = type.GetInterfaces();
                if (((IList<Type>)interfaces).Contains(typeof(IEncounter)))
                {
                    IEncounter iEncounter = (IEncounter)Activator.CreateInstance(type);
                    return iEncounter;
                }
            }

            return null;
        }

        private static Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // this is absurdly expensive...don't do this more than once, or load the assembly file in a more efficient way
            // also, if the code you're using to compile the CodeDom assembly doesn't/hasn't used the referenced assembly yet, this won't work
            // and you should use Assembly.Load(...)
            foreach (Assembly @assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (@assembly.FullName.Equals(args.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return @assembly;
                }
            }
            return null;
        }
        #endregion


    }
}
