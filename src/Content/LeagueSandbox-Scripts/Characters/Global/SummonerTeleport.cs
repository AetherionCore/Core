using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using GameServerCore.Enums;

namespace Spells
{
    public class SummonerTeleport : ISpellScript
    {
        private ObjAIBase Owner;
        private AttackableUnit Target;
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            CastingBreaksStealth = false,
            ChannelDuration = 4.0f,
            TriggersSpellCasts = false,
            NotSingleTargetSpell = false
        };

        void ProtectTarget(bool protect)
        {
            if (protect)
            {
                Target.StopMovement();
                if (Target is ObjAIBase minion)
                    minion.SetTargetUnit(null, true);
            }
            Target.SetStatus(StatusFlags.CanAttack, !protect);
            Target.SetStatus(StatusFlags.CanMove, !protect);
            Target.SetStatus(StatusFlags.CanCast, !protect);
            Target.SetStatus(StatusFlags.Invulnerable, protect);
            Target.SetStatus(StatusFlags.Immovable, protect);
            LogInfo($"Protecting Target {Target.CharData.Name}");
        }

        void ProtectCaster(bool protect)
        {
            if (protect)
            {
                Owner.StopMovement();
                Owner.SetTargetUnit(null, true);
            }
            Owner.SetStatus(StatusFlags.CanAttack, !protect);
            Owner.SetStatus(StatusFlags.CanMove, !protect);
            Owner.SetStatus(StatusFlags.CanMoveEver, !protect);
            Owner.SetStatus(StatusFlags.CanCast, !protect);
            LogInfo($"Protecting Caster {protect}");
        }

        public void OnSpellChannel(Spell spell)
        {
            Target = spell.CastInfo.Targets[0].Unit;
            Owner = spell.CastInfo.Owner as Champion;
            var p101 = AddParticleTarget(Owner, Owner, "Summoner_Teleport_purple.troy", Owner, 4f);
            //var p102 = AddParticleTarget(Owner, Owner, "Summoner_Teleport.troy", Target, 4f);
            var p104 = AddParticle(Owner, Target, "Summoner_Teleport.troy", Target.Position, 4f);
            var p103 = AddParticleTarget(Owner, Target, "Summoner_Cast.troy", Target, 4f);

            ProtectCaster(true);
            if (Target is Minion)
            {
                ProtectTarget(true);
            }
        }

        public void OnSpellChannelCancel(Spell spell, ChannelingStopSource reason)
        {
            ProtectCaster(false);
            if (Target is Minion)
            {
                ProtectTarget(false);
            }
        }

        public void OnSpellPostChannel(Spell spell)
        {
            Target = spell.CastInfo.Targets[0].Unit;
            Owner = spell.CastInfo.Owner as Champion;
            TeleportTo(Owner, Target.Position.X, Target.Position.Y);
            //AddParticleTarget(Owner, Owner, "TeleportArrive", Owner, flags: 0);
            var p201 = AddParticleTarget(Owner, Owner, "summoner_teleportarrive.troy", Target, 1f);
            //var p202 = AddParticleTarget(Owner, Owner, "teleportarrive.troy", Owner);
            //var p203 = AddParticleTarget(Owner, Owner, "scroll_teleportarrive.troy", Owner);

            ProtectCaster(false);
            if (Target is Minion)
            {
                ProtectTarget(false);
            }
        }
    }
}