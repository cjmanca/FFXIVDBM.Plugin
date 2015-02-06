using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace ImdugudNS
{
    
    public class MainEncounterLogic : AbilityController, IEncounter
    {
        int nextPhaseNum;

        bool midPointReached;
        bool chargeCalled;

        public void onStartEncounter()
        {


            // bossName is needed if you want health based phase swaps
            bossName = "Imdugud";

            midPointReached = false;
            chargeCalled = false;

            int phaseNum = 1; // for convenience
            

            // Boss's abilities

            TriggeredAbility Prey = new TriggeredAbility();
            timedAbilities.Add(Prey);
            Prey.match = new Regex(@"(?<member>[^ ]+?) [^ ]+ suffers the effect of Prey\.");
            Prey.matchMessage = "${member.1} ${member.2} ${member.3} prey";


            TriggeredAbility HeatLightningAnnounce = new TriggeredAbility(); // Heat Lightning
            timedAbilities.Add(HeatLightningAnnounce);
            HeatLightningAnnounce.match = new Regex(@"Imdugud readies Heat Lightning\.");
            HeatLightningAnnounce.matchMessage = @"Heat Lightning";


            RotationAbility CrackleHiss = new RotationAbility(); // Crackle Hiss
            CrackleHiss.announceWarning = false;
            CrackleHiss.match = new Regex(@"Imdugud uses Crackle Hiss\.");
            CrackleHiss.warningMessage = @"Crackle Hiss"; 
            
            RotationAbility CriticalRip = new RotationAbility(); // Critical Rip
            CriticalRip.announceWarning = true;
            CriticalRip.match = new Regex(@"Imdugud readies Critical Rip\.");
            CriticalRip.warningMessage = @"Critical Rip"; 
            
            RotationAbility WildCharge = new RotationAbility(); // Wild Charge
            WildCharge.warningTime = TimeSpan.FromSeconds(7);
            WildCharge.match = new Regex(@"Imdugud readies Wild Charge\.");
            WildCharge.warningCallback = delegate(Ability self)
            {
                if (!chargeCalled)
                {
                    tts("Charge");
                }
            };
            

            RotationAbility ErraticBlaster = new RotationAbility(); // Erratic Blaster
            ErraticBlaster.announceWarning = false;
            ErraticBlaster.match = new Regex(@"Imdugud uses Erratic Blaster\.");
            ErraticBlaster.warningMessage = @"Erratic Blaster"; 
            
            
            RotationAbility HeatLightning = new RotationAbility(); // Heat Lightning
            HeatLightning.announceWarning = false;
            HeatLightning.match = new Regex(@"Imdugud readies Heat Lightning\.");
            HeatLightning.warningMessage = @"Heat Lightning";

            RotationAbility HeatLightningUnique = new RotationAbility(); // Heat Lightning
            HeatLightningUnique.announceWarning = false;
            HeatLightningUnique.uniqueInPhase = true;
            HeatLightningUnique.match = new Regex(@"Imdugud readies Heat Lightning\.");
            HeatLightningUnique.warningMessage = @"Heat Lightning";


            RotationAbility Electrification = new RotationAbility(); // Electrification
            Electrification.announceWarning = false;
            Electrification.match = new Regex(@"\ uses\ Electrification\.");
            Electrification.warningMessage = @"Electrification"; 

            RotationAbility CyclonicChaos = new RotationAbility(); // Cyclonic Chaos
            CyclonicChaos.announceWarning = true;
            CyclonicChaos.match = new Regex(@" uses Cyclonic Chaos\.");
            CyclonicChaos.warningMessage = @"Tether"; 


            RotationAbility NextAdds = new RotationAbility();
            NextAdds.warningTime = TimeSpan.Zero;
            NextAdds.announceWarning = true;
            NextAdds.warningMessage = @"Adds soon";


            RotationAbility Tether = new RotationAbility();
            Tether.announceWarning = true;
            Tether.warningMessage = @"Tether";



            RotationAbility WildChargeAndTether = new RotationAbility();
            WildChargeAndTether.warningTime = TimeSpan.Zero;
            WildChargeAndTether.warningCallback = delegate(Ability self)
            {
                chargeCalled = true;
                tts("Charge");
            };




            TriggeredAbility SpikeFlail = new TriggeredAbility(); // Spike Flail
            timedAbilities.Add(SpikeFlail);
            SpikeFlail.announceWarning = false;
            SpikeFlail.match = new Regex(@"Imdugud readies Spike Flail\.");
            SpikeFlail.matchCallback = delegate(Ability self, Match match)
            {
                delayRotation(TimeSpan.FromSeconds(5.3));
            };

            TriggeredAbility ElectricBurst = new TriggeredAbility(); // Electric Burst
            timedAbilities.Add(ElectricBurst);
            ElectricBurst.announceWarning = false;
            ElectricBurst.match = new Regex(@"Imdugud readies Electric Burst\.");
            ElectricBurst.matchCallback = delegate(Ability self, Match match)
            {
                delayRotation(TimeSpan.FromSeconds(5.7));
            };



            RotationAbility GoTo6 = new RotationAbility();
            GoTo6.warningTime = TimeSpan.Zero;
            GoTo6.warningCallback = delegate(Ability self)
            {
                chargeCalled = false;
                midPointReached = false;
                setPhase(6);
            };

            RotationAbility GoTo7 = new RotationAbility();
            GoTo7.warningTime = TimeSpan.Zero;
            GoTo7.warningCallback = delegate(Ability self)
            {
                chargeCalled = false;
                midPointReached = false;
                setPhase(7);
            };

            RotationAbility GoTo8 = new RotationAbility();
            GoTo8.warningTime = TimeSpan.Zero;
            GoTo8.warningCallback = delegate(Ability self)
            {
                chargeCalled = false;
                midPointReached = false;
                setPhase(8);
            };

            RotationAbility MidRotation = new RotationAbility();
            MidRotation.warningTime = TimeSpan.Zero;
            MidRotation.warningCallback = delegate(Ability self)
            {
                midPointReached = true;
            };




            TriggeredAbility HeatLightningChargeSwapFromTether = new TriggeredAbility();
            HeatLightningChargeSwapFromTether.announceWarning = false;
            HeatLightningChargeSwapFromTether.match = new Regex(@"Imdugud readies Heat Lightning\.");
            HeatLightningChargeSwapFromTether.matchCallback = delegate(Ability self, Match match)
            {
                if (midPointReached)
                {
                    midPointReached = false;
                    setPhase(8);
                }
            };

            TriggeredAbility HeatLightningChargeSwap = new TriggeredAbility();
            HeatLightningChargeSwap.announceWarning = false;
            HeatLightningChargeSwap.match = new Regex(@"Imdugud readies Heat Lightning\.");
            HeatLightningChargeSwap.matchCallback = delegate(Ability self, Match match)
            {
                if (!midPointReached)
                {
                    midPointReached = false;
                    setPhase(8);
                }
            };


            TriggeredAbility HeatLightningTetherSwap = new TriggeredAbility();
            HeatLightningTetherSwap.announceWarning = false;
            HeatLightningTetherSwap.match = new Regex(@"Imdugud readies Heat Lightning\.");
            HeatLightningTetherSwap.matchCallback = delegate(Ability self, Match match)
            {
                if (midPointReached)
                {
                    midPointReached = false;
                    setPhase(7);
                }
            };





            //
            // Phase 1
            // 
            phaseNum = 1;
            phases[phaseNum] = new Phase();
            

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.2112981), CrackleHiss);      // 20:2:55:189 @ 99.3684518799455%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1175215), CriticalRip);      // 20:3:4:307 @ 97.7928231542308%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.7904456), CrackleHiss);      // 20:3:12:97 @ 95.8220576843757%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.3645356), WildCharge);      // 20:3:21:462 @ 93.4524667822456%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0074008), CrackleHiss);      // 20:3:28:469 @ 92.5618162653262%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.5968921), ErraticBlaster);      // 20:3:44:66 @ 89.6093008594643%


            phases[phaseNum].phaseEndRegex = new Regex(@"Imdugud readies Electrocharge\.");


            //
            // Phase 2
            // 
            phaseNum = 2;
            phases[phaseNum] = new Phase();

            // Nothing to warn about in this phase. Prey is being handled by a trigger.


            phases[phaseNum].phaseEndRegex = new Regex(@"Imdugud readies Electric Burst\.");


            //
            // Phase 3
            // 
            phaseNum = 3;
            phases[phaseNum] = new Phase();

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.637208), HeatLightning);      // 20:6:36:943 @ 76.1975661365016%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(16.1159218), ErraticBlaster);      // 20:6:57:482 @ 72.6669326993261%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3781932), CrackleHiss);      // 20:7:0:860 @ 72.1929921632144%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3411339), HeatLightning);      // 20:7:3:202 @ 71.7703579257054%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.7984461), WildCharge);      // 20:7:11:0 @ 70.705668395879%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.274416), CrackleHiss);      // 20:7:18:274 @ 70.1388399858265%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.5804908), CriticalRip);      // 20:7:27:381 @ 69.1835815373334%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.7583866), CrackleHiss);      // 20:7:34:139 @ 68.375647057377%


            phases[phaseNum].phaseEndRegex = new Regex(@"Imdugud readies Electrocharge\.");


            //
            // Phase 4
            // 
            phaseNum = 4;
            phases[phaseNum] = new Phase();


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20), NextAdds);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(120), RotationAbility.Blank());


            phases[phaseNum].phaseEndRegex = new Regex(@"Imdugud readies Electric Burst\.");

            /**/
            
            //
            // Phase 5
            // 
            phaseNum = 5;
            phases[phaseNum] = new Phase();

            phases[phaseNum].timedAbilities.Add(HeatLightningTetherSwap);

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.7), GoTo7);


            //
            // Phase 6
            // Tether/charge combo rotation
            phaseNum = 6;
            phases[phaseNum] = new Phase();


            phases[phaseNum].timedAbilities.Add(HeatLightningChargeSwap);
            phases[phaseNum].timedAbilities.Add(HeatLightningTetherSwap);

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), WildChargeAndTether);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(15), MidRotation); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(125.3316205), GoTo7); 




            //
            // Phase 7
            // Heat Lightning/Tether rotation
            // Seems like this is always the first rotation for the final phase
            // This also seems to always follow a charge/tether rotation
            phaseNum = 7;
            phases[phaseNum] = new Phase();

            phases[phaseNum].timedAbilities.Add(HeatLightningChargeSwapFromTether);

            //phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), HeatLightning); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.2), CyclonicChaos);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.7033262), CrackleHiss);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.767461), CriticalRip);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), MidRotation); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.7563865), ErraticBlaster); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3811933), CrackleHiss);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.498), GoTo6);





            //
            // Phase 8
            // Heat Lightning/Charge rotation
            phaseNum = 8;
            phases[phaseNum] = new Phase();

            phases[phaseNum].timedAbilities.Add(HeatLightningTetherSwap);

            //phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), HeatLightning); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.287417), Electrification); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.779045), WildCharge);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0214016), CrackleHiss);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), MidRotation); 
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(121.3940121), GoTo7);


            /**/


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
