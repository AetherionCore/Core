using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;

namespace ItemSpells
{
    public class YoumusBlade : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            AddBuff("SpectralFury", 6.0f, 1, spell, owner, owner);
        }

        public void OnSpellPostCast(Spell spell)
        {
            spell.SetCooldown(45, true);
            ApiFunctionManager.ResetItemSpellCooldown(spell.CastInfo.Owner, "YoumusBlade");
        }
    }
}
