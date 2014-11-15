using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FFXIVDBM.Plugin
{
    public class Phase
    {
        public Dictionary<TimeSpan, RotationAbility> rotationAbilities = new Dictionary<TimeSpan, RotationAbility>();
        public Dictionary<TimeSpan, RotationAbility> rotationAbilitiesWarningTime = new Dictionary<TimeSpan, RotationAbility>();
        public List<TriggeredAbility> timedAbilities = new List<TriggeredAbility>();
        public DateTime phaseStarted = DateTime.Now;
        public TimeSpan phaseStartDelay = TimeSpan.Zero;
        public TimeSpan phaseLength = TimeSpan.Zero;

        public double phaseEndHP = -1;
        public Regex phaseEndRegex = null;
        public TriggeredAbility onPhaseStart = null;



        public TimeSpan AddRotation(TimeSpan abilityTime, RotationAbility ability)
        {
            TimeSpan lastTS = TimeSpan.Zero;

            if (rotationAbilities.Any())
            {
                lastTS = rotationAbilities.Last().Key;
            }

            TimeSpan tmpTime = TimeSpan.FromSeconds(abilityTime.TotalSeconds + lastTS.TotalSeconds);
            TimeSpan tmpWarningTime = tmpTime - ability.warningTime;

            while (rotationAbilities.ContainsKey(tmpTime))
            {
                tmpTime += TimeSpan.FromSeconds(0.01);
            }

            while (rotationAbilitiesWarningTime.ContainsKey(tmpWarningTime))
            {
                tmpWarningTime += TimeSpan.FromSeconds(0.01);
            }

            rotationAbilities[tmpTime] = ability;
            rotationAbilitiesWarningTime[tmpWarningTime] = ability;

            rotationAbilities = rotationAbilities.OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Value);
            rotationAbilitiesWarningTime = rotationAbilitiesWarningTime.OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Value);


            return tmpTime;
        }
    }
}
