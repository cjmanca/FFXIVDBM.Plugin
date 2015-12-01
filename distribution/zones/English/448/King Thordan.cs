using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using System.Text.RegularExpressions;
using FFXIVAPP.Memory.Core;

namespace KingThordan
{
    
    public class MainEncounterLogic : AbilityController, IEncounter
    {
        DateTime ignoreDivineRightUntil = DateTime.MinValue;

        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "King Thordan";

            ignoreDivineRightUntil = DateTime.MinValue;


            int phaseNum = 1; // for convenience


            TriggeredAbility DivineRightTrigger = new TriggeredAbility(); // Divine Right
            timedAbilities.Add(DivineRightTrigger);
            DivineRightTrigger.announceWarning = false;
            DivineRightTrigger.match = new Regex(@"\ begins\ casting\ Divine\ Right\.");
            //DivineRightTrigger.matchMessage = @"Swap targets";
            DivineRightTrigger.matchCallback = delegate(Ability self, Match match)
            {
                if (DateTime.Now > ignoreDivineRightUntil)
                {
                    ignoreDivineRightUntil = DateTime.Now + TimeSpan.FromSeconds(5);

                    if (DivineRightTrigger.announceWarning)
                    {
                        tts("Swap targets");
                    }
                    else
                    {
                        DivineRightTrigger.announceWarning = true;
                    }
                }
            };
            



            TriggeredAbility TheDragonsGazeTrigger = new TriggeredAbility(); // Divine Right
            timedAbilities.Add(TheDragonsGazeTrigger);
            TheDragonsGazeTrigger.announceWarning = true;
            TheDragonsGazeTrigger.match = new Regex(@"\ begins\ casting\ The\ Dragon's\ Gaze\.");
            TheDragonsGazeTrigger.matchMessage = @"look away";




            // This ability list may contain duplicates. Please double check before using.
            RotationAbility VolleyFire = new RotationAbility(); // Volley Fire
            VolleyFire.announceWarning = false; 
            VolleyFire.match = new Regex(@"\ uses\ Volley\ Fire\.");
            VolleyFire.warningMessage = @"Volley Fire"; 
            
            RotationAbility ChargedVolleyFire = new RotationAbility(); // Charged Volley Fire
            ChargedVolleyFire.announceWarning = false; 
            ChargedVolleyFire.match = new Regex(@"\ uses\ Charged\ Volley\ Fire\.");
            ChargedVolleyFire.warningMessage = @"Charged Volley Fire"; 
            
            RotationAbility AscalonsMight = new RotationAbility(); // Ascalon's Might
            AscalonsMight.announceWarning = false; 
            AscalonsMight.match = new Regex(@"\ uses\ Ascalon's\ Might\.");
            AscalonsMight.warningMessage = @"Might"; 
            
            RotationAbility Meteorain = new RotationAbility(); // Meteorain
            Meteorain.announceWarning = true; 
            Meteorain.match = new Regex(@"\ begins\ casting\ Meteorain\.");
            Meteorain.warningMessage = @"";
            Meteorain.warningTime = TimeSpan.FromSeconds(2);
            
            RotationAbility AscalonsMercy = new RotationAbility(); // Ascalon's Mercy
            AscalonsMercy.announceWarning = false; 
            AscalonsMercy.match = new Regex(@"\ readies\ Ascalon's\ Mercy\.");
            AscalonsMercy.warningMessage = @"Mercy"; 
            
            RotationAbility TheDragonsEye = new RotationAbility(); // The Dragon's Eye
            TheDragonsEye.announceWarning = false; 
            TheDragonsEye.match = new Regex(@"\ begins\ casting\ The\ Dragon's\ Eye\.");
            TheDragonsEye.warningMessage = @"Eye"; 
            
            RotationAbility TheDragonsGaze = new RotationAbility(); // The Dragon's Gaze
            TheDragonsGaze.announceWarning = true; 
            TheDragonsGaze.match = new Regex(@"\ begins\ casting\ The\ Dragon's\ Gaze\.");
            TheDragonsGaze.warningMessage = @"Gaze";
            TheDragonsGaze.warningTime = TimeSpan.FromSeconds(3);
            
            RotationAbility LightningStorm = new RotationAbility(); // Lightning Storm
            LightningStorm.announceWarning = true; 
            LightningStorm.match = new Regex(@"\ begins\ casting\ Lightning\ Storm\.");
            LightningStorm.warningMessage = @"Spread out";
            LightningStorm.warningTime = TimeSpan.FromSeconds(2);
            
            RotationAbility TheDragonsRage = new RotationAbility(); // The Dragon's Rage
            TheDragonsRage.announceWarning = true; 
            TheDragonsRage.match = new Regex(@"\ begins\ casting\ The\ Dragon's\ Rage\.");
            TheDragonsRage.warningMessage = @"Stack";
            TheDragonsRage.warningTime = TimeSpan.FromSeconds(0);
            
            RotationAbility AncientQuaga = new RotationAbility(); // Ancient Quaga
            AncientQuaga.announceWarning = true; 
            AncientQuaga.match = new Regex(@"\ begins\ casting\ Ancient\ Quaga\.");
            AncientQuaga.warningMessage = @"Quaga";
            AncientQuaga.warningTime = TimeSpan.FromSeconds(2);
            
            RotationAbility HeavenlyHeel = new RotationAbility(); // Heavenly Heel
            HeavenlyHeel.announceWarning = true; 
            HeavenlyHeel.match = new Regex(@"\ readies\ Heavenly\ Heel\.");
            HeavenlyHeel.warningMessage = @"Tank buster";
            HeavenlyHeel.warningTime = TimeSpan.FromSeconds(1);
            
            RotationAbility ManaCapacitor = new RotationAbility(); // Mana Capacitor
            ManaCapacitor.announceWarning = false; 
            ManaCapacitor.match = new Regex(@"\ uses\ Mana\ Capacitor\.");
            ManaCapacitor.warningMessage = @"Mana Capacitor"; 
            
            RotationAbility Heavensflame = new RotationAbility(); // Heavensflame
            Heavensflame.announceWarning = false; 
            Heavensflame.match = new Regex(@"\ begins\ casting\ Heavensflame\.");
            //Heavensflame.warningMessage = @"stack then spread";
            Heavensflame.warningCallback = delegate(Ability self)
            {
                if (Heavensflame.announceWarning)
                {
                    tts("stack then spread");
                }
                else
                {
                    Heavensflame.announceWarning = true;
                }
            };
            
            RotationAbility HolyChain = new RotationAbility(); // Holy Chain
            HolyChain.announceWarning = false; 
            HolyChain.match = new Regex(@"\ uses\ Holy\ Chain\.");
            HolyChain.warningMessage = @"Holy Chain"; 
            
            RotationAbility Conviction = new RotationAbility(); // Conviction
            Conviction.announceWarning = false; 
            Conviction.match = new Regex(@"\ readies\ Conviction\.");
            Conviction.warningMessage = @"Conviction"; 
            
            RotationAbility SacredCross = new RotationAbility(); // Sacred Cross
            SacredCross.announceWarning = false; 
            SacredCross.match = new Regex(@"\ readies\ Sacred\ Cross\.");
            SacredCross.warningMessage = @"Sacred Cross";
            SacredCross.warningTime = TimeSpan.FromSeconds(2);
            
            RotationAbility SpiralThrust = new RotationAbility(); // Spiral Thrust
            SpiralThrust.announceWarning = true; 
            SpiralThrust.match = new Regex(@"\ readies\ Spiral\ Thrust\.");
            SpiralThrust.warningMessage = @"Dashes";
            SpiralThrust.warningTime = TimeSpan.FromSeconds(3);


            RotationAbility SkywardLeap = new RotationAbility(); // Skyward Leap
            SkywardLeap.announceWarning = true;
            SkywardLeap.warningMessage = @"Skyward Leap";
            SkywardLeap.warningTime = TimeSpan.FromSeconds(7);
            

            RotationAbility HolyBladedance = new RotationAbility(); // Holy Bladedance
            HolyBladedance.announceWarning = true;
            HolyBladedance.match = new Regex(@"\ readies\ Holy\ Bladedance\.");
            HolyBladedance.warningMessage = @"Tank buster";
            HolyBladedance.warningTime = TimeSpan.FromSeconds(2);

            RotationAbility HeavenlySlash = new RotationAbility(); // Heavenly Slash
            HeavenlySlash.announceWarning = false;
            HeavenlySlash.match = new Regex(@"\ uses\ Heavenly\ Slash\.");
            HeavenlySlash.warningMessage = @"Heavenly Slash";

            RotationAbility DivineRight = new RotationAbility(); // Divine Right
            DivineRight.announceWarning = false;
            DivineRight.match = new Regex(@"\ begins\ casting\ Divine\ Right\.");
            DivineRight.warningMessage = @"Swap targets";
            DivineRight.warningTime = TimeSpan.FromSeconds(0);

            RotationAbility HoliestofHoly = new RotationAbility(); // Holiest of Holy
            HoliestofHoly.announceWarning = true;
            HoliestofHoly.match = new Regex(@"\ readies\ Holiest\ of\ Holy\.");
            HoliestofHoly.warningMessage = @"Holiest of Holy";
            HoliestofHoly.warningTime = TimeSpan.FromSeconds(2);

            RotationAbility DimensionalCollapse = new RotationAbility(); // Dimensional Collapse
            DimensionalCollapse.announceWarning = false;
            DimensionalCollapse.match = new Regex(@"\ readies\ Dimensional\ Collapse\.");
            DimensionalCollapse.warningMessage = @"Dimensional Collapse";

            RotationAbility FaithUnmoving = new RotationAbility(); // Faith Unmoving
            FaithUnmoving.announceWarning = true;
            FaithUnmoving.match = new Regex(@"\ uses\ Faith\ Unmoving\.");
            FaithUnmoving.warningMessage = @"Center for knockback";

            RotationAbility HiemalStorm = new RotationAbility(); // Hiemal Storm
            HiemalStorm.announceWarning = false;
            HiemalStorm.match = new Regex(@"\ begins\ casting\ Hiemal\ Storm\.");
            HiemalStorm.warningMessage = @"Hiemal Storm";

            RotationAbility HolyMeteor = new RotationAbility(); // Holy Meteor
            HolyMeteor.announceWarning = false;
            HolyMeteor.match = new Regex(@"\ begins\ casting\ Holy\ Meteor\.");
            HolyMeteor.warningMessage = @"Holy Meteor";

            RotationAbility SpiralPierce = new RotationAbility(); // Spiral Pierce
            SpiralPierce.announceWarning = false;
            SpiralPierce.match = new Regex(@"\ readies\ Spiral\ Pierce\.");
            SpiralPierce.warningMessage = @"Spiral Pierce";

            RotationAbility HeavyImpact = new RotationAbility(); // Heavy Impact
            HeavyImpact.announceWarning = false;
            HeavyImpact.match = new Regex(@"\ uses\ Heavy\ Impact\.");
            HeavyImpact.warningMessage = @"Heavy Impact";

            RotationAbility TheLightofAscalon = new RotationAbility(); // The Light of Ascalon
            TheLightofAscalon.announceWarning = false;
            TheLightofAscalon.match = new Regex(@"\ casts\ The\ Light\ of\ Ascalon\.");
            TheLightofAscalon.warningMessage = @"The Light of Ascalon";


            RotationAbility UltimateEnd = new RotationAbility(); // Ultimate End
            UltimateEnd.announceWarning = false;
            UltimateEnd.match = new Regex(@"\ casts\ Ultimate\ End\.");
            UltimateEnd.warningMessage = @"Ultimate End";

            RotationAbility KnightsoftheRound = new RotationAbility(); // Knights of the Round
            KnightsoftheRound.announceWarning = false;
            KnightsoftheRound.match = new Regex(@"\ begins\ casting\ Knights\ of\ the\ Round\.");
            KnightsoftheRound.warningMessage = @"Knights of the Round";

            RotationAbility HolyShieldBash = new RotationAbility(); // Holy Shield Bash
            HolyShieldBash.announceWarning = true;
            HolyShieldBash.match = new Regex(@"\ readies\ Holy\ Shield\ Bash\.");
            HolyShieldBash.warningMessage = @"Stack for charge";
            HolyShieldBash.warningTime = TimeSpan.FromSeconds(0);

            RotationAbility SpearoftheFury = new RotationAbility(); // Spear of the Fury
            SpearoftheFury.announceWarning = false;
            SpearoftheFury.match = new Regex(@"\ readies\ Spear\ of\ the\ Fury\.");
            SpearoftheFury.warningMessage = @"Spear of the Fury";

            RotationAbility TheDragonsGlory = new RotationAbility(); // The Dragon's Glory
            TheDragonsGlory.announceWarning = false;
            TheDragonsGlory.match = new Regex(@"\ begins\ casting\ The\ Dragon's\ Glory\.");
            TheDragonsGlory.warningMessage = @"The Dragon's Glory"; 
            
            


            // You'll need to split these up into phases
            // And separate out any timed moves which aren't part of a rotation
            // For now we'll assume they're all part of phase 1
            
            phaseNum = 1;
            phases[phaseNum] = new Phase();
            
            // You can use one of the following two methods for determining the end of a phase.
            // Just choose which is appropriate to the encounter and uncomment/modify to fit
            //phases[phaseNum].phaseEndHP = 90;
            //phases[phaseNum].phaseEndRegex = new Regex("Titan uses Geocrush");

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.8490486), ChargedVolleyFire);      // 19:26:52:235 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.4241958), AscalonsMight);      // 19:26:55:659 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.4023662), Meteorain);      // 19:27:2:61 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.3210184), ChargedVolleyFire);      // 19:27:2:382 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.8902797), AscalonsMercy);      // 19:27:7:273 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.2990171), VolleyFire);      // 19:27:7:572 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.735328), AscalonsMight);      // 19:27:13:307 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.5580892), TheDragonsEye);      // 19:27:14:865 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.8451627), VolleyFire);      // 19:27:17:710 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.2934171), TheDragonsGaze);      // 19:27:25:4 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2543578), VolleyFire);      // 19:27:31:258 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.5691469), AscalonsMight);      // 19:27:33:827 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9532833), LightningStorm);      // 19:27:38:780 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.6923256), VolleyFire);      // 19:27:44:473 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0080005), TheDragonsRage);      // 19:27:44:481 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.5944343), AncientQuaga);      // 19:27:52:75 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.6331507), VolleyFire);      // 19:27:54:708 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.6953829), AscalonsMight);      // 19:28:1:404 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.5450884), HeavenlyHeel);      // 19:28:2:949 @ 99.727790453426%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.423253), VolleyFire);      // 19:28:7:372 @ 99.727790453426%


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(900), RotationAbility.Blank());      // 22:17:13:763 @ 99.9815297696654%


            phases[phaseNum].phaseEndRegex = new Regex(@"\ begins\ casting\ Heavensflame\.");

            phaseNum = 2;
            phases[phaseNum] = new Phase();
            


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Heavensflame);      // 22:16:40:757 @ 99.9815297696654%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.071119), Conviction);      // 22:16:42:828 @ 99.9815297696654%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.787045), HolyChain);      // 22:16:43:615 @ 99.9815297696654%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.353535), SacredCross);      // 22:16:52:969 @ 99.9815297696654%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.2492431), VolleyFire);      // 22:16:57:218 @ 99.9815297696654%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.0565752), VolleyFire);      // 22:17:7:274 @ 99.9815297696654%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.712327), SpiralThrust);      // 22:17:12:987 @ 99.9815297696654%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.7760444), SacredCross);      // 22:17:13:763 @ 99.9815297696654%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(20.231157), DivineRight);      // 20:36:8:440 @ 99.8712063595538%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.146466), HolyBladedance);      // 20:36:16:586 @ 99.8712063595538%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(18.950084), DivineRight);      // 20:36:35:536 @ 99.8712063595538%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.771044), SkywardLeap);      // 20:36:36:307 @ 99.8712063595538%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.932282), HeavenlySlash);      // 20:36:41:240 @ 99.8712063595538%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.084519), HoliestofHoly);      // 20:45:0:619 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.207298), HolyBladedance);      // 20:45:5:826 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.6954974), DivineRight);      // 20:45:14:522 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.374365), HoliestofHoly);      // 20:45:20:896 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.321476), HeavenlySlash);      // 20:45:29:218 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(15.340878), HolyBladedance);      // 20:45:44:559 @ 99.9079657395335%


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(900), RotationAbility.Blank());      // 22:17:13:763 @ 99.9815297696654%
            

            phases[phaseNum].phaseEndRegex = new Regex(@"\ readies\ Spiral\ Pierce\.");

            phaseNum = 3;
            phases[phaseNum] = new Phase();
            

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0), SpiralPierce);      // 20:46:10:809 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.297074), DimensionalCollapse);      // 20:46:12:106 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.0420596), HiemalStorm);      // 20:46:13:148 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.9763418), HolyMeteor);      // 20:46:19:125 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.604149), FaithUnmoving);      // 20:46:21:729 @ 99.9079657395335%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.654609), HeavyImpact);      // 0:4:4:766 @ 99.9516967260611%


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(900), RotationAbility.Blank());      // 22:17:13:763 @ 99.9815297696654%



            phases[phaseNum].phaseEndRegex = new Regex(@"\ casts\ Ultimate\ End\.");

            phaseNum = 4;
            phases[phaseNum] = new Phase();


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), UltimateEnd);      // 0:5:20:673 @ 99.9516967260611%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.535431), TheDragonsEye);      // 0:5:28:208 @ 99.9516967260611%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.1495806), KnightsoftheRound);      // 0:5:38:358 @ 99.9516967260611%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.1815251), AscalonsMight);      // 0:5:47:539 @ 99.9516967260611%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.293074), HolyShieldBash);      // 0:5:48:832 @ 99.9516967260611%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.557432), SpearoftheFury);      // 0:5:56:390 @ 99.9516967260611%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.3152468), HeavenlyHeel);      // 0:6:0:705 @ 99.9516967260611%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.995343), SacredCross);      // 0:6:6:700 @ 99.9516967260611%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.246014), TheDragonsGaze);      // 0:6:6:946 @ 99.9516967260611%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0050003), TheDragonsGlory);      // 20:15:23:726 @ 99.8097747356222%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(12.211698), VolleyFire);      // 20:15:35:938 @ 99.8097747356222%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0030002), AncientQuaga);      // 20:15:35:941 @ 99.8097747356222%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.2132981), TheDragonsEye);      // 20:25:6:76 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.2690726), VolleyFire);      // 20:25:7:345 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.8565066), KnightsoftheRound);      // 20:25:16:201 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.4042519), VolleyFire);      // 20:25:20:605 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1962972), Conviction);      // 20:25:25:802 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.0390594), TheDragonsGaze);      // 20:25:26:841 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0010001), TheDragonsGlory);      // 20:25:26:842 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.7780445), HeavyImpact);      // 20:25:27:620 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.128179), VolleyFire);      // 20:25:30:748 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.381193), DimensionalCollapse);      // 20:25:34:129 @ 99.8878205128205%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.8483917), TheDragonsRage);      // 23:42:31:457 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.7771588), FaithUnmoving);      // 23:42:34:234 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.3364768), AscalonsMight);      // 23:42:42:571 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.3793077), TheDragonsEye);      // 23:42:47:950 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.947569), KnightsoftheRound);      // 23:42:57:897 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.8294478), LightningStorm);      // 23:43:5:727 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.5542605), SpiralThrust);      // 23:43:10:281 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.1020058), SpiralPierce);      // 23:43:10:383 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.5741472), TheDragonsRage);      // 23:43:12:957 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.6512089), SkywardLeap);      // 23:43:16:609 @ 99.9809412574243%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.5824909), HeavenlyHeel);      // 23:43:25:191 @ 38.2107869766768%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.2644155), AscalonsMight);      // 23:43:32:455 @ 37.2555410691004%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2523576), TheDragonsEye);      // 23:43:38:708 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.875622), KnightsoftheRound);      // 23:43:49:583 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.3975375), AscalonsMight);      // 23:43:58:981 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.262015), HolyMeteor);      // 23:43:59:243 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.0320591), TheDragonsGaze);      // 23:44:0:275 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0040002), TheDragonsGlory);      // 23:44:0:279 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.04906), Heavensflame);      // 23:44:1:328 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.850163), HolyChain);      // 23:44:4:178 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.013001), HiemalStorm);      // 23:44:4:191 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.0681182), AscalonsMercy);      // 23:44:6:259 @ 37.1717912501811%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.0484604), AncientQuaga);      // 23:44:14:308 @ 32.4067887150514%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1952971), HeavenlyHeel);      // 23:44:19:503 @ 32.0361618137042%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.240357), AncientQuaga);      // 23:44:25:744 @ 31.8360223815732%


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9432827), TheDragonsEye);      // 23:44:42:736 @ 99.9743770824279%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.1145785), KnightsoftheRound);      // 23:44:52:851 @ 99.9743770824279%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.1165215), AscalonsMight);      // 23:45:1:967 @ 99.9743770824279%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.297074), HoliestofHoly);      // 23:45:3:264 @ 99.9743770824279%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.8505063), AscalonsMight);      // 23:45:12:115 @ 99.9743770824279%
            


            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(900), RotationAbility.Blank());      // 22:17:13:763 @ 99.9815297696654%
            
            

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
