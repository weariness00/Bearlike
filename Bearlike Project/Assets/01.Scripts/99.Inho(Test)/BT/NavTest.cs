using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class NavTest : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Animator animator;
    private NavMeshAgent _agent;
    
    NavMeshPath _path;
    private float _height;

    private float _startTime;

    public float risingSpeed = 1.0f;
    public float downSpeed = 1.0f;
    
    void Start()
    {
        _height = 0.0f;
        _path = new NavMeshPath();
        _agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        
        animator.SetTrigger("tAttack");
        animator.SetFloat("AttackType", 2);
        
        _agent.speed = FastDistance(player.transform.position, transform.position) / 3.0f;
        _agent.SetDestination(player.transform.position);
        StartCoroutine(JumpUpCoroutine(risingSpeed, 0.0f, 1));
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, _height, transform.position.z);
    }

    IEnumerator JumpUpCoroutine(float risingSpeed, float waitTime, int type)
    {
        _startTime = 0;
        
        while (true)
        {
            _startTime += Time.deltaTime;
            _height += risingSpeed * Time.deltaTime * (11 - _height);

            // transform.position = new Vector3(transform.position.x, _height, transform.position.z);

            yield return new WaitForSeconds(waitTime);

            if (_height >= 10.0f)
            {
                if(type == 2)
                {
                    animator.SetTrigger("tAttack");
                    animator.SetFloat("AttackType", 3);
                    yield return new WaitForSeconds(5.0f);
                }
                StartCoroutine(JumpDownCoroutine(downSpeed));
                yield break;
            }
        }
    }

    IEnumerator JumpDownCoroutine(float downSpeed)
    {
        while (true)
        {
            _startTime += Time.deltaTime;
            _height -= downSpeed * Time.deltaTime * (11 - _height);
            // transform.position = new Vector3(transform.position.x, _height, transform.position.z);
            yield return new WaitForSeconds(0.0f);

            if (_height < 0.0f)
            {
                Debug.Log(_startTime);
                _height = 0.0f;
                yield break;
            }
        }
    }
    
    public static float FastDistance(float3 pointA, float3 pointB)
    {
        return math.distance(pointA, pointB);
    }
}
