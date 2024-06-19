using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class TestTP : MonoBehaviour
{
    public Transform[] tpPlaces;
    public VisualEffect tpEffect;
    
    private int _tpPLaceIndex;
    private float _timer;       // timer는 fusion껄로 대체해야함
    
    IEnumerator SetTimerCoroutine()
    {
        yield return new WaitForSeconds(1.2f);
        StartCoroutine(StartVFXCoroutine());

    }
    
    IEnumerator StartVFXCoroutine()
    {
        while (true)
        {
            tpEffect.SendEvent("OnPlay");
                
            yield return new WaitForSeconds(3.0f);
        }
    }
    
    void Start()
    {
        tpEffect.SendEvent("StopPlay"); // spawn에서 실행해야함
        _timer = 0.0f;
        _tpPLaceIndex = 0;

        StartCoroutine(SetTimerCoroutine());
    }
    
    void Update()
    {
        _timer += Time.deltaTime;
        
        // 실제 구현
        // vfxtimer끝나면 이벤트 발생
        if (_timer > 3.0f)
        {
            // vfxtimer 1초 시작
            int index = Random.Range(0, 5);

            while (_tpPLaceIndex == index)
                index = Random.Range(0, 5);

            transform.position = tpPlaces[index].position;
            Debug.Log(index);
            
            _tpPLaceIndex = index;
            _timer = 0.0f;
        }
        
    }
}
