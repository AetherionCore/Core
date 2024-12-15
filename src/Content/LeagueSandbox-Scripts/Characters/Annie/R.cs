using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using Buffs;
using GameServerLib.GameObjects.AttackableUnits;
using System;

namespace Spells
{
    public class InfernalGuardian : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
            IsPetDurationBuff = true,
            NotSingleTargetSpell = true,
            SpellDamageRatio = 0.5f,
        };

        ObjAIBase Annie;
        Spell AnnieRSpell;
        bool isGoneStun;
        float stunDuration;
        float tibbersSpawnedTime;
        float spellCd;
        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            Annie = owner;
            AnnieRSpell = spell;
            spellCd = spell.GetCooldown();
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
            var owner = spell.CastInfo.Owner as Champion;
            var tibbers = CreatePet
            (
                owner,
                spell,
                new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z),
                "Tibbers",
                "AnnieTibbers",
                "InfernalGuardian",
                45.0f,
                showMinimapIfClone: false,
                isClone: false
            );
            var guideSpell = SetSpell(owner, "InfernalGuardianGuide", SpellSlotType.SpellSlots, 3);
            tibbersSpawnedTime = owner.GetGame().GameTime;

            AddBuff("InfernalGuardianBurning", 45.0f, 1, spell, tibbers, owner);
            AddBuff("InfernalGuardianTimer", 45.0f, 1, spell, owner, owner);

            // Pyromania stuff here

            string particles;
            switch (owner.SkinID)
            {
                case 1:
                    particles = "Annie_skin02_R_cas";
                    break;
                case 4:
                    particles = "Annie_skin05_R_cas";
                    break;
                case 8:
                    particles = "Annie_skin09_R_cas";
                    break;
                default:
                    particles = "Annie_R_cas";
                    break;
            }
            AddParticle(owner, null, particles, tibbers.Position);

            //ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute);
            var sector = spell.CreateSpellSector(new SectorParameters
            {
                Length = spell.SpellData.CastRadius[0],
                Lifetime = 1f,
                SingleTick = true,
                MaximumHits = 15,
                OverrideFlags = SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes,
                Type = SectorType.Area,
                Tickrate = 100 // this is required or else the sector will hit immediately and we won't get the notification.
            });

            ApiEventManager.OnSpellSectorHit.AddListener(this, sector, TargetExecute, false);

            AddBuff("Pyromania", 250000f, 1, spell, owner, owner);
            var buff = owner.GetBuffWithName("Pyromania");
            NotifyBuffStacks(buff);
        }

        public void TargetExecute(SpellSector sector, AttackableUnit target)
        {
            var spell = sector.SpellOrigin;
            var owner = spell.CastInfo.Owner;

            // Pyromania stun here
            var Ap = owner.Stats.AbilityPower.Total * 0.8f;
            var totalDamage = 50 + (125 * spell.CastInfo.SpellLevel) + Ap;
            target.TakeDamage(owner, totalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);

            if (isGoneStun)
                AddBuff("Stun", stunDuration, 1, spell, target, owner);
        }
    }

    public class InfernalGuardianGuide : BasePetController
    {
    }
}