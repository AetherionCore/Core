using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.API;
using System;
using GameServerCore;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace ItemPassives
{
    internal class ItemID_1500 : IItemScript
    {
        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        int buffTimeLeft = 0;
        ObjAIBase owner;

        public void OnActivate(ObjAIBase owner)
        {
            this.owner = owner;
            buffTimeLeft = 420;
            LogInfo($"Initialized peentrating Bulleettss");
            AddBuff("PenetratingBullets", 25000, 1, null, owner, owner, true);
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
