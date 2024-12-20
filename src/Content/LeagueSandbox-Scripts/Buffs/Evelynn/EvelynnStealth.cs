using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerLib.GameObjects.AttackableUnits;

namespace Buffs
{
    internal class EvelynnStealth : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL
            //BuffAddType = BuffAddType.RENEW_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; }


        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {

        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {            

        }

        public void OnDeath(DeathData deathData)
        {

        }
        public void OnUpdate(float diff)
        {

        }
    }
}
