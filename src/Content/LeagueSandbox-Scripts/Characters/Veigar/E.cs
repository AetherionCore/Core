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
using System;
using Buffs;
using GameMaths;

namespace Spells
{
    public class VeigarEventHorizon : ISpellScript
    {
        Vector2 truecoords;
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,

        };

        List<uint> AlreadyHit = new List<uint>();

        public void OnSpellPostCast(Spell spell)
        {
            AlreadyHit.Clear();
        }

        public void OnSpellCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            var ownerSkinID = owner.SkinID;
            var ownerPosition = spell.CastInfo.Owner.Position;
            var castTargetPosition = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var direction = (castTargetPosition - ownerPosition).Normalized();
            float distanceToTarget = Vector2.Distance(ownerPosition, castTargetPosition);
            truecoords = (distanceToTarget > 650f) ? ownerPosition + direction * 650f : castTargetPosition;


            string cage = owner.SkinID switch
            {
                8 => "Veigar_Skin08_E_cage_green.troy",
                6 => "Veigar_Skin06_E_cage_green.troy",
                4 => "Veigar_Skin04_E_cage_green.troy",
                _ => "Veigar_Base_E_cage_green.troy"
            };
            AddParticle(owner, null, cage, truecoords, lifetime: 3.1f);

            //TODO: Stun Hitbox & Buff
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
            var spellFlags = SpellDataFlags.AffectMinions | SpellDataFlags.AffectEnemies | SpellDataFlags.AffectFriends | SpellDataFlags.AffectBarracksOnly | SpellDataFlags.AffectNeutral;
            var sector = spell.CreateSpellSector(new SectorParameters
            {
                Length = 400f,
                Lifetime = 3f,
                Tickrate = 60,
                CanHitSameTargetConsecutively = true,
                CanHitSameTarget = true,
                SingleTick = false,
                //OverrideFlags = spellFlags,
                Type = SectorType.Area
            });
        }

        private void TargetExecute(Spell spell, AttackableUnit unit, SpellMissile missile, SpellSector sector)
        {
            float innerRadius = 290f;
            float outerRadius = 400f;
            float distanceFromCenter = Vector2.Distance(unit.Position, truecoords);
            //LogDebug($"Distance for {unit.CharData.Name} -> {distanceFromCenter} ");
            if (distanceFromCenter >= (innerRadius - 5f) && distanceFromCenter <= (outerRadius + 5f) && !AlreadyHit.Contains(unit.NetId))
            {
                AlreadyHit.Add(unit.NetId);
                unit.StopMovement();
                AddBuff("Stun", 3f, 1, spell, unit, spell.CastInfo.Owner);
                ApplyAssistMarker(unit, spell.CastInfo.Owner, 10f);
            }
        }
    }
}