using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace OppressorNS8
{
    
    public class MainEncounterLogic : AbilityController, IEncounter
    {
        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Oppressor";
            
            int phaseNum = 1; // for convenience


            RotationAbility NextPhase = new RotationAbility(); // Nerve Gas
            NextPhase.announceWarning = false;
            NextPhase.warningTime = TimeSpan.FromSeconds(0);
            NextPhase.warningCallback = delegate(Ability self)
            {
                nextPhase();
            };



            TriggeredAbility Prey = new TriggeredAbility();
            timedAbilities.Add(Prey);
            Prey.match = new Regex(@"(?<member>[^ ]+?) [^ ]+ suffers the effect of Prey\.");
            Prey.matchMessage = "${member.1} ${member.2} ${member.3} prey";



            RotationAbility ResinBomb = new RotationAbility(); // Resin Bomb
            ResinBomb.announceWarning = true;
            ResinBomb.warningTime = TimeSpan.FromSeconds(2);
            ResinBomb.match = new Regex(@"\ readies\ Resin\ Bomb\.");
            ResinBomb.warningMessage = @"Raisins";

            RotationAbility EmergencyDeployment = new RotationAbility(); // Emergency Deployment
            EmergencyDeployment.announceWarning = true;
            EmergencyDeployment.warningTime = TimeSpan.FromSeconds(3);
            EmergencyDeployment.match = new Regex(@"\ readies\ Emergency\ Deployment\.");
            EmergencyDeployment.warningMessage = @"Adds";

            RotationAbility DistressBeacon = new RotationAbility(); // Distress Beacon
            DistressBeacon.announceWarning = false;
            DistressBeacon.warningTime = TimeSpan.FromSeconds(0);
            DistressBeacon.match = new Regex(@"\ uses\ Distress\ Beacon\.");
            DistressBeacon.warningMessage = @"Distress Beacon";

            RotationAbility HypercompressedPlasma = new RotationAbility(); // Hypercompressed Plasma
            HypercompressedPlasma.announceWarning = true;
            HypercompressedPlasma.warningTime = TimeSpan.FromSeconds(4);
            HypercompressedPlasma.match = new Regex(@"\ readies\ Hypercompressed\ Plasma\.");
            HypercompressedPlasma.warningMessage = @"Plasma";

            RotationAbility EmergencyLiftoff = new RotationAbility(); // Emergency Liftoff
            EmergencyLiftoff.announceWarning = true;
            EmergencyLiftoff.warningTime = TimeSpan.FromSeconds(8);
            EmergencyLiftoff.match = new Regex(@"\ uses\ Emergency\ Liftoff\.");
            EmergencyLiftoff.warningMessage = @"Lift off";





            // Frontal laser cleave
            RotationAbility RoyalFount = new RotationAbility(); // Royal Fount
            RoyalFount.announceWarning = false; 
            RoyalFount.match = new Regex(@"\ uses\ Royal\ Fount\.");
            
            RotationAbility GunneryPod = new RotationAbility(); // Gunnery Pod
            GunneryPod.announceWarning = false; 
            GunneryPod.match = new Regex(@"\ uses\ Gunnery\ Pod\.");
            
            RotationAbility HydrothermalMissile = new RotationAbility(); // Hydrothermal Missile
            HydrothermalMissile.announceWarning = false; 
            HydrothermalMissile.match = new Regex(@"\ readies\ Hydrothermal\ Missile\.");
            HydrothermalMissile.warningMessage = @"Prey missile"; 
            
            RotationAbility PhotonSpaser = new RotationAbility(); // Photon Spaser
            PhotonSpaser.announceWarning = false; 
            PhotonSpaser.match = new Regex(@"\ readies\ Photon\ Spaser\.");
            

            RotationAbility a3000tonzeMissile = new RotationAbility(); // 3000-tonze Missile
            a3000tonzeMissile.announceWarning = false; 
            a3000tonzeMissile.match = new Regex(@"\ readies\ 3000-tonze\ Missile\.");
            a3000tonzeMissile.warningMessage = @"3000-tonze Missile"; 
            
            RotationAbility QuickLanding = new RotationAbility(); // Quick Landing
            QuickLanding.announceWarning = false; 
            QuickLanding.match = new Regex(@"\ uses\ Quick\ Landing\.");
            QuickLanding.warningMessage = @"Quick Landing"; 
            
            RotationAbility ParticleCollision = new RotationAbility(); // Particle Collision
            ParticleCollision.announceWarning = false;
            ParticleCollision.match = new Regex(@"\ uses\ Particle\ Collision\.");
            ParticleCollision.warningMessage = @"Particle Collision"; 
            



            // You'll need to split these up into phases
            // And separate out any timed moves which aren't part of a rotation
            // For now we'll assume they're all part of phase 1
            
            phaseNum = 1;
            phases[phaseNum] = new Phase();
            

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(4.9022804), RoyalFount);      // 18:52:44:542 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.9022232), GunneryPod);      // 18:52:48:444 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.307075), HydrothermalMissile);      // 18:52:49:751 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(8.3034749), PhotonSpaser);      // 18:52:58:55 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.6251501), HydrothermalMissile);      // 18:53:0:680 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.4493117), RoyalFount);      // 18:53:6:129 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.4531975), ResinBomb);      // 18:53:9:583 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.1894112), RoyalFount);      // 18:53:16:772 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.8361051), PhotonSpaser);      // 18:53:18:608 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.700326), GunneryPod);      // 18:53:24:308 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.1261788), RoyalFount);      // 18:53:27:435 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.3070748), EmergencyDeployment);      // 18:53:28:742 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.271416), HydrothermalMissile);      // 18:53:36:13 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.733328), GunneryPod);      // 18:53:41:746 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.363192), RoyalFount);      // 18:53:45:110 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.767101), HydrothermalMissile);      // 18:53:46:877 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(3.443197), PhotonSpaser);      // 18:53:50:320 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.7203272), GunneryPod);      // 18:53:56:40 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.6203786), RoyalFount);      // 18:54:2:661 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.2121266), DistressBeacon);      // 18:54:4:873 @ 99.9460681483161%

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(5.1922973), NextPhase);      // 18:54:4:873 @ 99.9460681483161%

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(120), RotationAbility.Blank());      // 18:54:4:873 @ 99.9460681483161%



            phaseNum = 2;
            phases[phaseNum] = new Phase();

            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.703669), a3000tonzeMissile);      // 18:54:21:769 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.1375798), GunneryPod);      // 18:54:31:907 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.696097), EmergencyDeployment);      // 18:54:33:603 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(6.8933942), ResinBomb);      // 18:54:40:499 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.259415), RoyalFount);      // 18:54:47:759 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.561089), PhotonSpaser);      // 18:54:49:320 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.027402), GunneryPod);      // 18:54:56:348 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(2.330132), HydrothermalMissile);      // 18:54:58:678 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.636551), RoyalFount);      // 18:55:8:314 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.035059), HydrothermalMissile);      // 18:55:9:349 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(1.808103), HypercompressedPlasma);      // 18:55:11:158 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(11.706669), PhotonSpaser);      // 18:55:22:864 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(10.908624), GunneryPod);      // 18:55:33:773 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(9.099521), RoyalFount);      // 18:55:42:872 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.016401), EmergencyLiftoff);      // 18:55:49:889 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(14.559832), ResinBomb);      // 18:56:4:449 @ 99.9460681483161%
            phases[phaseNum].AddRotation(TimeSpan.FromSeconds(7.281416), QuickLanding);      // 18:56:11:730 @ 99.9460681483161%



            
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
