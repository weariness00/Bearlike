using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Lobby,
    Game,
}

public class SceneManagerEx : MonoBehaviour
{
    public void LoadScene(SceneType type)
    {
        SceneManager.LoadScene((int)type);
    }

    public void LoadScene(int type) => LoadScene((SceneType)type);
    public void LoadScene(string type) => LoadScene((SceneType)Enum.Parse(typeof(SceneType), type));

}

