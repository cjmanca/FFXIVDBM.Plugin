using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace TitanNS
{

    public class MainEncounterLogic : AbilityController, IEncounter
    {
        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Titan";



            RotationAbility Rockbuster = new RotationAbility(); // swipe, doesn't really need to be announced
            RotationAbility Landslide = new RotationAbility();
            RotationAbility Stomps = new RotationAbility(); // Tumult
            RotationAbility Plumes = new RotationAbility(); // Weight of the Land
            RotationAbility Bombs = new RotationAbility(); // Rock Buster
            RotationAbility Gaol = new RotationAbility(); // Rock Throw
            RotationAbility TableFlip = new RotationAbility(); // Mountain Buster


            Rockbuster.announceWarning = false;
            Rockbuster.match = new Regex(" uses Rock Buster");
            Rockbuster.warningMessage = "Swipe";
            Plumes.warningTime = TimeSpan.FromSeconds(3);

            Landslide.announceWarning = true;
            Landslide.match = new Regex(" uses Landslide");
            Landslide.warningMessage = "Land slide";

            Stomps.announceWarning = true;
            Stomps.match = new Regex(" uses Tumult");
            Stomps.warningMessage = "Stomps";

            Plumes.announceWarning = true;
            Plumes.match = new Regex(" uses Weight of the Land");
            Plumes.warningMessage = "Weight of the land";
            Plumes.warningTime = TimeSpan.FromSeconds(7);

            Bombs.announceWarning = false;
            Bombs.match = new Regex(" strikes the earth, bringing a shower of bomb boulders");
            Bombs.warningMessage = "Bombs";

            Gaol.announceWarning = false;
            Gaol.match = new Regex(" uses Rock Throw");
            Gaol.warningMessage = "Gaol";

            TableFlip.announceWarning = true;
            TableFlip.match = new Regex(" uses Mountain Buster");
            TableFlip.warningMessage = "Mountain buster";



            // started @ 18:37
            // first rotation ability @ 18:46
            phases[1] = new Phase();
            //phases[1].phaseEndHP = 90;
            phases[1].phaseEndRegex = new Regex(" uses Geocrush");
            phases[1].phaseStartDelay = TimeSpan.FromSeconds(3);


            // 18:37 started
            phases[1].AddRotation(TimeSpan.FromSeconds(6), Landslide);      // 18:46
            phases[1].AddRotation(TimeSpan.FromSeconds(6), Stomps);         // 18:52
            phases[1].AddRotation(TimeSpan.FromSeconds(7), Rockbuster);     // 18:59
            // 19:05 repeat - 6 seconds (same as first landslide), perfect


            // jump @ 19:06
            // Geocrush @ 19:12
            // first rotation ability @ 19:21
            phases[2] = new Phase();
            //phases[2].phaseEndHP = 75;
            //phases[2].phaseStartDelay = TimeSpan.FromSeconds(6);
            phases[2].phaseEndRegex = new Regex(" uses Geocrush");
            phases[2].phaseStartDelay = TimeSpan.FromSeconds(3);

            // Geocrush @ 19:12
            phases[2].AddRotation(TimeSpan.FromSeconds(6), Plumes);         // 19:21
            phases[2].AddRotation(TimeSpan.FromSeconds(6), Rockbuster);     // 19:27
            phases[2].AddRotation(TimeSpan.FromSeconds(5), Landslide);      // 19:32
            phases[2].AddRotation(TimeSpan.FromSeconds(8), Stomps);         // 19:40
            phases[2].AddRotation(TimeSpan.FromSeconds(3), RotationAbility.Blank());// 19:49 repeat




            // jump @ 19:51
            // Geocrush @ 19:57
            // first rotation ability @ 20:07
            phases[3] = new Phase();
            //phases[3].phaseEndHP = 62.5;
            phases[3].phaseEndRegex = new Regex(" uses Geocrush");
            phases[3].phaseStartDelay = TimeSpan.FromSeconds(3);

            // Geocrush @ 19:57
            phases[3].AddRotation(TimeSpan.FromSeconds(7), Landslide);     // 20:07
            phases[3].AddRotation(TimeSpan.FromSeconds(8), Plumes);         // 20:15
            phases[3].AddRotation(TimeSpan.FromSeconds(3), Bombs);          // 20:18
            phases[3].AddRotation(TimeSpan.FromSeconds(7), Rockbuster);     // 20:25
            phases[3].AddRotation(TimeSpan.FromSeconds(10), Landslide);     // 20:35
            phases[3].AddRotation(TimeSpan.FromSeconds(6), Plumes);         // 20:41
            phases[3].AddRotation(TimeSpan.FromSeconds(5), Gaol);           // 20:46
            phases[3].AddRotation(TimeSpan.FromSeconds(7), Rockbuster);     // 20:53
            phases[3].AddRotation(TimeSpan.FromSeconds(3), Stomps);         // 20:56
            phases[3].AddRotation(TimeSpan.FromSeconds(3), RotationAbility.Blank());// 21:06 repeat - 10 seconds, perfect

            // jump @ 21:08
            // Geocrush @ 21:14
            // first rotation ability @ 21:25
            phases[4] = new Phase();
            //phases[3].phaseStartDelay = TimeSpan.FromSeconds(6);
            phases[4].phaseEndRegex = new Regex(" uses Earthen Fury");
            phases[4].phaseStartDelay = TimeSpan.FromSeconds(3);


            // Geocrush @ 21:14
            phases[4].AddRotation(TimeSpan.FromSeconds(11), Gaol);                      // 21:25
            phases[4].AddRotation(TimeSpan.FromSeconds(4), Rockbuster);                 // 21:29
            phases[4].AddRotation(TimeSpan.FromSeconds(5), Landslide);                  // 21:34
            phases[4].AddRotation(TimeSpan.FromSeconds(8), Plumes);                     // 21:42
            phases[4].AddRotation(TimeSpan.FromSeconds(6), Stomps);                     // 21:48
            phases[4].AddRotation(TimeSpan.FromSeconds(8), Gaol);                       // 21:56
            phases[4].AddRotation(TimeSpan.FromSeconds(4), Rockbuster);                 // 22:00
            phases[4].AddRotation(TimeSpan.FromSeconds(5), Landslide);                  // 22:05
            phases[4].AddRotation(TimeSpan.FromSeconds(8), Plumes);                     // ~22:13
            phases[4].AddRotation(TimeSpan.FromSeconds(20), RotationAbility.Blank());   // ~22:20 die if not phase transitioned





            // jump @ 22:06
            // Earthen Fury @ 22:22
            // first rotation ability @ 22:30
            phases[5] = new Phase();
            //phases[3].phaseEndHP = 0;
            //phases[3].phaseStartDelay = TimeSpan.FromSeconds(6);
            //phases[5].phaseEndRegex = new Regex(" uses Geocrush"); // phase 5 doesn't end until titan dies or the party wipes
            phases[5].phaseStartDelay = TimeSpan.FromSeconds(4);


            // Earthen Fury @ 22:22
            phases[5].AddRotation(TimeSpan.FromSeconds(4), TableFlip);      // 22:30 - 8 seconds after earthen, read notes at bottom
            phases[5].AddRotation(TimeSpan.FromSeconds(3), Stomps);         // 22:33
            phases[5].AddRotation(TimeSpan.FromSeconds(9), Plumes);         // 22:42
            phases[5].AddRotation(TimeSpan.FromSeconds(3), Bombs);          // 22:45
            phases[5].AddRotation(TimeSpan.FromSeconds(10), Landslide);     // 22:55
            phases[5].AddRotation(TimeSpan.FromSeconds(4), Rockbuster);     // 22:59
            phases[5].AddRotation(TimeSpan.FromSeconds(4), TableFlip);      // 23:03
            phases[5].AddRotation(TimeSpan.FromSeconds(6), Plumes);         // 23:09
            phases[5].AddRotation(TimeSpan.FromSeconds(5), Gaol);           // 23:14
            phases[5].AddRotation(TimeSpan.FromSeconds(11), Landslide);     // 23:25
            phases[5].AddRotation(TimeSpan.FromSeconds(4), Rockbuster);     // 23:29
            // 23:33 repeat - 4 seconds. Was 8 seconds between Earthen Fury and start of rotation, 
            // so pad here with 4 seconds, reduce start of rotation from 8 to 4, and set phaseStartDelay to 4



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
    }
}