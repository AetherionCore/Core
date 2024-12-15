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
using static LeaguePackets.Game.Common.CastInfo;
using System.Linq;

namespace Spells
{
    public class VeigarBalefulStrike : ISpellScript
    {
        int ticks;
        ObjAIBase Owner;
        StatsModifier statsModifier = new StatsModifier();
        Spell Spell;
        float stacks;

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
            Owner = owner;
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
                count = stacksPerLevel + 2;
                stacks = count - buffer;
            }

            else if (target is Minion minion)
            {
                string minionName = minion.CharData.Name;
                if (minionName.Contains("Cannon") || minionName.Contains("Mech")
                    || minionName.Contains("Dragon") || minionName.Contains("Worm")
                    || minionName.Contains("LizardElder") || minionName.Contains("Golem")
                    || minionName.Contains("GreatWraith") || minionName.Contains("GiantWolf"))
                {
                    count = 2;
                    stacks = 2f - buffer;
                }
                else
                {
                    count = 1;
                    stacks = 1f - buffer;
                }
            }

            // give veigar his ability popwers
            statsmodifier.AbilityPower.FlatBonus = owner.Stats.AbilityPower.FlatBonus + stacks;
            owner.AddStatModifier(statsmodifier);

            // give veigar his Q ability ocunt
            AddBuff("VeigarQPassive", 25000, (byte)count, null, owner, owner, true);
        }


        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner;
            //var target = spell.CastInfo.Targets.Count > 0 ? spell.CastInfo.Targets[0]?.Unit : null;
            if (target == null)
                return;
            LogInfo($"Target {target.NetId} - {owner.NetId}");
            var ownerSkinID = owner.SkinID;
            var APratio = owner.Stats.AbilityPower.Total * 0.6f;
            var damage = 80f + ((spell.CastInfo.SpellLevel - 1) * 45) + APratio;
            var StacksPerLevel = spell.CastInfo.SpellLevel;

            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            if (ownerSkinID == 8)
            {
                AddParticleTarget(owner, null, "Veigar_Skin08_Q_tar.troy", target);
            }
            else
            {
                AddParticleTarget(owner, null, "Veigar_Base_Q_tar.troy", target);
            }

            if (target.IsDead)
            {
                ProcessDeath(target, owner);
                //if (target is Champion)
                //{
                //    var buffer = owner.Stats.AbilityPower.FlatBonus;
                //    var stacks = StacksPerLevel + 2 - buffer;
                //    statsModifier.AbilityPower.FlatBonus = owner.Stats.AbilityPower.FlatBonus + stacks;
                //    owner.AddStatModifier(statsModifier);
                //    AddBuff("VeigarQPassive", 25000, (byte)(StacksPerLevel+2), spell, owner, owner, true);

                //}
                //else
                //{
                //    var buffer = owner.Stats.AbilityPower.FlatBonus;

                //    statsModifier.AbilityPower.FlatBonus = owner.Stats.AbilityPower.FlatBonus + 1f - buffer;
                //    owner.AddStatModifier(statsModifier);
                //    AddBuff("VeigarQPassive", 25000, 1, spell, owner, owner, true);
                //}
                if (ownerSkinID == 8)
                {
                    AddParticleTarget(owner, null, "Veigar_Skin08_Q_powerup.troy", owner);
                }
                else
                {
                    AddParticleTarget(owner, null, "Veigar_Base_Q_powerup.troy", owner);
                }
            }
        }

        public void OnUpdate(float diff)
        {
            // this is the correct implementation

        }
    }
}