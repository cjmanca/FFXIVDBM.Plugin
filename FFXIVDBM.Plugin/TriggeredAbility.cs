using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVDBM.Plugin
{
    public delegate void TimerCallback(TriggeredAbility self);
    public class TriggeredAbility : Ability
    {
        public bool timerStarted = false;
        public bool timerAutoRestart = false;
        public TimeSpan timerDuration = TimeSpan.Zero;
        public DateTime nextAbilityTime = DateTime.Now;
        public DateTime timerStartedAt = DateTime.Now;

        public TimeSpan collectMultipleLinesFor = TimeSpan.FromSeconds(0.25);
        public bool warningDelayStarted = false;

        public bool healthWarningAlreadyTriggered = false;
        public double healthTriggerAt = -1;
        public string healthMessage = "";
        public WarningCallback healthCallback = null;


        public void start(bool subtractWarning = true)
        {
            nextAbilityTime = DateTime.Now + timerDuration;
            if (subtractWarning)
            {
                nextAbilityTime = nextAbilityTime - warningTime;
            }
            timerStartedAt = DateTime.Now;
            timerStarted = true;
        }

        public static new TriggeredAbility Blank()
        {
            return new TriggeredAbility();
        }
    }
}
