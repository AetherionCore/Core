using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiEventManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.API;
using LeaguePackets.Game.Events;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using System;

namespace Buffs
{
    internal class NasusR : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            MaxStacks = 1
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        ObjAIBase owner;
        Particle p;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            owner = ownerSpell.CastInfo.Owner as Champion;

            var HealthBuff = 150f + 150f * ownerSpell.CastInfo.SpellLevel;

            p = AddParticleTarget(owner, unit, "Nasus_Base_R_Aura.troy", unit, buff.Duration);
            p = AddParticleTarget(owner, unit, "Nasus_Base_R_Avatar.troy", unit, buff.Duration);
            StatsModifier.Size.BaseBonus = StatsModifier.Size.BaseBonus + 0.4f;
            StatsModifier.HealthPoints.BaseBonus += HealthBuff;
            StatsModifier.Range.FlatBonus += 50;

            unit.AddStatModifier(StatsModifier);
            unit.TakeHeal(unit, HealthBuff);

            OnSpellHit.AddListener(this, ownerSpell, RSpellHit);

            var sector = ownerSpell.CreateSpellSector(new SectorParameters()
            {
                BindObject = ownerSpell.CastInfo.Owner,
                CanHitSameTarget = true,
                CanHitSameTargetConsecutively = true,
                Length = 300,
                Lifetime = 15f,
                MaximumHits = 50,
                Type = SectorType.Area,
                OverrideFlags =  SpellDataFlags.AffectAllUnitTypes | SpellDataFlags.AffectAllSides,
                Tickrate = 1f
            });
        }

        private void RSpellHit(Spell spell, AttackableUnit unit, SpellMissile missile, SpellSector sector)
        {
            if (unit.NetId == spell.CastInfo.Owner.NetId) // can't hit nasus himself
                return;
            if (unit.IsDead || unit.Team == spell.CastInfo.Owner.Team) return;

            var ap = spell.CastInfo.Owner.Stats.AbilityPower.Total;
            var increasedPercentage = ap > 0 ? ap / 100 : 0;
            var damage = Math.Min(240, unit.Stats.HealthPoints.Total * (0.05f + (increasedPercentage / 100f)));
            unit.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, DamageResultType.RESULT_NORMAL);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(p);
            //StatsModifier.Size.BaseBonus = StatsModifier.Size.BaseBonus - 0.1f;
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
