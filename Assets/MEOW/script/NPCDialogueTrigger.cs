using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue; // ลาก Dialogue ของ NPC นี้มาที่ Inspector
    public float interactDistance = 3f; // ระยะใกล้ที่สุดที่กด F ได้

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (Vector3.Distance(player.position, transform.position) <= interactDistance)
        {
            // กด F เพื่อเปิด Dialogue
            if (Input.GetKeyDown(KeyCode.F) && !dialogue.gameObject.activeSelf)
            {
                dialogue.gameObject.SetActive(true);
                dialogue.StartDialogue();
            }
        }
    }
}
