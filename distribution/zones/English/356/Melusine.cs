using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace MelusineNS
{

    public class MainEncounterLogic : AbilityController, IEncounter
    {
        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Melusine";



            TriggeredAbility CursedShriek = new TriggeredAbility();

            CursedShriek.match = new Regex("(?<member>[^ ]+?) [^ ]+ suffers the effect of Cursed Shriek");
            CursedShriek.matchMessage = "Shriek on ${member}";
            CursedShriek.warningMessage = "3. 2. 1";
            CursedShriek.timerDuration = TimeSpan.FromSeconds(10);
            CursedShriek.warningTime = TimeSpan.FromSeconds(3);

            timedAbilities.Add(CursedShriek);


            TriggeredAbility CursedVoice = new TriggeredAbility();

            CursedVoice.match = new Regex("(?<member>[^ ]+?) [^ ]+ suffers the effect of Cursed Voice");
            CursedVoice.matchMessage = "${member.1}, ${member.2}, and ${member.3}";

            timedAbilities.Add(CursedVoice);


            TriggeredAbility Petrifaction = new TriggeredAbility();

            Petrifaction.match = new Regex("readies Petrifaction");
            Petrifaction.matchMessage = "Petrefaction look away";

            timedAbilities.Add(Petrifaction);


            phases[1] = new Phase();




        }
        DateTime lastAddedNewMob = DateTime.Now;

        public void onMobAdded(ActorEntity mob)
        {

            if (mob.Name.Contains("Renaud"))
            {
                tts("Renaud");
            }
            if (mob.Name.Contains("Lamia Prosector"))
            {
                tts("Add");
            }
            if (mob.Name.Contains("Lamia Deathdancer"))
            {
                tts("Add");
            }

            if (DateTime.Now < (lastAddedNewMob + TimeSpan.FromSeconds(5)))
            {
                return;
            }
            
            if (mob.Name.Contains("Lamia Fatedealer"))
            {
                tts("Adds");
                lastAddedNewMob = DateTime.Now;
            }

        }


        public void onEndEncounter()
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