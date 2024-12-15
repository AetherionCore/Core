using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class SummonerDot : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.DAMAGE,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Particle ignite;
        Buff grevious;
        ObjAIBase Owner;
        AttackableUnit Target;

        float timeSinceLastTick = 1000.0f;
        float damage;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Owner = ownerSpell.CastInfo.Owner;
            Target = unit;
            damage = 10 + Owner.Stats.Level * 4;
            ignite = AddParticleTarget(Owner, unit, "Global_SS_Ignite", unit, buff.Duration, bone: "C_BUFFBONE_GLB_CHEST_LOC");
            grevious = AddBuff("GreviousWounds", buff.Duration, 1, ownerSpell, buff.TargetUnit, buff.SourceUnit);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Owner = null;
            Target = null;
            ignite.SetToRemove();
            RemoveBuff(grevious);
        }

        public void OnUpdate(float diff)
        {
            if (Target == null || Owner == null)
            {
                return;
            }

            if (Target.IsDead)
            {
                RemoveBuff(Target, "SummonerDot");
                return;
            }

            timeSinceLastTick += diff;

            if (timeSinceLastTick >= 1000.0f)
            {
                Target.TakeDamage(Owner, damage, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_SPELL, false);
                timeSinceLastTick = 0;
            }
        }
    }
}
