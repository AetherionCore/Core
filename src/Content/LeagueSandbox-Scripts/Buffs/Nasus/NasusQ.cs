using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiEventManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.API;
using LeaguePackets.Game.Events;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Buffs
{
    internal class NasusQ : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Particle pbuff;
        Particle pbuff2;
        Buff thisBuff;
        ObjAIBase owner;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            thisBuff = buff;
            if (unit is ObjAIBase ai)
            {
                var owner = ownerSpell.CastInfo.Owner as Champion;
                pbuff = AddParticleTarget(ownerSpell.CastInfo.Owner, ownerSpell.CastInfo.Owner, "Nasus_Base_Q_Buf.troy", unit, buff.Duration, 1, "BUFFBONE_CSTM_WEAPON_1");
                //pbuff2 = AddParticleTarget(ownerSpell.CastInfo.Owner, ownerSpell.CastInfo.Owner, "Nasus_Base_Q_Wpn_trail.troy", unit, buff.Duration, 1, "BUFFBONE_CSTM_WEAPON_1");
                StatsModifier.Range.FlatBonus = 50.0f;
                unit.AddStatModifier(StatsModifier);
                OverrideAnimation(owner, "Spell1", "Attack1");
                OverrideAnimation(owner, "Spell1", "Attack2");
                //SetAnimStates(owner, new Dictionary<string, string> { { "Attack1", "Spell1" } });
                SealSpellSlot(owner, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, true);
                LogInfo($"Sealing Nasus Q!");
                ApiEventManager.OnLaunchAttack.AddListener(this, owner, OnLaunchAttack, false);
                owner.CancelAutoAttack(true);
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            var owner = ownerSpell.CastInfo.Owner as Champion;
            RemoveParticle(pbuff);
            RemoveParticle(pbuff2);
            RemoveBuff(thisBuff);
            //OverrideAnimation(owner, "Attack1", "Spell1");
            //OverrideAnimation(owner, "Attack2", "Spell1");
            //OverrideAnimation(owner, "Attack3", "Spell1");
            if (buff.TimeElapsed >= buff.Duration)
            {
                ApiEventManager.OnLaunchAttack.RemoveListener(this);
            }
            if (unit is ObjAIBase ai)
            {
                SealSpellSlot(ai, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, false);
            }
        }

        public void OnLaunchAttack(Spell spell)
        {
            var owner = spell.CastInfo.Owner as Champion;
            if (thisBuff != null && thisBuff.StackCount != 0 && !thisBuff.Elapsed())
            {
                spell.CastInfo.Owner.RemoveBuff(thisBuff);
                spell.CastInfo.Owner.SkipNextAutoAttack();
                SpellCast(spell.CastInfo.Owner, 0, SpellSlotType.ExtraSlots, false, spell.CastInfo.Owner.TargetUnit, Vector2.Zero);
                thisBuff.DeactivateBuff();
            }
            SealSpellSlot(owner, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, false);
           
        }
    }
}
