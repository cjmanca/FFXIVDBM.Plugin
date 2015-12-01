using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Memory.Core;
using System.Text.RegularExpressions;

namespace PhoenixNS
{
    
    public class MainEncounterLogic : AbilityController, IEncounter
    {
        int bennuCount;
        TriggeredAbility Bennu;

        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Phoenix";
            
            int phaseNum = 1; // for convenience

            bennuCount = 0;


            TriggeredAbility EnrageTrigger = new TriggeredAbility();
            timedAbilities.Add(EnrageTrigger);
            EnrageTrigger.warningMessage = "Enrage soon";
            EnrageTrigger.timerDuration = TimeSpan.FromMinutes(13);
            EnrageTrigger.warningTime = TimeSpan.FromSeconds(30);

            EnrageTrigger.start();


            TriggeredAbility FlamesofUnforgivenessTrigger = new TriggeredAbility(); // Flames of Unforgiveness
            timedAbilities.Add(FlamesofUnforgivenessTrigger);
            FlamesofUnforgivenessTrigger.announceWarning = true; 
            FlamesofUnforgivenessTrigger.match = new Regex(@"\ readies\ Flames\ of\ Unforgiveness\.");
            FlamesofUnforgivenessTrigger.matchMessage = @"Flames of Unforgiveness";


            TriggeredAbility BrandofPurgatoryTrigger = new TriggeredAbility();
            timedAbilities.Add(BrandofPurgatoryTrigger);
            BrandofPurgatoryTrigger.announceWarning = true; 
            BrandofPurgatoryTrigger.match = new Regex(@"\ readies\ Brand\ of\ Purgatory\.");
            BrandofPurgatoryTrigger.matchMessage = "Purgatory";


            TriggeredAbility ChainofPurgatory = new TriggeredAbility(); // Chain of Purgatory
            timedAbilities.Add(ChainofPurgatory);
            ChainofPurgatory.announceWarning = false; 
            ChainofPurgatory.match = new Regex(@"(?<member>[^ ]+?) [^ ]+ suffers\ the\ effect\ of\ Chain\ of\ Purgatory\.");
            ChainofPurgatory.matchMessage = "${member.1} ${member.2} ${member.3} Purgatory";

            TriggeredAbility ArmofPurgatory = new TriggeredAbility(); // Chain of Purgatory
            timedAbilities.Add(ArmofPurgatory);
            ArmofPurgatory.announceWarning = false; 
            ArmofPurgatory.match = new Regex(@"(?<member>[^ ]+?) [^ ]+ suffers\ the\ effect\ of\ Arm\ of\ Purgatory\.");
            ArmofPurgatory.matchMessage = "${member.1} ${member.2} ${member.3} Purgatory";


            Bennu = new TriggeredAbility();
            Bennu.warningTime = TimeSpan.FromSeconds(6);
            Bennu.announceWarning = true;
            Bennu.warningCallback = delegate(Ability abil)
            {
                bennuCount++;

                tts("Add " + bennuCount);
            };

            Bennu.timerDuration = TimeSpan.FromSeconds(6);
            Bennu.start();
            Bennu.timerDuration = TimeSpan.FromSeconds(60);
            

            // Boss's abilities
            RotationAbility Revelation = new RotationAbility(); // Revelation
            Revelation.announceWarning = true; 
            Revelation.match = new Regex(@"\ readies\ Revelation\.");
            Revelation.warningMessage = @"Revelation";
            Revelation.warningTime = TimeSpan.FromSeconds(4);


            
            RotationAbility Blackfire = new RotationAbility(); // Blackfire
            Blackfire.announceWarning = true; 
            Blackfire.match = new Regex(@"\ readies\ Blackfire\.");
            Blackfire.warningMessage = @"Blackfire";
            Blackfire.warningTime = TimeSpan.FromSeconds(2);


            RotationAbility Whitefire = new RotationAbility(); // Whitefire
            Whitefire.announceWarning = false; 
            Whitefire.match = new Regex(@"\ uses\ Whitefire\.");
            Whitefire.warningMessage = @"Whitefire"; 
            
            RotationAbility FlamesofUnforgiveness = new RotationAbility(); // Flames of Unforgiveness
            FlamesofUnforgiveness.announceWarning = false;
            FlamesofUnforgiveness.match = new Regex(@"\ readies\ Flames\ of\ Unforgiveness\.");
            FlamesofUnforgiveness.warningMessage = @"Flames of Unforgiveness";
            FlamesofUnforgiveness.warningTime = TimeSpan.FromSeconds(2);
            
            RotationAbility Bluefire = new RotationAbility(); // Bluefire
            Bluefire.announceWarning = true; 
            Bluefire.match = new Regex(@"\ uses\ Bluefire\.");
            Bluefire.warningMessage = @"Blue fire";
            Bluefire.warningTime = TimeSpan.FromSeconds(7);
            
            RotationAbility Redfire = new RotationAbility(); // Redfire
            Redfire.announceWarning = true; 
            Redfire.match = new Regex(@"\ readies\ Redfire\.");
            Redfire.warningMessage = @"Red fire";
            Redfire.warningTime = TimeSpan.FromSeconds(3);



            RotationAbility RedfirePlume = new RotationAbility(); // Redfire Plume
            RedfirePlume.announceWarning = false;
            RedfirePlume.match = new Regex(@"\ uses\ Redfire\ Plume\.");
            RedfirePlume.warningMessage = @"Plumes";


            RotationAbility FountainofFire = new RotationAbility(); // Fountain of Fire
            FountainofFire.announceWarning = true;
            FountainofFire.match = new Regex(@"\ readies\ Fountain\ of\ Fire\.");
            FountainofFire.warningMessage = @"Fountain of Fire";
            FountainofFire.warningTime = TimeSpan.FromSeconds(0);

            RotationAbility Summon = new RotationAbility(); // Summon
            Summon.announceWarning = false;
            Summon.match = new Regex(@"\ uses\ Summon\.");
            Summon.warningMessage = @"Pinions";


            RotationAbility FlamesofRebirth = new RotationAbility(); // Flames of Rebirth
            FlamesofRebirth.announceWarning = false;
            FlamesofRebirth.match = new Regex(@"\ uses\ Flames\ of\ Rebirth\.");
            FlamesofRebirth.warningMessage = @"Flames of Rebirth"; 
            


            phaseNum = 1;
            phases[phaseNum] = new Phase();

            phases[phaseNum].timedAbilities.Add(Bennu);

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.2996463), Revelation);      // 19:54:26:533 @ 98.6916419856706%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.063804), Blackfire);      // 19:54:40:597 @ 96.5580581595509%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.0318026), Whitefire);      // 19:54:54:629 @ 93.7983939241556%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(19.4051099), RotationAbility.Blank());      // 19:54:54:629 @ 93.7983939241556%
            

            //phases[phaseNum].phaseEndHP = 80;
            phases[phaseNum].phaseEndRegex = new Regex(@"\ readies\ Brand\ of\ Purgatory\.");



            phaseNum = 2;
            phases[phaseNum] = new Phase();
            phases[phaseNum].phaseStartDelay = TimeSpan.FromSeconds(0);

            phases[phaseNum].timedAbilities.Add(Bennu);

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.512459), Bluefire);      // 19:56:49:932 @ 77.817657377155%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.0053435), FlamesofUnforgiveness);      // 19:56:55:937 @ 76.7435452792082%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.917625), Redfire);      // 19:57:6:855 @ 74.8648493815126%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1972973), Revelation);      // 19:57:12:52 @ 74.180298427115%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.819848), Bluefire);      // 19:57:26:872 @ 72.2938731416506%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.237357), FlamesofUnforgiveness);      // 19:57:33:110 @ 71.8471145286119%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.917625), Redfire);      // 19:57:6:855 @ 74.8648493815126%


            phases[phaseNum].phaseEndRegex = new Regex(@"\ readies\ Flames\ of\ Rebirth\.");



            phaseNum = 3;
            phases[phaseNum] = new Phase();





            phases[phaseNum].phaseEndRegex = new Regex(@"\ readies\ Rebirth\.");



            phaseNum = 4;
            phases[phaseNum] = new Phase();



            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.436654), FountainofFire);      // 18:50:49:525 @ 51.3640550732519%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0204016), Summon);      // 18:50:56:545 @ 50.2009186150028%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.162238), Revelation);      // 18:51:0:708 @ 49.2614797321813%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.979085), FlamesofUnforgiveness);      // 18:51:19:687 @ 46.1791817579174%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(17.156981), FlamesofRebirth);      // 18:51:36:844 @ 43.348316221075%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.3194759), FountainofFire);      // 18:51:45:163 @ 41.7393304895249%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0194014), Summon);      // 18:51:52:183 @ 40.0732382224591%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1652383), Revelation);      // 18:51:56:348 @ 39.2448297215875%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.977086), FlamesofUnforgiveness);      // 18:52:15:325 @ 35.5056247209464%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(17.144980), FlamesofRebirth);      // 18:52:32:470 @ 32.1134237641391%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.024402), RedfirePlume);      // 18:52:39:494 @ 30.9370109457224%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.3985949), RotationAbility.Blank());

        }


        public void onEndEncounter()
        {

        }

        public void onMobAdded(ActorEntity mob)
        {
            if (phase < 3 && mob.Name.Contains("Bennu"))
            {

                Bennu.start();

                //bennuCount++;

                //tts("Add " + bennuCount);
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
