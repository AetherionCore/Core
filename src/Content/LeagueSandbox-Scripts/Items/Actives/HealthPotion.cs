using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.API;
using System;
using GameServerCore.Enums;
using GameServerLib.GameObjects.AttackableUnits;

namespace ItemSpells
{
    public class RegenerationPotion : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            // TODO
            CastingBreaksStealth = false,
            DoesntBreakShields = true
        };

        Spell healthPotion;
        private float lastWarning;

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            healthPotion = spell;
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
            if (spell.SpellName == healthPotion.SpellName)
            {
                if (unit.Stats.CurrentHealth >= unit.Stats.HealthPoints.Total)
                {
                    if (CanWarn())
                        SendWarningPopup(champ, "Full HP", champ.ClientId, FloatTextType.Heal);
                    return false;
                }
            }
            return true;
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            AddBuff("RegenerationPotion", 15.0f, 1, spell, owner, owner);
        }
    }
}
