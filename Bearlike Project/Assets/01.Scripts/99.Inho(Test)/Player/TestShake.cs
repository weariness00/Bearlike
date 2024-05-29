using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class TestShake : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WeaponCameraShake());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    private TweenerCore<Vector3, Vector3, VectorOptions> weaponMove;
    private TweenerCore<Quaternion, Vector3, QuaternionOptions> weaponRotate;
    
    [SerializeField] private float movement = 0.01f;
    [SerializeField] private float rotationAngle = 1;
    [SerializeField] private float time = 0.4f;
    
    private IEnumerator WeaponCameraShake()
    {
        var weaponCameraPosition = transform.position;
        var weaponCameraRotation = transform.rotation;
            
        weaponMove = transform.DOLocalMoveY(weaponCameraPosition.y + movement, time / 2).SetEase(Ease.InOutCubic);
        weaponRotate = transform.DOLocalRotate(weaponCameraRotation.eulerAngles + new Vector3(rotationAngle, 0, 0), time / 2);
        yield return new WaitForSeconds(time / 2);
        while (true)
        {
            weaponMove?.Kill();
            weaponRotate?.Kill();
            
            weaponMove = transform.DOLocalMoveY(weaponCameraPosition.y - movement, time).SetEase(Ease.InOutCubic);
            weaponRotate = transform.DOLocalRotate(weaponCameraRotation.eulerAngles + new Vector3(-rotationAngle, 0, 0), time);
            yield return new WaitForSeconds(time);
            
            weaponMove?.Kill();
            weaponRotate?.Kill();
            
            weaponMove = transform.DOLocalMoveY(weaponCameraPosition.y + movement, time).SetEase(Ease.InOutCubic);
            weaponRotate = transform.DOLocalRotate(weaponCameraRotation.eulerAngles + new Vector3(rotationAngle, 0, 0), time);
            yield return new WaitForSeconds(time);
        }
    }
}
