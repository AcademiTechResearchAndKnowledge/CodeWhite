using UnityEngine;
using UnityEngine.AI;

public class C_Entity : MonoBehaviour
{
    private Transform target;
    private NavMeshAgent agent;

    private bool hasCaughtPlayer = false;

    private void Start()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetTarget(Transform t)
    {
        target = t;
        agent.SetDestination(target.position);
    }

    void Update()
    {
        if (target == null || hasCaughtPlayer) return;


        agent.SetDestination(target.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCaughtPlayer) return;

        if (other.CompareTag("Player"))
        {
            hasCaughtPlayer = true;

            StairsEffectManager.Instance.OnEntityCaughtPlayer();
        }
    }
}