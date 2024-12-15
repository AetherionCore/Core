using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore;
using System.Collections.Generic;

namespace Buffs
{
    internal class KatarinaR : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.RENEW_EXISTING,
            IsHidden = true,
            MaxStacks = 1
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        private ObjAIBase Owner;
        private float tickInterval = 250f; // Tick every 0.25 seconds
        private float elapsedTickTime = 0f;
        private float finalDamage;
        private Particle particleEffect;
        private Spell spell;
        private AttackableUnit[] targets = new AttackableUnit[3];
        private List<uint> GotSpelled;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Owner = ownerSpell.CastInfo.Owner;
            spell = ownerSpell;

            GotSpelled = new List<uint>();

            // Calculate the proper base and scaling damage
            float baseDamage = 35f + (20f * (spell.CastInfo.SpellLevel - 1));
            float apScaling = Owner.Stats.AbilityPower.Total * 0.25f;
            float adScaling = Owner.Stats.AttackDamage.FlatBonus * 0.375f;
            finalDamage = baseDamage + apScaling + adScaling;
            elapsedTickTime = tickInterval;
            // Add spin particle effect
            particleEffect = AddParticleTarget(Owner, Owner, "Katarina_deathLotus_cas.troy", Owner, lifetime: 2.5f, bone: "C_BUFFBONE_GLB_CHEST_LOC");

            // Play the ultimate animation (spin)
            PlayAnimation(Owner, "Spell4", 1.0f);

        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(particleEffect);
            unit.StopAnimation("Spell4");
            unit.PlayAnimation("Idle");
            //PlayAnimation(Owner, "Idle", 1);
        }

        public void OnUpdate(float diff)
        {
            // Tick-based damage logic
            elapsedTickTime += diff;

            if (elapsedTickTime >= tickInterval)
            {
                FindAndDamageTargets();
                elapsedTickTime -= tickInterval;
            }
        }

        private void FindAndDamageTargets()
        {
            // Reset the list of targets
            for (int i = 0; i < targets.Length; i++)
                targets[i] = null;

            var enemies = GetChampionsInRange(Owner.Position, 500f, true);
            int targetCount = 0;

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];

                // Check team, avoid allies
                if (enemy.Team == Owner.Team)
                    continue;

                if (targetCount < 3)
                {
                    targets[targetCount] = enemy;
                    targetCount++;
                }
                else
                {
                    float currentEnemyDistance = Vector2.DistanceSquared(Owner.Position, enemy.Position);
                    for (int j = 0; j < targets.Length; j++)
                    {
                        if (targets[j] == null)
                            continue;

                        float targetDistance = Vector2.DistanceSquared(Owner.Position, targets[j].Position);
                        if (currentEnemyDistance < targetDistance)
                        {
                            targets[j] = enemy;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    SpellCast(Owner, 0, SpellSlotType.ExtraSlots, true, targets[i], Owner.Position, overrideForceLevel: spell.CastInfo.SpellLevel);
                    //targets[i].TakeDamage(Owner, finalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
                    AddBuff("GrievousWounds", 3f, 1, spell, targets[i], Owner);
                    //AddParticleTarget(Owner, targets[i], "katarina_deathLotus_dagger.troy", targets[i], 0.25f, bone: "weapon", targetBone: "chest");
                }
            }
        }
    }
}
