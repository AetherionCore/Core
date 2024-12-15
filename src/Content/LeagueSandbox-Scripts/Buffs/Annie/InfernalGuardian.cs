using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using GameServerLib.GameObjects.AttackableUnits;
using LeaguePackets.Game.Events;
using System;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace Buffs
{
    internal class InfernalGuardian : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Buff thisBuff;
        float tibbersSpawnedTime;
        float spellCd;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            thisBuff = buff;
            tibbersSpawnedTime = unit.GetGame().GameTime;
            spellCd = ownerSpell.GetCooldown();
            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, true);
        }

        public void OnDeath(DeathData data)
        {
            thisBuff.DeactivateBuff();
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveBuff(buff.SourceUnit, "InfernalGuardianTimer");
            SetSpell(buff.SourceUnit, "InfernalGuardian", SpellSlotType.SpellSlots, 3);
            var timeAlive = (unit.GetGame().GameTime - tibbersSpawnedTime) / 1000f;
            if (buff.SourceUnit is Champion annie)
            {
                var timeLeft = Math.Max(0, spellCd - timeAlive);
                annie.Spells[3].SetCooldown(timeLeft);
            }
        }
    }
}