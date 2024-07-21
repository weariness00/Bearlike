using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _00.Scenes.Test___Dong_Woo__.Issue_Test
{
    public class CallOtherScene : NetworkBehaviour
    {
        public SceneReference s;

        public override void FixedUpdateNetwork()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
            Runner.LoadScene(s, LoadSceneMode.Additive);
                
            }
        }
    }
}
