using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using GameServerLib.GameObjects.AttackableUnits;
using LeaguePackets.Game.Events;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    class VeigarW : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.STACKS_AND_OVERLAPS,
            MaxStacks = byte.MaxValue
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();
        SpellSector DamageSector;
        string particles2;
        float explodeTimer = 1.2f * 1000f;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, ownerSpell, TargetExecute, false);

            DamageSector = ownerSpell.CreateSpellSector(new SectorParameters
            {
                Length = 225f,
                Lifetime = -1.0f,
                Tickrate = 0.85f,
                MaximumHits = 0,
                SingleTick = true,
                OverrideFlags = SpellDataFlags.AffectMinions | SpellDataFlags.AffectEnemies | SpellDataFlags.AffectHeroes | SpellDataFlags.AffectBarracksOnly | SpellDataFlags.AffectNeutral,
                Type = SectorType.Area
            });

            switch ((unit as ObjAIBase).SkinID)
            {
                case 8:
                    particles2 = "Veigar_Skin08_W_aoe_explosion.troy";
                    break;

                case 4:
                    particles2 = "Veigar_Skin04_W_aoe_explosion.troy";
                    break;

                default:
                    particles2 = "Veigar_Base_W_aoe_explosion.troy";
                    break;
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {
            explodeTimer -= diff;
            if (explodeTimer <= 0)
            {
                AddParticle(DamageSector.CastInfo.Owner, null, particles2, DamageSector.Position);
            }
        }

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner;
            LogInfo($"Target {target.CharData.Name}");
            if (!(target is BaseTurret or LaneTurret or Nexus or Inhibitor || target.Team == owner.Team || target == owner) && !target.IsDead)
            {
                var ownerSkinID = owner.SkinID;
                var APratio = owner.Stats.AbilityPower.Total;
                var damage = 120f + ((spell.CastInfo.SpellLevel - 1) * 50) + APratio;
                var StacksPerLevel = owner.Spells[0].CastInfo.SpellLevel;

                target.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

                if (target.IsDead)
                {
                    // we're using this because OnKill doesn't info on the spell used and can't be relied on.
                    ProcessDeath(target, owner);
                }
            }
        }

        private static void ProcessDeath(AttackableUnit target, ObjAIBase owner)
        {
            var stacksPerLevel = owner.Spells[0].CastInfo.SpellLevel;
            var buffer = owner.Stats.AbilityPower.FlatBonus;
            var statsmodifier = new StatsModifier();
            var stacks = 0f;
            var count = 0;

            if (target is Champion)
            {
                count = stacksPerLevel;
                stacks = count - buffer;

                // give veigar his ability popwers
                statsmodifier.AbilityPower.FlatBonus = owner.Stats.AbilityPower.FlatBonus + stacks;
                owner.AddStatModifier(statsmodifier);

                // give veigar his Q ability ocunt
                AddBuff("VeigarQPassive", 25000, (byte)count, null, owner, owner, true);
            }
        }
    }
}