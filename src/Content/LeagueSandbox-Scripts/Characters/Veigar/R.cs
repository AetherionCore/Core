using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.API;
using System.Collections.Generic;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings;
using GameServerLib.GameObjects.AttackableUnits;
using LeaguePackets.Game.Common;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Spells
{
    public class VeigarPrimordialBurst : ISpellScript
    {
        StatsModifier statsModifier = new StatsModifier();

        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Target
            }
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
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


        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner;
            var ownerSkinID = owner.SkinID;
            var APratio = owner.Stats.AbilityPower.Total * 1.2f;
            var targetAP = target.Stats.AbilityPower.Total * 0.8f;
            var damage = 250 + ((spell.CastInfo.SpellLevel - 1) * 125) + APratio + targetAP;
            var StacksPerLevel = spell.CastInfo.SpellLevel;

            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

            string particles;
            if (ownerSkinID == 8)
            {
                particles = "Veigar_Skin08_R_tar.troy";
            }
            else
            {
                particles = "Veigar_Base_R_tar.troy";
            }

            if (!target.IsDead)
            {
                AddParticleTarget(owner, target, particles, target, 1f);
            }
            else
            {
                ProcessDeath(target, owner);
                //var buffer = owner.Stats.AbilityPower.FlatBonus;

                //var stacks = (StacksPerLevel + 2) - buffer;
                //statsModifier.AbilityPower.FlatBonus += stacks;
                //owner.AddStatModifier(statsModifier);

                //AddBuff("VeigarQPassive", 25000, (byte)stacks, spell, owner, owner, true);

                if (ownerSkinID == 8)
                {
                    AddParticle(owner, target, "Veigar_Skin08_R_tar.troy", target.Position, 1f);

                }
                else
                {
                    AddParticle(owner, target, "Veigar_Base_R_tar.troy", target.Position, 1f);
                }
            }
        }
    }
}