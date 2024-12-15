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
using LeaguePackets.Game.Events;
using System;
using LeaguePackets.Game;

namespace CharScripts
{
    internal class CharScriptNasus : ICharScript
    {
        ObjAIBase Nasus;
        StatsModifier modifier = new StatsModifier();
        public void OnActivate(ObjAIBase owner, Spell spell = null)
        {
            Nasus = owner;
            RefreshLifeSteal(owner);
            ApiEventManager.OnLevelUp.AddListener(this, owner, OnLevelUp);
        }

        private void RefreshLifeSteal(AttackableUnit owner)
        {
            modifier.LifeSteal.BaseBonus = owner.Stats.Level switch
            {
                >= 1 and < 7 => 0.1f,
                >= 7 and < 13 => 0.15f,
                >= 13 => 0.2f,
                _ => .1f
            };
            owner.AddStatModifier(modifier);
        }

        private void OnLevelUp(AttackableUnit owner)
        {
            owner.RemoveStatModifier(modifier); // remove the old modifier
            // apply new modifier
            RefreshLifeSteal(owner);
        }

        public void OnUpdate(float diff)
        {
            //if (Nasus == null) return;
            //var buffQ = Nasus.GetBuffWithName("NasusQ");
            //if (buffQ == null && !SpellSlotEnabled(Nasus, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION))
            //{
            //    SealSpellSlot(Nasus, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, false);
            //}
        }
    }
}
