
using System;
using System.Linq;
using FFXIVAPP.IPluginInterface.Events;
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
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Speech.AudioFormat;
using FFXIVAPP.Common.Core.Network;
using FFXIVAPP.Common.Core.Constant;
using FFXIVAPP.Memory.Models;
using FFXIVAPP.Memory.Core;
using FFXIVAPP.Memory.Helpers;

namespace FFXIVDBM.Plugin
{
    public enum MarkerTypeEnum
    {
        Ground = 1,
        Target = 2,
    }

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


        internal static ConcurrentDictionary<uint, ActorEntity> _mobList = new ConcurrentDictionary<uint, ActorEntity>();
        public static ConcurrentDictionary<uint, ActorEntity> mobList
        {
            get
            {
                return _mobCached;
            }
            set
            {
                _mobList = value;
            }
        }


        private static ConcurrentDictionary<uint, ActorEntity> _npcList = new ConcurrentDictionary<uint, ActorEntity>();
        public static ConcurrentDictionary<uint, ActorEntity> npcList
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


        private static ConcurrentDictionary<uint, ActorEntity> _pcEntities = new ConcurrentDictionary<uint, ActorEntity>();
        public static ConcurrentDictionary<uint, ActorEntity> pcEntities
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

        private static ConcurrentDictionary<uint, PartyEntity> _partyList = new ConcurrentDictionary<uint, PartyEntity>();
        public static ConcurrentDictionary<uint, PartyEntity> partyList
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
        private static TargetEntity _targetEntity = null;
        public static TargetEntity targetEntity
        {
            get
            {
                return _targetEntity;
            }
            set
            {
                _targetEntity = value;
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
                if (value == null || value.Index == 0)
                {
                    return;
                }

                bool newZone = false;
                try
                {
                    if (_zone == null || (value != null &&_zone.Index != value.Index))
                    {
                        newZone = true;
                    }

                }
                catch (Exception e)
                {
                    debug("zone property set error 1: ", DBMErrorLevel.EngineErrors, e);
                }

                _zone = value;

                try
                {
                    if (newZone && _zone != null)
                    {
                        handleRemoveEntryName = "Zone change";
                        endEncounter();
                        handleRemoveEntryName = "";
                        debug("Entered zone: " + _zone.Index, DBMErrorLevel.Notice);
                        compileScripts();
                    }
                }
                catch (Exception e2)
                {
                    debug("zone property set error 2: ", DBMErrorLevel.EngineErrors, e2);
                }
            }
        }

        #endregion

        private static CompilerParameters cp;
        private static CodeDomProvider cdp;

        private static IEncounter implementationClass = null;
        private static IEncounter learningHelperClass = null;

        private static ConcurrentDictionary<uint, ActorEntity> _mobCached = null;
        private static List<EnmityEntry> _enmityCached = null;
        private static System.Timers.Timer tickTimer = null;
        private static System.Timers.Timer enmityTimer = null;

        private static Dictionary<string, Assembly> encounters = new Dictionary<string,Assembly>();
        public static Dictionary<string, DateTime> classModified = new Dictionary<string,DateTime>();

        private static string debugLogPath = "";
        private static string encounterDebugLogPath = "";

        public static bool encounterStarted = false;

        public static DateTime started = DateTime.Now;

        public static string encounterZonePath = "";

        public static Thread ttsThread;

        public static Regex logStripper = new Regex(@" ⇒ ");


        public static List<VoiceThroughNetAudio> TTSQueue = new List<VoiceThroughNetAudio>();
        public static List<DirectSoundOut> TTSQueueDSO = new List<DirectSoundOut>();
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
        private static Dictionary<string, Regex> noLongerSealedDict = new Dictionary<string, Regex>();



        private static Replacements youGain = null;
        private static Replacements youLose = null;
        private static Replacements youSuffer = null;
        private static Replacements youRecover = null;
        private static Regex sealedOff = null;
        private static Regex noLongerSealed = null;

        private static bool isSealedOff = false;

        private static Regex testString = null;
        private static Regex repeatString = null;
        private static Regex endString = null;

        public static List<string> ignoreMobs;
        static DateTime firstAgro = DateTime.MinValue;

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
                _mobCached = new ConcurrentDictionary<uint, ActorEntity>();



                youGainDict["English"] = new Replacements(new Regex(@"^( ⇒ )?You gain the effect of "), " gains the effect of ");
                youLoseDict["English"] = new Replacements(new Regex(@"^( ⇒ )?You lose the effect of "), " loses the effect of ");
                youSufferDict["English"] = new Replacements(new Regex(@"^( ⇒ )?You suffer the effect of "), " suffers the effect of ");
                youRecoverDict["English"] = new Replacements(new Regex(@"^( ⇒ )?You recover from the effect of "), " recovers from the effect of ");
                sealedOffDict["English"] = new Regex(@" will be sealed off in 15 seconds\!");
                noLongerSealedDict["English"] = new Regex(@" is no longer sealed\!");

                // please send corrections for the replacements if needed
                youGainDict["French"] = new Replacements(new Regex(@"^( ⇒ )?Vous bénéficiez? de l'effet "), " bénéficiez de l'effet ");
                youLoseDict["French"] = new Replacements(new Regex(@"^( ⇒ )?Vous perd(ez?)? l'effet "), " perdez l'effet ");
                youSufferDict["French"] = new Replacements(new Regex(@"^( ⇒ )?Vous subi(t|ssez?) l'effet "), " subissez l'effet ");
                youRecoverDict["French"] = new Replacements(new Regex(@"^( ⇒ )?Vous (perd(ez?)?|ne subi(t|ssez?)) plus l'effet "), " perdez subissez plus l'effet ");
                sealedOffDict["French"] = new Regex(@" will be sealed off in 15 seconds\!"); // TODO: Find out the French translation for this
                noLongerSealedDict["French"] = new Regex(@" is no longer sealed\!"); // TODO: Find out the French translation for this

                // Japanese log lines already use your own name by the looks of things from parser plugin regex file
                // If this isn't correct, please tell me the proper lines to match
                youGainDict["Japanese"] = null;
                youLoseDict["Japanese"] = null;
                youSufferDict["Japanese"] = null;
                youRecoverDict["Japanese"] = null;
                sealedOffDict["Japanese"] = new Regex(@" will be sealed off in 15 seconds\!"); // TODO: Find out the Japanese translation for this
                noLongerSealedDict["Japanese"] = new Regex(@" is no longer sealed\!"); // TODO: Find out the Japanese translation for this

                // I need working regex strings for several of these for the German language version
                // please send corrections for the replacements if needed
                youGainDict["German"] = new Replacements(new Regex(@"^( ⇒ )?(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<target>.+) erh lt(st| den) Effekt von "), " erh lt(st| den) Effekt von ");
                youLoseDict["German"] = null; // new Replacements(new Regex(@""), "");
                youSufferDict["German"] = null; // new Replacements(new Regex(@""), "");
                youRecoverDict["German"] = null; // new Replacements(new Regex(@""), "");
                sealedOffDict["German"] = new Regex(@" will be sealed off in 15 seconds\!"); // TODO: Find out the Japanese translation for this
                noLongerSealedDict["German"] = new Regex(@" is no longer sealed\!"); // TODO: Find out the Japanese translation for this

                // Setup parameters for dynamic class loading
                setupClassAssembly();

                // Setup text-to-speech queue system
                ttsThread = new Thread(TTSThread);
                ttsThread.Start();

                // send ticks 20 times per second to the encounter controllers
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
            if (Constants.GameLanguage == null || CurrentUser == null || _targetEntity == null || CurrentUser.HPMax == 0 || zone.Index == 0)
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
                if (noLongerSealedDict.ContainsKey(Constants.GameLanguage))
                {
                    noLongerSealed = noLongerSealedDict[Constants.GameLanguage];
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
                repeatString = new Regex(@"\@(.*)");
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

                compileScripts();

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

                    foreach (uint ID in diff)
                    {
                        ActorEntity currentEntity = null;
                        if (_mobCached.ContainsKey(ID))
                        {
                            currentEntity = _mobCached[ID];
                        }
                        else if (_mobList.ContainsKey(ID))
                        {
                            currentEntity = _mobList[ID];
                        }

                        if (currentEntity != null)
                        {
                            if (currentEntity.IsValid && currentEntity.HPCurrent > 0) // && currentEntity.ClaimedByID != 0 && currentEntity.ClaimedByID < 0xE0000000)
                            {
                                if (!_agroList.Any() && implementationClass == null && learningHelperClass == null)
                                {
                                    // This is the first enmity entry we found. Start the encounter timer.
                                    started = DateTime.Now;
                                }

                                // make sure it's our own copy so it won't mysteriously disappear or change without our knowledge
                                currentEntity = cloneActorEntity(currentEntity);

                                if (_agroList.Any())
                                {
                                    firstAgro = DateTime.Now;
                                }

                                _agroList.Add(currentEntity);


                                if (implementationClass != null)
                                {
                                    // send the new mobs to the encounter script
                                    try
                                    {
                                        debug("Agroed Mob: " + currentEntity.Name, DBMErrorLevel.Trace);
                                        inController = false;
                                        implementationClass.onMobAgro(currentEntity);
                                        inController = true;
                                    }
                                    catch (Exception e2)
                                    {
                                        debug("onMobAgro", getEngineVsEncounter(implementationClass), e2);
                                        inController = true;
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
                                        // No encounter script loaded. Lets see if we have one that matches this mob
                                        Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                                        string mobName = rgx.Replace(currentEntity.Name, "");

                                        string dir = getScriptsDirectory();

                                        checkRecompile(currentEntity.Name);

                                        if (encounters.ContainsKey(mobName))
                                        {
                                            IEncounter inst = findEntryPoint(encounters[mobName]);
                                            implementationClass = inst;

                                            if (implementationClass != null)
                                            {
                                                setupNewClass(inst);

                                                debug("Encounter started (" + dir + @"\" + mobName + ".cs)", DBMErrorLevel.Trace);
                                                debug("Agroed Mob: " + currentEntity.Name, DBMErrorLevel.Trace);

                                                resetLearningHelper();
                                            }
                                            else
                                            {
                                                debug("Couldn't find entry point in class: " + dir + @"\" + mobName + ".cs", DBMErrorLevel.EngineErrors);
                                            }
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
                            else
                            {
                                debug("New agro invalid: IsValid: " + currentEntity.IsValid + ", HPCurrent: " + currentEntity.HPCurrent + ", ClaimedByID: " + currentEntity.ClaimedByID, DBMErrorLevel.Notice);
                            }
                        }
                        else
                        {
                            debug("New agro not found (ID: " + ID + ")", DBMErrorLevel.Notice);
                        }
                    }
                }
            }
            catch (Exception e2)
            {
                debug("updateData check for new agro", DBMErrorLevel.EngineErrors, e2);
            }
        }

        static bool inController
        {
            get
            {
                if (implementationClass != null)
                {
                    return implementationClass.inController();
                }
                else
                {
                    if (learningHelperClass != null)
                    {
                        return learningHelperClass.inController();
                    }
                }

                return true;
            }
            set
            {
                if (implementationClass != null)
                {
                    implementationClass.inController(value);
                }
                if (learningHelperClass != null)
                {
                    learningHelperClass.inController(value);
                }
            }
        }

        static void handleNewMob(ActorEntity currentEntity)
        {
            if (currentEntity != null && currentEntity.IsValid && currentEntity.HPCurrent > 0 && (currentEntity.OwnerID == 0 || currentEntity.OwnerID >= 0xE0000000))
            {
                // make sure it's our own copy so it won't mysteriously disappear or change without our knowledge
                currentEntity = cloneActorEntity(currentEntity);

                _mobCached[currentEntity.ID] = currentEntity;


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
                        inController = false;
                        debug("Added Mob: " + currentEntity.Name + " ID: " + currentEntity.ID + " NPCID1: " + currentEntity.NPCID1 + " NPCID2: " + currentEntity.NPCID2, DBMErrorLevel.FineTimings);
                        implementationClass.onMobAdded(currentEntity);
                        inController = true;
                    }
                    catch (Exception e2)
                    {
                        debug("onMobAdded", getEngineVsEncounter(implementationClass), e2);
                        inController = true;
                    }

                }
            }
        }

        // Stub for the new event system
        static void newMobsEvent(List<uint> mobs)
        {
            foreach (uint ID in mobs)
            {
                if (_mobList.ContainsKey(ID))
                {
                    handleNewMob(_mobList[ID]);
                }
            }
        }


        static void checkForNewMobs()
        {
            try
            {

                // Check for new entries in the mob list
                List<uint> anonDiff = _mobList.Select(i => i.Key).Except(_mobCached.Select(y => y.Key)).ToList();


                if (anonDiff.Any())
                {
                    // loop through the new mob entries
                    foreach (var ID in anonDiff)
                    {
                        if (_mobList.ContainsKey(ID))
                        {
                            handleNewMob(_mobList[ID]);
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
                Dictionary<uint, ActorEntity> tmpList = _mobCached.ToDictionary(x => x.Key, x => x.Value);
                
                foreach (var cachedKVP in tmpList)
                {
                    ActorEntity cachedEntity = cachedKVP.Value;

                    handleRemoveEntryName = cachedEntity.Name;

                    if (!_mobList.ContainsKey(cachedEntity.ID))
                    {
                        if (!removeEnemyTimeouts.ContainsKey(cachedEntity.ID))
                        {
                            removeEnemyTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                        }

                        if (DateTime.Now > removeEnemyTimeouts[cachedEntity.ID])
                        {
                            handleRemoveEntry = 1;
                            handleRemove(cachedEntity, null);
                        }
                    }
                    else
                    {
                        ActorEntity currentEntity = _mobList[cachedEntity.ID];

                        if (currentEntity == null)
                        {
                            if (!removeEnemyTimeouts.ContainsKey(cachedEntity.ID))
                            {
                                removeEnemyTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                            }

                            if (DateTime.Now > removeEnemyTimeouts[cachedEntity.ID])
                            {
                                handleRemoveEntry = 2;
                                handleRemove(cachedEntity, currentEntity);
                            }
                        }
                        else
                        {
                            if (!currentEntity.IsValid)
                            {
                                if (!removeEnemyTimeouts.ContainsKey(cachedEntity.ID))
                                {
                                    removeEnemyTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                                }

                                if (DateTime.Now > removeEnemyTimeouts[cachedEntity.ID])
                                {
                                    handleRemoveEntry = 3;
                                    handleRemove(cachedEntity, currentEntity);
                                }
                            }
                            else
                            {
                                // remove if dead
                                if (currentEntity.HPCurrent <= 0)
                                {
                                    if (!removeEnemyTimeouts.ContainsKey(cachedEntity.ID))
                                    {
                                        removeEnemyTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                                    }

                                    if (DateTime.Now > removeEnemyTimeouts[cachedEntity.ID])
                                    {
                                        handleRemoveEntry = 5;
                                        handleRemove(cachedEntity, currentEntity);
                                    }
                                }
                                else
                                {
                                    if (removeEnemyTimeouts.ContainsKey(cachedEntity.ID))
                                    {
                                        removeEnemyTimeouts.Remove(cachedEntity.ID);
                                    }

                                    // update our copy with the new info
                                    copyActorEntity(currentEntity, cachedEntity);
                                }
                            }
                        }
                    }
                    handleRemoveEntry = 0;
                    handleRemoveEntryName = "";
                }



            }
            catch (Exception e2)
            {
                debug("updateData check mob removal", DBMErrorLevel.EngineErrors, e2);
            }

        }

        static Dictionary<uint, DateTime> removeAgroTimeouts = new Dictionary<uint, DateTime>();
        static Dictionary<uint, DateTime> removeEnemyTimeouts = new Dictionary<uint, DateTime>();

        static void checkForRemovedAgro()
        {
            try
            {
                Dictionary<uint, uint> newEnmityList = playerEntity.EnmityEntries.ToDictionary(x => x.ID, x => x.ID);

                // Duplicate the list first before looping through it, or we'll get exceptions when we try to remove entries from the original list
                List<ActorEntity> tmpList = _agroList.ToList();
                
                foreach (ActorEntity cachedEntity in tmpList)
                {
                    if (newEnmityList.ContainsKey(cachedEntity.ID))
                    {
                        continue;
                    }

                    handleRemoveEntryName = cachedEntity.Name;

                    if (!mobList.ContainsKey(cachedEntity.ID))
                    {
                        if (!removeAgroTimeouts.ContainsKey(cachedEntity.ID))
                        {
                            removeAgroTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                        }

                        if (DateTime.Now > removeAgroTimeouts[cachedEntity.ID])
                        {
                            handleRemoveEntry = 1;
                            handleRemoveAgro(cachedEntity, null);
                        }
                    }
                    else
                    {
                        ActorEntity currentEntity = mobList[cachedEntity.ID];
                        if (currentEntity == null)
                        {
                            if (!removeAgroTimeouts.ContainsKey(cachedEntity.ID))
                            {
                                removeAgroTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                            }

                            if (DateTime.Now > removeAgroTimeouts[cachedEntity.ID])
                            {
                                handleRemoveEntry = 2;
                                handleRemoveAgro(cachedEntity, currentEntity);
                            }
                        }
                        else if (!currentEntity.IsValid)
                        {
                            if (!removeAgroTimeouts.ContainsKey(cachedEntity.ID))
                            {
                                removeAgroTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                            }

                            if (DateTime.Now > removeAgroTimeouts[cachedEntity.ID])
                            {
                                handleRemoveEntry = 3;
                                handleRemoveAgro(cachedEntity, currentEntity);
                            }
                        }
                        else if (currentEntity.ClaimedByID == 0 || currentEntity.ClaimedByID >= 0xE0000000)
                        {
                            if (!removeAgroTimeouts.ContainsKey(cachedEntity.ID))
                            {
                                removeAgroTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                            }

                            if (DateTime.Now > removeAgroTimeouts[cachedEntity.ID])
                            {
                                handleRemoveEntry = 4;
                                handleRemoveAgro(cachedEntity, currentEntity);
                            }
                        }
                        else if (currentEntity.HPCurrent <= 0)
                        {
                            if (!removeAgroTimeouts.ContainsKey(cachedEntity.ID))
                            {
                                removeAgroTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                            }

                            if (DateTime.Now > removeAgroTimeouts[cachedEntity.ID])
                            {
                                handleRemoveEntry = 5;
                                handleRemoveAgro(cachedEntity, currentEntity);
                            }
                        }
                        else if (!newEnmityList.ContainsKey(currentEntity.ID) && CurrentUser.HPCurrent > 0)
                        {
                            if (!removeAgroTimeouts.ContainsKey(cachedEntity.ID))
                            {
                                removeAgroTimeouts[cachedEntity.ID] = DateTime.Now + TimeSpan.FromSeconds(3);
                            }

                            if (DateTime.Now > removeAgroTimeouts[cachedEntity.ID])
                            {
                                // ClaimedByID seems unreliable sometimes, and IsClaimed never seems to work,
                                // so instead, check to see if the mob is full health but NOT in the agroList anymore
                                // Also make sure we're alive, since the agro list is temporarily empty when dead.
                                handleRemoveEntry = 6;
                                handleRemoveAgro(cachedEntity, currentEntity);
                            }
                        }
                        else
                        {
                            if (removeAgroTimeouts.ContainsKey(cachedEntity.ID))
                            {
                                removeAgroTimeouts.Remove(cachedEntity.ID);
                            }
                            // update our copy with the new info
                            copyActorEntity(currentEntity, cachedEntity);
                        }
                    }

                    handleRemoveEntry = 0;
                    handleRemoveEntryName = "";
                }
            }
            catch (Exception e2)
            {
                debug("updateData check agro mob removal", DBMErrorLevel.EngineErrors, e2);
            }

        }

        static DateTime lastMapIndexChangePlusSafe = DateTime.MinValue;
        static DateTime lastLevelChangePlusSafe = DateTime.MinValue;
        static DateTime lastJobChangePlusSafe = DateTime.MinValue;

        static uint lastMapIndex = 0;
        static int lastLevel = 0;
        static FFXIVAPP.Memory.Core.Enums.Actor.Job lastJob = FFXIVAPP.Memory.Core.Enums.Actor.Job.Unknown;


        static bool checkCurrentUserIntegrety()
        {

            bool shouldUpdate = true;

            if (!pcEntities.Any())
            {
                return false;
            }

            ActorEntity tmpUser = pcEntities.FirstOrDefault().Value;

            if (tmpUser == null)
            {
                debug("checkCurrentUserIntegrety: pcEntities.First().Value is null", DBMErrorLevel.EngineErrors);
                return false;
            }

            tmpUser = tmpUser.CurrentUser;

            if (tmpUser == null)
            {
                debug("checkCurrentUserIntegrety: pcEntities.First().Value.CurrentUser is null", DBMErrorLevel.EngineErrors);
                return false;
            }

            if (ZoneHelper.GetMapInfo(tmpUser.MapIndex).Index == 0)
            {
                return false;
            }

            try
            {
                if (_currentUser != null && tmpUser != null)
                {
                    if (tmpUser.Level != lastLevel && tmpUser.Level != _currentUser.Level)
                    {
                        lastLevel = tmpUser.Level;
                        lastLevelChangePlusSafe = DateTime.Now + TimeSpan.FromSeconds(1);
                        shouldUpdate = false;
                    }
                    if (tmpUser.Job != lastJob && tmpUser.Job != _currentUser.Job)
                    {
                        lastJob = tmpUser.Job;
                        lastJobChangePlusSafe = DateTime.Now + TimeSpan.FromSeconds(1);
                        shouldUpdate = false;
                    }
                    if (tmpUser.MapIndex != lastMapIndex && tmpUser.MapIndex != _currentUser.MapIndex)
                    {
                        lastMapIndex = tmpUser.MapIndex;
                        lastMapIndexChangePlusSafe = DateTime.Now + TimeSpan.FromSeconds(1);
                        shouldUpdate = false;
                    }

                    if (DateTime.Now > lastLevelChangePlusSafe && DateTime.Now > lastJobChangePlusSafe && DateTime.Now > lastMapIndexChangePlusSafe)
                    {
                        shouldUpdate = true;
                    }
                    else if (tmpUser.Job != _currentUser.Job || tmpUser.Level != _currentUser.Level || tmpUser.MapIndex != _currentUser.MapIndex)
                    {
                        shouldUpdate = false;
                    }

                }
            }
            catch (Exception e)
            {
                debug("checkCurrentUserIntegrety error:", DBMErrorLevel.EngineErrors, e);
            }

            if (shouldUpdate)
            {
                // clone it so comparisons work
                CurrentUser = cloneActorEntity(tmpUser);
                EncounterController.zone = ZoneHelper.GetMapInfo(tmpUser.MapIndex);
            }

            return shouldUpdate;
        }

        static void updateData(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!checkCurrentUserIntegrety())
                {
                    return;
                }

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
                    checkForRemovedAgro();
                    checkForRemovedMobs();

                    try
                    {
                        // cache the enmity list so we can compare next time to detect changes
                        _enmityCached = playerEntity.EnmityEntries.ToList();

                        if (!encounterStarted)
                        {
                            _agroList.Clear();
                            _enmityCached.Clear();
                        }


                    }
                    catch (Exception e2)
                    {
                        debug("updateData cache lists", DBMErrorLevel.EngineErrors, e2);
                    }

                }
            }
            catch (Exception ex2)
            {
                debug("updateData error: ", DBMErrorLevel.EngineErrors, ex2);
            }
            inUpdate = false;
        }






        static void tickTimerEvent(object sender, ElapsedEventArgs e)
        {
            lock (tickLock)
            {
                if (inTick || playerEntity == null || _mobList == null || !setupDone)
                {
                    return;
                }
                // this ensures that we don't try to process more than one tick at a time on slower computers. Not likely to happen though
                inTick = true;
            }

            try
            {
                checkCurrentUserIntegrety();



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
                            inController = false;
                            implementationClass.onTick();
                            inController = true;
                        }
                        catch (Exception e2)
                        {
                            debug("tick", getEngineVsEncounter(implementationClass), e2);
                        }
                    }
                }
            }
            catch (Exception ex2)
            {
                debug("tickTimerEvent error: ", DBMErrorLevel.EngineErrors, ex2);
            }
            finally
            {
                inTick = false;
            }
        }


        public static void handleRemove(ActorEntity cachedEntity, ActorEntity currentEntity)
        {
            if (CurrentUser == null || CurrentUser.HPCurrent <= 0)
            {
                return;
            }

            try
            {
                //debug("Removed Mob: " + currentEntity.Name + " (" + handleRemoveEntry + ", " + (currentEntity.IsValid ? "valid" : "not valid") + ", " + (currentEntity.IsClaimed ? "claimed" : "not claimed") + ", by id: " + currentEntity.ClaimedByID + ")", DBMErrorLevel.Trace);

                ActorEntity tmp = null;

                if (cachedEntity != null)
                {
                    _mobCached.TryRemove(cachedEntity.ID, out tmp);
                }

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
                        if (currentEntity == null && cachedEntity != null)
                        {
                            debug("Removed Mob: " + cachedEntity.Name + " ID: " + cachedEntity.ID + " NPCID1: " + cachedEntity.NPCID1 + " NPCID2: " + cachedEntity.NPCID2 + " (" + handleRemoveEntry + ", null)", DBMErrorLevel.FineTimings);
                        }
                        else if (currentEntity != null)
                        {
                            debug("Removed Mob: " + currentEntity.Name + " ID: " + currentEntity.ID + " NPCID1: " + currentEntity.NPCID1 + " NPCID2: " + currentEntity.NPCID2 + " (" + handleRemoveEntry + ", " + (currentEntity.IsValid ? "valid" : "not valid") + ")", DBMErrorLevel.FineTimings);
                        
                        }
                        else
                        {
                            debug("Removed Mob - currentEntity == null && cachedEntity == null (" + handleRemoveEntry, DBMErrorLevel.FineTimings);
                        }
                        inController = false;
                        implementationClass.onMobRemoved(cachedEntity);
                        inController = true;
                    }
                    catch (Exception e2)
                    {
                        debug("onMobRemoved", getEngineVsEncounter(implementationClass), e2);
                    }
                }
            }
            catch (Exception ex2)
            {
                debug("handleRemove error: ", DBMErrorLevel.EngineErrors, ex2);
            }
        }

        public static void handleRemoveAgro(ActorEntity cachedEntity, ActorEntity currentEntity)
        {
            if (CurrentUser == null || CurrentUser.HPCurrent <= 0)
            {
                return;
            }

            try
            {
                if (currentEntity == null && cachedEntity != null)
                {
                    debug("Removed Agro: " + cachedEntity.Name + " (" + handleRemoveEntry + ", null)", DBMErrorLevel.Trace);
                }
                else if (currentEntity != null)
                {
                    debug("Removed Agro: " + currentEntity.Name + " (" + handleRemoveEntry + ", " + (currentEntity.IsValid ? "valid" : "not valid") + ", " + (currentEntity.IsClaimed ? "claimed" : "not claimed") + ", by id: " + currentEntity.ClaimedByID + ")", DBMErrorLevel.Trace);
                }
                else
                {
                    debug("Removed Agro: (" + handleRemoveEntry + ", null)", DBMErrorLevel.Trace);
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
                        inController = false;
                        implementationClass.onAgroRemoved(cachedEntity);
                        inController = true;
                    }
                    catch (Exception e2)
                    {
                        debug("onAgroRemoved", getEngineVsEncounter(implementationClass), e2);
                        inController = true;
                    }
                }


                if (encounterStarted && !_agroList.Any() && currentEntity != null && currentEntity.HPCurrent > 0)
                {
                    endEncounter();
                }
            }
            catch (Exception ex2)
            {
                debug("handleRemoveAgro error: ", DBMErrorLevel.EngineErrors, ex2);
            }
        }

        public static void endEncounter(bool force = false)
        {
            try
            {
                if (!force)
                {
                    if (isSealedOff || (playerEntity != null && playerEntity.EnmityEntries != null && playerEntity.EnmityEntries.Any()))
                    {
                        debug("endEncounter error: " + isSealedOff + ", " + (playerEntity == null ? "null, " : "not null, ") + (playerEntity.EnmityEntries == null ? "null, " : "not null, ") + playerEntity.EnmityEntries.Any(), DBMErrorLevel.EngineErrors);
                        return;
                    }
                }

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
                        inController = false;
                        implementationClass.endEncounter();
                        implementationClass.onEndEncounter();
                        inController = true;
                    }
                    catch (Exception e2)
                    {
                        debug("endEncounter", getEngineVsEncounter(implementationClass), e2);
                        inController = true;
                    }
                    implementationClass = null;
                }
                _agroList.Clear();
                _enmityCached.Clear();
                _mobCached.Clear();
                encounterStarted = false;
            }
            catch (Exception ex2)
            {
                debug("endEncounter error: ", DBMErrorLevel.EngineErrors, ex2);
            }
        }
        

        public static void setupNewClass(IEncounter inst)
        {
            try
            {
                encounterStarted = true;
                try
                {
                    inController = false;
                    inst.onStartEncounter();
                    inController = true;
                }
                catch (Exception e2)
                {
                    debug("onStartEncounter", getEngineVsEncounter(inst), e2);
                    inController = true;
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
                        inController = false;
                        inst.onMobAgro(tmpEnt);
                        inController = true;
                    }
                    catch (Exception e2)
                    {
                        debug("onMobAgro", getEngineVsEncounter(inst), e2);
                        inController = true;
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

                foreach (var KVP in _mobCached)
                {
                    ActorEntity tmpEnt = KVP.Value;
                    try
                    {
                        if (inst == implementationClass)
                        {
                            debug("Added Mob (Setup): " + tmpEnt.Name + " ID: " + tmpEnt.ID + " NPCID1: " + tmpEnt.NPCID1 + " NPCID2: " + tmpEnt.NPCID2, DBMErrorLevel.FineTimings);
                        }
                        inController = false;
                        inst.onMobAdded(tmpEnt);
                        inController = true;
                    }
                    catch (Exception e2)
                    {
                        debug("onMobAdded", getEngineVsEncounter(inst), e2);
                    }
                }
            }
            catch (Exception ex2)
            {
                debug("setupNewClass error: ", DBMErrorLevel.EngineErrors, ex2);
            }
        }

        public static bool checkRecompile(string mobNameRaw)
        {
            try
            {
                // No encounter script loaded. Lets see if we have one that matches this mob
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                string mobName = rgx.Replace(mobNameRaw, "");

                string dir = getScriptsDirectory();

                FileInfo fi = new FileInfo(dir + @"\" + mobName + ".cs");

                if (classModified.ContainsKey(mobName))
                {

                    if (fi.LastWriteTimeUtc != classModified[mobName])
                    {
                        endEncounter();

                        compileScripts();
                        return true;
                    }
                }
                else if (fi.Exists)
                {
                    compileScripts();
                }
            }
            catch (Exception ex2)
            {
                debug("checkRecompile error: ", DBMErrorLevel.EngineErrors, ex2);
            }

            return false;
        }

        private static void resetLearningHelper()
        {
            if (learningHelperClass != null)
            {
                try
                {
                    learningHelperClass.endEncounter();
                    inController = false;
                    learningHelperClass.onEndEncounter();
                    inController = true;
                }
                catch (Exception e2)
                {
                    inController = true;
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

        public static void NetworkPacket(NetworkPacket packet)
        {
            byte[] buffer = packet.Buffer;
            int currentPos = packet.CurrentPosition;

            uint ActorID;
            uint TargetID;
            uint Key;
            int SkillID;
            int MarkerType;
            short subType;
            ActorEntity Actor = null;
            ActorEntity Target = null;

            switch (packet.Key)
            {
                case 0x1960014: // network starts casting
                    ActorID = BitConverter.ToUInt32(packet.Buffer, packet.CurrentPosition + 0x4);
                    TargetID = BitConverter.ToUInt32(packet.Buffer, packet.CurrentPosition + 0x2C);
                    SkillID = BitConverter.ToInt32(packet.Buffer, packet.CurrentPosition + 0x24);

                    //debug("OnStartsCasting Raw: " + ActorID + ", " + TargetID + ", " + TargetID + ", " + SkillID);
                    
                    if (EncounterController._mobList.ContainsKey(ActorID))
                        Actor = EncounterController._mobList[ActorID];
                    else if (EncounterController.pcEntities.ContainsKey(ActorID))
                        Actor = EncounterController.pcEntities[ActorID];
                    else if (EncounterController.npcList.ContainsKey(ActorID))
                        Actor = EncounterController.npcList[ActorID];

                    if (EncounterController._mobList.ContainsKey(TargetID))
                        Target = EncounterController._mobList[TargetID];
                    else if (EncounterController.pcEntities.ContainsKey(TargetID))
                        Target = EncounterController.pcEntities[TargetID];
                    else if (EncounterController.npcList.ContainsKey(TargetID))
                        Target = EncounterController.npcList[TargetID];

                    if (Actor == null || Target == null)
                    {
                        return;
                    }

                    //if (OnStartsCasting != null)
                    {
                        OnStartsCasting(SkillID, Actor, Target);
                    }


                    break;

                case 0x1460014: // network ability
                    ActorID = BitConverter.ToUInt32(packet.Buffer, packet.CurrentPosition + 0x4);
                    TargetID = BitConverter.ToUInt32(packet.Buffer, packet.CurrentPosition + 0x40);
                    SkillID = BitConverter.ToInt32(packet.Buffer, packet.CurrentPosition + 0x2C);

                    //debug("OnStartsAbility Raw: " + ActorID + ", " + TargetID + ", " + SkillID);
                    
                    if (EncounterController._mobList.ContainsKey(ActorID))
                        Actor = EncounterController._mobList[ActorID];
                    else if (EncounterController.pcEntities.ContainsKey(ActorID))
                        Actor = EncounterController.pcEntities[ActorID];
                    else if (EncounterController.npcList.ContainsKey(ActorID))
                        Actor = EncounterController.npcList[ActorID];

                    if (EncounterController._mobList.ContainsKey(TargetID))
                        Target = EncounterController._mobList[TargetID];
                    else if (EncounterController.pcEntities.ContainsKey(TargetID))
                        Target = EncounterController.pcEntities[TargetID];
                    else if (EncounterController.npcList.ContainsKey(TargetID))
                        Target = EncounterController.npcList[TargetID];

                    if (Actor == null || Target == null)
                    {
                        return;
                    }

                    //if (OnAbility != null)
                    {
                        OnAbility(SkillID, Actor, Target);
                    }

                    break;
                case 0x1470014: // network aoe ability
                    ActorID = BitConverter.ToUInt32(packet.Buffer, packet.CurrentPosition + 0x4);
                    SkillID = BitConverter.ToInt32(packet.Buffer, packet.CurrentPosition + 0x2C);

                    //debug("OnStartsAOEAbility Raw: " + ActorID + ", " + SkillID);

                    if (EncounterController._mobList.ContainsKey(ActorID))
                        Actor = EncounterController._mobList[ActorID];
                    else if (EncounterController.pcEntities.ContainsKey(ActorID))
                        Actor = EncounterController.pcEntities[ActorID];
                    else if (EncounterController.npcList.ContainsKey(ActorID))
                        Actor = EncounterController.npcList[ActorID];

                    if (Actor == null)
                    {
                        return;
                    }



                    for (int index1 = 0; index1 < 16; ++index1)
                    {
                        TargetID = BitConverter.ToUInt32(buffer, currentPos + (273 + index1 * 2 + 1) * 4);
                        if (TargetID != 0)
                        {
                            if (EncounterController._mobList.ContainsKey(TargetID))
                                Target = EncounterController._mobList[TargetID];
                            else if (EncounterController.pcEntities.ContainsKey(TargetID))
                                Target = EncounterController.pcEntities[TargetID];
                            else if (EncounterController.npcList.ContainsKey(TargetID))
                                Target = EncounterController.npcList[TargetID];

                            if (Target == null)
                            {
                                continue;
                            }

                            //if (OnAbility != null)
                            {
                                OnAbility(SkillID, Actor, Target);
                            }

                        }
                    }



                    break;
                case 0x1410014:
                    // network actor/buffs
                    Key = BitConverter.ToUInt32(packet.Buffer, packet.CurrentPosition + 0x4);

                    break;
                case 0x1420014:
                    // cancel action, death, target icon, dot
                    subType = BitConverter.ToInt16(buffer, currentPos + 8*4);

                    switch (subType)
                    {
                        case 0x22: //NetworkTargetIcon

                            debug("NetworkTargetIcon" + BitConverter.ToUInt32(buffer, currentPos + 28).ToString("X4") + "|" + (BitConverter.ToUInt32(buffer, currentPos + 32) >> 16).ToString("X4") + "|" + BitConverter.ToUInt32(buffer, currentPos + 36).ToString("X4") + "|" + BitConverter.ToUInt32(buffer, currentPos + 40).ToString("X4") + "|" + BitConverter.ToUInt32(buffer, currentPos + 44).ToString("X4") + "|" + BitConverter.ToUInt32(buffer, currentPos + 48).ToString("X4"));
                    
                            break;
                    }


                    break;
                case 0x1400014:
                    // network actor/buffs
                    Key = BitConverter.ToUInt32(packet.Buffer, packet.CurrentPosition + 0x4);

                    break;
                case 0x3350014:
                    // network marker

                    for (int i = 0; i < 3; ++i)
                    {
                        BitConverter.ToInt32(buffer, currentPos + (8 + i * 5) * 4);
                        float PosX = BitConverter.ToSingle(buffer, currentPos + (8 + i * 5 + 2) * 4);
                        float PosZ = BitConverter.ToSingle(buffer, currentPos + (8 + i * 5 + 3) * 4);
                        float PosY = BitConverter.ToSingle(buffer, currentPos + (8 + i * 5 + 4) * 4);

                        if (PosX != 0.0 || PosY != 0.0 || PosZ != 0.0)
                        {
                            //if (OnTargetMarker != null)
                            {
                                OnGroundMarker(i, PosX, PosZ, PosY);
                            }
                        }
                    }

                    break;
                case 0x1440014:
                    // network target marker
                    TargetID = BitConverter.ToUInt32(buffer, currentPos + 56);
                    ActorID = BitConverter.ToUInt32(buffer, currentPos + 40);
                    MarkerType = BitConverter.ToInt32(buffer, currentPos + 36);

                    if (MarkerType == 0)
                    {
                        return;
                    }

                    if (TargetID == 0)
                        return;

                    if ((int)TargetID == -536870912)
                        return;

                    if (EncounterController._mobList.ContainsKey(ActorID))
                        Actor = EncounterController._mobList[ActorID];
                    else if (EncounterController.pcEntities.ContainsKey(ActorID))
                        Actor = EncounterController.pcEntities[ActorID];
                    else if (EncounterController.npcList.ContainsKey(ActorID))
                        Actor = EncounterController.npcList[ActorID];

                    if (EncounterController._mobList.ContainsKey(TargetID))
                        Target = EncounterController._mobList[TargetID];
                    else if (EncounterController.pcEntities.ContainsKey(TargetID))
                        Target = EncounterController.pcEntities[TargetID];
                    else if (EncounterController.npcList.ContainsKey(TargetID))
                        Target = EncounterController.npcList[TargetID];

                    if (Target == null)
                    {
                        return;
                    }

                    //if (OnTargetMarker != null)
                    {
                        OnTargetMarker(MarkerType, Actor, Target);
                    }

                    break;
                default:
                    break;
            }
        }

        public static string GetLocalizedString(ActionInfo statusInfo)
        {
            var statusKey = statusInfo.EN;
            switch (Constants.GameLanguage)
            {
                case "French":
                    statusKey = statusInfo.FR;
                    break;
                case "Japanese":
                    statusKey = statusInfo.JA;
                    break;
                case "German":
                    statusKey = statusInfo.DE;
                    break;
                case "Chinese":
                    statusKey = statusInfo.ZH;
                    break;
            }

            return statusKey;
        }
        public static string GetLocalizedString(StatusLocalization statusInfo)
        {
            var statusKey = statusInfo.English;
            switch (Constants.GameLanguage)
            {
                case "French":
                    statusKey = statusInfo.French;
                    break;
                case "Japanese":
                    statusKey = statusInfo.Japanese;
                    break;
                case "German":
                    statusKey = statusInfo.German;
                    break;
                case "Chinese":
                    statusKey = statusInfo.Chinese;
                    break;
            }

            return statusKey;
        }

        private static string GetHexData(byte[] buffer, int offset)
        {
            uint num = BitConverter.ToUInt32(buffer, offset);
            if ((int)num == 0)
                return "0";
            return num.ToString("X2");
        }


        static void OnAbility(int SkillID, ActorEntity Actor, ActorEntity Target)
        {
            return;

            try
            {
                lock (accessControl)
                {
                    string chatLogEntry = Actor.Name + " uses " + StringHelper.TitleCase(GetLocalizedString(Constants.Acitons[SkillID.ToString()])) + " on " + Target.Name;

                    //if (Actor.Type == FFXIVAPP.Common.Core.Memory.Enums.Actor.Type.Monster)
                    {
                        //debug("OnAbility: " + Actor.ID + " " + chatLogEntry + " " + Target.ID);
                    }

                    //dispatchChatLogEntry(chatLogEntry);
                }
            }
            catch (Exception e)
            {
                debug("OnAbility Exception", DBMErrorLevel.EngineErrors, e, "#FF0000");
            }
        }


        static void OnStartsCasting(int SkillID, ActorEntity Actor, ActorEntity Target)
        {
            try
            {
                string ability = "";

                if (Constants.Acitons.ContainsKey(SkillID.ToString()))
                {
                    ability = StringHelper.TitleCase(GetLocalizedString(Constants.Acitons[SkillID.ToString()]));
                }
                string chatLogEntry = Actor.Name + " starts using " + ability + " (" + SkillID.ToString() + ") on " + Target.Name;

                lock (accessControl)
                {
                    //if (Actor.Type == FFXIVAPP.Common.Core.Memory.Enums.Actor.Type.Monster)
                    {
                        //debug("OnStartsCasting: " + Actor.ID + " " + chatLogEntry + " " + Target.ID);
                    }

                    dispatchChatLogEntry(chatLogEntry);
                }
            }
            catch (Exception e)
            {
                debug("OnStartsCasting Exception", DBMErrorLevel.EngineErrors, e, "#FF0000");
            }
        }


        static void OnGroundMarker(int type, float x, float y, float z)
        {
            debug("Ground Marker received: " + type + " pos: " + x + ", " + y + ", " + z);
        }

        static void OnTargetMarker(int type, ActorEntity sourceEnt, ActorEntity targetEnt)
        {
            string actor = "";
            string target = "";

            try
            {
                if (sourceEnt != null)
                {
                    actor = sourceEnt.Name;
                }
                if (targetEnt != null)
                {
                    target = targetEnt.Name;
                }

                string chatLogEntry = actor + " places a target marker over " + target + " of type " + type + ".";

                dispatchChatLogEntry(chatLogEntry);

                debug("Target Marker received: " + type + " Source: " + actor + " Target: " + target);

            }
            catch (Exception e)
            {
                debug("Target Mark Exception", DBMErrorLevel.EngineErrors, e, "#FF0000");
            }
        }

        public static void OnNewChatLogEntry(object sender, ChatLogEntryEvent chatLogEntryEvent)
        {
            try
            {
                string chatLogEntry = logStripper.Replace(chatLogEntryEvent.ChatLogEntry.Line, "");

                if (chatLogEntry.StartsWith("@"))
                {
                    tts(chatLogEntry.Trim('@'));
                }

                // delegate event from chat log, not required to subsribe
                // this updates 100 times a second and only sends a line when it gets a new one
                if (sender == null || chatLogEntryEvent == null || chatLogEntryEvent.ChatLogEntry == null || !setupDone)
                {
                    return;
                }

                if (testString.IsMatch(chatLogEntry))
                {
                    /*
                    if (!encounterStarted)
                    {
                        tts("DBM Ready");
                    }
                    else
                    {
                     * */
                    debug("Dumping actors...");

                    foreach (ActorEntity pActor in _agroList)
                    {
                        debug("Dump Mob: " + pActor.Name + " (" + (pActor.IsValid ? "valid" : "not valid") + ", " + (pActor.IsClaimed ? "claimed" : "not claimed") + ", by id: " + pActor.ClaimedByID + ")", DBMErrorLevel.Trace);
                    }
                    foreach (EnmityEntry enmity in playerEntity.EnmityEntries)
                    {
                        debug("Dump Enmity: " + enmity.Name + " ID: " + enmity.ID + "", DBMErrorLevel.Trace);
                    }
                    //}
                }

                if (sealedOff != null && sealedOff.IsMatch(chatLogEntry))
                {
                    isSealedOff = true;

                    debug("Sealed Off");

                    if ((DateTime.Now - started).Duration() > TimeSpan.FromSeconds(1))
                    {
                        handleRemoveEntryName = "Sealed Off";
                        endEncounter(true);
                        handleRemoveEntryName = "";
                    }

                }

                if (isSealedOff && noLongerSealed != null && noLongerSealed.IsMatch(chatLogEntry))
                {
                    isSealedOff = false;

                    debug("No Longer Sealed Off");

                    handleRemoveEntryName = "No Longer Sealed Off";
                    endEncounter(true);
                    handleRemoveEntryName = "";
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
                        chatLogEntry = youGain.regex.Replace(chatLogEntry, CurrentUser.Name + youGain.replacement);
                        dispatchChatLogEntry(chatLogEntry);
                    }
                    else if (youLose.regex != null && youLose.regex.IsMatch(chatLogEntry))
                    {
                        chatLogEntry = youLose.regex.Replace(chatLogEntry, CurrentUser.Name + youLose.replacement);
                        dispatchChatLogEntry(chatLogEntry);
                    }
                    else if (youSuffer.regex != null && youSuffer.regex.IsMatch(chatLogEntry))
                    {
                        chatLogEntry = youSuffer.regex.Replace(chatLogEntry, CurrentUser.Name + youSuffer.replacement);
                        dispatchChatLogEntry(chatLogEntry);
                    }
                    else if (youRecover.regex != null && youRecover.regex.IsMatch(chatLogEntry))
                    {
                        chatLogEntry = youRecover.regex.Replace(chatLogEntry, CurrentUser.Name + youRecover.replacement);
                        dispatchChatLogEntry(chatLogEntry);
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
                    inController = false;
                    implementationClass.onNewChatLine(chatLogEntry);
                    inController = true;
                }
                catch (Exception e2)
                {
                    inController = true;
                    debug("processChatLine", getEngineVsEncounter(implementationClass), e2);
                }
            }
        }




        #region utility

        public static string getScriptsDirectory()
        {
            try
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
            catch (Exception ex2)
            {
                debug("getScriptsDirectory error: ", DBMErrorLevel.EngineErrors, ex2);
            }

            return "";
        }

        public static void refreshIgnoreMobsList()
        {
            try
            {
                ignoreMobs = new List<string>();

                string ignoreFile = getScriptsDirectory() + Path.PathSeparator + "ignore.txt";

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
            catch (Exception ex2)
            {
                debug("refreshIgnoreMobsList error: ", DBMErrorLevel.EngineErrors, ex2);
            }
        }

        private static DBMErrorLevel getEngineVsEncounter(IEncounter controller)
        {
            try
            {
                if (controller == null)
                {
                    return DBMErrorLevel.EngineErrors;
                }
                return (controller.inController() ? DBMErrorLevel.EngineErrors : DBMErrorLevel.EncounterErrors);
            }
            catch (Exception ex2)
            {
                debug("getEngineVsEncounter error: ", DBMErrorLevel.EngineErrors, ex2);
            }
            return DBMErrorLevel.EngineErrors;
        }

        static object debugLock = new object();

        public static void debug(string message, DBMErrorLevel level = DBMErrorLevel.EncounterErrors, Exception ex = null, string color = "#FFFFFF")
        {
            try
            {
                lock (debugLock)
                {
                    if (level >= errorLevel)
                    {
                        var timeStampColor = Settings.Default.TimeStampColor.ToString();
                        DateTime now = DateTime.Now.ToUniversalTime();
                        string debugInfo = "): ";
                        string timeStamp = "[" + now.ToShortDateString() + " " + now.Hour + ":" + now.Minute.ToString("D2") + ":" + now.Second.ToString("D2") + ":" + now.Millisecond.ToString("D3") + "] ";
                        string line = "";

                        if (ex != null)
                        {
                            StackTrace st = new StackTrace(ex, true);
                            // Get the top stack frame
                            StackFrame frame = st.GetFrame(0);

                            debugInfo = ":" + frame.GetFileName() + ":" + frame.GetMethod() + ":" + frame.GetFileLineNumber() + ":" + frame.GetFileColumnNumber() + ": " + ex.Message + "): ";
                        }


                        line = timeStamp + "(" + Enum.GetName(typeof(DBMErrorLevel), level) + debugInfo + message;

                        File.AppendAllText(debugLogPath, line + Environment.NewLine);

                        FFXIVAPP.Common.Constants.FD.AppendFlow(timeStamp, "", line, new[]
                            {
                                timeStampColor, color
                            }, MainView.View.ChatLogFD._FDR
                        );
                    }
                }
            }
            catch (Exception ex2)
            {
                // stack overflow.... dur
                //debug("Debug Error", DBMErrorLevel.EngineErrors, ex2);
            }
        }

        /** /
        public static void tts(string toRead)
        {

            try
            {

                lock (speakingLock)
                {
                    while (speaking)
                    {
                        // wait for previous speech to finish
                        Thread.Sleep(0);
                    }
                    speaking = true;
                }

                try
                {
                    using (SpeechSynthesizer m_speechSynth = new SpeechSynthesizer())
                    {
                        m_speechSynth.Volume = speechVolume;
                        //m_speechSynth.Rate = 2;

                        MemoryStream waveStream = new MemoryStream();
                        //m_speechSynth.SetOutputToWaveStream(waveStream);
                        m_speechSynth.SetOutputToAudioStream(waveStream, new SpeechAudioFormatInfo(48000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));


                        m_speechSynth.Speak(toRead);
                        //m_speechSynth.SpeakAsync(toRead);
                        //m_speechSynth.SpeakCompleted += delegate
                        {
                            try
                            {
                                m_speechSynth.SetOutputToNull();

                                waveStream.Position = 0; // reset counter to start
                                


                                IWaveProvider provider = new RawSourceWaveStream(waveStream, new WaveFormat(48000, 1));

                                DirectSoundOut _waveOutDevice = new DirectSoundOut(FFXIVAPP.Common.Constants.DefaultAudioDevice);

                                _waveOutDevice.Init(provider);

                                _waveOutDevice.PlaybackStopped += delegate
                                {
                                    waveStream.Dispose();
                                };

                                lock (TTSQueueDSO)
                                {
                                    TTSQueueDSO.Add(_waveOutDevice);
                                }

                            }
                            catch (Exception ex2)
                            {
                                debug("SpeakCompleted", DBMErrorLevel.EngineErrors, ex2);
                            }
                        };
                    }
                }
                catch (Exception ex1)
                {
                    debug("tts2", DBMErrorLevel.EngineErrors, ex1);
                }

                speaking = false;
            }
            catch (Exception ex)
            {
                debug("tts1", DBMErrorLevel.EngineErrors, ex);
            }


        }


        public static void TTSThread()
        {
            DirectSoundOut toPlay = null;

            while (true)
            {
                bool any = false;
                
                try
                {
                    try
                    {
                        lock (TTSQueueDSO)
                        {
                            any = TTSQueueDSO.Any();
                        }
                    }
                    catch (Exception ex)
                    {
                        debug("TTSThread Error 1", DBMErrorLevel.EngineErrors, ex);
                    }

                    if (any) // something in queue
                    {
                        try
                        {
                            //MessageBox.Show("test");
                            lock (TTSQueueDSO)
                            {
                                if (TTSQueueDSO.Any()) // could have changed between our locks
                                {
                                    toPlay = TTSQueueDSO.First();
                                    TTSQueueDSO.Remove(toPlay);
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            try
                            {
                                toPlay.Play();

                                while (toPlay.PlaybackState != PlaybackState.Playing)
                                {
                                    Thread.Sleep(10);
                                }

                                DateTime started = DateTime.Now;

                                while (toPlay.PlaybackState == PlaybackState.Playing)
                                {
                                    Thread.Sleep(50);
                                }
                            }
                            catch (Exception ex2)
                            {
                                debug("TTSThread Error 2: ", DBMErrorLevel.EngineErrors, ex2);
                            }

                        }
                        catch (Exception ex3)
                        {
                            debug("TTSThread Error 3", DBMErrorLevel.EngineErrors, ex3);
                        }
                        finally
                        {
                            if (toPlay != null)
                            {
                                toPlay.Dispose();
                            }
                        }

                        Thread.Sleep(50);
                    }

                    Thread.Sleep(50);
                }
                catch (Exception ex4)
                {
                    debug("TTSThread Error 4", DBMErrorLevel.EngineErrors, ex4);
                }
            }
        }

        /**/
        
        
        public static void tts(string toRead)
        {
            toRead = toRead.Trim();

            if (toRead == "")
            {
                return;
            }

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
                    using (SpeechSynthesizer m_speechSynth = new SpeechSynthesizer())
                    {
                        m_speechSynth.Volume = speechVolume;
                        //m_speechSynth.Rate = 2;

                        MemoryStream waveStream = new MemoryStream();
                        m_speechSynth.SetOutputToWaveStream(waveStream);

                        //m_speechSynth.SpeakAsync(toRead);
                        //m_speechSynth.SpeakCompleted += delegate

                        m_speechSynth.Speak(toRead);
                        //m_speechSynth.SpeakCompleted += delegate
                        {
                            try
                            {
                                m_speechSynth.SetOutputToNull();

                                waveStream.Position = 0; // reset counter to start


                                VoiceThroughNetAudio netAudio = new VoiceThroughNetAudio(waveStream, "WAV", FFXIVAPP.Common.Constants.DefaultAudioDevice);


                                lock (TTSQueue)
                                {
                                    TTSQueue.Add(netAudio);
                                }
                            }
                            catch (Exception ex)
                            {
                                debug("SpeakCompleted", DBMErrorLevel.EngineErrors, ex);
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    debug("tts", DBMErrorLevel.EngineErrors, ex);
                }
                speaking = false;
            }
            catch
            {

            }


        }

        
        public static void TTSThread()
        {
            VoiceThroughNetAudio toPlay = null;

            while (true)
            {
                bool any = false;

                try
                {
                    try
                    {
                        lock (TTSQueue)
                        {
                            any = TTSQueue.Any();
                        }
                    }
                    catch (Exception ex)
                    {
                        debug("TTSThread Error 1", DBMErrorLevel.EngineErrors, ex);
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
                                try
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
                                }
                                catch (Exception ex2)
                                {
                                    debug("TTSThread Error 2: ", DBMErrorLevel.EngineErrors, ex2);
                                }
                            }

                        }
                        catch (Exception ex3)
                        {
                            debug("TTSThread Error 3", DBMErrorLevel.EngineErrors, ex3);
                        }
                        finally
                        {
                            if (toPlay != null)
                            {
                                toPlay.Dispose();
                            }
                        }

                        Thread.Sleep(50);
                    }

                    Thread.Sleep(50);
                }
                catch (Exception ex4)
                {
                    debug("TTSThread Error 4", DBMErrorLevel.EngineErrors, ex4);
                }
            }
        }
        /**/

        public static ActorEntity cloneActorEntity(ActorEntity toClone)
        {
            ActorEntity newEntity = new ActorEntity();

            copyActorEntity(toClone, newEntity);

            return newEntity;
        }

        public static void copyActorEntity(ActorEntity fromEntity, ActorEntity toEntity)
        {
            try
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
                tmpActor.HitBoxRadius = tmpEnt.HitBoxRadius;
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
            catch (Exception ex)
            {
                debug("Error copyActorEntity", DBMErrorLevel.EngineErrors, ex);
            }

        }

        #endregion

        #region DynamicClassLoading

        public static void compileScripts()
        {
            try
            {
                string dir = getScriptsDirectory();

                DirectoryInfo di = new DirectoryInfo(dir);

                FileInfo[] files = di.GetFiles("*.cs");

                if (implementationClass != null)
                {
                    endEncounter();
                    implementationClass = null;
                }



                foreach (FileInfo fi in files)
                {
                    if (File.Exists(fi.FullName))
                    {
                        string key = Path.GetFileNameWithoutExtension(fi.FullName);

                        if (!encounters.ContainsKey(key) || fi.LastWriteTimeUtc != classModified[key])
                        {
                            try
                            {
                                Assembly tmpAssembly = loadAssemblyFile(fi.FullName);

                                if (tmpAssembly != null)
                                {
                                    encounters[key] = tmpAssembly;
                                    classModified[key] = fi.LastWriteTimeUtc;

                                    debug("Compiled (" + fi.FullName + ".cs)", DBMErrorLevel.Notice);
                                }
                            }
                            catch (Exception e)
                            {
                                debug("Compile error (" + fi.FullName + "): ", DBMErrorLevel.EncounterErrors, e);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                debug("Error compiling scripts", DBMErrorLevel.EngineErrors, ex);
            }
        }

        private static void setupClassAssembly()
        {
            try
            {
                string language = CSharpCodeProvider.GetLanguageFromExtension(".cs");

                string extAssembly;

                cp = new CompilerParameters();
                cdp = CodeDomProvider.CreateProvider(language);
                cp.GenerateExecutable = false;
                cp.GenerateInMemory = true;

                cp.TreatWarningsAsErrors = false;
                cp.IncludeDebugInformation = false;



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

                cp.ReferencedAssemblies.Add(typeof(ActorEntity).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(FFXIVAPP.Memory.Helpers.ZoneHelper).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(FFXIVAPP.Memory.Core.StatusEntry).Assembly.Location);
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
                              "FFXIVAPP.Memory.dll");
                cp.ReferencedAssemblies.Add(extAssembly);

                extAssembly = Path.Combine(
                             Constants.AppDirectory,
                              "FFXIVAPP.IPluginInterface.dll");
                cp.ReferencedAssemblies.Add(extAssembly);


                extAssembly = Path.Combine(
                             Constants.BaseDirectory,
                              AssemblyHelper.Name + ".dll");
                cp.ReferencedAssemblies.Add(extAssembly);



            }
            catch (Exception ex)
            {
                debug("Error initializing script compiler", DBMErrorLevel.EngineErrors, ex);
            }

        }


        private static Assembly loadAssemblyFile(string path)
        {
            int count = 0;
            try
            {
                Exception e = null;

                string[] source = new string[1];
                source[0] = File.ReadAllText(path);

                CompilerResults cr = null;
                try
                {
                    cr = cdp.CompileAssemblyFromSource(cp, source);
                    //CompilerResults cr = cdp.CompileAssemblyFromFile(cp, path);
                }
                catch (Exception ex2)
                {
                    debug("Error loading script (" + path + ")", DBMErrorLevel.EncounterErrors, e);
                    e = ex2;
                }

                if (cr == null)
                {

                    return null;
                }
                else if (cr.Errors.HasErrors)
                {
                    StringBuilder errors = new StringBuilder();
                    string filename = Path.GetFileName(path);
                    foreach (CompilerError err in cr.Errors)
                    {
                        if (err.ErrorNumber == "CS1504")
                        {
                            //continue;
                        }
                        errors.Append(string.Format("\r\n{0}({1},{2}): {3}: {4}",
                                    filename, err.Line, err.Column,
                                    err.ErrorNumber, err.ErrorText));
                    }
                    string str = "Error loading script\r\n" + errors.ToString();
                    if (count > 0)
                    {
                        throw new ApplicationException(str);
                    }
                    return null;
                }
                
                return cr.CompiledAssembly;


            }
            catch (Exception ex)
            {
                debug("Error loading script (" + path + ")", DBMErrorLevel.EncounterErrors, ex);
            }

            return null;
        }

        private static IEncounter findEntryPoint(Assembly assembly)
        {
            try
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
            }
            catch (Exception ex)
            {
                debug("Error locating entry point in encounter script", DBMErrorLevel.EngineErrors, ex);
            }

            return null;
        }

        

        private static Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
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
            }
            catch (Exception ex)
            {
                debug("Error OnCurrentDomainAssemblyResolve", DBMErrorLevel.EngineErrors, ex);
            }

            return null;
        }
        #endregion


    }
}
