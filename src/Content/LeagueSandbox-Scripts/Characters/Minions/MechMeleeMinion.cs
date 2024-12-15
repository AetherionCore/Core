using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
namespace Spells
{
    public class Blue_Minion_MechMeleeBasicAttack : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        };

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell) { }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            //LogInfo($"Super Minion Attacking BasicAttack1 {spell.SpellName}");
        }

        public void OnSpellPostCast(Spell spell)
        {
            LogInfo($"Super Minion Attacking BasicAttack1 {spell.SpellName} - {spell.SpellData.Flags}");
        }
    }
    public class Blue_Minion_MechMeleeBasicAttack2 : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        }; public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell) { }
    }
    public class Blue_Minion_MechMeleeBasicAttack3 : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        }; public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell) { }
    }
    public class Red_Minion_MechMeleeBasicAttack : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        }; public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell) { }
    }
    public class Red_Minion_MechMeleeBasicAttack2 : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        }; public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell) { }
    }
    public class Red_Minion_MechMeleeBasicAttack3 : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        }; public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell) { }
    }
}