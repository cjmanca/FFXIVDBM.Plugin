using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace KaliyaNS
{
    
    public class MainEncounterLogic : AbilityController, IEncounter
    {
        bool NerveCloudRotation;

        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Kaliya";

            NerveCloudRotation = true;

            int phaseNum = 1; // for convenience


            TriggeredAbility EnrageTrigger = new TriggeredAbility();
            EnrageTrigger.warningMessage = "Enrage soon";
            EnrageTrigger.timerDuration = TimeSpan.FromMinutes(11);
            EnrageTrigger.warningTime = TimeSpan.FromSeconds(30);

            timedAbilities.Add(EnrageTrigger);

            EnrageTrigger.start();


            TriggeredAbility ForkedLightning = new TriggeredAbility(); // Forked Lightning
            timedAbilities.Add(ForkedLightning);
            ForkedLightning.match = new Regex(@"(?<member>[^ ]+?) [^ ]+ suffers the effect of Forked Lightning\.");
            ForkedLightning.matchMessage = "${member.1} ${member.2} ${member.3} forked lightning";

            TriggeredAbility SecondaryHeadTrigger = new TriggeredAbility(); // Forked Lightning
            //timedAbilities.Add(SecondaryHeadTrigger);
            SecondaryHeadTrigger.match = new Regex(@"\ uses\ Secondary\ Head\.");
            SecondaryHeadTrigger.matchMessage = "Second Head";


            TriggeredAbility ResonanceTrigger = new TriggeredAbility();
            ResonanceTrigger.match = new Regex(@"\ uses\ Resonance\.");
            ResonanceTrigger.warningCallback = delegate(Ability self)
            {
                setPhase(6);
            };

            TriggeredAbility NerveGasTrigger = new TriggeredAbility();
            NerveGasTrigger.match = new Regex(@"\ readies\ Nerve\ Gas\.");
            NerveGasTrigger.warningCallback = delegate(Ability self)
            {
                setPhase(7);
            };

            


            RotationAbility NextPhase = new RotationAbility(); // Nerve Gas
            NextPhase.announceWarning = false;
            NextPhase.warningTime = TimeSpan.FromSeconds(0);
            NextPhase.warningCallback = delegate(Ability self)
            {
                nextPhase();
            };

            RotationAbility GoToPhase5 = new RotationAbility(); // Nerve Gas
            GoToPhase5.announceWarning = false;
            GoToPhase5.warningTime = TimeSpan.FromSeconds(0);
            GoToPhase5.warningCallback = delegate(Ability self)
            {
                setPhase(5);
            };

            RotationAbility GoToPhase6 = new RotationAbility(); // Nerve Gas
            GoToPhase6.announceWarning = false;
            GoToPhase6.warningTime = TimeSpan.FromSeconds(0);
            GoToPhase6.warningCallback = delegate(Ability self)
            {
                setPhase(6);
            };

            RotationAbility GoToPhase7 = new RotationAbility(); // Nerve Gas
            GoToPhase7.announceWarning = false;
            GoToPhase7.warningTime = TimeSpan.FromSeconds(0);
            GoToPhase7.warningCallback = delegate(Ability self)
            {
                setPhase(7);
            };


            RotationAbility NerveCloud = new RotationAbility(); // Nerve Cloud
            NerveCloud.announceWarning = false;
            NerveCloud.match = new Regex(@"\ readies\ Nerve\ Cloud\.");
            NerveCloud.warningMessage = @"Nerve Cloud";
            NerveCloud.warningCallback = delegate(Ability self)
            {
                if (NerveCloudRotation)
                {
                    tts("Nerve Cloud");
                    delayRotation(TimeSpan.FromSeconds(8.5));
                }
                else
                {
                    delayRotation(TimeSpan.FromSeconds(-2));
                }
                NerveCloudRotation = !NerveCloudRotation;
            };


            RotationAbility NerveGasMaybe = new RotationAbility(); // Nerve Gas
            NerveGasMaybe.announceWarning = false;
            NerveGasMaybe.warningMessage = @"Nerve Gas";
            NerveGasMaybe.warningTime = TimeSpan.FromSeconds(3);

            RotationAbility Missiles = new RotationAbility(); // Seed of the Sea
            Missiles.announceWarning = true;
            //Missiles.match = new Regex(@"\ uses\ Seed\ of\ the\ Sea\.");
            Missiles.warningMessage = @"Missiles";
            Missiles.warningTime = TimeSpan.FromSeconds(8);

            RotationAbility NerveGas = new RotationAbility(); // Nerve Gas
            NerveGas.announceWarning = true;
            NerveGas.match = new Regex(@"\ readies\ Nerve\ Gas\.");
            NerveGas.warningMessage = @"Nerve Gas";
            NerveGas.warningTime = TimeSpan.FromSeconds(3);



            RotationAbility Resonance = new RotationAbility(); // Resonance
            Resonance.announceWarning = false;
            Resonance.match = new Regex(@"\ uses\ Resonance\.");
            Resonance.warningMessage = @"Resonance";


            RotationAbility Barofield = new RotationAbility(); // Barofield
            Barofield.announceWarning = false;
            Barofield.match = new Regex(@"\ readies\ Barofield\.");
            Barofield.warningMessage = @"Barofield";

            RotationAbility SeedoftheSea = new RotationAbility(); // Seed of the Sea
            SeedoftheSea.announceWarning = true;
            SeedoftheSea.match = new Regex(@"\ uses\ Seed\ of\ the\ Sea\.");
            SeedoftheSea.warningMessage = @"Seed of the Sea";

            RotationAbility SeedoftheRivers = new RotationAbility(); // Seed of the Rivers
            SeedoftheRivers.announceWarning = true;
            SeedoftheRivers.match = new Regex(@"\ uses\ Seed\ of\ the\ Rivers\.");
            SeedoftheRivers.warningMessage = @"Seed of the Rivers";

            RotationAbility SecondaryHead = new RotationAbility(); // Secondary Head
            SecondaryHead.announceWarning = true;
            SecondaryHead.match = new Regex(@"\ uses\ Secondary\ Head\.");
            SecondaryHead.warningMessage = @"Second Head";
            SecondaryHead.warningTime = TimeSpan.FromSeconds(2);


            RotationAbility IntheHeadlights = new RotationAbility(); // In the Headlights
            IntheHeadlights.announceWarning = false;
            IntheHeadlights.match = new Regex(@"\ suffers\ the\ effect\ of\ In\ the\ Headlights\.");
            IntheHeadlights.warningMessage = @"In the Headlights";

            RotationAbility MainHead = new RotationAbility(); // Main Head
            MainHead.announceWarning = false;
            MainHead.match = new Regex(@"\ uses\ Main\ Head\.");
            MainHead.warningMessage = @"Main Head";


            RotationAbility EmergencyMode = new RotationAbility(); // Emergency Mode
            EmergencyMode.announceWarning = true;
            EmergencyMode.match = new Regex(@"\ readies\ Emergency\ Mode\.");
            EmergencyMode.warningMessage = @"Emergency Mode";

            RotationAbility NanosporeJet = new RotationAbility(); // Nanospore Jet
            NanosporeJet.announceWarning = true;
            NanosporeJet.match = new Regex(@"\ readies\ Nanospore\ Jet\.");
            NanosporeJet.warningMessage = @"Tether";
            NanosporeJet.uniqueInPhase = true;
            NanosporeJet.warningTime = TimeSpan.FromSeconds(2);

            RotationAbility AetherochemicalNanosporesA = new RotationAbility(); // Aetherochemical Nanospores α
            AetherochemicalNanosporesA.announceWarning = false;
            AetherochemicalNanosporesA.match = new Regex(@"\ suffers\ the\ effect\ of\ Aetherochemical\ Nanospores\ α\.");
            AetherochemicalNanosporesA.warningMessage = @"Tether";

            RotationAbility AetherochemicalNanosporesB = new RotationAbility(); // Aetherochemical Nanospores β
            AetherochemicalNanosporesB.announceWarning = false;
            AetherochemicalNanosporesB.match = new Regex(@"\ suffers\ the\ effect\ of\ Aetherochemical\ Nanospores\ β\.");
            AetherochemicalNanosporesB.warningMessage = @"Aetherochemical Nanospores β";

            RotationAbility NanosporeCloud = new RotationAbility(); // Nanospore Cloud
            NanosporeCloud.announceWarning = true;
            NanosporeCloud.match = new Regex(@"\ uses\ Nanospore\ Cloud\.");
            NanosporeCloud.warningMessage = @"Nanospore Cloud"; 

            


            
            phaseNum = 1;
            phases[phaseNum] = new Phase();
            


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.2751301), Resonance);      // 19:29:13:475 @ 99.5841004208438%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.4933714), NerveGas);      // 19:29:19:968 @ 98.3110017556973%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(17.1639818), Resonance);      // 19:29:37:132 @ 94.0020300552885%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.4336539), NerveGas);      // 19:29:48:566 @ 91.4464289349772%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(60), RotationAbility.Blank());      // 19:29:58:717 @ 89.5235962419741%



            phases[phaseNum].phaseEndRegex = new Regex(" readies Barofield");

            phaseNum = 2;
            phases[phaseNum] = new Phase();
            phases[phaseNum].phaseStartDelay = TimeSpan.FromSeconds(5);
            

            /**/
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.9616842), Missiles);      // 19:30:15:871 @ 85.844484580208%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.09252), Resonance);      // 19:30:24:963 @ 84.3301835484087%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.391194), SecondaryHead);      // 19:30:28:354 @ 83.4234449088172%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9222816), IntheHeadlights);      // 19:30:33:278 @ 82.4329446756614%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.5640894), MainHead);      // 19:30:34:842 @ 82.39183944919%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.161238), Resonance);      // 19:30:39:3 @ 81.1284354129282%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3351336), NerveGas);      // 19:30:41:338 @ 80.9259146155256%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(17.1779825), Resonance);      // 19:30:58:516 @ 77.3020623384026%


            phases[phaseNum].phaseEndHP = 60;


            phaseNum = 3;
            phases[phaseNum] = new Phase();



            phases[phaseNum].phaseEndRegex = new Regex(@" readies Emergency Mode\.");

            phaseNum = 4;
            phases[phaseNum] = new Phase();

            //phases[phaseNum].timedAbilities.Add(SecondaryHeadTrigger);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3631923), NextPhase); // This phase only exists for the extra delay after Emergency Mode
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(60), RotationAbility.Blank());


            phaseNum = 5;
            phases[phaseNum] = new Phase();

            phases[phaseNum].timedAbilities.Add(ResonanceTrigger);
            phases[phaseNum].timedAbilities.Add(NerveGasTrigger);

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.6522089), NerveCloud);      // 19:2:29:120 @ 58.8832369202491%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.7880451), NanosporeJet);      // 19:17:57:888 @ 58.7992814341071%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1962973), AetherochemicalNanosporesA);      // 19:18:3:84 @ 58.331127805262%

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.760387), NerveGasMaybe);      // 19:17:57:888 @ 58.7992814341071%

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(60), RotationAbility.Blank());

            /**/


            phaseNum = 6;
            phases[phaseNum] = new Phase();


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Resonance);      // 19:5:0:749 @ 31.635803815612%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.1145213), Missiles);      // 19:5:9:864 @ 29.9071545391707%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.985342), NerveGas);      // 19:5:15:849 @ 28.8234889224323%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.226042), SecondaryHead);      // 19:5:34:75 @ 23.7207210089394%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.961284), IntheHeadlights);      // 19:5:39:36 @ 22.8935752724918%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.0520601), MainHead);      // 19:5:40:89 @ 22.7810206311397%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.1261788), Resonance);      // 19:5:43:215 @ 22.2782571771761%

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), GoToPhase5);  


            phaseNum = 7;
            phases[phaseNum] = new Phase();


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), NerveGas); // Nerve Gas           19:2:52:543 @ 54.8601598256518%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(16.373936), Resonance);      // 19:3:8:917 @ 51.5358715810679%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.9356255), Missiles);      // 19:3:19:853 @ 49.0501687350157%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.221299), Resonance);      // 19:3:25:74 @ 48.1602987496837%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.0741187), SecondaryHead);      // 19:3:27:148 @ 47.8853590745119%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.195297), IntheHeadlights);      // 19:3:32:343 @ 47.3280148599272%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.0400595), MainHead);      // 19:3:33:383 @ 47.0754697774792%

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), GoToPhase5); 



        }


        public void onEndEncounter()
        {

        }

        public void onMobAdded(ActorEntity mob)
        {
            if (mob.Name.Contains("Weapons"))
            {
                setPhase(3);
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
