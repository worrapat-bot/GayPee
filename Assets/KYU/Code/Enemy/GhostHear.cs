using UnityEngine;
using UnityEngine.AI;

public class GhostHear : MonoBehaviour
{
    public Transform player;
    public float hearDistance = 10f;
    public float loudnessThreshold = 0.05f;
    private NavMeshAgent agent;
    private Vector3 wanderTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetRandomDestination();
    }

    void Update()
    {
        float loud = MicLoudness.loudness;
        float distance = Vector3.Distance(transform.position, player.position);

        if (loud > loudnessThreshold && distance < hearDistance)
        {
            agent.SetDestination(player.position);
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetRandomDestination();
        }
    }

    void SetRandomDestination()
    {
        Vector3 randomDir = Random.insideUnitSphere * 5f + transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, 5f, NavMesh.AllAreas))
        {
            wanderTarget = hit.position;
            agent.SetDestination(wanderTarget);
        }
    }
}