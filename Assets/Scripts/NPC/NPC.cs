using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPC : MonoBehaviour
{
    [Header("Behavior")]
    public Transform home;
    public float timeOutside = 60f;
    public Vector2 idleTimeRange = new Vector2(2f, 5f);

    [Header("Animation Thresholds")]
    public float walkSpeedThreshold = 0.1f; // Minimum agent speed to start walking
    public float runSpeedThreshold = 3f;    // Speed to consider running

    NavMeshAgent agent;
    Animator anim;

    float outsideTimer;
    float idleTimer;

    bool goingHome;
    bool isIdle;

    int sidewalkMask;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        sidewalkMask = 1 << NavMesh.GetAreaFromName("SideWalk");
        agent.areaMask = sidewalkMask;

        if (anim != null)
            anim.applyRootMotion = false;
    }

    public void Init(Transform homeTransform)
    {
        home = homeTransform;

        // Spawn EXACTLY at house position
        transform.position = home.position;
        agent.Warp(home.position);

        agent.isStopped = true;

        Debug.Log(name + " SPAWNED at home");

        // Wait 3 seconds, then start wandering
        StartCoroutine(StartAfterDelay(3f));
    }

    IEnumerator StartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartIdle();
    }

    void Update()
    {
        UpdateAnimation();

        if (!goingHome)
            outsideTimer += Time.deltaTime;

        if (isIdle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                if (!goingHome)
                    GoWander();
            }
            return;
        }

        if (!goingHome && outsideTimer >= timeOutside)
        {
            GoHome();
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartIdle();
        }
    }

    void UpdateAnimation()
    {
        if (anim == null) return;

        float speed = agent.velocity.magnitude;

        // Set animation bools based on speed and state
        if (isIdle)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
        }
        else
        {
            if (speed > runSpeedThreshold)
            {
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", true);
            }
            else if (speed > walkSpeedThreshold)
            {
                anim.SetBool("isWalking", true);
                anim.SetBool("isRunning", false);
            }
            else
            {
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
            }
        }
    }

    void StartIdle()
    {
        isIdle = true;
        agent.isStopped = true;
        idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);

        Debug.Log(name + " is IDLE for " + idleTimer.ToString("F1") + " seconds");
    }

    void GoWander()
    {
        isIdle = false;
        goingHome = false;
        agent.isStopped = false;

        Vector3 randomPoint = GetRandomSidewalkPoint();
        agent.SetDestination(randomPoint);

        Debug.Log(name + " is WANDERING to " + randomPoint);
    }

    void GoHome()
    {
        goingHome = true;
        isIdle = false;
        agent.isStopped = false;

        agent.SetDestination(home.position);

        Debug.Log(name + " is GOING HOME");
    }

    Vector3 GetRandomSidewalkPoint()
    {
        var triangulation = NavMesh.CalculateTriangulation();

        if (triangulation.indices.Length < 3)
            return transform.position;

        int index = Random.Range(0, triangulation.indices.Length / 3) * 3;

        Vector3 v1 = triangulation.vertices[triangulation.indices[index]];
        Vector3 v2 = triangulation.vertices[triangulation.indices[index + 1]];
        Vector3 v3 = triangulation.vertices[triangulation.indices[index + 2]];

        float r1 = Random.value;
        float r2 = Random.value;

        if (r1 + r2 > 1f)
        {
            r1 = 1f - r1;
            r2 = 1f - r2;
        }

        Vector3 randomPoint = v1 + r1 * (v2 - v1) + r2 * (v3 - v1);

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, sidewalkMask))
            return hit.position;

        return transform.position;
    }
}
