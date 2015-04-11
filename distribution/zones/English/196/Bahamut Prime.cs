using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace BahamutPrimeNS
{
    
    public class MainEncounterLogic : AbilityController, IEncounter
    {
        DateTime lastAddedNewMob = DateTime.Now;
        int akhMornCount = 0;

        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Bahamut Prime";
            
            int phaseNum = 1; // for convenience

            lastAddedNewMob = DateTime.Now;
            akhMornCount = 0;

            RotationAbility NextPhase = new RotationAbility(); // Nerve Gas
            NextPhase.announceWarning = false;
            NextPhase.warningTime = TimeSpan.FromSeconds(0);
            NextPhase.warningCallback = delegate(Ability self)
            {
                nextPhase();
            };


            TriggeredAbility GigaflareTrigger = new TriggeredAbility();
            timedAbilities.Add(GigaflareTrigger);
            GigaflareTrigger.announceWarning = true;
            GigaflareTrigger.match = new Regex(@" readies Gigaflare.");
            GigaflareTrigger.matchMessage = "Giga-flare";


            // Boss's abilities
            RotationAbility FlareBreath = new RotationAbility(); // Flare Breath
            FlareBreath.announceWarning = false; 
            FlareBreath.match = new Regex(@"\ uses\ Flare\ Breath\.");
            FlareBreath.warningMessage = @"Flare Breath"; 
            
            RotationAbility Megaflare = new RotationAbility(); // Megaflare
            Megaflare.announceWarning = true; 
            Megaflare.match = new Regex(@"\ readies\ Megaflare\.");
            Megaflare.warningMessage = @"Mega-flare";
            Megaflare.warningTime = TimeSpan.FromSeconds(2);
            
            RotationAbility Flatten = new RotationAbility(); // Flatten
            Flatten.announceWarning = true; 
            Flatten.match = new Regex(@"\ readies\ Flatten\.");
            Flatten.warningMessage = @"Flatten";
            Flatten.warningTime = TimeSpan.FromSeconds(2);
            
            RotationAbility EarthShaker = new RotationAbility(); // Earth Shaker
            EarthShaker.announceWarning = true; 
            EarthShaker.match = new Regex(@"\ uses\ Earth\ Shaker\.");
            EarthShaker.warningMessage = @"Earth Shaker";
            EarthShaker.warningTime = TimeSpan.FromSeconds(8);

            RotationAbility FlareStar = new RotationAbility(); // Flare Star
            FlareStar.announceWarning = false;
            FlareStar.match = new Regex(@"\ readies\ Flare\ Star\.");
            FlareStar.warningMessage = @"Flare Star";

            RotationAbility RageofBahamut = new RotationAbility(); // Rage of Bahamut
            RageofBahamut.announceWarning = false;
            RageofBahamut.match = new Regex(@"\ readies\ Rage\ of\ Bahamut\.");
            RageofBahamut.warningMessage = @"Rage of Bahamut";



            RotationAbility MegaflareDive = new RotationAbility(); // Megaflare Dive
            MegaflareDive.announceWarning = true;
            MegaflareDive.match = new Regex(@"\ readies\ Megaflare\ Dive\.");
            MegaflareDive.warningMessage = @"Dive bombs";
            MegaflareDive.warningTime = TimeSpan.FromSeconds(6);

            RotationAbility DoubleDive = new RotationAbility(); // Double Dive
            DoubleDive.announceWarning = true;
            DoubleDive.match = new Regex(@"\ readies\ Double\ Dive\.");
            DoubleDive.warningMessage = @"Move";
            DoubleDive.warningTime = TimeSpan.FromSeconds(1);



            RotationAbility EvilEye = new RotationAbility(); // Evil Eye
            EvilEye.announceWarning = true;
            EvilEye.match = new Regex(@"\ uses\ Evil\ Eye\.");
            EvilEye.warningMessage = @"Evil Eye";
            EvilEye.warningTime = TimeSpan.FromSeconds(6);


            RotationAbility DeathSentence = new RotationAbility(); // Death Sentence
            DeathSentence.announceWarning = true;
            DeathSentence.match = new Regex(@"\ readies\ Death\ Sentence\.");
            DeathSentence.warningMessage = @"Death Sentence";
            DeathSentence.warningTime = TimeSpan.FromSeconds(6);


            RotationAbility Teraflare = new RotationAbility(); // Teraflare
            Teraflare.announceWarning = true;
            Teraflare.match = new Regex(@"\ readies\ Teraflare\.");
            Teraflare.warningMessage = @"Tera flare";
            Teraflare.warningTime = TimeSpan.FromSeconds(-15);

            RotationAbility AkhMorn = new RotationAbility(); // Akh Morn
            AkhMorn.announceWarning = true;
            AkhMorn.match = new Regex(@"\ readies\ Akh\ Morn\.");
            //AkhMorn.warningMessage = @"Akh Morn";
            AkhMorn.warningTime = TimeSpan.FromSeconds(3);
            AkhMorn.warningCallback = delegate(Ability abil)
            {
                akhMornCount++;

                tts("Akh Morn " + akhMornCount);
            };

            RotationAbility TempestWing = new RotationAbility(); // Tempest Wing
            TempestWing.announceWarning = false;
            TempestWing.match = new Regex(@"\ uses\ Tempest\ Wing\.");
            TempestWing.warningMessage = @"Tempest Wing";

            RotationAbility MegaflareStrike = new RotationAbility(); // Megaflare Strike
            MegaflareStrike.announceWarning = false;
            MegaflareStrike.match = new Regex(@"\ uses\ Megaflare\ Strike\.");
            MegaflareStrike.warningMessage = @"Megaflare Strike"; 


            // Boss ability rotation
            // You'll need to split these up into phases
            // And separate out any timed moves which aren't part of a rotation
            // For now we'll assume they're all part of phase 1
            
            phaseNum = 1;
            phases[phaseNum] = new Phase();

            
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.4933142), FlareBreath);      // 20:13:17:626 @ 99.7963599245106%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.4212529), Megaflare);      // 20:13:22:47 @ 99.3454269458595%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(13.769788), FlareBreath);      // 20:13:35:817 @ 97.7115535215109%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3851936), Flatten);      // 20:13:39:202 @ 97.4483935101334%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0224017), FlareBreath);      // 20:13:46:225 @ 96.5393617620241%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(15.6008923), EarthShaker);      // 20:14:1:826 @ 94.7920056097905%


            phases[phaseNum].phaseEndRegex = new Regex(@" readies Gigaflare.");


            phaseNum = 2;
            phases[phaseNum] = new Phase();
            phases[phaseNum].phaseStartDelay = TimeSpan.FromSeconds(4);
            

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.14158), FlareStar);      // 17:56:45:834 @ 65.0017658555918%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.992686), FlareBreath);      // 17:56:57:827 @ 64.6718469972046%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.3335338), Flatten);      // 17:57:7:160 @ 64.4565076714178%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.052461), FlareBreath);      // 17:57:15:213 @ 64.129513625605%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.184640), Megaflare);      // 17:57:26:398 @ 63.1931348799255%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.035803), FlareBreath);      // 17:57:40:433 @ 61.249377563324%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.2970742), RageofBahamut);      // 17:57:41:730 @ 61.070671515031%


            phases[phaseNum].phaseEndRegex = new Regex(@" readies Gigaflare.");

            phaseNum = 3;
            phases[phaseNum] = new Phase();


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.5868343), MegaflareDive);      // 20:35:25:639 @ 50.201921748102%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.080119), DoubleDive);      // 20:35:27:719 @ 50.201921748102%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.5484317), Megaflare);      // 20:35:35:267 @ 50.201921748102%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(73.822223), EvilEye);      // 20:36:49:89 @ 50.201921748102%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.8134469), MegaflareDive);      // 20:36:56:903 @ 50.201921748102%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3261331), DoubleDive);      // 20:36:59:229 @ 50.201921748102%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.2824165), Megaflare);      // 20:37:6:511 @ 50.201921748102%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(29.619694), DeathSentence);      // 20:37:36:131 @ 50.201921748102%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(34.063948), EvilEye);      // 21:28:43:170 @ 50.2021411090451%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.506029), DeathSentence);      // 21:28:43:676 @ 50.2021411090451%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20.797189), MegaflareDive);      // 21:29:4:473 @ 50.2021411090451%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2), DoubleDive);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.621551), Megaflare);      // 20:44:13:610 @ 48.7553825691407%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.8141038), Teraflare);      // 20:44:15:424 @ 48.7553825691407%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(500), RotationAbility.Blank());      // 21:29:10:972 @ 50.2021411090451%





            phases[phaseNum].phaseEndRegex = new Regex(@" uses Teraflare.");

            phaseNum = 4;
            phases[phaseNum] = new Phase();
            phases[phaseNum].phaseStartDelay = TimeSpan.FromSeconds(7.7964459);


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2493575), AkhMorn);      // 21:30:29:675 @ 50.5811968186814%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(15.0688619), Megaflare);      // 21:30:44:744 @ 48.0261536740399%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.187640), TempestWing);      // 21:30:55:931 @ 45.5459857313019%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20.019145), EarthShaker);      // 21:31:15:950 @ 41.7421207377255%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.010401), TempestWing);      // 21:31:22:961 @ 40.6754415918585%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.834105), FlareBreath);      // 21:31:24:795 @ 40.3881518767426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.4923713), AkhMorn);      // 21:31:31:287 @ 39.5602105572572%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(16.3789368), Megaflare);      // 21:31:47:666 @ 36.3374327018907%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.914624), TempestWing);      // 21:31:58:581 @ 34.9454412774411%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20.279160), EarthShaker);      // 21:32:18:861 @ 31.3452894796685%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0264019), TempestWing);      // 21:32:25:887 @ 30.1016591730531%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.801103), FlareBreath);      // 21:32:27:688 @ 29.9565153490508%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(16.937969), AkhMorn);      // 21:32:44:626 @ 26.559272423624%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(17.3839943), Megaflare);      // 21:33:2:10 @ 22.8380333852731%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.168639), TempestWing);      // 21:33:13:179 @ 21.1687697287748%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20.036146), EarthShaker);      // 21:33:33:216 @ 18.0587433981496%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.263415), TempestWing);      // 21:33:40:480 @ 17.1958505684008%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.8871079), FlareBreath);      // 21:33:42:367 @ 16.8741943055362%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.1823536), AkhMorn);      // 21:33:48:549 @ 15.7258397685303%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.698069), Megaflare);      // 21:34:7:247 @ 12.1152317658044%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.192640), TempestWing);      // 21:34:18:440 @ 10.5601820403346%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20.525174), EarthShaker);      // 21:34:38:965 @ 7.6563549962014%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.516373), TempestWing);      // 21:34:45:481 @ 7.1320823422484%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.0921197), FlareBreath);      // 21:34:47:573 @ 4.52461193221162%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(16.616950), AkhMorn);      // 21:35:4:190 @ 1.68505764439982%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(500), RotationAbility.Blank());      // 21:29:10:972 @ 50.2021411090451%

        }


        public void onEndEncounter()
        {

        }

        public void onMobAdded(ActorEntity mob)
        {
            if (DateTime.Now < (lastAddedNewMob + TimeSpan.FromSeconds(5)))
            {
                return;
            }

            if (mob.Name.Contains("Meracydia"))
            {
                //tts("Add");
                lastAddedNewMob = DateTime.Now;
            }
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
