using GameServerCore.Enums;
using GameServerCore.Domain;
using System.Numerics;
using System.Collections.Generic;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using System;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer;
using System.Linq;

namespace AIScripts
{
    public class LaneMinionAI : IAIScript
    {
        private const float DELAY_FIND_ENEMIES = 0.25f;
        private const float MAX_ENGAGE_DISTANCE = 2500f;
        private const float FEAR_WANDER_DISTANCE = 300f;
        private const float ANTI_KITE_TIMER = 4.0f;

        private readonly Dictionary<Action, RunningTimer> _timers = new();
        private readonly Dictionary<uint, float> _ignoreList = new();
        private float _timeSinceLastAttack;
        private bool _isFollowingPath = true;

        private LaneMinion Me;
        private AttackableUnit _tauntTarget;
        private Vector2 _fearLeashPoint;
        private AIState _currentState = AIState.AI_IDLE;
        private int _currentWaypointIndex = 0;

        public AIScriptMetaData AIScriptMetaData { get; set; } = new()
        {
            HandlesCallsForHelp = true
        };

        public void OnActivate(ObjAIBase owner)
        {
            Me = owner as LaneMinion;
            InitTimers();
            SetupEventListeners();
        }

        private void InitTimers()
        {
            InitTimer(TimerFindEnemies, DELAY_FIND_ENEMIES, true);
            InitTimer(TimerMoveForward, 0, true);
            InitTimer(TimerAntiKite, ANTI_KITE_TIMER, false);
            InitTimer(TimerFeared, 1, true);
            StopTimer(TimerAntiKite);
            StopTimer(TimerFeared);
        }

        private void SetupEventListeners()
        {
            ApiEventManager.OnUnitBuffActivated.AddListener(this, Me, OnBuffActivate);
            ApiEventManager.OnUnitBuffDeactivated.AddListener(this, Me, OnBuffDeactivate);
        }

        private void OnBuffActivate(AttackableUnit unit, Buff buff)
        {
            switch (buff.BuffType)
            {
                case BuffType.TAUNT:
                    _tauntTarget = buff.SourceUnit;
                    OnTauntBegin();
                    break;
                case BuffType.FEAR:
                    _fearLeashPoint = buff.SourceUnit.Position;
                    OnFearBegin();
                    break;
            }
        }

        private void OnBuffDeactivate(AttackableUnit unit, Buff buff)
        {
            switch (buff.BuffType)
            {
                case BuffType.TAUNT:
                    OnTauntEnd();
                    break;
                case BuffType.FEAR:
                    OnFearEnd();
                    break;
            }
        }

        public void OnUpdate(float delta)
        {
            UpdateTimers(delta);
            RemoveExpiredIgnores(Me.GetGame().GameTime );
        }

        private void UpdateTimers(float delta)
        {
            foreach (var timer in _timers.Values)
            {
                timer.Update(delta);
            }

            if (Me.IsAttacking)
            {
                _timeSinceLastAttack = 0;
            }
            else if (Me.TargetUnit != null)
            {
                _timeSinceLastAttack += delta;
            }
        }

        protected void TimerFindEnemies()
        {
            if (_currentState == AIState.AI_HALTED) return;

            if (_currentState == AIState.AI_ATTACKMOVESTATE)
            {
                var target = FindTargetInAcR();
                if (target == null)
                {
                    TurnOffAutoAttack(MoveStopReason.LostTarget);
                    return;
                }
                SetStateAndCloseToTarget(AIState.AI_ATTACKMOVE_ATTACKING, target);
                ResetAndStartTimer(TimerAntiKite);
            }
            else if (_currentState == AIState.AI_TAUNTED && _tauntTarget != null)
            {
                SetStateAndCloseToTarget(AIState.AI_TAUNTED, _tauntTarget);
            }

            if (_currentState != AIState.AI_ATTACKMOVE_ATTACKING && _currentState != AIState.AI_TAUNTED)
                return;

            if (Me.TargetUnit == null)
                FindTargetOrMove();
            else if (DistanceBetweenObjectAndTarget(Me.TargetUnit) > MAX_ENGAGE_DISTANCE)
                FindTargetOrMove();

            if (TargetInAttackRange())
            {
                if (_currentState != AIState.AI_TAUNTED)
                    ResetAndStartTimer(TimerAntiKite);
                TurnOnAutoAttack(Me.TargetUnit);
            }
            else if (!TargetInCancelAttackRange())
            {
                TurnOffAutoAttack(MoveStopReason.ForceMovement);
            }
        }

        protected void TimerMoveForward()
        {
            if (_currentState == AIState.AI_HALTED) return;

            if (_currentState == AIState.AI_IDLE)
                FindTargetOrMove();
            else if (_currentState == AIState.AI_ATTACKMOVESTATE)
            {
                if (UpdateWaypoint())
                {
                    SetStateAndMoveToForwardNav();
                }
            }
        }

        protected void TimerFeared()
        {
            if (_currentState == AIState.AI_HALTED) return;

            if (_currentState == AIState.AI_FEARED)
            {
                var fearPoint = MakeWanderPoint(_fearLeashPoint, FEAR_WANDER_DISTANCE);
                SetStateAndMove(AIState.AI_FEARED, fearPoint);
            }
        }

        protected void TimerAntiKite()
        {
            if (_currentState == AIState.AI_HALTED) return;

            if (_currentState != AIState.AI_ATTACKMOVE_ATTACKING || !IsMoving())
                return;

            AddToIgnoreList(Me.TargetUnit?.NetId ?? 0, 0.1f);
            FindTargetOrMove();
        }

        protected void FindTargetOrMove()
        {
            if (_currentState == AIState.AI_HALTED) return;

            var target = FindTargetInAcR();
            if (target != null)
            {
                if (!Me.HasAutoAttacked)
                {
                    InitTimer(TimerFindEnemies, DELAY_FIND_ENEMIES, true);
                    return;
                }
                SetStateAndCloseToTarget(AIState.AI_ATTACKMOVE_ATTACKING, target);
                ResetAndStartTimer(TimerAntiKite);
            }
            else
            {
                SetStateAndMoveToForwardNav();
                StopTimer(TimerAntiKite);
            }
        }

        private void SetStateAndCloseToTarget(AIState state, AttackableUnit target)
        {
            _currentState = state;
            Me.SetTargetUnit(target, true);

            var order = GetMoveOrderForState(state);
            if (ShouldCancelAutoAttack(state, order))
            {
                TurnOffAutoAttack(MoveStopReason.Finished);
            }

            _isFollowingPath = false;
            Me.UpdateMoveOrder(order);
        }

        private void SetStateAndMove(AIState state, Vector2 pos)
        {
            _currentState = state;
            var order = GetMoveOrderForState(state);

            if (ShouldCancelAutoAttack(state, order))
            {
                TurnOffAutoAttack(MoveStopReason.Finished);
            }

            Me.SetWaypoints(new List<Vector2> { Me.Position, pos });
            Me.UpdateMoveOrder(order);
        }

        private void SetStateAndMoveToForwardNav()
        {
            _currentState = AIState.AI_ATTACKMOVESTATE;

            if (_currentWaypointIndex < Me.PathingWaypoints.Count)
            {
                if (!_isFollowingPath)
                {
                    // Get back to lane path
                    var targetWaypoint = Me.PathingWaypoints[_currentWaypointIndex];
                    var path = ApiFunctionManager.GetPath(Me.Position, targetWaypoint, Me.PathfindingRadius);
                    if (path != null && path.Count > 0)
                    {
                        path.Insert(0, Me.Position);
                        Me.SetWaypoints(path);
                        _isFollowingPath = true;
                    }
                }
                else
                {
                    var waypoint = Me.PathingWaypoints[_currentWaypointIndex];
                    Me.SetWaypoints(new List<Vector2> { Me.Position, waypoint });
                }
            }
            Me.UpdateMoveOrder(OrderType.AttackMove);
        }

        private bool UpdateWaypoint()
        {
            if (_currentWaypointIndex >= Me.PathingWaypoints.Count)
                return false;

            if (IsAtCurrentWaypoint())
            {
                _currentWaypointIndex++;
                return true;
            }

            return false;
        }

        private bool IsAtCurrentWaypoint()
        {
            var currentWaypoint = Me.PathingWaypoints[_currentWaypointIndex];
            return Vector2.Distance(Me.Position, currentWaypoint) <= Me.CollisionRadius + 25f;
        }

        private bool IsMoving()
        {
            return !Me.IsPathEnded();
        }

        private void TurnOnAutoAttack(AttackableUnit target)
        {
            if (!Me.IsAttacking)
            {
                Me.SetTargetUnit(target, true);
            }
        }

        private void TurnOffAutoAttack(MoveStopReason reason)
        {
            if (Me.IsAttacking)
            {     
                Me.CancelAutoAttack(false);
            }
        }

        private bool TargetInAttackRange()
        {
            return ObjectInAttackRange(Me.TargetUnit);
        }

        private bool ObjectInAttackRange(AttackableUnit target)
        {
            if (target == null) return false;
            var totalAttackRange = Me.Stats.Range.Total;
            return Vector2.DistanceSquared(Me.Position, target.Position) <= totalAttackRange * totalAttackRange;
        }

        private bool TargetInCancelAttackRange()
        {
            var target = Me.TargetUnit;
            if (target == null) return true;
            var totalCancelRange = Me.Stats.Range.Total * 2;
            return !(Vector2.DistanceSquared(Me.Position, target.Position) > totalCancelRange * totalCancelRange);
        }

        private Vector2 MakeWanderPoint(Vector2 pos, float dist)
        {
            var direction = Me.Position - pos;
            if (direction != Vector2.Zero)
            {
                direction = Vector2.Normalize(direction);
            }
            return Me.Position + direction * dist;
        }

        private float DistanceBetweenObjectAndTarget(AttackableUnit target)
        {
            return Vector2.Distance(Me.Position, target.Position);
        }

        private AttackableUnit FindTargetInAcR()
        {
            var possibleTargets = ApiFunctionManager.GetUnitsInRange(Me.Position, Me.Stats.AcquisitionRange.Total, true)
                .Where(u => IsValidTarget(u));

            return possibleTargets.OrderBy(u => (int)Me.ClassifyTarget(u))
                                .ThenBy(u => Vector2.Distance(Me.Position, u.Position))
                                .FirstOrDefault();
        }

        private bool IsValidTarget(AttackableUnit unit)
        {
            return unit != null
                && !unit.IsDead
                && unit.Team != Me.Team
                && !IsTargetIgnored(unit.NetId)
                && unit.Status.HasFlag(StatusFlags.Targetable)
                && unit.IsVisibleByTeam(Me.Team);
        }

        private void OnTauntBegin()
        {
            if (_tauntTarget != null)
            {
                SetStateAndCloseToTarget(AIState.AI_TAUNTED, _tauntTarget);
            }
        }

        private void OnTauntEnd()
        {
            _tauntTarget = null;
            FindTargetOrMove();
        }

        private void OnFearBegin()
        {
            SetStateAndMove(AIState.AI_FEARED, MakeWanderPoint(_fearLeashPoint, FEAR_WANDER_DISTANCE));
            ResetAndStartTimer(TimerFeared);
        }

        private void OnFearEnd()
        {
            StopTimer(TimerFeared);
            FindTargetOrMove();
        }

        public void OnCallForHelp(AttackableUnit attacker, AttackableUnit victim)
        {
            if (_currentState != AIState.AI_FEARED && _currentState != AIState.AI_TAUNTED && IsValidTarget(attacker))
            {
                var priority = (int)Me.ClassifyTarget(attacker, victim);
                if (priority < (int)ClassifyUnit.DEFAULT)
                {
                    SetStateAndCloseToTarget(AIState.AI_ATTACKMOVE_ATTACKING, attacker);
                }
            }
        }

        #region Timer Management
        private void InitTimer(Action callback, float interval, bool repeat)
        {
            _timers[callback] = new RunningTimer(callback, interval * 1000, repeat);
        }

        private void StopTimer(Action callback)
        {
            if (_timers.TryGetValue(callback, out var timer))
            {
                timer.Stop();
            }
        }

        private void ResetAndStartTimer(Action callback)
        {
            if (_timers.TryGetValue(callback, out var timer))
            {
                timer.Reset();
            }
        }

        private class RunningTimer
        {
            private readonly Action _callback;
            private readonly float _interval;
            private readonly bool _repeat;
            private float _accumulated;
            private bool _isRunning;

            public RunningTimer(Action callback, float interval, bool repeat)
            {
                _callback = callback;
                _interval = interval;
                _repeat = repeat;
                _isRunning = true;
            }

            public void Update(float delta)
            {
                if (!_isRunning) return;

                _accumulated += delta;
                if (_accumulated >= _interval)
                {
                    _callback();
                    if (_repeat)
                        _accumulated -= _interval;
                    else
                        _isRunning = false;
                }
            }

            public void Stop() => _isRunning = false;
            public void Reset()
            {
                _accumulated = 0;
                _isRunning = true;
            }
        }
        #endregion

        #region Target Ignore List
        private void AddToIgnoreList(uint targetId, float duration)
        {
            _ignoreList[targetId] = Me.GetGame().GameTime  + duration * 1000;
        }
        private void RemoveExpiredIgnores(float currentTime)
        {
            var expiredTargets = _ignoreList.Where(pair => pair.Value <= currentTime).Select(pair => pair.Key).ToList();
            foreach (var target in expiredTargets)
            {
                _ignoreList.Remove(target);
            }
        }

        private bool IsTargetIgnored(uint targetId)
        {
            return _ignoreList.TryGetValue(targetId, out var expireTime) && Me.GetGame().GameTime  < expireTime;
        }
        #endregion

        #region Move Order Management
        private static OrderType GetMoveOrderForState(AIState state)
        {
            switch (state)
            {
                case AIState.AI_ATTACKMOVE_ATTACKING:
                    return OrderType.AttackTo;
                case AIState.AI_ATTACKMOVESTATE:
                    return OrderType.AttackMove;
                case AIState.AI_TAUNTED:
                    return OrderType.AttackTo;
                case AIState.AI_FEARED:
                    return OrderType.MoveTo;
                case AIState.AI_IDLE:
                    return OrderType.Stop;
                case AIState.AI_HALTED:
                    return OrderType.Hold;
                default:
                    return OrderType.MoveTo;
            }
        }

        private static bool ShouldCancelAutoAttack(AIState state, OrderType order)
        {
            switch (state)
            {
                case AIState.AI_HARDIDLE_ATTACKING:
                case AIState.AI_ATTACKMOVE_ATTACKING:
                    return false;
            }

            switch (order)
            {
                case OrderType.OrderNone:
                case OrderType.Hold:
                case OrderType.MoveTo:
                case OrderType.Stop:
                    return true;
                default:
                    return false;
            }
        }
        #endregion
    }
}
