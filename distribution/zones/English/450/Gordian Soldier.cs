using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Memory.Core;
using System.Text.RegularExpressions;

namespace AlexFloor2Savage
{
    
    public class MainEncounterLogic : AbilityController, IEncounter
    {
        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Gordian Soldier";
            
            int phaseNum = 1; // for convenience


            RotationAbility NextPhase = new RotationAbility(); // Nerve Gas
            NextPhase.announceWarning = false;
            NextPhase.warningTime = TimeSpan.FromSeconds(0);
            NextPhase.warningCallback = delegate(Ability self)
            {
                nextPhase();
            };




            // You'll need to split these up into phases
            // And separate out any timed moves which aren't part of a rotation
            // For now we'll assume they're all part of phase 1
            
            phaseNum = 1;
            phases[phaseNum] = new Phase();
            

            
        }


        public void onEndEncounter()
        {

        }

        public void onMobAdded(ActorEntity mob)
        {
            if (mob.Name.Contains("Bomb"))
            {
                tts("Balm");
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
