using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using Buffs;
using System.Numerics;

namespace Spells
{
    public class Disintegrate : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Target
            }
        };

        bool isGoneStun;
        float stunDuration;

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            var pyromarker = owner.GetBuffWithName("Pyromania_Particle");
            if (pyromarker != null && pyromarker.BuffScript is Pyromania_Particle p)
            {
                stunDuration = p.StunDuration;
                RemoveBuff(pyromarker);
            }

            isGoneStun = pyromarker != null;
        }

        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            AddBuff("Pyromania", 250000f, 1, spell, owner, owner);
            var buff = owner.GetBuffWithName("Pyromania");
            NotifyBuffStacks(buff);
        }

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner as Champion;
            var ownerSkinID = owner.SkinID;
            var ap = owner.Stats.AbilityPower.Total * 0.7f;
            var damage = 45 + (spell.CastInfo.SpellLevel * 35) + ap;

            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            if (isGoneStun)
            {
                AddBuff("Stun", stunDuration, 1, spell, target, owner);
            }
             
            if (target.IsDead)
            {
                // return mana etc
                spell.SetCooldown(spell.GetCooldown() / 2);
                var mana = spell.SpellData.ManaCost[spell.CastInfo.SpellLevel];
                owner.TakeMana(owner, mana);
            }

            if (ownerSkinID == 5)
            {
                AddParticleTarget(owner, target, "DisintegrateHit_tar_frost", target);
            }
            else
            {
                AddParticleTarget(owner, target, "DisintegrateHit_tar", target);
            }
        }
    }
}