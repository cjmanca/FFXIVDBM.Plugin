using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace RafflesiaNS
{

    public class MainEncounterLogic : AbilityController, IEncounter
    {
        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Rafflesia";
            int phaseNum = 1; // for convenience



            TriggeredAbility HoneyGlazed = new TriggeredAbility();

            HoneyGlazed.match = new Regex("(?<beemember>[^ ]+?) [^ ]* suffers the effect of Honey-glazed");
            HoneyGlazed.matchMessage = "Bee is up";
            //HoneyGlazed.warningMessage = "${beemember} get devoured"; // no longer need to be devoured
            HoneyGlazed.timerDuration = TimeSpan.FromSeconds(25);
            HoneyGlazed.warningTime = TimeSpan.FromSeconds(0);

            timedAbilities.Add(HoneyGlazed);



            TriggeredAbility LeafstormTriggered = new TriggeredAbility();

            LeafstormTriggered.match = new Regex("The rafflesia readies Leafstorm");
            LeafstormTriggered.matchMessage = "Leaf storm";

            timedAbilities.Add(LeafstormTriggered);



            TriggeredAbility Blighted = new TriggeredAbility();

            Blighted.match = new Regex("The rafflesia readies Blighted Bouquet");
            Blighted.matchMessage = "Stay still";

            timedAbilities.Add(Blighted);

            




            // Boss's abilities
            RotationAbility BloodyCaress = new RotationAbility(); // Bloody Caress
            BloodyCaress.announceWarning = false;
            BloodyCaress.match = new Regex(@" uses Bloody Caress\.");
            BloodyCaress.warningMessage = @"Bloody Caress";


            RotationAbility ThornyVine = new RotationAbility(); // Thorny Vine
            ThornyVine.announceWarning = false;
            ThornyVine.match = new Regex(@" suffers the effect of Thorny Vine");
            ThornyVine.warningMessage = @"Vines";


            RotationAbility BriaryGrowth = new RotationAbility(); // Briary Growth
            BriaryGrowth.announceWarning = false;
            BriaryGrowth.match = new Regex(@" uses Briary Growth\.");
            BriaryGrowth.warningMessage = @"Briary Growth";

            RotationAbility FloralTrap = new RotationAbility(); // Floral Trap
            FloralTrap.announceWarning = false;
            FloralTrap.match = new Regex(@" uses Floral Trap\.");
            FloralTrap.warningMessage = @"Floral Trap";

            RotationAbility Devour = new RotationAbility(); // Devour
            Devour.announceWarning = true;
            Devour.match = new Regex(@" uses Devour\.");
            Devour.warningMessage = @"Devour";
            Devour.warningTime = TimeSpan.FromSeconds(10);

            RotationAbility Spit = new RotationAbility(); // Spit
            Spit.announceWarning = false;
            Spit.match = new Regex(@" readies Spit\.");
            Spit.warningMessage = @"Spit";

            RotationAbility BlightedBouquet = new RotationAbility(); // Blighted Bouquet
            BlightedBouquet.announceWarning = true;
            BlightedBouquet.match = new Regex(@" readies Blighted Bouquet\.");
            BlightedBouquet.warningMessage = @"Blighted Bouquet";
            BlightedBouquet.warningTime = TimeSpan.FromSeconds(3);

            RotationAbility ViscidEmission = new RotationAbility(); // Viscid Emission
            ViscidEmission.announceWarning = false;
            ViscidEmission.match = new Regex(@" uses Viscid Emission\.");
            ViscidEmission.warningMessage = @"Viscid Emission";

            RotationAbility Leafstorm = new RotationAbility(); // Leafstorm
            Leafstorm.announceWarning = false;
            Leafstorm.match = new Regex(@" uses Leafstorm");
            Leafstorm.warningMessage = @"Leafstorm";

            RotationAbility AcidRain = new RotationAbility(); // Acid Rain
            AcidRain.announceWarning = true;
            AcidRain.match = new Regex(@" readies Acid Rain");
            AcidRain.warningMessage = @"Acid Rain";

            RotationAbility Swarm = new RotationAbility(); // Swarm
            Swarm.announceWarning = true;
            Swarm.match = new Regex(@" readies Swarm\.");
            Swarm.warningMessage = @"Swarm";

            RotationAbility RottenStench = new RotationAbility(); // Rotten Stench
            RottenStench.announceWarning = true;
            RottenStench.match = new Regex(@" uses Rotten Stench\.");
            RottenStench.warningMessage = @"Rotten Stench";



            phaseNum = 1;
            phases[phaseNum] = new Phase();

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), BloodyCaress);      // 19:48:24:941
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4), ThornyVine);      // 19:48:28
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.5346598), BriaryGrowth);      // 19:48:36:476
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.6230928), BloodyCaress);      // 19:48:38:99
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), ThornyVine);              // 19:48:46:99
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.8748508), FloralTrap);      // 19:48:52:974
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.7561004), Devour);      // 19:48:54:730
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.3300761), Spit);      // 19:48:56:60
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.709441), BloodyCaress);      // 19:49:3:769

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), ThornyVine);              // 19:49:11:769

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.8926802), BloodyCaress);      // 19:49:15:662
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.41931), BriaryGrowth);      // 19:49:21:81
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.4563692), BloodyCaress);      // 19:49:27:538

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), ThornyVine);              // 19:49:29:538

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.9805709), FloralTrap);      // 19:49:37:518
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.0681183), Devour);      // 19:49:39:586
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.0800618), Spit);      // 19:49:40:666
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(12.8897372), RotationAbility.Blank());      // 19:49:53:556


            phases[phaseNum].phaseEndRegex = new Regex(@" readies Blighted Bouquet");
            //phases[phaseNum].phaseEndHP = 69;
            // Ends at 70, but also search for Blighted Bouquet, just in case


            phaseNum = 2;
            phases[phaseNum] = new Phase();
            //phases[phaseNum].phaseStartDelay = TimeSpan.FromSeconds(3);

            //phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), BloodyCaress);      // 19:50:18:571
            //phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.1051204), BlightedBouquet);      // 19:50:20:881

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), BlightedBouquet);      // 19:50:20:881
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.8725647), BriaryGrowth);      // 19:50:30:753
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.5852622), BloodyCaress);      // 19:50:35:339

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4), ThornyVine);              // 19:50:39:339

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.8386772), FloralTrap);      // 19:50:47:177
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3001315), Devour);      // 19:50:49:477
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.1110636), Spit);      // 19:50:50:588
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.077462), ViscidEmission);      // 19:50:58:666
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.9062234), BloodyCaress);      // 19:51:2:572

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), ThornyVine);              // 19:51:5:572

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.4674843), BlightedBouquet);      // 19:51:11:40
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.5324881), BloodyCaress);      // 19:51:19:572
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3561347), BriaryGrowth);      // 19:51:21:928

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), ThornyVine);              // 19:51:29:928

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.3658789), FloralTrap);      // 19:51:37:294
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.1531231), Devour);      // 19:51:39:447
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.0600607), Spit);      // 19:51:40:507
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.3703643), BloodyCaress);      // 19:51:46:878

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), ThornyVine);              // 19:51:46:878


            // moved these to the bottom of this rotation, so that blighted is right at the start
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3), BloodyCaress);      // 19:50:18:571
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.1051204), RotationAbility.Blank());      // 19:50:18:571



            phases[phaseNum].phaseEndRegex = new Regex(@" readies Leafstorm");
            phases[phaseNum].phaseEndHP = 39;


            phaseNum = 3;
            phases[phaseNum] = new Phase();

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), Leafstorm);      // 19:38:23:727
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.8), AcidRain);      // 19:38:31:530

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9), ThornyVine);              // 19:38:40:530

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.3), Leafstorm);      // 19:38:40:834
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9), Swarm);      // 19:38:49:848
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6), Leafstorm);      // 19:38:55:918

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), ThornyVine);              // 19:39:05:918

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), Leafstorm);      // 19:39:10:925
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.5), RottenStench);      // 19:39:11:361
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11), Swarm);      // 19:39:22:382
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.4), Leafstorm);      // 19:39:25:771

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(13), ThornyVine);              // 19:39:38:771

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.4), Leafstorm);      // 19:39:41:203
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2), Swarm);      // 19:39:47:420

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8), ThornyVine);              // 19:39:47:420

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.5), Leafstorm);      // 19:39:55:972
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5), RottenStench);      // 19:40:1:71
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10), Leafstorm);      // 19:40:11:60
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9), Swarm);      // 19:40:25:286

        }



        public void onMobAdded(ActorEntity mob)
        {
            if (mob.Name.Contains("Dark Matter Bulb"))
            {
                if (mob.Coordinate.DistanceTo(bossEntity.Coordinate) <= 7)
                {
                    tts("Bulb under boss");
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


        public void onAgroRemoved(ActorEntity mob)
        {

        }
    }
}