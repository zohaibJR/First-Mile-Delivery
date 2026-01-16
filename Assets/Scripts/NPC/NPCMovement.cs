using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;

    [Header("Roaming Settings")]
    public float roamRadius = 50f;
    public float minDistance = 15f;
    public float minWaitTime = 0.5f;
    public float maxWaitTime = 1.5f;
    public int sideWalkAreaIndex = 3; // NavMesh area mask

    [HideInInspector] public bool returningHome = false;
    [HideInInspector] public Transform homeTransform;

    private NavMeshAgent agent;
    private Animator anim;

    private bool isWaiting;
    private float waitTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        agent.updateRotation = false;
        agent.speed = walkSpeed;
        agent.autoBraking = true;
    }

    void Start()
    {
        // Initially disabled if manager controls start
        if (!returningHome)
        {
            agent.enabled = true;
            PickNewDestination(true);
        }
    }

    void Update()
    {
        HandleAnimation();
        SmoothRotate();

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
            }
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                isWaiting = false;

                if (!returningHome)
                    PickNewDestination(false);
                else
                    agent.destination = homeTransform.position; // go home
            }
        }

        // If returning home and reached home, stop
        if (returningHome && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            agent.isStopped = true;
        }
    }

    void HandleAnimation()
    {
        float v = agent.velocity.magnitude;

        if (v > 0.1f)
        {
            anim.SetBool("isWalking", true);
            anim.SetBool("isRunning", v > walkSpeed + 0.3f);
        }
        else
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
        }
    }

    void SmoothRotate()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion rot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 6f);
        }
    }

    void PickNewDestination(bool firstTime)
    {
        NavMeshHit hit;
        int mask = 1 << sideWalkAreaIndex;

        for (int i = 0; i < 25; i++)
        {
            float forwardDist = Random.Range(20f, roamRadius);
            Vector3 forwardBias = transform.forward * forwardDist;

            float sideOffset = Random.Range(-5f, 5f);
            Vector3 candidate = transform.position + forwardBias + (transform.right * sideOffset);

            if (NavMesh.SamplePosition(candidate, out hit, 6f, mask))
            {
                if (Vector3.Distance(transform.position, hit.position) >= minDistance || firstTime)
                {
                    agent.speed = (Random.value < 0.9f) ? walkSpeed : runSpeed;
                    agent.destination = hit.position;
                    return;
                }
            }
        }

        // Fallback
        Vector3 rand = transform.position + Random.insideUnitSphere * roamRadius;
        if (NavMesh.SamplePosition(rand, out hit, roamRadius, mask))
        {
            agent.speed = walkSpeed;
            agent.destination = hit.position;
        }
    }

    // Call this to send NPC home
    public void GoHome(Transform home)
    {
        homeTransform = home;
        returningHome = true;
        agent.isStopped = false;
        agent.destination = home.position;
    }
}
