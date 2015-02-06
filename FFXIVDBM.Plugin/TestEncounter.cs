using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace FFXIVDBM.Plugin
{

    public class TestEncounter : AbilityController, IEncounter
    {
        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Kaliya";
            
            int phaseNum = 1; // for convenience


            TriggeredAbility ForkedLightning = new TriggeredAbility(); // Forked Lightning
            timedAbilities.Add(ForkedLightning);
            ForkedLightning.match = new Regex(@"(?<member>[^ ]+?) [^ ]+ suffers the effect of Forked Lightning\.");
            ForkedLightning.matchMessage = "${member.1} ${member.2} ${member.3} forked lightning";

            
            
            // This ability list may contain duplicates. Please double check before using.
            RotationAbility Resonance = new RotationAbility(); // Resonance
            Resonance.announceWarning = true; 
            Resonance.match = new Regex(@"\ uses\ Resonance\.");
            Resonance.warningMessage = @"Resonance"; 
            
            RotationAbility NerveGas = new RotationAbility(); // Nerve Gas
            NerveGas.announceWarning = true; 
            NerveGas.match = new Regex(@"\ readies\ Nerve\ Gas\.");
            NerveGas.warningMessage = @"Nerve Gas"; 
            
            RotationAbility VulnerabilityUp = new RotationAbility(); // Vulnerability Up
            VulnerabilityUp.announceWarning = true; 
            VulnerabilityUp.match = new Regex(@"\ suffers\ the\ effect\ of\ Vulnerability\ Up\.");
            VulnerabilityUp.warningMessage = @"Vulnerability Up"; 
            
            RotationAbility Barofield = new RotationAbility(); // Barofield
            Barofield.announceWarning = true; 
            Barofield.match = new Regex(@"\ readies\ Barofield\.");
            Barofield.warningMessage = @"Barofield"; 
            
            RotationAbility SeedoftheRivers = new RotationAbility(); // Seed of the Rivers
            SeedoftheRivers.announceWarning = true; 
            SeedoftheRivers.match = new Regex(@"\ uses\ Seed\ of\ the\ Rivers\.");
            SeedoftheRivers.warningMessage = @"Seed of the Rivers"; 
            
            RotationAbility SeedoftheSea = new RotationAbility(); // Seed of the Sea
            SeedoftheSea.announceWarning = true; 
            SeedoftheSea.match = new Regex(@"\ uses\ Seed\ of\ the\ Sea\.");
            SeedoftheSea.warningMessage = @"Seed of the Sea"; 
            
            RotationAbility SecondaryHead = new RotationAbility(); // Secondary Head
            SecondaryHead.announceWarning = true; 
            SecondaryHead.match = new Regex(@"\ uses\ Secondary\ Head\.");
            SecondaryHead.warningMessage = @"Secondary Head"; 
            
            RotationAbility Stun = new RotationAbility(); // Stun
            Stun.announceWarning = true; 
            Stun.match = new Regex(@"\ suffers\ the\ effect\ of\ Stun\.");
            Stun.warningMessage = @"Stun"; 
            
            RotationAbility IntheHeadlights = new RotationAbility(); // In the Headlights
            IntheHeadlights.announceWarning = true; 
            IntheHeadlights.match = new Regex(@"\ suffers\ the\ effect\ of\ In\ the\ Headlights\.");
            IntheHeadlights.warningMessage = @"In the Headlights"; 
            
            RotationAbility MainHead = new RotationAbility(); // Main Head
            MainHead.announceWarning = true; 
            MainHead.match = new Regex(@"\ uses\ Main\ Head\.");
            MainHead.warningMessage = @"Main Head"; 
            
            RotationAbility Heavy = new RotationAbility(); // Heavy
            Heavy.announceWarning = true; 
            Heavy.match = new Regex(@"\ suffers\ the\ effect\ of\ Heavy\.");
            Heavy.warningMessage = @"Heavy"; 
            
            RotationAbility VacuumWave = new RotationAbility(); // Vacuum Wave
            VacuumWave.announceWarning = true; 
            VacuumWave.match = new Regex(@"\ uses\ Vacuum\ Wave\.");
            VacuumWave.warningMessage = @"Vacuum Wave"; 
            
            RotationAbility Paralysis = new RotationAbility(); // Paralysis
            Paralysis.announceWarning = true; 
            Paralysis.match = new Regex(@"\ suffers\ the\ effect\ of\ Paralysis\.");
            Paralysis.warningMessage = @"Paralysis"; 
            
            RotationAbility RepellingCannons = new RotationAbility(); // Repelling Cannons
            RepellingCannons.announceWarning = true; 
            RepellingCannons.match = new Regex(@"\ uses\ Repelling\ Cannons\.");
            RepellingCannons.warningMessage = @"Repelling Cannons"; 
            
            RotationAbility NodeRetrieval = new RotationAbility(); // Node Retrieval
            NodeRetrieval.announceWarning = true; 
            NodeRetrieval.match = new Regex(@"\ uses\ Node\ Retrieval\.");
            NodeRetrieval.warningMessage = @"Node Retrieval"; 
            
            RotationAbility Object199 = new RotationAbility(); // Object 199
            Object199.announceWarning = true; 
            Object199.match = new Regex(@"\ readies\ Object\ 199\.");
            Object199.warningMessage = @"Object 199"; 
            
            
            RotationAbility Autocannons = new RotationAbility(); // Auto-cannons
            Autocannons.announceWarning = true; 
            Autocannons.match = new Regex(@"\ readies\ Auto-cannons\.");
            Autocannons.warningMessage = @"Auto-cannons"; 
            
            RotationAbility GravityField = new RotationAbility(); // Gravity Field
            GravityField.announceWarning = true; 
            GravityField.match = new Regex(@"\ uses\ Gravity\ Field\.");
            GravityField.warningMessage = @"Gravity Field"; 
            
            // You'll need to split these up into phases
            // And separate out any timed moves which aren't part of a rotation
            // For now we'll assume they're all part of phase 1
            
            phaseNum = 1;
            phases[phaseNum] = new Phase();
            
            // You can use one of the following two methods for determining the end of a phase.
            // Just choose which is appropriate to the encounter and uncomment/modify to fit
            //phases[phaseNum].phaseEndHP = 90;
            //phases[phaseNum].phaseEndRegex = new Regex("Titan uses Geocrush");


            /** /
            
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Resonance);      // 19:11:43:34 @ 99.3224423872442%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2413569), NerveGas);      // 19:11:49:276 @ 97.7958291767612%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1492374), VulnerabilityUp);      // 19:11:53:425 @ 96.8771661436414%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.9853423), NerveGas);      // 19:11:59:410 @ 95.4825937785689%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0254018), Resonance);      // 19:12:6:436 @ 93.8614579364594%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.6976691), NerveGas);      // 19:12:18:133 @ 91.0755124338463%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.1255792), NerveGas);      // 19:12:28:259 @ 89.0567998611729%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.2082978), Barofield);      // 19:12:33:467 @ 88.0243218849461%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(12.7407288), SeedoftheSea);      // 19:12:46:208 @ 85.4544696601742%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1662383), SeedoftheRivers);      // 19:12:50:374 @ 84.6769604236165%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1402368), Resonance);      // 19:12:54:514 @ 83.7340608182848%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3841935), SecondaryHead);      // 19:12:57:899 @ 83.2458393076484%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0010001), Stun);      // 19:12:57:900 @ 83.2458393076484%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9452829), IntheHeadlights);      // 19:13:2:845 @ 82.3439479824992%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.2930739), MainHead);      // 19:13:4:138 @ 82.1480195327383%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.9032233), Resonance);      // 19:13:8:41 @ 81.4597978282092%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3361336), NerveGas);      // 19:13:10:377 @ 80.9628511515765%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.4125956), NerveGas);      // 19:13:20:790 @ 78.5700228502403%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0094009), Resonance);      // 19:13:27:799 @ 77.1329880106524%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.7623868), SeedoftheRivers);      // 19:13:34:562 @ 75.8408878728918%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1892968), SeedoftheSea);      // 19:13:39:751 @ 74.7013811937772%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1672383), Resonance);      // 19:13:43:918 @ 74.0397227723924%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3711928), SecondaryHead);      // 19:13:47:290 @ 73.2530036383942%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0010001), Stun);      // 19:13:47:291 @ 73.2530036383942%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9422827), IntheHeadlights);      // 19:13:52:233 @ 72.3450046970477%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.2960741), MainHead);      // 19:13:53:529 @ 72.0733611956968%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1672384), Resonance);      // 19:13:57:696 @ 71.4112180428677%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3381337), NerveGas);      // 19:14:0:34 @ 71.0410771120476%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.1365798), NerveGas);      // 19:14:10:171 @ 69.1514000498304%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.2764162), Resonance);      // 19:14:17:447 @ 67.9084516805154%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.767387), SeedoftheRivers);      // 19:14:24:215 @ 66.5764096717496%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1982974), SeedoftheSea);      // 19:14:29:413 @ 65.7012755223224%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1472372), Resonance);      // 19:14:33:560 @ 64.8530924411948%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.1231786), SecondaryHead);      // 19:14:36:683 @ 64.3786373035747%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0010001), Stun);      // 19:14:36:684 @ 64.3786373035747%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.948283), IntheHeadlights);      // 19:14:41:633 @ 63.5336534499791%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.2890737), MainHead);      // 19:14:42:922 @ 63.4029698526126%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1732387), Resonance);      // 19:14:47:95 @ 62.5724309960552%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.3291332), NerveGas);      // 19:14:49:424 @ 62.0477576808121%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.14058), NerveGas);      // 19:14:59:565 @ 60.3002038780454%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.2734161), Barofield);      // 19:15:6:838 @ 59.2805228119465%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.001), VulnerabilityUp);      // 19:15:6:839 @ 59.2805228119465%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Heavy);      // 19:15:6:839 @ 59.2805228119465%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.0584609), VacuumWave);      // 19:15:14:898 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9502832), ForkedLightning);      // 19:15:19:848 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(15.0688619), VacuumWave);      // 19:15:34:917 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9412826), ForkedLightning);      // 19:15:39:858 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1982973), VacuumWave);      // 19:15:45:56 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.6031489), Heavy);      // 19:15:47:659 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.535431), VacuumWave);      // 19:15:55:195 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.1241787), ForkedLightning);      // 19:15:58:319 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.8178475), ForkedLightning);      // 19:16:13:137 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.8431054), VacuumWave);      // 19:16:14:980 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.1085782), VacuumWave);      // 19:16:25:89 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.8435058), ForkedLightning);      // 19:16:33:932 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.2910739), VacuumWave);      // 19:16:35:223 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(13.5367742), ForkedLightning);      // 19:16:48:760 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.2313565), VacuumWave);      // 19:16:54:991 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.3701927), Heavy);      // 19:16:58:361 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0164013), VacuumWave);      // 19:17:5:378 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.0821191), ForkedLightning);      // 19:17:7:460 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(17.6820114), VacuumWave);      // 19:17:25:142 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.6352079), ForkedLightning);      // 19:17:28:777 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.4893712), VacuumWave);      // 19:17:35:267 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.4312534), EmergencyMode);      // 19:17:39:698 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0214016), NerveCloud);      // 19:17:46:719 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.5170296), ForkedLightning);      // 19:17:47:236 @ 59.2537656362246%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.6516092), NanosporeJet);      // 19:17:57:888 @ 58.7992814341071%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1962973), AetherochemicalNanospores);      // 19:18:3:84 @ 58.331127805262%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.001), AetherochemicalNanospores);      // 19:18:3:85 @ 58.331127805262%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.7950455), NanosporeCloud);      // 19:18:3:880 @ 58.2351509793029%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0030002), VulnerabilityUp);      // 19:18:3:883 @ 58.2351509793029%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.749386), Resonance);      // 19:18:10:633 @ 57.9067938989762%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.0955202), SeedoftheRivers);      // 19:18:19:728 @ 57.089536683991%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.4583122), SeedoftheSea);      // 19:18:25:186 @ 56.8726678358389%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.7800446), NerveGas);      // 19:18:25:966 @ 56.8726678358389%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.0234018), Barofield);      // 19:18:32:990 @ 56.4523087273958%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.001), VulnerabilityUp);      // 19:18:32:991 @ 56.4523087273958%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Heavy);      // 19:18:32:991 @ 56.4523087273958%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.1161783), NerveGas);      // 19:18:36:107 @ 56.2877908752214%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.8004461), SecondaryHead);      // 19:18:43:907 @ 55.8605485802701%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0020001), Stun);      // 19:18:43:909 @ 55.8605485802701%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.930282), IntheHeadlights);      // 19:18:48:840 @ 55.6809071070355%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.3120751), MainHead);      // 19:18:50:152 @ 55.6126569196868%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.1181783), Resonance);      // 19:18:53:270 @ 55.4421283976039%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.4152526), NanosporeJet);      // 19:18:57:685 @ 55.2643289038574%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1972972), AetherochemicalNanospores);      // 19:19:2:883 @ 54.957978631099%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0020001), AetherochemicalNanospores);      // 19:19:2:885 @ 54.957978631099%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.5274306), NerveGas);      // 19:19:10:412 @ 54.3914245190737%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.1385799), NerveGas);      // 19:19:20:551 @ 53.5921023675253%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.5670896), NanosporeCloud);      // 19:19:22:118 @ 53.4352432721699%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.2680153), Barofield);      // 19:19:22:386 @ 53.4352432721699%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.0010001), VulnerabilityUp);      // 19:19:22:387 @ 53.4352432721699%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Heavy);      // 19:19:22:387 @ 53.4352432721699%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.4212529), Resonance);      // 19:19:26:808 @ 52.8692708378777%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.2714159), Barofield);      // 19:19:34:79 @ 52.4182767021587%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0.001), VulnerabilityUp);      // 19:19:34:80 @ 52.4182767021587%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(0), Heavy);      // 19:19:34:80 @ 52.4182767021587%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.1452371), SeedoftheSea);      // 19:19:38:226 @ 52.2454214691434%
            
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
