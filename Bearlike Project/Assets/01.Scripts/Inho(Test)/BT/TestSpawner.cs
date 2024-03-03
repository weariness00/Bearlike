using Monster;
using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    public GameObject prefab;
    // public MonsterManager monsterManager;

    public int count;
    
    void Start()
    {
        // monsterManager = GameObject.Find("MonsterManager").GetComponent<MonsterManager>();
        
        for (var i = 0; i < count; i++)
        {
            // for (var j = 0; j < monsterManager.monsterList.Count; ++j)
            // {
            //     if (prefab.name == monsterManager.monsterList[j])
            //         monsterManager.MonsterCountDictionary[prefab.name] += 1;
            // }

            Instantiate(prefab, new Vector3(i * 10, 3, 0), new Quaternion(0, 0, 0, 0));
        }
    }
}
