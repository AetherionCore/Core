using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerLib.GameObjects.AttackableUnits;
using GameServerCore;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Spells
{
    public class InfectedCleaverMissileCast : ISpellScript
    {
        private ObjAIBase Owner;

        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
            AutoFaceDirection = true
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            Owner = owner;
        }

        public void OnDeactivate(ObjAIBase owner, Spell spell)
        {
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            FaceDirection(end, owner, false, turnTime: 0.08333f*2);
        }

        public void OnSpellCast(Spell spell)
        {
        }

        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner as Champion;
            var targetPos = GetPointFromUnit(owner, 975f);
            float SelfDamage = 40 + 10f * spell.CastInfo.SpellLevel;

            SpellCast(owner, 0, SpellSlotType.ExtraSlots, targetPos, targetPos, false, Vector2.Zero);
            owner.StopMovement();
            owner.SetTargetUnit(null);
            if (owner.Stats.CurrentHealth > SelfDamage)
            {
                owner.Stats.CurrentHealth -= SelfDamage;
            }
            else
            {
                owner.Stats.CurrentHealth = 1f;
            }
        }

        public void OnSpellChannel(Spell spell)
        {
        }

        public void OnSpellChannelCancel(Spell spell, ChannelingStopSource source)
        {
        }

        public void OnSpellPostChannel(Spell spell)
        {
        }

        public void OnUpdate(float diff)
        {
            if (Owner != null)
            {
                Owner.Stats.HealthRegeneration.FlatBonus = (Owner.Stats.HealthPoints.Total / 100f) * 0.3f;
                //Double Check if that's right
            }
        }
    }

    public class InfectedCleaverMissile : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            },
            IsDamagingSpell = true
            // TODO
        };

        //Vector2 direction;

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        private void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector arg4)
        {
            var owner = spell.CastInfo.Owner;
            var spellLevel = owner.GetSpell("InfectedCleaverMissileCast").CastInfo.SpellLevel;
            var damage = target.Stats.CurrentHealth * (0.12f + 0.03f * spellLevel);
            float minimunDamage = 30f + (50f * spellLevel);
            float maxDamageMonsters = 200 + 100f * spellLevel;
            float Heal = 20 + 5f * spellLevel;
            //TODO: Implement max damage when monsters gets added.

            if (damage < minimunDamage)
            {
                damage = minimunDamage;
            }

            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            owner.Stats.CurrentHealth += Heal;
            AddParticleTarget(owner, null, "dr_mundo_as_mundo_infected_cleaver_tar.troy", target);
            AddParticleTarget(owner, null, "dr_mundo_infected_cleaver_tar.troy", target);
            AddBuff("InfectedCleaverMissile", 2f, 1, spell, target, owner);

            missile.SetToRemove();
        }

        public void OnDeactivate(ObjAIBase owner, Spell spell)
        {
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(Spell spell)
        {
        }

        public void OnSpellPostCast(Spell spell)
        {
        }

        public void OnSpellChannel(Spell spell)
        {
        }

        public void OnSpellChannelCancel(Spell spell, ChannelingStopSource source)
        {
        }

        public void OnSpellPostChannel(Spell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}