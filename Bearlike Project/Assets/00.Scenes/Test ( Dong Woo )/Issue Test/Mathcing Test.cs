using Fusion;
using UnityEngine;

namespace _00.Scenes.Test___Dong_Woo__.Issue_Test
{
    public class MathcingTest : NetworkBehaviour
    {
        public SceneReference s;
        public int sceneindex;

        public NetworkPrefabRef pr;
        
        public override void FixedUpdateNetwork()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var sr = SceneRef.FromIndex(sceneindex);
                Runner.LoadScene(sr);
                Runner.Spawn(pr);
            }
        }
    }
}
