using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using System.Numerics;
using LeagueSandbox.GameServer.API;
using log4net.Core;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleCastSpell : PacketHandlerBase<CastSpellRequest>
    {
        private readonly Game _game;
        private readonly NetworkIdManager _networkIdManager;
        private readonly PlayerManager _playerManager;

        public HandleCastSpell(Game game)
        {
            _game = game;
            _networkIdManager = game.NetworkIdManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, CastSpellRequest req)
        {
            var targetObj = _game.ObjectManager.GetObjectById(req.TargetNetID);
            var targetUnit = targetObj as AttackableUnit;
            var owner = _playerManager.GetPeerInfo(userId).Champion;
            if (owner == null)
            {
                return false;
            }

            var s = owner.Spells[req.Slot];

            var ownerCastingSpell = owner.GetCastSpell();
            var canCast = s != null ? owner.CanCast(s) : false;
            // Instant cast spells can be cast during other spell casts.
            if (s != null && canCast
                && (ownerCastingSpell == null
                || (ownerCastingSpell != null
                    && s.SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
                    && !ownerCastingSpell.SpellData.CantCancelWhileWindingUp))
            {

                // hack for item swaps to avoid cooldown glitching.
                var isInventoryItem = s.CastInfo.SpellSlot >= (int)SpellSlotType.InventorySlots && s.CastInfo.SpellSlot < (int)SpellSlotType.BluePillSlot;
                if (isInventoryItem && s.CastInfo.SpellSlot != req.Slot)
                {
                    s.CastInfo.SpellSlot = req.Slot;
                }

                if (isInventoryItem && (!ApiEventManager.OnAllowUseItem.Publish(owner, (s, req.Slot))
                    || (!s.SpellData.CanCastWhileDisabled && owner.IsDead)))
                    return false;

                if (s.Cast(req.Position, req.EndPosition, targetUnit))
                {
                    if (s.CastInfo.SpellSlot >= (int)SpellSlotType.InventorySlots && s.CastInfo.SpellSlot < (int)SpellSlotType.BluePillSlot)
                    {
                        var item = s.CastInfo.Owner.Inventory.GetItem(s.SpellName);
                        if (item != null && item.ItemData.Consumed)
                        {
                            var inventory = owner.Inventory;
                            inventory.RemoveItem(inventory.GetItemSlot(item), owner);
                        }
                    }

                    return true;
                }
            }

            if (s != null && owner.CanDelayCast(s) && ownerCastingSpell != null)
            {
                var targetPos = req.EndPosition != Vector2.Zero ? req.EndPosition : req.Position;
                if (targetPos == Vector2.Zero)
                    targetPos = owner.Position;
                owner.IssueOrDelayOrder(OrderType.CastSpell, targetUnit, targetPos, req.Slot);
            }

            return false;
        }
    }
}
