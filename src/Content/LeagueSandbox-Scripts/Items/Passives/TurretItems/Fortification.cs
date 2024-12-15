using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.API;
using System;
using GameServerCore;

namespace ItemPassives
{
    internal class ItemID_1501 : IItemScript
    {
        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        int buffTimeLeft = 0;
        ObjAIBase owner;

        public void OnActivate(ObjAIBase owner)
        {
            this.owner = owner;
            buffTimeLeft = 420;

            ApiFunctionManager.AddBuff("Fortification", 420, 1, null, owner, owner);
        }


        public void OnDeactivate(ObjAIBase owner)
        {
        }

        public void OnUpdate(float diff)
        {
            if (buffTimeLeft == -1) return;
            if (owner.GetGame().GameTime / 1000f  > buffTimeLeft)
            {
                ApiFunctionManager.RemoveBuff(owner, "Fortification");
                buffTimeLeft = -1;
            }
        }
    }
}
