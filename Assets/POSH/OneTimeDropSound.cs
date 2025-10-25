using UnityEngine;

public class FakeDropSoundTrigger : MonoBehaviour
{
    public AudioSource dropSound;
    public Transform player;
    public float triggerDistance = 8f;
    private bool hasPlayed = false;

    void Update()
    {
        if (hasPlayed || player == null || dropSound == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= triggerDistance)
        {
            dropSound.Play();
            hasPlayed = true; // เล่นแค่ครั้งเดียว
        }
    }
}