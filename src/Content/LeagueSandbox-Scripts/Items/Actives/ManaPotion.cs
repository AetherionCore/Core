using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.API;
using GameServerCore.Enums;
using GameServerLib.GameObjects.AttackableUnits;

namespace ItemSpells
{
    public class FlaskOfCrystalWater : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            // TODO
        };
        float lastWarning = 0;
        Spell manaPotion;

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            manaPotion = spell;
            ApiEventManager.OnAllowUseItem.AddListener(this, owner, UseItem);
        }

        private bool CanWarn()
        {
            var timeNow = GameTime / 1000f;
            if (timeNow - lastWarning >= 4) // 4 seconds passed since last warning
            {
                lastWarning = timeNow;
                return true;
            }
            return false;
        }

        private bool UseItem(ObjAIBase unit, Spell spell, byte slot)
        {
            var champ = unit as Champion;
            if (unit.IsDead)
            {
                if (CanWarn())
                    SendWarningPopup(champ, "Dead", champ.ClientId);
                return false;
            }
            if (spell.SpellName == manaPotion.SpellName)
            {
                if (unit.Stats.CurrentMana >= unit.Stats.ManaPoints.Total)
                {
                    if (CanWarn())
                        SendWarningPopup(champ, "Full MP", champ.ClientId, FloatTextType.Absorbed);
                    return false;
                }
            }
            return true;
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            AddBuff("FlaskOfCrystalWater", 15.0f, 1, spell, owner, owner);
        }
    }
}
