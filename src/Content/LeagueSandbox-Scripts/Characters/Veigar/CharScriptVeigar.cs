using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.API;
using GameServerLib.GameObjects.AttackableUnits;
using Buffs;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects;
using System;
using LeaguePackets.Game.Events;
using Spells;

namespace CharScripts
{
    public class CharScriptVeigar : ICharScript
    {
        Spell Spell;
        ObjAIBase Owner;
        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            Owner = owner;
            Spell = spell;
            ApiEventManager.OnKill.AddListener(this, Owner, OnKillTarget, false);
        }

        private void OnKillTarget(DeathData data)
        {
            
        }

        public void OnDeactivate(ObjAIBase owner, Spell spell)
        {
        }

        public void OnUpdate(float diff)
        {
            // mana regeration (correct implementation)
            if (Owner == null)
                return;

            Owner.Stats.ManaRegeneration.FlatBonus = Owner.Stats.ManaRegeneration.BaseValue * ((Owner.Stats.ManaPoints.Total - Owner.Stats.CurrentMana) / Owner.Stats.ManaPoints.Total);
        }
    }
}