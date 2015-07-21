using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace RavanaNS
{

    public class MainEncounterLogic : AbilityController, IEncounter
    {
        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Ravana";

            int phaseNum = 1; // for convenience


            RotationAbility NextPhase = new RotationAbility(); // Nerve Gas
            NextPhase.announceWarning = false;
            NextPhase.warningTime = TimeSpan.FromSeconds(0);
            NextPhase.warningCallback = delegate(Ability self)
            {
                nextPhase();
            };


            TriggeredAbility Prey = new TriggeredAbility();
            TriggeredAbility PreyToTank = new TriggeredAbility();



            timedAbilities.Add(PreyToTank);
            PreyToTank.announceWarning = true;
            PreyToTank.collectMultipleLinesFor = TimeSpan.FromSeconds(0.5);
            PreyToTank.match = new Regex(@"(?<member>[^ ]+?) [^ ]+ suffers the effect of Prey\.");
            PreyToTank.matchMessage = "${member.last} prey to tank";
            PreyToTank.timerDuration = TimeSpan.FromSeconds(1);
            PreyToTank.warningTime = TimeSpan.FromSeconds(0);
            PreyToTank.warningCallback = delegate(Ability self)
            {
                Prey.announceWarning = false;
                PreyToTank.announceWarning = false;
            };


            timedAbilities.Add(Prey);
            Prey.announceWarning = false;
            Prey.collectMultipleLinesFor = TimeSpan.FromSeconds(0.5);
            Prey.match = new Regex(@"(?<member>[^ ]+?) [^ ]+ suffers the effect of Prey\.");
            Prey.matchMessage = "${member.1} ${member.2} ${member.3} prey to markers";
            Prey.timerDuration = TimeSpan.FromSeconds(1);
            Prey.warningTime = TimeSpan.FromSeconds(0);
            Prey.warningCallback = delegate(Ability self)
            {
                Prey.announceWarning = false;
                PreyToTank.announceWarning = false;
            };


            TriggeredAbility TheSeeingLeft = new TriggeredAbility();
            timedAbilities.Add(TheSeeingLeft);
            TheSeeingLeft.match = new Regex(@"Ravana readies The Seeing Left");
            TheSeeingLeft.matchMessage = "Right";


            TriggeredAbility TheSeeingRight = new TriggeredAbility();
            timedAbilities.Add(TheSeeingRight);
            TheSeeingRight.match = new Regex(@"Ravana readies The Seeing Right");
            TheSeeingRight.matchMessage = "Left";


            TriggeredAbility TheSeeingWings = new TriggeredAbility();
            timedAbilities.Add(TheSeeingWings);
            TheSeeingWings.match = new Regex(@"Ravana readies The Seeing Wings");
            TheSeeingWings.matchMessage = "Wings";



            // Boss's abilities
            RotationAbility BlindingBlade = new RotationAbility(); // Blinding Blade
            BlindingBlade.announceWarning = true;
            BlindingBlade.warningTime = TimeSpan.FromSeconds(6);
            BlindingBlade.match = new Regex(@"\ uses\ Blinding\ Blade\.");
            BlindingBlade.warningMessage = @"Blinding Blade";

            RotationAbility BladesofCarnageandLiberationAway = new RotationAbility(); // Blades of Carnage and Liberation
            BladesofCarnageandLiberationAway.announceWarning = true;
            BladesofCarnageandLiberationAway.warningTime = TimeSpan.FromSeconds(-3);
            BladesofCarnageandLiberationAway.match = new Regex(@"\ uses\ Blades\ of\ Carnage\ and\ Liberation\.");
            BladesofCarnageandLiberationAway.warningMessage = @"Move to wall";


            RotationAbility BladesofCarnageandLiberationBehind = new RotationAbility(); // Blades of Carnage and Liberation
            BladesofCarnageandLiberationBehind.announceWarning = true;
            BladesofCarnageandLiberationBehind.warningTime = TimeSpan.FromSeconds(0);
            BladesofCarnageandLiberationBehind.match = new Regex(@"\ uses\ Blades\ of\ Carnage\ and\ Liberation\.");
            BladesofCarnageandLiberationBehind.warningMessage = @"Move behind";

            RotationAbility BladesofCarnageandLiberationNumbers = new RotationAbility(); // Blades of Carnage and Liberation
            BladesofCarnageandLiberationNumbers.announceWarning = true;
            BladesofCarnageandLiberationNumbers.warningTime = TimeSpan.FromSeconds(0);
            BladesofCarnageandLiberationNumbers.match = new Regex(@"\ uses\ Blades\ of\ Carnage\ and\ Liberation\.");
            BladesofCarnageandLiberationNumbers.warningMessage = @"Numbers to markers. Everyone else to 4.";

            RotationAbility BladesofCarnageandLiberationPrey = new RotationAbility(); // Blades of Carnage and Liberation
            BladesofCarnageandLiberationPrey.announceWarning = false;
            BladesofCarnageandLiberationPrey.warningTime = TimeSpan.FromSeconds(0);
            BladesofCarnageandLiberationPrey.match = new Regex(@"\ uses\ Blades\ of\ Carnage\ and\ Liberation\.");
            BladesofCarnageandLiberationPrey.warningCallback = delegate(Ability self)
            {
                Prey.announceWarning = true;
                PreyToTank.announceWarning = false;
            };


            RotationAbility PillarsofHeaven = new RotationAbility(); // Pillars of Heaven
            PillarsofHeaven.announceWarning = true;
            PillarsofHeaven.warningTime = TimeSpan.FromSeconds(2);
            PillarsofHeaven.match = new Regex(@"\ readies\ Pillars\ of\ Heaven\.");
            PillarsofHeaven.warningMessage = @"Knockback soon";
            PillarsofHeaven.warningCallback = delegate(Ability self)
            {
                Prey.announceWarning = false;
                PreyToTank.announceWarning = true;
            };



             

            RotationAbility LaughingRose = new RotationAbility(); // Laughing Rose
            LaughingRose.announceWarning = true;
            LaughingRose.warningTime = TimeSpan.FromSeconds(6);
            LaughingRose.match = new Regex(@"\ uses\ Laughing\ Rose\.");
            LaughingRose.warningMessage = @"Gather in black circle";

            RotationAbility TheRoseofConviction = new RotationAbility(); // The Rose of Conviction
            TheRoseofConviction.announceWarning = true;
            TheRoseofConviction.warningTime = TimeSpan.FromSeconds(5);
            TheRoseofConviction.match = new Regex(@"\ uses\ The\ Rose\ of\ Conviction\.");
            TheRoseofConviction.warningMessage = @"Orbs soon";

            RotationAbility BeetleAvatar = new RotationAbility(); // Beetle Avatar
            BeetleAvatar.announceWarning = false;
            BeetleAvatar.match = new Regex(@"\ uses\ Beetle\ Avatar\.");




            RotationAbility DragonflyAvatar = new RotationAbility(); // Dragonfly Avatar
            DragonflyAvatar.announceWarning = false;
            DragonflyAvatar.match = new Regex(@"\ uses\ Dragonfly\ Avatar\.");

                
            RotationAbility TheSeeingRotation = new RotationAbility(); // The Seeing Right
            TheSeeingRotation.announceWarning = false;
            TheSeeingRotation.match = new Regex(@"\ readies\ The\ Seeing\ ");

            RotationAbility AtmaLinga = new RotationAbility(); // Atma-Linga
            AtmaLinga.announceWarning = false;
            AtmaLinga.match = new Regex(@"\ uses\ Atma-Linga\.");

            RotationAbility Tapasya = new RotationAbility(); // Tapasya
            Tapasya.announceWarning = false;
            Tapasya.match = new Regex(@"\ uses\ Tapasya\.");

            RotationAbility ScorpionAvatar = new RotationAbility(); // Scorpion Avatar
            ScorpionAvatar.announceWarning = false;
            ScorpionAvatar.match = new Regex(@"\ uses\ Scorpion\ Avatar\.");

            RotationAbility BladesofCarnageandLiberation = new RotationAbility(); // Blades of Carnage and Liberation
            BladesofCarnageandLiberation.announceWarning = false;
            BladesofCarnageandLiberation.match = new Regex(@"\ uses\ Blades\ of\ Carnage\ and\ Liberation\.");




            RotationAbility PreludetoLiberation = new RotationAbility(); // Prelude to Liberation
            PreludetoLiberation.announceWarning = false;
            PreludetoLiberation.match = new Regex(@"\ readies\ Prelude\ to\ Liberation\.");

            RotationAbility Liberation = new RotationAbility(); // Liberation
            Liberation.announceWarning = false;
            Liberation.match = new Regex(@"\ readies\ Liberation\.");

            RotationAbility WarlordShell = new RotationAbility(); // Warlord Shell
            WarlordShell.announceWarning = false;
            WarlordShell.match = new Regex(@"\ readies\ Warlord\ Shell\.");

            RotationAbility BloodyFuller = new RotationAbility(); // Bloody Fuller
            BloodyFuller.announceWarning = false;
            BloodyFuller.match = new Regex(@"\ readies\ Bloody\ Fuller\.");


            RotationAbility Surpanakha = new RotationAbility(); // Surpanakha
            Surpanakha.announceWarning = false;
            Surpanakha.match = new Regex(@"\ uses\ Surpanakha\.");

            RotationAbility TheRoseofHate = new RotationAbility(); // The Rose of Hate
            TheRoseofHate.announceWarning = false;
            TheRoseofHate.match = new Regex(@"\ readies\ The\ Rose\ of\ Hate\.");

            RotationAbility SwiftLiberation = new RotationAbility(); // Swift Liberation
            SwiftLiberation.announceWarning = false;
            SwiftLiberation.match = new Regex(@"\ readies\ Swift\ Liberation\.");

            RotationAbility FinalLiberation = new RotationAbility(); // Final Liberation
            FinalLiberation.announceWarning = false;
            FinalLiberation.match = new Regex(@"\ readies\ Final\ Liberation\.");

            // Boss ability rotation
            // You'll need to split these up into phases
            // And separate out any timed moves which aren't part of a rotation
            // For now we'll assume they're all part of phase 1

            phaseNum = 1;
            phases[phaseNum] = new Phase();
            phases[phaseNum].phaseStartDelay = TimeSpan.FromSeconds(2);

            // You can use one of the following two methods for determining the end of a phase.
            // Just choose which is appropriate to the encounter and uncomment/modify to fit
            //phases[phaseNum].phaseEndHP = 90;
            //phases[phaseNum].phaseEndRegex = new Regex(@"Titan uses Geocrush");


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3891938), BlindingBlade);      // 18:59:14:969 @ 99.1163158144839%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3481343), TheSeeingRotation);      // 18:59:17:317 @ 98.6382698156244%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.979342), AtmaLinga);      // 18:59:23:296 @ 96.7243038395742%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2423571), Tapasya);      // 18:59:29:538 @ 95.2761475955142%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0094009), AtmaLinga);      // 18:59:36:548 @ 93.2072087055693%


            phases[phaseNum].phaseEndRegex = new Regex(@"\ uses\ Scorpion\ Avatar\.");
            

            phaseNum = 2;
            phases[phaseNum] = new Phase();

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.3094752), ScorpionAvatar);      // 18:59:58:389 @ 86.9275090287018%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9582836), BladesofCarnageandLiberationAway);      // 19:0:3:347 @ 85.2610007603117%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.7750444), PreludetoLiberation);      // 19:0:4:122 @ 84.9592520433378%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(17.6610101), PreludetoLiberation);      // 19:0:21:783 @ 75.0606467401635%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(13.5337741), PreludetoLiberation);      // 19:0:35:317 @ 75.0606467401635%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.273416), BladesofCarnageandLiberationBehind);      // 19:0:42:591 @ 74.7178530697586%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.3180754), Liberation);      // 19:0:43:909 @ 74.2740804980042%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(17.6720108), Liberation);      // 19:1:1:581 @ 67.2606206044478%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.1836397), Liberation);      // 19:1:12:764 @ 67.2606206044478%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(13.2937604), NextPhase);      
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(120), RotationAbility.Blank());      // 19:1:12:764 @ 67.2606206044478%


            phases[phaseNum].phaseEndRegex = new Regex(@"\ uses\ Dragonfly\ Avatar\.");


            phaseNum = 3;
            phases[phaseNum] = new Phase();


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), DragonflyAvatar);      // 19:1:23:948 @ 66.4834394601787%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.5080291), WarlordShell);      // 19:1:24:456 @ 66.1968969777609%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9522832), TheSeeingRotation);      // 19:1:29:408 @ 65.4513162896788%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.7914457), TheSeeingRotation);      // 19:1:37:200 @ 65.2590405816385%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(15.0728621), Tapasya);      // 19:1:52:273 @ 62.1980136856111%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.2854167), AtmaLinga);      // 19:1:59:558 @ 60.2155008553507%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.238128), BlindingBlade);      // 19:2:1:796 @ 59.9950104542863%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.1791247), TheSeeingRotation);      // 19:2:3:975 @ 59.8162183995438%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2433571), AtmaLinga);      // 19:2:10:219 @ 59.101050180574%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.0852908), Tapasya);      // 19:2:15:304 @ 58.8442667743775%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.4054236), AtmaLinga);      // 19:2:22:709 @ 58.2063177152633%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.8361622), BlindingBlade);      // 19:2:25:545 @ 58.081519673066%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.7283276), Tapasya);      // 19:2:31:274 @ 57.2644815624406%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.5784907), BlindingBlade);      // 19:2:39:852 @ 56.4530863904201%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.0181726), TheSeeingRotation);      // 19:2:42:870 @ 56.1412397833112%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.885394), AtmaLinga);      // 19:2:49:756 @ 55.3360815434328%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.6145499), BloodyFuller);      // 19:2:59:370 @ 53.8129633149591%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), NextPhase);
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(120), RotationAbility.Blank());      // 19:1:12:764 @ 67.2606206044478%



            phases[phaseNum].phaseEndRegex = new Regex(@"\ readies\ Bloody\ Fuller\.");



            phaseNum = 4;
            phases[phaseNum] = new Phase();

            phases[phaseNum].phaseStartDelay = TimeSpan.FromSeconds(23.390337);


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.8845654), BeetleAvatar);      // 19:3:25:612 @ 50.9159380345942%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.2750157), PillarsofHeaven);      // 19:3:25:887 @ 50.9159380345942%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.9646844), LaughingRose);      // 19:3:37:852 @ 49.9891299182665%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.805847), Surpanakha);      // 19:3:52:657 @ 48.0743917506178%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.5814909), TheRoseofConviction);      // 19:4:1:239 @ 46.9768104923018%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.8301046), TheRoseofHate);      // 19:4:3:69 @ 46.8498740733701%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.8415057), Surpanakha);      // 19:4:11:911 @ 46.0647928150542%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.14058), ScorpionAvatar);      // 19:4:22:51 @ 45.216094848888%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9242817), BladesofCarnageandLiberationNumbers);      // 19:4:26:975 @ 44.2476477855921%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.5210298), SwiftLiberation);      // 19:4:27:496 @ 44.2476477855921%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.7100701), SwiftLiberation);      // 19:4:46:207 @ 35.82856158525%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.433654), SwiftLiberation);      // 19:4:57:640 @ 35.82856158525%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.634666), BladesofCarnageandLiberationPrey);      // 19:5:9:275 @ 35.416745865805%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.646037), FinalLiberation);      // 19:5:9:921 @ 35.1754062915796%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.6980695), FinalLiberation);      // 19:5:28:619 @ 30.7056049230184%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(13.516773), FinalLiberation);      // 19:5:42:136 @ 30.7056049230184%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(15.3408775), DragonflyAvatar);      // 19:5:57:477 @ 29.974220680479%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.508029), WarlordShell);      // 19:5:57:985 @ 29.8415819235887%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1942971), TheSeeingRotation);      // 19:6:3:179 @ 29.0033382436799%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2553578), AtmaLinga);      // 19:6:9:434 @ 27.4062084204524%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.5430883), TheSeeingRotation);      // 19:6:10:977 @ 27.0402490020909%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.456655), BlindingBlade);      // 19:6:22:434 @ 24.7106657479567%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.5881481), AtmaLinga);      // 19:6:25:22 @ 23.7753635240449%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.09812), Tapasya);      // 19:6:27:120 @ 23.0389303364379%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.3435344), BlindingBlade);      // 19:6:36:464 @ 20.3507531838054%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.8621637), AtmaLinga);      // 19:6:39:326 @ 19.7432759931572%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9422827), Tapasya);      // 19:6:44:268 @ 18.3481633719825%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.765387), BlindingBlade);      // 19:6:51:34 @ 16.5104661661281%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.1511802), AtmaLinga);      // 19:6:54:185 @ 15.7070305075081%

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.808847), BeetleAvatar);      // 19:27:20:62 @ 23.4730208135336%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.5130293), PillarsofHeaven);      // 19:27:20:575 @ 23.2640538870937%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(12.2467005), LaughingRose);      // 19:27:32:822 @ 21.9825722296141%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.558832), Surpanakha);      // 19:27:47:381 @ 20.3381011214598%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.584491), TheRoseofConviction);      // 19:27:55:965 @ 19.184684470633%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.0701184), TheRoseofHate);      // 19:27:58:35 @ 19.0196136666033%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.5794908), Surpanakha);      // 19:28:6:615 @ 18.4432617373123%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.6782675), ScorpionAvatar);      // 19:28:11:293 @ 18.1206044478236%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.6792677), BladesofCarnageandLiberationPrey);      // 19:28:15:972 @ 17.3441360957993%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.808047), FinalLiberation);      // 19:28:16:780 @ 17.2332969017297%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.692069), FinalLiberation);      // 19:28:35:473 @ 5.62892035734651%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(13.509773), FinalLiberation);      // 19:28:48:982 @ 5.62892035734651%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.570833), BladesofCarnageandLiberation);      // 19:29:3:553 @ 5.00445495153013%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.5680325), Liberation);      // 19:29:4:121 @ 4.95028274092378%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(120), RotationAbility.Blank());      // 19:1:12:764 @ 67.2606206044478%


        }

        public void onMobAdded(ActorEntity mob)
        {


        }


        public void onMobRemoved(ActorEntity mob)
        {

        }

        public void onEndEncounter()
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