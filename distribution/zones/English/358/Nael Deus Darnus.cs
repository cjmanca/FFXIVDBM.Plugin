using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace NaelDeusDarnusNS
{

    public class MainEncounterLogic : AbilityController, IEncounter
    {

        public void onStartEncounter()
        {
            // Enrage @ 13 minutes


            // bossName is needed if you want health based phase swaps
            bossName = "Nael Deus Darnus";


            // Triggered abilities

            // Enrage Timer
            TriggeredAbility EnrageTrigger = new TriggeredAbility();
            timedAbilities.Add(EnrageTrigger);
            EnrageTrigger.warningMessage = "Enrage in one minute";
            EnrageTrigger.timerDuration = TimeSpan.FromMinutes(13);
            EnrageTrigger.warningTime = TimeSpan.FromSeconds(65);


            EnrageTrigger.start();


            // Person with thunderstruck needs to get away from everyone else
            TriggeredAbility ThunderstruckTrigger = new TriggeredAbility();
            timedAbilities.Add(ThunderstruckTrigger);
            ThunderstruckTrigger.match = new Regex(@"(?<member>[^ ]+?) [^ ]* suffers the effect of Thunderstruck\.");
            ThunderstruckTrigger.matchMessage = "${member} Thunder";


            // When this drops, need to be about 55% - 75% along the radius of the arena away from the edge (0% is at edge, 100% is at center of arena)
            TriggeredAbility HeavensfallTrigger = new TriggeredAbility();
            timedAbilities.Add(HeavensfallTrigger);
            HeavensfallTrigger.match = new Regex(@" readies Heavensfall\.");
            HeavensfallTrigger.matchMessage = "Heaven's fall";


            // Needs to be silenced
            TriggeredAbility EarthshockTrigger = new TriggeredAbility();
            timedAbilities.Add(EarthshockTrigger);
            EarthshockTrigger.match = new Regex(@" readies Earthshock\.");
            EarthshockTrigger.matchMessage = "Earth shock";



            // Blight lasts 12 seconds. Warn 5 seconds before it ends (12-5=7)
            TriggeredAbility RavenBlightTrigger = new TriggeredAbility();
            timedAbilities.Add(RavenBlightTrigger);
            RavenBlightTrigger.match = new Regex(@" suffers the effect of Raven Blight\.");
            RavenBlightTrigger.warningMessage = "Blight";
            RavenBlightTrigger.timerDuration = TimeSpan.FromSeconds(12);
            RavenBlightTrigger.warningTime = TimeSpan.FromSeconds(5);



            TriggeredAbility GarroteTwist = new TriggeredAbility(); // Garrote Twist
            timedAbilities.Add(GarroteTwist);
            GarroteTwist.announceWarning = true;
            GarroteTwist.match = new Regex(@"(?<member>[^ ]+?) [^ ]* suffers the effect of Garrote Twist");
            GarroteTwist.matchMessage = @"${member} Twist"; 
            

            // Rotation abilities

            // 4k conal AOE
            RotationAbility Ravensclaw = new RotationAbility();
            Ravensclaw.announceWarning = false;
            Ravensclaw.match = new Regex(@" uses Ravensclaw\.");
            Ravensclaw.warningMessage = "Ravensclaw";

            // T9 version of Death sentance. On 47 second timer outside of rotation. Places Raven's Blight debuff on tank, which does big damage when it wears off
            RotationAbility Ravensbeak = new RotationAbility();
            Ravensbeak.announceWarning = true;
            Ravensbeak.match = new Regex(@" readies Ravensbeak\.");
            Ravensbeak.warningMessage = "Ravensbeak";
            Ravensbeak.warningTime = TimeSpan.FromSeconds(4);

            RotationAbility RavenBlight = new RotationAbility(); // Raven Blight
            RavenBlight.announceWarning = false;
            RavenBlight.match = new Regex(@" suffers the effect of Raven Blight\.");
            RavenBlight.warningMessage = @"Raven Blight"; 


            // This is the damage from Raven Blight after it wears off (applied by Ravensbeak)
            // Not announcing here, since I have a triggered ability above for it (better timing)
            // This is just here to help with rotation timings
            RotationAbility RavensAscent = new RotationAbility();
            RavensAscent.announceWarning = false;
            RavensAscent.match = new Regex(@" uses Raven's Ascent\.");
            RavensAscent.warningMessage = "Raven's Ascent";


            // Stardust on  38 second timer, outside of rotation. Starts at ???
            // Meteor lands 12 seconds after stardust
            // Announcing 2 seconds after it happens (right when icon should show up), since this will just act as a prompt to look for the icon above your head
            RotationAbility Stardust = new RotationAbility();
            Stardust.announceWarning = true;
            Stardust.match = new Regex(@" readies Stardust\.");
            Stardust.warningMessage = "Stardust";
            Stardust.warningTime = TimeSpan.FromSeconds(-2);


            // Jumps on main tank.
            RotationAbility RavenDive = new RotationAbility();
            RavenDive.announceWarning = false;
            RavenDive.match = new Regex(@" uses Raven Dive\.");
            RavenDive.warningMessage = "Raven Dive";


            // Iron chariot point blank AOE, which starts off the chariot/dive/thermionic beam combo
            RotationAbility IronChariot = new RotationAbility();
            IronChariot.announceWarning = true;
            IronChariot.match = new Regex(@" readies Iron Chariot\.");
            IronChariot.warningMessage = "Chariot";
            IronChariot.warningTime = TimeSpan.FromSeconds(7);


            // Damage is split by all who it hits. Very small range. Need to stack tight on one person (usually in the center of the arena)
            RotationAbility ThermionicBeam = new RotationAbility(); // Inertia Stream
            ThermionicBeam.announceWarning = true;
            ThermionicBeam.match = new Regex(@" uses Thermionic Beam\.");
            ThermionicBeam.warningMessage = "Thermeonic";
            ThermionicBeam.warningTime = TimeSpan.FromSeconds(7);


            RotationAbility LunarDynamo = new RotationAbility();
            LunarDynamo.announceWarning = true;
            LunarDynamo.match = new Regex(@" readies Lunar Dynamo\.");
            LunarDynamo.warningMessage = "Lunar Dynamo";
            LunarDynamo.warningTime = TimeSpan.FromSeconds(5);



            RotationAbility MeteorStream = new RotationAbility();
            MeteorStream.announceWarning = true;
            MeteorStream.match = new Regex(@" uses Meteor Stream\.");
            MeteorStream.warningMessage = "Meteor Stream";

            RotationAbility DalamudDive = new RotationAbility();
            DalamudDive.announceWarning = false;
            DalamudDive.match = new Regex(@" uses Dalamud Dive\.");
            DalamudDive.warningMessage = "Dalamud Dive";

            RotationAbility MeteorImpact = new RotationAbility(); // Meteor Impact
            MeteorImpact.announceWarning = false;
            MeteorImpact.match = new Regex(@" uses Meteor Impact\.");
            MeteorImpact.warningMessage = @"Meteor Impact"; 
            

            /*
            RotationAbility GarroteTwist = new RotationAbility();
            GarroteTwist.announceWarning = true;
            GarroteTwist.match = new Regex("uses Garrote Twist");
            GarroteTwist.warningMessage = "Garrote Twist";

            RotationAbility Heavensfall = new RotationAbility();
            Heavensfall.announceWarning = true;
            Heavensfall.match = new Regex("readies Heavensfall");
            Heavensfall.warningMessage = "Heaven's fall";
            */

            RotationAbility BahamutsFavor = new RotationAbility();
            BahamutsFavor.announceWarning = false;
            BahamutsFavor.match = new Regex(@"Nael Deus Darnus uses Bahamut's Favor\.");
            BahamutsFavor.warningMessage = "Bahamut's Favor";

            RotationAbility BahamutsClaw = new RotationAbility();
            BahamutsClaw.announceWarning = false;
            BahamutsClaw.match = new Regex(@"Nael Deus Darnus uses Bahamut's Claw\.");
            BahamutsClaw.warningMessage = "Bahamut's Claw";

            RotationAbility FireTetherOut = new RotationAbility();
            FireTetherOut.announceWarning = true;
            FireTetherOut.match = new Regex(@"uses Fire Tether");
            FireTetherOut.warningMessage = "Fire away";
            FireTetherOut.warningTime = TimeSpan.FromSeconds(9);

            RotationAbility FireTetherIn = new RotationAbility();
            FireTetherIn.announceWarning = true;
            FireTetherIn.match = new Regex(@"uses Fire Tether");
            FireTetherIn.warningMessage = "Fire in";
            FireTetherIn.warningTime = TimeSpan.FromSeconds(9);
            FireTetherIn.warningCallback = delegate(Ability self)
            {
                // PartyList seems to be broken currently in FFXIV-APP, so disabling this until it's fixed
                return;

                string output = "except ";

                foreach (PartyEntity member in partyList)
                {
                    foreach (StatusEntry status in member.StatusEntries)
                    {
                        if (status.StatusName == "Firescorched")
                        {
                            output += member.Name + " ";
                        }
                    }
                }

                if (output != "except ")
                {
                    tts(output);
                }
            };

            /*
            RotationAbility Thunderstruck = new RotationAbility();
            Thunderstruck.announceWarning = true;
            Thunderstruck.match = new Regex("uses Thunderstruck");
            Thunderstruck.warningMessage = "Thunderstruck";
            */

            RotationAbility SuperNova = new RotationAbility();
            SuperNova.announceWarning = false;
            SuperNova.match = new Regex(@" uses SuperNova\.");
            SuperNova.warningMessage = "Super Nova";
            SuperNova.warningTime = TimeSpan.FromSeconds(7);

            RotationAbility DoubleDragonDivebombs = new RotationAbility();
            DoubleDragonDivebombs.announceWarning = true;
            DoubleDragonDivebombs.warningMessage = "Divebombs soon";
            DoubleDragonDivebombs.warningCallback = delegate(Ability self)
            {
                string output = "";

                foreach (ActorEntity mob in mobList)
                {
                    output += Environment.NewLine + "Mob: " + mob.Name + " (" + mob.Coordinate.X + ", " + mob.Coordinate.Y + ", " + mob.Coordinate.Z + ")";
                }
                foreach (ActorEntity mob in pcEntities)
                {
                    output += Environment.NewLine + "Player: " + mob.Name + " (" + mob.Coordinate.X + ", " + mob.Coordinate.Y + ", " + mob.Coordinate.Z + ")";
                }

                debug(output, DBMErrorLevel.EncounterInfo);
            };

            RotationAbility SingleDivebomb = new RotationAbility();
            SingleDivebomb.announceWarning = false;
            SingleDivebomb.warningMessage = "Divebomb";


            int phaseNum = 1;
            phases[phaseNum] = new Phase();
            phases[phaseNum].phaseEndHP = 65;


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), Ravensclaw);      // 20:20:12:381 @ 99.7669476312431%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.1666387), Ravensclaw);      // 20:20:23:548 @ 97.2918055007918%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.343134), Stardust);      // 20:20:25:891 @ 96.9166541742199%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.0644612), Ravensbeak);      // 20:20:33:955 @ 94.6969878296274%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.374193), RavenBlight);      // 20:20:37:329 @ 93.9135539478441%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0020002), MeteorImpact);      // 20:20:37:331 @ 93.9135539478441%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2323564), RavenDive);      // 20:20:43:564 @ 92.5051617950323%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.3050747), IronChariot);      // 20:20:44:869 @ 92.2785341131979%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1972972), RavensAscent);      // 20:20:50:66 @ 92.0977610595946%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.2690154), ThermionicBeam);      // 20:20:50:335 @ 92.0977610595946%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.9723416), Ravensclaw);      // 20:20:56:308 @ 91.5090833735401%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.5324309), Stardust);      // 20:21:3:840 @ 90.4482541858095%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.8274477), Ravensclaw);      // 20:21:11:667 @ 88.884031881564%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.5771474), LunarDynamo);      // 20:21:14:245 @ 88.2785996198097%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.2960741), MeteorImpact);      // 20:21:15:541 @ 88.159679924617%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1692385), MeteorStream);      // 20:21:19:710 @ 87.7250687504488%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.1141781), DalamudDive);      // 20:21:22:824 @ 87.7250687504488%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.4493117), Ravensbeak);      // 20:21:28:273 @ 87.293229009855%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.392194), RavenBlight);      // 20:21:31:666 @ 86.8082281343339%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.4953715), Ravensclaw);      // 20:21:38:161 @ 85.3030877549246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.342134), Stardust);      // 20:21:40:503 @ 84.8061193253323%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1522375), RavensAscent);      // 20:21:44:655 @ 83.9348813889417%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.5474316), MeteorImpact);      // 20:21:52:203 @ 82.3632266037467%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.8551634), MeteorStream);      // 20:21:55:58 @ 82.1137975820502%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.3104753), DalamudDive);      // 20:22:3:368 @ 82.1137975820502%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9562835), Ravensclaw);      // 20:22:8:325 @ 81.9026647333818%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.4833708), Ravensbeak);      // 20:22:14:808 @ 80.9578578330161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3881938), RavenBlight);      // 20:22:18:196 @ 80.5018310357729%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.8481629), Stardust);      // 20:22:21:44 @ 80.1182394342244%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.291417), Ravensclaw);      // 20:22:28:336 @ 78.8102487613581%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.5911482), RavensAscent);      // 20:22:30:927 @ 78.3490570197263%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.5580892), MeteorImpact);      // 20:22:32:485 @ 78.2656620639874%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.8661639), RavenDive);      // 20:22:35:351 @ 77.9222562492678%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.0330591), IronChariot);      // 20:22:36:384 @ 77.7649144067936%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9402826), ThermionicBeam);      // 20:22:41:325 @ 77.4672372460516%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.9306251), Ravensclaw);      // 20:22:52:255 @ 76.5414524579466%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.866164), Stardust);      // 20:22:55:121 @ 76.0711905700713%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.7794449), Ravensbeak);      // 20:23:2:901 @ 74.8106921907301%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.6432084), RavenBlight);      // 20:23:6:544 @ 74.3047795891476%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.2560147), MeteorImpact);      // 20:23:6:800 @ 74.3047795891476%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.8551633), LunarDynamo);      // 20:23:9:655 @ 73.7472175436784%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.4643125), MeteorStream);      // 20:23:15:119 @ 73.1727749482561%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.1221786), DalamudDive);      // 20:23:18:242 @ 73.1727749482561%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.424253), Ravensclaw);      // 20:23:22:666 @ 72.9851992849701%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.7884455), Stardust);      // 20:23:30:454 @ 72.0324560066414%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.2804164), Ravensclaw);      // 20:23:37:735 @ 71.0940737931981%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.4282533), MeteorImpact);      // 20:23:42:163 @ 70.5569195767769%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.4202528), MeteorStream);      // 20:23:46:583 @ 70.0340004509878%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.0484604), DalamudDive);      // 20:23:54:632 @ 70.0340004509878%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9442828), Ravensbeak);      // 20:23:59:576 @ 69.7943974210551%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3731929), RavenBlight);      // 20:24:2:949 @ 69.2946575578883%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2383568), Ravensclaw);      // 20:24:9:188 @ 68.3382610262115%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.8701642), Stardust);      // 20:24:12:58 @ 67.9610941115855%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3681926), RavensAscent);      // 20:24:15:426 @ 67.4105866243057%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.3294765), MeteorImpact);      // 20:24:23:755 @ 66.2358767115177%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.5600892), RavenDive);      // 20:24:25:315 @ 66.0641738041579%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.032059), IronChariot);      // 20:24:26:348 @ 66.0321763437988%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.7443286), ThermionicBeam);      // 20:24:32:92 @ 65.8216733663974%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.6592665), Ravensclaw);      // 20:24:36:751 @ 65.5844638472789%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(200), RotationAbility.Blank());      // 20:24:36:751 @ 65.5844638472789%
            // I couldn't make my group kill slower than this, so no idea where the rotation goes after 7 meteors.
            // Shouldn't be an issue though - usually you won't have more than 4 or 5 meteors
            
            /** /
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Ravensclaw);      // 00:06
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11), Ravensclaw);      // 00:17
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), Stardust);      // 00:22
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Ravensbeak);      // 00:28
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), RedMeteorLands);      // 00:31

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), RavenDive);      // 00:37
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1), IronChariot);      // 00:38
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), ThermionicBeam);      // 00:43
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), RavensAscent);      // 00:43
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), RavenDive);      // 00:45

            // Ravensbeak here?

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), Ravensclaw);      // 00:50
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7), Stardust);      // 00:57
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), Ravensclaw);      // 01:05
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), LunarDynamo);      // 01:08
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1), YellowMeteorLands);      // 01:09
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), MeteorStream);      // 01:14
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), DalamudDive);      // 01:16
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Ravensbeak);      // 01:22
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), Ravensclaw);      // 01:32
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), Stardust);      // 01:34
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), RavensAscent);      // 01:37
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9), RedMeteorLands);      // 01:46
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), MeteorStream);      // 01:49
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), MeteorStream);      // 01:54
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), DalamudDive);      // 01:57
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), Ravensclaw);      // 02:02
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Ravensbeak);      // 02:08
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Stardust);      // 02:14
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9), Ravensclaw);      // 02:23
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1), RavensAscent);      // 02:24
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), YellowMeteorLands);      // 02:26

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), RavenDive);      // 02:28
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), IronChariot);      // 02:30
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), ThermionicBeam);      // 02:35
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), RavenDive);      // 02:37

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9), Ravensclaw);      // 02:46
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), Stardust);      // 02:49
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), Ravensbeak);      // 02:57
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), RedMeteorLands);      // 03:00
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), LunarDynamo);      // 03:02
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), MeteorStream);      // 03:07
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), RavensAscent);      // 03:09
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), Ravensbeak);      // ??
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), Stardust);      // ??
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), MeteorStream);      // ??
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), MeteorStream);      // ??
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), YellowMeteorLands);      // ??
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(200), RotationAbility.Blank());      // ??
            /**/


            phaseNum = 2;
            phases[phaseNum] = new Phase();
            phases[phaseNum].phaseEndRegex = new Regex("Nael deus Darnus uses Megaflare");
            // Golems


            phaseNum = 3;
            phases[phaseNum] = new Phase();
            phases[phaseNum].phaseEndRegex = new Regex("Nael deus Darnus uses Bahamut's Favor");
            // Heavenfall/ghosts
            // Start @ 6:06

            //phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Heavensfall); 


            phaseNum = 4;
            phases[phaseNum] = new Phase();
            // Dragons/soft enrage

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), BahamutsFavor);     
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.5013718), BahamutsClaw);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.4737135), FireTetherOut); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.7770444), LunarDynamo);   
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.1926402), FireTetherIn);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.8685644), IronChariot);   
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.3431341), FireTetherOut);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.3771931), SuperNova);   
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2523576), ThermionicBeam);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3321334), FireTetherIn);   
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1582379), BahamutsClaw);     
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7), DoubleDragonDivebombs);   
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.8188476), MeteorStream); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.3945945), DalamudDive);  


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(12.4797138), BahamutsFavor);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2383568), BahamutsClaw);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(12.4867142), LunarDynamo);   
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0030002), FireTetherOut);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(12.2156987), FireTetherIn);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.8735075), IronChariot);      
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.0841764), FireTetherOut); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.0741186), SuperNova);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.4673128), ThermionicBeam);   
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.6742673), FireTetherIn);    
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.0741186), BahamutsClaw);   
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7), DoubleDragonDivebombs);  
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.9186245), IronChariot);    
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0184015), RavenDive);    
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.3000743), LunarDynamo);    

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(19.7651305), RotationAbility.Blank());      // 11:08



        }



        public void onMobAdded(ActorEntity mob)
        {
            if (mob.Name.Contains("The Ghost Of Meracydia"))
            {
                tts("Add");
            }
        }
        
        public void onAgroRemoved(ActorEntity mob)
        {
            // PartyList seems to be broken currently in FFXIV-APP, so disabling this until it's fixed
            return;
            if (mob.Name.Contains("The Ghost Of Meracydia"))
            {
                bool found = false;
                foreach (PartyEntity player in partyList)
                {
                    try
                    {
                        debug("player: " + player.Name);
                        foreach (StatusEntry status in player.StatusEntries)
                        {
                            try
                            {
                                debug("player: " + player.Name + " - " + status.StatusName);
                                if (status.StatusName.Contains("Garrote Twist"))
                                {
                                    found = true;
                                    tts(player.Name);
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                if (found)
                {
                    tts("to white circles");
                }
            }
            
        }


        public void onEndEncounter()
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


        public void onMobAgro(ActorEntity mob)
        {

        }

    }
}