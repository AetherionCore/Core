using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Linq;
using GameServerCore;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using System;

namespace Spells
{
    public class KarthusLayWasteA1 : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            AddParticleTarget(owner, owner, "Karthus_Base_Q_Hand_Glow", owner, bone: "R_Hand");
            AddParticle(owner, null, "Karthus_Base_Q_Point", spellPos);
            AddParticle(owner, null, "Karthus_Base_Q_Ring", spellPos);
            AddParticle(owner, null, "Karthus_Base_Q_Skull_Child", spellPos);
        }

        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            GameObject m = AddParticle(owner, null, "Karthus_Base_Q_Explosion", spellPos);

            var enemyTeam = CustomConvert.GetEnemyTeam(spell.CastInfo.Owner.Team);
            var affectedUnits = GetUnitsInRange(m.Position, 150, true);
            var affectedUnitsCount = 0;

            for (int i = affectedUnits.Count - 1; i >= 0; i--)
            {
                var team = affectedUnits[i]?.Team;
                if (team == owner.Team) affectedUnits.RemoveAt(i);
                if (team == enemyTeam || team == TeamId.TEAM_NEUTRAL)
                    affectedUnitsCount++;
            }

            var ap = spell.CastInfo.Owner.Stats.AbilityPower.Total;
            var damage = 20f + spell.CastInfo.SpellLevel * 20f + ap * 0.3f;
            if (affectedUnitsCount == 0)
            {
                AddParticle(owner, null, "Karthus_Base_Q_Hit_Miss", spellPos);
            }
            foreach (var unit in affectedUnits)
            {
                if (unit.NetId == owner.NetId)
                    continue;

                if (unit is Champion || unit is Minion)
                {
                    if (unit.Team == TeamId.TEAM_NEUTRAL)
                        owner.SetVisibleByTeam(TeamId.TEAM_NEUTRAL, true);
                    if (affectedUnitsCount == 1)
                    {
                        damage *= 2;
                        AddParticle(owner, null, "Karthus_Base_Q_Hit_Single", spellPos);
                        unit.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, true);
                    }
                    if (affectedUnitsCount > 1)
                    {
                        AddParticle(owner, null, "Karthus_Base_Q_Hit_Many", spellPos);
                        unit.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    }


                }
            }
            m.SetToRemove();
            AddParticle(owner, null, "Karthus_Base_Q_Explosion_Sound", spellPos);
        }
    }
}
