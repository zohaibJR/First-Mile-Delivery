using System.Collections;
using UnityEngine;

[System.Serializable]
public class NPCInfo
{
    public GameObject npcPrefab;   // NPC prefab
    public Transform house;        // Home transform
}

public class NPCManager : MonoBehaviour
{
    public NPCInfo[] npcList;

    [Header("Timers")]
    public float minSpawnDelay = 1f;
    public float maxSpawnDelay = 5f;

    public float minRoamTime = 5f;
    public float maxRoamTime = 15f;

    void Start()
    {
        foreach (NPCInfo npcInfo in npcList)
        {
            StartCoroutine(SpawnAndControlNPC(npcInfo));
        }
    }

    IEnumerator SpawnAndControlNPC(NPCInfo npcInfo)
    {
        // Spawn at house
        GameObject npc = Instantiate(npcInfo.npcPrefab, npcInfo.house.position, npcInfo.house.rotation);

        // Wait a random time at house
        float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
        yield return new WaitForSeconds(waitTime);

        // Enable NPC movement
        NPCMovement movement = npc.GetComponent<NPCMovement>();
        if (movement != null)
            movement.enabled = true;

        // Roam for random time
        float roamTime = Random.Range(minRoamTime, maxRoamTime);
        yield return new WaitForSeconds(roamTime);

        // Send NPC home
        if (movement != null)
            movement.GoHome(npcInfo.house);

        // Wait until NPC reaches home
        while (Vector3.Distance(npc.transform.position, npcInfo.house.position) > 0.5f)
            yield return null;

        // Destroy NPC
        Destroy(npc);
    }
}
