using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.API;
using System;
using GameServerCore;

namespace ItemPassives
{
    internal class ItemID_1502 : IItemScript
    {
        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        float minionsCheckTimer = 0f;
        bool hasBuff = false;
        ObjAIBase owner;

        public void OnActivate(ObjAIBase owner)
        {
            this.owner = owner;
            StatsModifier.Armor.FlatBonus += 200f;
            StatsModifier.MagicResist.FlatBonus += 200f;
            owner.AddStatModifier(StatsModifier);
            hasBuff = true;
            minionsCheckTimer = 2 * 1000f;// every 2 seconds
        }

        private bool HasEnemyMinionsInRange()
        {
            var enemyTeam = CustomConvert.GetEnemyTeam(owner.Team);
            var nearbyUnits = ApiFunctionManager.GetUnitsInRangeUnOrdered(owner.Position, 1000, true);
            foreach (var unit in nearbyUnits)
            {
                if (unit is Minion && unit is not Pet && unit.Team == enemyTeam)
                    return true;
            }
            return false;
        }

        public void OnDeactivate(ObjAIBase owner)
        {
            owner.RemoveStatModifier(StatsModifier);
        }

        public void OnUpdate(float diff)
        {
            if (minionsCheckTimer > 0)
            {
                minionsCheckTimer -= diff;

                if (minionsCheckTimer <= 0)
                {
                    var minionsInRange = HasEnemyMinionsInRange();
                    if (!minionsInRange && !hasBuff)
                    {
                        owner.AddStatModifier(StatsModifier);
                        hasBuff = true;
                    }
                    if (minionsInRange && hasBuff)
                    {
                        owner.RemoveStatModifier(StatsModifier);
                        hasBuff = false;
                    }
                    minionsCheckTimer = 2f;
                }
            }
        }
    }
}
