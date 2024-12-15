using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using GameServerLib.GameObjects.AttackableUnits;

namespace CharScripts
{
    public class CharScriptAshe : ICharScript
    {
        float Damage;
        float QDamage;
        Spell Passive;
        Champion Ashe;
        float Ampdamage;
        public void OnActivate(ObjAIBase owner, Spell spell = null)
        {
            Passive = spell;
            Ashe = owner as Champion;
            owner.Stats.MagicPenetration.PercentBaseBonus = 0;
            owner.Stats.MagicPenetration.PercentBonus = 0;
            ApiEventManager.OnHitUnit.AddListener(this, Ashe, OnHitUnit, false);
        }
        public void OnHitUnit(DamageData damageData)
        {
            Damage = Ashe.Stats.AttackDamage.Total * 0.1f;
            Ampdamage = Ashe.Stats.AttackDamage.Total * 0.01f * Ashe.Stats.Level;
            QDamage = (30 * Ashe.Spells[0].CastInfo.SpellLevel) + (Ashe.Stats.AttackDamage.Total * 0.3f);
            if (Ashe.HasBuff("FrostShot"))
            {
                if (Ashe.IsNextAutoCrit)
                {
                    damageData.Target.TakeDamage(Ashe, QDamage * 2, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, true);
                }
                else
                {
                    damageData.Target.TakeDamage(Ashe, QDamage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                }
            }
            if (Ashe.Spells[2]?.CastInfo?.SpellLevel >= 1 && (damageData.Target?.IsDead ?? false))
            {
                Ashe.AddGold(damageData.Target, 3);
            }
        }
    }
}