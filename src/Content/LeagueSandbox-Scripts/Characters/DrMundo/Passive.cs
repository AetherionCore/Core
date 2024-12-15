using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace CharScripts
{
    public class Nevershade : ICharScript
    {
        private ObjAIBase _owner;

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            AddBuff("MundoPassiveCooldown", 0.9f, 1, spell, owner, owner);
            owner.Stats.MagicPenetration.PercentBaseBonus = 0;
            owner.Stats.MagicPenetration.PercentBonus = 0;
        }

        public void OnDeactivate(ObjAIBase owner, Spell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
