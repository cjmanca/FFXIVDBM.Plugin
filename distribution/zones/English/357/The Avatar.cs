using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace TheAvatarEncounter
{

    public class MainEncounterLogic : AbilityController, IEncounter
    {
        // Settings
        bool calloutDefensivePush = true;
        bool calloutDefensivePushByName = false;

        bool calloutTowerStrategy = true;


        public void onStartEncounter()
        {







            // bossName is needed if you want health based phase swaps, or access to "bossEntity" for custom logic
            bossName = "The Avatar";



            TriggeredAbility EnrageTrigger = new TriggeredAbility();
            EnrageTrigger.warningMessage = "Enrage in one minute";
            EnrageTrigger.timerDuration = TimeSpan.FromMinutes(11);
            EnrageTrigger.warningTime = TimeSpan.FromSeconds(60);

            timedAbilities.Add(EnrageTrigger);

            EnrageTrigger.start();


            TriggeredAbility mineTowerWarning = new TriggeredAbility();
            mineTowerWarning.warningTime = TimeSpan.FromSeconds(5);
            mineTowerWarning.warningMessage = "Mines soon";

            TriggeredAbility dreadTowerWarning = new TriggeredAbility();
            dreadTowerWarning.warningTime = TimeSpan.FromSeconds(3);
            dreadTowerWarning.warningMessage = "Dreadnaught";

            TriggeredAbility westTowerWarning = new TriggeredAbility();
            westTowerWarning.warningTime = TimeSpan.FromSeconds(5);
            westTowerWarning.warningMessage = "Defensive reaction";

            TriggeredAbility southTowerWarning = new TriggeredAbility();
            southTowerWarning.warningTime = TimeSpan.FromSeconds(5);
            southTowerWarning.warningMessage = "Defensive reaction";


            towers[Towers.mine] = new Tower(Towers.mine, this, mineTowerWarning);
            towers[Towers.dread] = new Tower(Towers.dread, this, dreadTowerWarning);
            towers[Towers.west] = new Tower(Towers.west, this, westTowerWarning);
            towers[Towers.south] = new Tower(Towers.south, this, southTowerWarning);




            TriggeredAbility LifeDrainMessage = new TriggeredAbility();
            LifeDrainMessage.announceWarning = false;
            LifeDrainMessage.match = new Regex("The support module uses Life Drain");
            LifeDrainMessage.matchCallback = delegate(Ability self, Match match)
            {
                // life drain should happen before Languishing.
                // We take a snapshot of everyone's position here, and use that when determining which tower.
                // Otherwise, if a monk were to use shoulder tackle between life drain and languishing, it may be difficult to tell which tower he pushed
                takeLifedrainSnapshot();
            };

            timedAbilities.Add(LifeDrainMessage);

            TriggeredAbility LanguishingMessage = new TriggeredAbility();
            LanguishingMessage.collectMultipleLinesFor = TimeSpan.Zero; // set to zero so we can process each match ourselves
            LanguishingMessage.announceWarning = false;
            LanguishingMessage.match = new Regex("(?<member>.+?) suffers the effect of Languishing");
            LanguishingMessage.matchCallback = delegate(Ability self, Match match)
            {
                if (match.Groups["member"].Success)
                {
                    string dOut = "Languishing: " + match.Groups["member"].Value.Trim();

                    // snapshot is only valid for 2 seconds
                    if (!lifeDrainSnapshot.Any() || (DateTime.Now - lastSnapshot).Duration() > TimeSpan.FromSeconds(1))
                    {
                        takeLifedrainSnapshot();
                    }
                    if (lifeDrainSnapshot.ContainsKey(match.Groups["member"].Value.Trim()))
                    {
                        if (!foundCoords)
                        {
                            towers[Towers.mine].pos = new Coordinate(bossEntity.Coordinate.X + 15, bossEntity.Coordinate.Z, bossEntity.Coordinate.Y);
                            towers[Towers.dread].pos = new Coordinate(bossEntity.Coordinate.X, bossEntity.Coordinate.Z, bossEntity.Coordinate.Y - 15);
                            towers[Towers.west].pos = new Coordinate(bossEntity.Coordinate.X - 15, bossEntity.Coordinate.Z, bossEntity.Coordinate.Y);
                            towers[Towers.south].pos = new Coordinate(bossEntity.Coordinate.X, bossEntity.Coordinate.Z, bossEntity.Coordinate.Y + 15);

                            foundCoords = true;
                        }
                        Coordinate playerCoord = lifeDrainSnapshot[match.Groups["member"].Value.Trim()];

                        SortedDictionary<double, Towers> distances = new SortedDictionary<double, Towers>();

                        distances.Add(playerCoord.DistanceTo(towers[Towers.mine].pos), Towers.mine);
                        distances.Add(playerCoord.DistanceTo(towers[Towers.dread].pos), Towers.dread);
                        distances.Add(playerCoord.DistanceTo(towers[Towers.west].pos), Towers.west);
                        distances.Add(playerCoord.DistanceTo(towers[Towers.south].pos), Towers.south);

                        towers[distances.First().Value].addCharge();

                        dOut += " Tower: " + Enum.GetName(typeof(Towers), distances.First().Value);

                        debug(dOut, DBMErrorLevel.EncounterInfo);
                    }
                }
            };

            timedAbilities.Add(LanguishingMessage);





            SpawnTowers.timerDuration = TimeSpan.FromSeconds(20);
            SpawnTowers.warningTime = TimeSpan.Zero;
            SpawnTowers.warningCallback = this.startTowers;

            timedAbilities.Add(SpawnTowers);



            RotationAbility startTowers = new RotationAbility();
            startTowers.warningTime = TimeSpan.Zero;
            startTowers.warningCallback = this.startTowers;





            TriggeredAbility BrainjackOver = new TriggeredAbility();

            BrainjackOver.match = new Regex("recovers from the effect of Confused");
            BrainjackOver.matchMessage = "Brainjack over";

            timedAbilities.Add(BrainjackOver);



            TriggeredAbility AllaganFieldMessage = new TriggeredAbility();
            AllaganFieldMessage.timerDuration = TimeSpan.FromSeconds(30);
            AllaganFieldMessage.warningTime = TimeSpan.FromSeconds(5);
            AllaganFieldMessage.warningMessage = "Field exploding";

            AllaganFieldMessage.match = new Regex("(?<member>[^ ]+?) [^ ]* suffers the effect of Allagan Field");
            AllaganFieldMessage.matchMessage = "Allagan field on ${member}";
            AllaganFieldMessage.matchCallback = delegate(Ability self, Match match)
            {
                if (match.Groups["member"].Success)
                {
                    lastAllaganField = match.Groups["member"].Value;
                }
                else
                {
                    lastAllaganField = "";
                }
            };

            timedAbilities.Add(AllaganFieldMessage);



            if (calloutDefensivePush)
            {
                TriggeredAbility AllaganFieldRecovery = new TriggeredAbility();

                AllaganFieldRecovery.announceWarning = false;
                AllaganFieldRecovery.match = new Regex("(?<member>[^ ]+?) [^ ]* recovers from the effect of Allagan Field");
                AllaganFieldRecovery.matchCallback = delegate(Ability self, Match match)
                {
                    if (defensiveWarningOnFieldDrop)
                    {
                        if (calloutDefensivePushByName)
                        {
                            tts(lastAllaganField + " west tower now");
                        }
                        else
                        {
                            tts("One to west now");
                        }
                        defensiveWarningOnFieldDrop = false;
                    }
                };

                timedAbilities.Add(AllaganFieldRecovery);
            }



            RotationAbility Brainjack = new RotationAbility(); // Homing Missile
            Brainjack.announceWarning = true;
            Brainjack.match = new Regex("The Avatar readies Brainjack");
            Brainjack.warningMessage = "Brain jack";


            RotationAbility HomingMissile = new RotationAbility(); // Homing Missile
            HomingMissile.announceWarning = true;
            HomingMissile.match = new Regex("The Avatar uses Homing Missile");
            HomingMissile.warningMessage = "Homing Missile";
            HomingMissile.warningTime = TimeSpan.FromSeconds(5);


            RotationAbility GaseousBomb = new RotationAbility(); // Gaseous Bomb
            GaseousBomb.announceWarning = false;
            GaseousBomb.match = new Regex("The Avatar uses Gaseous Bomb");
            GaseousBomb.warningMessage = "Gaseous";


            RotationAbility InertiaStream = new RotationAbility(); // Inertia Stream
            InertiaStream.announceWarning = true;
            InertiaStream.match = new Regex("The Avatar uses Inertia Stream");
            InertiaStream.warningMessage = "Gather for circles";


            RotationAbility BallisticMissile = new RotationAbility(); // Ballistic Missile
            BallisticMissile.announceWarning = false;
            BallisticMissile.match = new Regex("The Avatar readies Ballistic Missile");
            BallisticMissile.warningMessage = "Ballistic Missile";


            RotationAbility NextPhase = new RotationAbility();
            NextPhase.announceWarning = false;
            NextPhase.matchRegex = false;
            NextPhase.warningTime = TimeSpan.Zero;
            NextPhase.warningCallback = delegate(Ability self)
            {
                nextPhase();
            };



            // This is just a placeholder. Rotation can watch for it to make sure the timing is right.
            RotationAbility AllaganField = new RotationAbility();
            AllaganField.announceWarning = false;
            AllaganField.match = new Regex("The Avatar uses Allagan Field");
            AllaganField.warningMessage = "";




            RotationAbility Dreadnaught = new RotationAbility(); // Dreadnaught
            RotationAbility Mines = new RotationAbility(); // Mines
            RotationAbility Defensive = new RotationAbility(); // Defensive
            RotationAbility DreadnaughtTower = new RotationAbility(); // 
            RotationAbility MineTower = new RotationAbility(); // 
            RotationAbility P3TowerSet1 = new RotationAbility();
            RotationAbility P3TowerSet2 = new RotationAbility(); // 


            // This is the old way of doing towers. Hard coded into the rotation.
            // New way understands towers much better, and will work with strategies other than the one this rotation was scripted for
            
            /** /
            Dreadnaught.announceWarning = true;
            Dreadnaught.match = new Regex("A reinforcement dreadnaught appears!");
            Dreadnaught.warningMessage = "Dreadnaught";
            Dreadnaught.warningTime = TimeSpan.FromSeconds(5);


            Mines.announceWarning = true;
            Mines.match = new Regex("Landmines have been scattered");
            Mines.warningMessage = "Mines soon";
            Mines.warningTime = TimeSpan.FromSeconds(5);


            
            Defensive.announceWarning = true;
            Defensive.match = new Regex("The support module uses Defensive Reaction.");
            Defensive.warningMessage = "Defensive reaction";
            Defensive.warningTime = TimeSpan.FromSeconds(5);



            DreadnaughtTower.announceWarning = true;
            DreadnaughtTower.matchRegex = false;
            DreadnaughtTower.warningMessage = "Two to dreadnaught tower";
            DreadnaughtTower.warningTime = TimeSpan.FromSeconds(1);
            

            
            MineTower.announceWarning = true;
            MineTower.matchRegex = false;
            MineTower.warningMessage = "Two to mine tower.";
            MineTower.warningTime = TimeSpan.FromSeconds(1);
            

            
            P3TowerSet1.announceWarning = false;
            P3TowerSet1.matchRegex = false;
            P3TowerSet1.warningMessage = "";
            P3TowerSet1.warningTime = TimeSpan.FromSeconds(1);
            P3TowerSet1.warningCallback = delegate(Ability self)
            {
                tts("Two to west defensive. One to south tower. " + lastAllaganField + " stand near west tower");
                defensiveWarningOnFieldDrop = true;
            };
             


            
            P3TowerSet2.announceWarning = true;
            P3TowerSet2.matchRegex = false;
            P3TowerSet2.warningTime = TimeSpan.FromSeconds(1);
            P3TowerSet2.warningMessage = "Two to mine tower. One to dreadnaught tower.";
            /**/
            


            int phaseNum = 1;
            phases[phaseNum] = new Phase();

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), DreadnaughtTower);      // 19:45:31:821
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), startTowers);      // 19:45:31:821

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.6771255), HomingMissile);      // 19:45:41:498
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.1835252), GaseousBomb);      // 19:45:50:681
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(13), Dreadnaught);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.0937785), HomingMissile);      // 19:46:21:775
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), GaseousBomb);      // 19:46:30:704
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Defensive);      // 


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20), MineTower);      // 19:46:54:704

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.8667655), HomingMissile);      // 19:47:1:570
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.074519), GaseousBomb);      // 19:47:10:645
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), Mines);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20), HomingMissile);      // 19:47:41:906
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9), GaseousBomb);      // 19:47:50:768
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.5), Defensive);      // 

            // maybe add in other logic for the phase switch someday, but I've never heard of anyone not phase swapping at this point
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), NextPhase);

            phases[phaseNum].phaseEndRegex = new Regex("The Avatar uses Inertia Stream");


            phaseNum = 2;
            phases[phaseNum] = new Phase();

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.4024234), InertiaStream);      // 19:47:58:171
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.255129), BallisticMissile);      // 19:48:0:426

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), DreadnaughtTower);      // 19:48:10:371

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Brainjack);      // 19:48:16:371
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.5), HomingMissile);      // 19:48:35:170
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7), Dreadnaught);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), GaseousBomb);      // 19:48:50:876
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), Brainjack);      // 19:48:56:315
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14), Defensive);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4), HomingMissile);      // 19:49:14:960

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), InertiaStream);      // 19:49:17:29
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.5), BallisticMissile);      // 19:49:19:398
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11), GaseousBomb);      // 19:49:31:70

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), MineTower);      // 19:49:36:253

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1832965), Brainjack);      // 19:49:36:253
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.8150762), HomingMissile);      // 19:49:55:68
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Mines);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), GaseousBomb);      // 19:50:11:108
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.280302), Brainjack);      // 19:50:16:389
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(15), Defensive);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.654067), HomingMissile);      // 19:50:35:43

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.1181211), InertiaStream);      // 19:50:37:161
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.4061376), BallisticMissile);      // 19:50:39:567
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.503658), GaseousBomb);      // 19:50:51:71

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), MineTower);      // 19:49:36:253

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.332305), Brainjack);      // 19:50:56:403
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.7060699), HomingMissile);      // 19:51:15:109
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), Mines);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.9849143), GaseousBomb);      // 19:51:31:94
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1712958), Brainjack);      // 19:51:36:265
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(15), Dreadnaught);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.2110988), HomingMissile);      // 19:51:55:476

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.8141038), InertiaStream);      // 19:51:57:290
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.2931311), BallisticMissile);      // 19:51:59:584
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.503658), GaseousBomb);      // 19:52:10.503
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.332305), Brainjack);      // 19:52:15.835
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20), RotationAbility.Blank());
            // Next phase should be here somewhere. Let Allagan Field trigger it.
            //phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), NextPhase);      // 19:52:15.835


            phases[phaseNum].phaseEndRegex = new Regex("The Avatar readies Allagan Field");


            phaseNum = 3;
            phases[phaseNum] = new Phase();

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), AllaganField);      // 20:12:25.00 / 19:52:3:380
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.6915543), HomingMissile);      // 20:12:35.00

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), P3TowerSet1);      // 20:12:43.00

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), GaseousBomb);      // 20:12:43.00


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(22), AllaganField);      // 20:13:05.00
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), HomingMissile);      // 20:13:15.00
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), GaseousBomb);      // 20:13:23.00

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Defensive);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(16), AllaganField);      // 20:13:45.00
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), HomingMissile);      // 20:13:55.00
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Mines);      //
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7), GaseousBomb);      // 20:14:03.00


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(13), P3TowerSet2);      // 20:14:03.00

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9), AllaganField);      // 20:14:25.00
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), HomingMissile);      // 20:14:35.00
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), GaseousBomb);      // 20:14:43.00

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Mines);      // 

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14), AllaganField);      // 20:15:05.00
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), HomingMissile);      // 20:15:15.00
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Dreadnaught);      // 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), GaseousBomb);      // 20:15:23.00

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(12), Defensive);      // 

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), RotationAbility.Blank());      // 20:15:45.00

            
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

        public void onNewChatLine(string line)
        {

        }

        public void onTick()
        {

        }




        string lastAllaganField = "";
        bool defensiveWarningOnFieldDrop = false;
        bool foundCoords = false;

        DateTime lastSnapshot = DateTime.Now;

        int P1TowerNumber = 0;
        int P2TowerNumber = 0;
        int P3TowerNumber = 0;

        Dictionary<string, Coordinate> lifeDrainSnapshot = new Dictionary<string, Coordinate>();

        Dictionary<Towers, Tower> towers = new Dictionary<Towers, Tower>();

        TriggeredAbility SpawnTowers = new TriggeredAbility();

        enum Towers
        {
            mine = 1,
            dread = 2,
            west = 3,
            south = 4
        }


        public void takeLifedrainSnapshot()
        {
            lifeDrainSnapshot.Clear();

            foreach (ActorEntity pe in pcEntities)
            {
                lifeDrainSnapshot[pe.Name.Trim()] = new Coordinate(pe.Coordinate.X, pe.Coordinate.Z, pe.Coordinate.Y);
            }

            lastSnapshot = DateTime.Now;
        }

        public void startTowers(Ability abil)
        {
            if (phase == 1)
            {
                P1TowerNumber++;

                switch (P1TowerNumber)
                {
                    case 1:
                        towers[Towers.dread].start();
                        towers[Towers.west].start();

                        if (calloutTowerStrategy)
                        {
                            tts("Team 1 to dreadnaught");
                        }
                        else
                        {
                            tts("Tower set 1");
                        }
                        break;
                    case 2:
                        towers[Towers.mine].start();
                        towers[Towers.west].start();

                        if (calloutTowerStrategy)
                        {
                            tts("Team 2 to mines");
                        }
                        else
                        {
                            tts("Tower set 2");
                        }
                        break;

                }
            }
            else if (phase == 2)
            {
                P2TowerNumber++;

                switch (P2TowerNumber)
                {
                    case 1:
                        towers[Towers.dread].start();
                        towers[Towers.west].start();

                        if (calloutTowerStrategy)
                        {
                            tts("Team 1 to dreadnaught");
                        }
                        else
                        {
                            tts("Tower set 3");
                        }
                        break;
                    case 2:
                        towers[Towers.mine].start();
                        towers[Towers.west].start();

                        if (calloutTowerStrategy)
                        {
                            tts("Team 2 to mines");
                        }
                        else
                        {
                            tts("Tower set 4");
                        }
                        break;
                    case 3:
                        towers[Towers.dread].start();
                        towers[Towers.mine].start();

                        if (calloutTowerStrategy)
                        {
                            tts("Team 1 to mines");
                        }
                        else
                        {
                            tts("Tower set 5");
                        }
                        break;

                }
            }
            else if (phase == 3)
            {
                P3TowerNumber++;

                switch (P3TowerNumber)
                {
                    case 1:
                        towers[Towers.mine].start();
                        towers[Towers.west].start();
                        towers[Towers.south].start();

                        if (calloutTowerStrategy)
                        {
                            tts("Two to west, one to south");
                        }
                        else
                        {
                            tts("Tower set 6");
                        }
                        break;
                    case 2:
                        towers[Towers.dread].start();
                        towers[Towers.mine].start();
                        towers[Towers.west].start();

                        if (calloutTowerStrategy)
                        {
                            tts("Two to mines, one to dreadnaught");
                        }
                        else
                        {
                            tts("Tower set 7");
                        }
                        break;
                    case 3:
                        towers[Towers.mine].start();
                        towers[Towers.west].start();
                        towers[Towers.south].start();

                        if (calloutTowerStrategy)
                        {
                            tts("Two to west, one to south");
                        }
                        else
                        {
                            tts("Tower set 8");
                        }
                        break;

                }
            }
        }

        public void checkTowers()
        {
            bool anyActive = false;

            foreach (KeyValuePair<Towers, Tower> tower in towers)
            {
                if (tower.Value.isActive())
                {
                    anyActive = true;
                }
            }

            if (!anyActive)
            {
                SpawnTowers.start();
            }
        }

        private class Tower
        {
            public Coordinate pos = null;


            private Towers whichTower = Towers.mine;
            private MainEncounterLogic parent;
            private bool active = false;
            private int charges = 0;
            private int forcedCharges = 0;
            private TriggeredAbility warningAbility = null;
            private TriggeredAbility nextTick = null;



            public Tower(Towers tower, MainEncounterLogic caller, TriggeredAbility ability)
            {
                whichTower = tower;
                parent = caller;
                warningAbility = ability;
                nextTick = new TriggeredAbility();

                nextTick.warningTime = TimeSpan.Zero;
                nextTick.warningCallback = this.tick;

                parent.timedAbilities.Add(warningAbility);
                parent.timedAbilities.Add(nextTick);
            }

            public bool isActive()
            {
                return active;
            }

            public void reset()
            {
                active = false;
                charges = 0;
                warningAbility.timerAutoRestart = false;
                nextTick.timerAutoRestart = false;
                nextTick.timerStarted = false;
            }


            public void start()
            {
                reset();

                active = true;

                if (parent.phase < 3)
                {
                    warningAbility.timerDuration = TimeSpan.FromSeconds(60);
                    nextTick.timerDuration = TimeSpan.FromSeconds(15);
                }
                else
                {
                    warningAbility.timerDuration = TimeSpan.FromSeconds(80);
                    nextTick.timerDuration = TimeSpan.FromSeconds(20);
                }

                nextTick.timerAutoRestart = true;
                nextTick.start();

                warningAbility.start();

                addCharge(false);

                debug("Tower started: " + Enum.GetName(typeof(Towers), whichTower), DBMErrorLevel.EncounterInfo);
            }


            public void addCharge(bool forced = true)
            {
                int chargeTime = 15;
                if (active)
                {
                    charges++;

                    if (forced)
                    {
                        //debug("Tower pushed (addCharge): " + Enum.GetName(typeof(Towers), whichTower), DBMErrorLevel.EncounterInfo);
                        if (parent.phase < 3)
                        {
                            chargeTime = 15;
                        }
                        else
                        {
                            chargeTime = 20;
                        }

                        warningAbility.nextAbilityTime = warningAbility.nextAbilityTime - TimeSpan.FromSeconds(chargeTime);
                    }
                    else
                    {
                        //debug("Tower pushed (tick): " + Enum.GetName(typeof(Towers), whichTower), DBMErrorLevel.EncounterInfo);
                    }

                    checkCharges();
                }
            }


            private void tick(Ability self)
            {
                if (active)
                {
                    addCharge(false);
                    checkCharges();
                }
            }

            private void checkCharges()
            {
                if (active)
                {
                    if (charges >= 5)
                    {
                        debug("Tower done: " + Enum.GetName(typeof(Towers), whichTower), DBMErrorLevel.EncounterInfo);
                        // Don't need to do anything here. The warningAbility will have handled the warning already.
                        // Just need to cleanup
                        reset();
                        parent.checkTowers();
                    }
                }
            }
        }




        public void onMobAgro(ActorEntity mob)
        {

        }


        public void onAgroRemoved(ActorEntity mob)
        {

        }
    }
}