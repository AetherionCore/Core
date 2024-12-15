using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class DariusBasicAttack : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            if (owner.HasBuff("DariusNoxianTacticsONH"))
            {
                OverrideAnimation(owner, "Spell2", "Attack1");
            }
            else
            {
                OverrideAnimation(owner, "Attack1", "Spell2");
                spell.CastInfo.Owner.SetAutoAttackSpellWithoutReset("DariusBasicAttack");
            }
        }

        public void OnSpellPostCast(Spell spell)
        {
            spell.CastInfo.Owner.SetAutoAttackSpellWithoutReset("DariusBasicAttack2");
        }
    }

    public class DariusBasicAttack2 : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        };


        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            //ApiEventManager.OnLaunchAttack.AddListener(this, owner, OnLaunchAttack, true);
            if (owner.HasBuff("DariusNoxianTacticsONH"))
            {
                OverrideAnimation(owner, "Spell2", "Attack2");
            }
            else
            {
                OverrideAnimation(owner, "Attack2", "Spell2");
                spell.CastInfo.Owner.SetAutoAttackSpell("DariusBasicAttack2", false);
            }
        }

        public void OnSpellPostCast(Spell spell)
        {
            spell.CastInfo.Owner.SetAutoAttackSpellWithoutReset("DariusBasicAttack");
        }
    }
    public class DariusCritAttack : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            ApiEventManager.OnLaunchAttack.AddListener(this, owner, OnLaunchAttack, true);
            if (owner.HasBuff("DariusNoxianTacticsONH"))
            {
                OverrideAnimation(owner, "Spell2", "Crit");
            }
            else
            {
                OverrideAnimation(owner, "Crit", "Spell2");
                spell.CastInfo.Owner.SetAutoAttackSpell("DariusCritAttack", false);
            }
        }
        public void OnLaunchAttack(Spell spell)
        {
            if (!spell.CastInfo.Owner.IsNextAutoCrit)
                spell.CastInfo.Owner.SetAutoAttackSpellWithoutReset("DariusBasicAttack");
        }
    }
}