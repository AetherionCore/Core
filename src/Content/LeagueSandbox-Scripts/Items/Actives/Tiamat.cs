using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Enums;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings;

namespace ItemSpells
{
    public class ItemTiamatCleave : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
        }
        public void OnDeactivate(ObjAIBase owner, Spell spell)
        {
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            spell.SetCooldown(10, true);
        }

        public void OnSpellCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            //AddParticleTarget(owner, null, "TiamatMelee_itm_hydra.troy", owner);
            //AddParticleTarget(owner, null, "TiamatMelee_itm_hydra_active.troy", owner);
            //AddParticleTarget(owner, null, "TiamatMelee_itm_active.troy", owner);
            //AddParticleTarget(owner, null, "TiamatMelee_itm.troy", owner);
        }

        public void OnSpellPostCast(Spell spell)
        {
            if (spell.CastInfo.Owner is Champion c)
            {
                //c.GetSpell(1).LowerCooldown(20);
                var lifesteal = spell.CastInfo.Owner.Stats.LifeSteal;
                LogInfo($"Tiamat: {lifesteal.Total} - {lifesteal.BaseBonus} - {lifesteal.PercentBaseBonus} - {lifesteal.PercentBonus}");
                var targetPos = GetPointFromUnit(c, 125f);
                AddParticle(c, null, "TiamatMelee_itm_active.troy", targetPos);

                var units = GetUnitsInRange(targetPos, 350f, true);
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].Team != c.Team && !(units[i] is ObjBuilding || units[i] is BaseTurret))
                    {
                        var damage = c.Stats.AttackDamage.Total;
                        units[i].TakeDamage(c, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
                    }
                }

            }
        }

        public void OnSpellChannel(Spell spell)
        {
        }

        public void OnSpellChannelCancel(Spell spell, ChannelingStopSource reason)
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