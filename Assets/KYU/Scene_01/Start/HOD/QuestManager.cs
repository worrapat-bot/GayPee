using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("Quest Item Pool")]
    [Tooltip("ไอเท็มทั้งหมดที่สามารถถูกสุ่มได้")]
    public List<string> availableItems = new List<string>()
    {
        "Keycard", "Battery", "Syringe", "Lab Note", "Chip", "Scanner"
    };

    [Header("Map Pool")]
    [Tooltip("ชื่อแผนที่ทั้งหมดที่สามารถถูกสุ่มได้")]
    public List<string> availableMaps = new List<string>()
    {
        "Secret Lab", "Abandoned Bunker", "Ruined Facility"
    };

    [Header("Quest Display")]
    [Tooltip("TextMeshProUGUI ที่ใช้แสดงข้อความภารกิจ")]
    public TextMeshProUGUI questTextUI;

    [Header("Voice System")]
    [Tooltip("เสียงที่จะเล่นเมื่อ Quest ใหม่ถูกสร้าง")]
    public AudioClip bossVoiceClip;
    private AudioSource audioSource;

    [Header("Generated Quest Info")]
    public string currentMap;
    public List<string> currentItems = new List<string>();

    private string questTemplate =
        "From : BOSS\n" +
        "Yo, listen up!\n" +
        "I’ve got a job for you.\n" +
        "Head into the <color=#780606>{MAP}</color> " +
        // and bring me " + "<color=#00ffff>{ITEM1}</color>, <color=#00ffff>{ITEM2}</color>, and <color=#00ffff>{ITEM3}</color>.\n" +
        " And break the middle core of it.\n" +
        "You know what happens if you fail...";

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        GenerateQuest();
    }

    // 🔹 สุ่มภารกิจใหม่
    public void GenerateQuest()
    {
        if (availableMaps.Count == 0 || availableItems.Count < 3)
        {
            Debug.LogError("❌ Missing items or maps for quest generation!");
            return;
        }

        // ✅ สุ่มแผนที่
        currentMap = availableMaps[Random.Range(0, availableMaps.Count)];

        // ✅ สุ่ม 3 ไอเท็มไม่ซ้ำ
        currentItems.Clear();
        List<string> tempPool = new List<string>(availableItems);
        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, tempPool.Count);
            currentItems.Add(tempPool[index]);
            tempPool.RemoveAt(index);
        }

        // ✅ สร้างข้อความภารกิจ
        string questText = questTemplate
            .Replace("{MAP}", currentMap)
            .Replace("{ITEM1}", currentItems[0])
            .Replace("{ITEM2}", currentItems[1])
            .Replace("{ITEM3}", currentItems[2]);

        // ✅ แสดงบน UI
        if (questTextUI != null)
        {
            questTextUI.text = questText;
        }
        else
        {
            Debug.LogWarning("⚠️ QuestTextUI is not assigned!");
        }

        // ✅ เล่นเสียงเมื่อสร้าง Quest
        if (bossVoiceClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(bossVoiceClip);
        }

        // ✅ Debug Console
        Debug.Log("=== NEW QUEST GENERATED ===");
        Debug.Log(questText);
    }

    // 🔹 ใช้กับปุ่มกด UI
    public void GenerateNewQuestButton()
    {
        GenerateQuest();
    }
}
