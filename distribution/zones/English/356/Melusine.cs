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
        int renaudCount;
        bool pausedDamage;
        DateTime lastRenaud;
        DateTime lastVoice;

        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "Melusine";


            renaudCount = 0;
            pausedDamage = false;
            lastRenaud = DateTime.MinValue;
            lastVoice = DateTime.MinValue;


            TriggeredAbility PetrifactionTrigger = new TriggeredAbility();
            timedAbilities.Add(PetrifactionTrigger);

            PetrifactionTrigger.match = new Regex(@" readies Petrifaction\.");
            PetrifactionTrigger.matchMessage = "petrefaction look away";





            TriggeredAbility CursedShriekTrigger = new TriggeredAbility();
            timedAbilities.Add(CursedShriekTrigger);

            CursedShriekTrigger.match = new Regex("(?<member>[^ ]+?) [^ ]+ suffers the effect of Cursed Shriek");
            CursedShriekTrigger.matchMessage = "Shriek on ${member}";
            CursedShriekTrigger.warningMessage = "3. 2. 1";
            CursedShriekTrigger.timerDuration = TimeSpan.FromSeconds(10);
            CursedShriekTrigger.warningTime = TimeSpan.FromSeconds(3);



            // Every 24 seconds, although phase transitions will alter
            TriggeredAbility CursedVoiceTrigger = new TriggeredAbility();
            timedAbilities.Add(CursedVoiceTrigger);

            CursedVoiceTrigger.match = new Regex("(?<member>[^ ]+?) [^ ]+ suffers the effect of Cursed Voice");
            CursedVoiceTrigger.matchMessage = "${member.1}, ${member.2}, and ${member.3}";



            TriggeredAbility CursedVoiceUsed = new TriggeredAbility();
            timedAbilities.Add(CursedVoiceUsed);
            CursedVoiceUsed.announceWarning = false;
            CursedVoiceUsed.match = new Regex(@" readies Cursed Voice\.");
            CursedVoiceUsed.matchCallback = delegate(Ability self, Match m)
            {
                lastVoice = DateTime.Now;

                if (pausedDamage)
                {
                    TimeSpan timeSinceRenaud = DateTime.Now - lastRenaud;

                    if (renaudCount >= 4 || timeSinceRenaud < TimeSpan.FromSeconds(20))
                    {
                        pausedDamage = false;
                        tts("Continue damage");
                    }
                }
            };




            TriggeredAbility firstDamagePause = new TriggeredAbility();
            timedAbilities.Add(firstDamagePause);
            firstDamagePause.healthTriggerAt = 84;
            firstDamagePause.healthCallback = delegate(Ability self)
            {
                checkStopDamage();
            };

            TriggeredAbility secondDamagePause = new TriggeredAbility();
            timedAbilities.Add(secondDamagePause);
            secondDamagePause.healthTriggerAt = 64;
            secondDamagePause.healthCallback = delegate(Ability self)
            {
                checkStopDamage();
            };

            TriggeredAbility thirdDamagePause = new TriggeredAbility();
            timedAbilities.Add(thirdDamagePause);
            thirdDamagePause.healthTriggerAt = 39;
            thirdDamagePause.healthCallback = delegate(Ability self)
            {
                checkStopDamage();
            };


            phases[1] = new Phase();


        }
        DateTime lastAddedNewMob = DateTime.Now;

        public void onMobAdded(ActorEntity mob)
        {

            if (mob.Name.Contains("Renaud"))
            {
                renaudCount++;
                lastRenaud = DateTime.Now;

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


        public void checkStopDamage()
        {
            // Renaud's are now petrified for 60 seconds, so no need to pause damage on phase changes anymore
            return;

            TimeSpan timeSinceVoice = DateTime.Now - lastVoice;
            TimeSpan timeSinceRenaud = DateTime.Now - lastRenaud;

            if (timeSinceVoice < timeSinceRenaud && timeSinceRenaud < TimeSpan.FromSeconds(20) && timeSinceVoice < TimeSpan.FromSeconds(5))
            {
                // Timing is close enough, don't need to pause DPS
            }
            else
            {
                if (renaudCount < 4)
                {
                    pausedDamage = true;
                    tts("Stop damage");
                }
                else
                {
                    // if we already have 4 renauds stacked, just test for voice
                    if (timeSinceVoice > TimeSpan.FromSeconds(5))
                    {
                        pausedDamage = true;
                        tts("Stop damage");
                    }
                }
            }
        }

        public void onMobRemoved(ActorEntity mob)
        {
            if (mob.Name.Contains("Renaud"))
            {
                renaudCount--;
            }
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