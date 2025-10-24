using UnityEngine;

public class ProximitySound : MonoBehaviour
{
    public AudioSource soundSource;
    public Transform player;
    public float triggerDistance = 8f; // ระยะที่เริ่มได้ยินเสียง

    void Update()
    {
        if (player == null || soundSource == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < triggerDistance)
        {
            // ถ้าเข้าใกล้และเสียงยังไม่เล่น → เล่นเลย
            if (!soundSource.isPlaying)
            {
                soundSource.Play();
            }

            // ทำให้เสียงค่อย ๆ ดังขึ้นตามระยะ
            soundSource.volume = Mathf.Lerp(0f, 1f, 1 - (distance / triggerDistance));
        }
        else
        {
            // ถ้าออกห่าง → ค่อย ๆ เบาเสียงลง
            if (soundSource.isPlaying)
            {
                soundSource.volume = Mathf.MoveTowards(soundSource.volume, 0f, Time.deltaTime * 2f);

                if (soundSource.volume <= 0.01f)
                    soundSource.Stop();
            }
        }
    }
}