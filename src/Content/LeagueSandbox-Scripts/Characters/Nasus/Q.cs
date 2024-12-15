using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeaguePackets.Game;

namespace Spells
{
    public class NasusQ : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            NotSingleTargetSpell = true
            // TODO
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnLevelUpSpell.AddListener(this, spell, OnLevelUp, true);
        }
        public void OnLevelUp(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
        }
        public void OnDeactivate(ObjAIBase owner, Spell spell)
        {
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            SealSpellSlot(owner, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, true);
            AddBuff("NasusQ", 6.0f, 1, spell, owner, owner);
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
    public class NasusQAttack : ISpellScript
    {
        AttackableUnit Target;
        ObjAIBase Owner;
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            NotSingleTargetSpell = true,
            IsDamagingSpell = true,
            // TODO
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            Target = target;
            Owner = owner;
        }
        public void OnSpellCast(Spell spell)
        {
            float ownerdamage = spell.CastInfo.Owner.Stats.AttackDamage.Total;
            float damage = 15 + 25 * Owner.GetSpell("NasusQ").CastInfo.SpellLevel + ownerdamage;
            Target.TakeDamage(Owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            AddParticleTarget(Owner, Target, "Nasus_Base_Q_Tar.troy", Target);
            if (Owner.HasBuff("NasusQStacks"))
            {
                if (Target.IsDead)
                {
                    for (int i = 0; i < 3; ++i)
                        AddBuff("NasusQStacks", 2500000f, 1, spell, Owner, Owner);
                    var stackDamage = Owner.GetBuffWithName("NasusQStacks").StackCount;
                    LogInfo($"StackDamage: {stackDamage}!");
                    SetBuffToolTipVar(Owner.GetBuffWithName("NasusQStacks"), 0, stackDamage);
                    SetSpellToolTipVar(Owner, 0, stackDamage, SpellbookType.SPELLBOOK_CHAMPION, 0, SpellSlotType.SpellSlots);
                }
            }
            else
            {
                if (Target.IsDead)
                {
                    for (int i = 0; i < 3; ++i)
                        AddBuff("NasusQStacks", 2500000f, 1, spell, Owner, Owner);
                    int stackDamage = Owner.GetBuffWithName("NasusQStacks").StackCount;
                    LogInfo($"StackDamage2: {stackDamage}!");
                    SetBuffToolTipVar(Owner.GetBuffWithName("NasusQStacks"), 0, 3);
                    SetSpellToolTipVar(Owner, 0, stackDamage, SpellbookType.SPELLBOOK_CHAMPION, 0, SpellSlotType.SpellSlots);
                }
            }
            SealSpellSlot(Owner, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, false);
        }
        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            int StackDamage = owner.GetBuffWithName("NasusQStacks")?.StackCount ?? 0;
            OverrideAnimation(owner, "Attack1", "Spell1");
            OverrideAnimation(owner, "Attack2", "Spell1");
            OverrideAnimation(owner, "Attack3", "Spell1");
            SealSpellSlot(owner, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, false);
            //SetSpellToolTipVar(Owner, 1, StackDamage, SpellbookType.SPELLBOOK_CHAMPION, 0, SpellSlotType.SpellSlots);
        }
    }
}