using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.API;
using GameServerLib.GameObjects.AttackableUnits;
using System;
using System.Collections.Generic;

namespace Buffs
{
    public class PenetratingBullets : IBuffGameScript
    {


        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.STACKS_AND_RENEWS,
            MaxStacks = 1,
            IsHidden = true
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public bool HeatedUp;
        LaneTurret turret;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (unit is not LaneTurret)
            {
                RemoveBuff(unit, "PenetratingBullets");
                return;
            }

            StatsModifier.ArmorPenetration.PercentBonus += 0.30f;
            turret = unit as LaneTurret;

            turret.AddStatModifier(StatsModifier);
            ApiEventManager.OnHitUnit.AddListener(this, turret, OnHitUnit);
        }

        private void OnHitUnit(DamageData data)
        {
            if (data.Target is Champion)
            {
                if (StatsModifier.AttackDamage.PercentBonus >= 0.75)
                {
                    // reset anyway
                    HeatedUp = true;
                    return;
                }

                turret.RemoveStatModifier(StatsModifier);
                StatsModifier.AttackDamage.PercentBonus += .375f;
                turret.AddStatModifier(StatsModifier);

                if (StatsModifier.AttackDamage.PercentBaseBonus >= 0.75)
                    HeatedUp = true;
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
        }
    }
}