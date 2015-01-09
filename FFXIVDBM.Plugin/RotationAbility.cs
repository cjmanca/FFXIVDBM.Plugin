using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVDBM.Plugin
{
    public class RotationAbility : Ability
    {

        public bool uniqueInPhase = false;

        public static new RotationAbility Blank()
        {
            return new RotationAbility();
        }
    }
}
