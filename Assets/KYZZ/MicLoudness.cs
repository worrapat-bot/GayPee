using UnityEngine;

public class MicLoudness : MonoBehaviour
{
    public static float loudness;
    private AudioClip clip;
    private string micName;
    private const int sampleWindow = 128;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            clip = Microphone.Start(micName, true, 1, 44100);
            Debug.Log("?? Mic started: " + micName);
        }
        else
        {
            Debug.LogWarning("? No microphone detected!");
        }
    }

    void Update()
    {
        if (clip != null)
        {
            loudness = GetLoudnessFromMic();
            Debug.Log("Mic loudness: " + loudness); // ?? ดูค่านี้ใน Console
        }
    }

    float GetLoudnessFromMic()
    {
        int micPos = Microphone.GetPosition(micName) - sampleWindow;
        if (micPos < 0) return 0;

        float[] data = new float[sampleWindow];
        clip.GetData(data, micPos);

        float sum = 0;
        foreach (float s in data)
            sum += Mathf.Abs(s);

        return sum / sampleWindow;
    }
}