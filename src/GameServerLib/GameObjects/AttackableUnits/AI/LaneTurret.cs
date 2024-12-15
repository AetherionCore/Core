using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using static LeaguePackets.Game.Common.CastInfo;
using static System.Net.Mime.MediaTypeNames;
using log4net.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class LaneTurret : BaseTurret
    {
        class ChampionAttackHistory
        {
            public uint NetId;
            public int AttackCounter;
            public float LastAttacked;
        }

        public TurretType Type { get; }
        Dictionary<uint, ChampionAttackHistory> championTargetDamageIncrease = [];

        public LaneTurret(
            Game game,
            string name,
            string model,
            Vector2 position,
            TeamId team = TeamId.TEAM_BLUE,
            TurretType type = TurretType.OUTER_TURRET,
            uint netId = 0,
            Lane lane = Lane.LANE_Unknown,
            MapObject mapObject = default,
            Stats stats = null,
            string aiScript = ""
        ) : base(game, name, model, position, team, netId, lane, mapObject, stats: stats, aiScript: aiScript)
        {
            Type = type;

            if (type == TurretType.FOUNTAIN_TURRET)
            {
                SetIsTargetableToTeam(TeamId.TEAM_BLUE, false);
                SetIsTargetableToTeam(TeamId.TEAM_PURPLE, false);
            }
        }

        //TODO: Decide wether we want MapScrits to handle this with Events or leave this here
        public override void Die(DeathData data)
        {
            float localGold = CharData.LocalGoldGivenOnDeath;
            float globalGold = CharData.GlobalGoldGivenOnDeath;
            float globalEXP = CharData.GlobalExpGivenOnDeath;

            //TODO: change this to assists
            var championsInRange = _game.ObjectManager.GetChampionsInRange(Position, Stats.Range.Total * 1.5f, true);

            if (localGold > 0 && championsInRange.Count > 0)
            {
                foreach (var champion in championsInRange)
                {
                    if (champion.Team == Team)
                    {
                        continue;
                    }

                    float gold = CharData.LocalGoldGivenOnDeath / championsInRange.Count;
                    champion.AddGold(champion, gold);
                    champion.AddGold(this, globalGold);
                }

                foreach (var player in _game.PlayerManager.GetPlayers(true))
                {
                    var champion = player.Champion;
                    if (player.Team != Team)
                    {
                        if (!championsInRange.Contains(champion))
                        {
                            champion.AddGold(champion, globalGold);
                        }
                        champion.AddExperience(globalEXP);
                    }
                }
            }
            else
            {
                foreach (var player in _game.PlayerManager.GetPlayers(true))
                {
                    var champion = player.Champion;
                    if (player.Team != Team)
                    {
                        {
                            champion.AddGold(champion, globalGold);
                            champion.AddExperience(globalEXP);
                        }
                    }
                }
            }
            base.Die(data);
        }

        public override void CalculateAutoAttackDamage(ref DamageData data)
        {
            var penetrating = GetBuffWithName("PenetratingBullets");
            if (penetrating == null)
            {
                // if turret hasn't heated up yet
                return;
            }

            var target = data.Target;
            if (target is not Champion || Type == TurretType.FOUNTAIN_TURRET)
            {
                return;
            }

            var bonusDamage = 0f;
            var heatingUp = false;
            var gameTime = _game.GameTime / 1000f;

            if (championTargetDamageIncrease.TryGetValue(target.NetId, out var championAttackHistory))
            {
                if (TotalChampionAttacks() + 1 == 2)
                {
                    // we set the flag here because after we set attackCounter++;
                    // it will set heatedup to true and won't increase the damage to 75% 
                    // hacky to avoid missing the 2nd damage increase.
                    heatingUp = true;
                }

                if (championAttackHistory.LastAttacked > 0 && gameTime - championAttackHistory.LastAttacked >= 4) // 4 seconds untargeted
                {
                    championAttackHistory.AttackCounter = 0;
                }
                var attackCounter = Math.Min(championAttackHistory.AttackCounter, 2);
                bonusDamage = data.Damage * (0.25f * attackCounter);
                championAttackHistory.AttackCounter++;
                championAttackHistory.LastAttacked = gameTime;
            }
            else
            {
                championTargetDamageIncrease[target.NetId] = new ChampionAttackHistory()
                {
                    NetId = target.NetId,
                    AttackCounter = 1,
                    LastAttacked = gameTime
                };
                heatingUp = TotalChampionAttacks() == 1;
            }

            if (bonusDamage > 0)
            {
                data.Damage += bonusDamage;
                data.PostMitigationDamage = target.Stats.GetPostMitigationDamage(data.Damage, data.DamageType, this);
            }

            // only after the initial damage modification not before
            if (heatingUp && Stats.AttackDamage.PercentBaseBonus < 0.75f) // should only execute two times.
            {
                Stats.AttackDamage.PercentBaseBonus += 0.375f;
            }
        }

        public override void AutoAttackHit(AttackableUnit target)
        {
            if (Type == TurretType.FOUNTAIN_TURRET)
            {
                target.TakeDamage(this, 1000, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            }
            else
            {
                base.AutoAttackHit(target);
            }
        }

        public override void TakeDamage(DamageData damageData, DamageResultType damageText, IEventSource sourceScript = null)
        {
            if (damageData.IsAutoAttack && damageData.Attacker is Champion)
            {
                // if has fortification negate 30 flat damage from the attack.
                if (HasBuff("Fortification"))
                {
                    damageData.PostMitigationDamage = Stats.GetPostMitigationDamage(damageData.Damage, DamageType.DAMAGE_TYPE_PHYSICAL, damageData.Attacker);
                }
            }

            base.TakeDamage(damageData, damageText, sourceScript);
        }

        #region Damage Utilities 
        public int TotalChampionAttacks()
        {
            var totalAttacks = 0;
            foreach (var kvp in championTargetDamageIncrease)
            {
                totalAttacks += kvp.Value.AttackCounter;
            }
            return totalAttacks;
        }
        #endregion
    }
}
