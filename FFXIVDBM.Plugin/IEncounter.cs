using FFXIVAPP.Common.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVDBM.Plugin
{
    public interface IEncounter
    {
        // Must implement these methods in encounters
        void onStartEncounter();
        void onEndEncounter();
        void onMobAdded(ActorEntity mob);
        void onMobRemoved(ActorEntity mob);
        void onTick();
        void onNewChatLine(string line);



        bool bossCheck(ActorEntity mob);

        void tick();

        void endEncounter();

        void processChatLine(string line);

        void setPhase(int phaseNum);

        bool inController();

    }
}
