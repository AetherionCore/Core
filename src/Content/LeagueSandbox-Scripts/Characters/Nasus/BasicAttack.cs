using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class NasusBasicAttack : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            if (owner.HasBuff("NasusQ"))
                ApiFunctionManager.OverrideAnimation(owner, "Spell1", "Attack1");
            else
                ApiEventManager.OnLaunchAttack.AddListener(this, owner, OnLaunchAttack, true);
        }

        public void OnLaunchAttack(Spell spell)
        {
            spell.CastInfo.Owner.SetAutoAttackSpellWithoutReset("NasusBasicAttack2");
        }
    }

    public class NasusBasicAttack2 : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            if (owner.HasBuff("NasusQ"))
                ApiFunctionManager.OverrideAnimation(owner, "Spell1", "Attack2");
            else
                ApiEventManager.OnLaunchAttack.AddListener(this, owner, OnLaunchAttack, true);
        }

        public void OnLaunchAttack(Spell spell)
        {
            spell.CastInfo.Owner.SetAutoAttackSpellWithoutReset("NasusBasicAttack");
        }
    }
}

