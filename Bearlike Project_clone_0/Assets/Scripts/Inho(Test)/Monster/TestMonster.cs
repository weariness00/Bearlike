using Fusion;
using State.StateClass;
using State.StateClass.Base;
using State.StateSystem;

namespace Inho_Test_.Monster
{
    public class TestMonster : NetworkBehaviour
    {
        private void ApplyDamage(Hitbox playerHitbox)
        {
            var playerStateSystem = playerHitbox.Root.GetComponent<StateSystem>();
            var playerState = playerStateSystem.State;

            var monsterStateSystem = gameObject.GetComponent<StateSystem>();
            var monsterState = monsterStateSystem.State;
            
            playerState.ApplyDamage(monsterState.attack.Current, (ObjectProperty)monsterState.property);
        }
    }
}