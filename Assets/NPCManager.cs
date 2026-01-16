using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    public class NPCSlot
    {
        public NPC npcPrefab;
        public Transform house;
    }

    public List<NPCSlot> npcs = new List<NPCSlot>();

    void Start()
    {
        foreach (var slot in npcs)
        {
            if (slot.npcPrefab != null && slot.house != null)
            {
                NPC npc = Instantiate(slot.npcPrefab);
                npc.Init(slot.house);
            }
        }
    }
}
