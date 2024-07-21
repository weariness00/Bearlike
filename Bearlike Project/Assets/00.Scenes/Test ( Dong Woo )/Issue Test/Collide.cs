using Fusion;
using UnityEngine;

namespace _00.Scenes.Test___Dong_Woo__.Issue_Test
{
    public class Collide : NetworkBehaviour
    {
        public SceneReference s;
        public SceneReference c;
        public int serverindex;
        public int stageindex;
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.Log($"Object : {other.name}");
                Destroy(GetComponent<BoxCollider>());
                
            }
        }

        public override void Spawned()
        {
            var sc =SceneRef.FromIndex(serverindex);
            Runner.MoveGameObjectToScene(gameObject, sc);

            // Runner.UnloadScene(SceneRef.FromIndex(stageindex));
        }
    }
}
