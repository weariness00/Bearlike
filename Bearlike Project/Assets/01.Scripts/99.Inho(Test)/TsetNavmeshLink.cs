using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class NavMeshLinkManager : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float height = 5f;
    public float duration = 1f;
    private Queue<NavMeshAgent> agentQueue = new Queue<NavMeshAgent>();
    private bool isLinkInUse = false;

    private void OnTriggerEnter(Collider other)
    {
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agentQueue.Enqueue(agent);
            if (!isLinkInUse)
            {
                StartCoroutine(ProcessQueue());
            }
        }
    }

    private IEnumerator ProcessQueue()
    {
        while (agentQueue.Count > 0)
        {
            isLinkInUse = true;
            NavMeshAgent agent = agentQueue.Dequeue();
            yield return StartCoroutine(MoveAgentParabolically(agent));
            yield return new WaitForSeconds(0.5f); // 다음 에이전트가 접근하기 전 대기 시간
            isLinkInUse = false;
        }
    }

    private IEnumerator MoveAgentParabolically(NavMeshAgent agent)
    {
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;
        float elapsedTime = 0f;

        agent.enabled = false;
        agent.transform.position = startPos;
        agent.enabled = true;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float parabolicT = t * 2 - 1;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y += (1 - parabolicT * parabolicT) * height;

            agent.transform.position = currentPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        agent.transform.position = endPos;
    }
}