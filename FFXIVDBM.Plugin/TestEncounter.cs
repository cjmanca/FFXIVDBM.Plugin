using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using FFXIVDBM.Plugin;
using FFXIVAPP.Common.Core.Memory;
using System.Text.RegularExpressions;

namespace EncounterNS
{

    public class TestEncounter : AbilityController, IEncounter
    {

        public void onStartEncounter()
        {
            // bossName is needed if you want health based phase swaps
            bossName = "";



            
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



        public void onTick()
        {

        }

        public void onNewChatLine(string line)
        {

        }
    }
}