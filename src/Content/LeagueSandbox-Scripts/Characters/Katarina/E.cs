using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerLib.GameObjects.AttackableUnits;
using GameServerCore;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Spells
{
    public class KatarinaE : ISpellScript
    {
        float Damage;
        float MarkDamage;
        private ObjAIBase Katarina;
        private AttackableUnit Target;
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
        };
        public void OnSpellCast(Spell spell)
        {
            Target = spell.CastInfo.Targets[0].Unit;
            Katarina = spell.CastInfo.Owner as Champion;
            if (Target.Team != Katarina.Team)
            {
                Damage = 45f + (25 * spell.CastInfo.SpellLevel) + (Katarina.Stats.AbilityPower.Total * 0.4f);
                MarkDamage = (15f * Katarina.Spells[0].CastInfo.SpellLevel) + (Katarina.Stats.AbilityPower.Total * 0.15f);
                if (Target.HasBuff("KatarinaQMark"))
                {
                    Target.TakeDamage(Katarina, MarkDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_PROC, false);
                    RemoveBuff(Target, "KatarinaQMark");
                }
                Target.TakeDamage(Katarina, Damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                AddParticleTarget(Katarina, null, "katarina_shadowStep_tar.troy", Target);
            }
            AddParticleTarget(Katarina, null, "katarina_shadowStep_cas.troy", Katarina);

            ForceMovement(Katarina, "Spell3", Vector2.Zero, 20, 20, 0.3f, 20);
            TeleportTo(Katarina, Target.Position.X, Target.Position.Y);
            AddBuff("KatarinaEReduction", 1.5f, 1, spell, Katarina, Katarina);
            PlayAnimation(Katarina, "Spell3", 0.5f);
        }
    }
}