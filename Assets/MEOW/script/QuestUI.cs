using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This handles the visual display of quests on screen
public class QuestUI : MonoBehaviour
{
    public GameObject questPanel;
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questProgressText;
    public Button acceptButton;

    public GameObject questListPanel;
    public Transform questListContent;
    public GameObject questItemPrefab;

    private Quest currentDisplayedQuest;

    void Start()
    {
        // Hide panels at start
        questPanel.SetActive(false);
        questListPanel.SetActive(false);

        acceptButton.onClick.AddListener(AcceptCurrentQuest);

        // Listen to quest events
        QuestManager.instance.onQuestAccepted.AddListener(OnQuestAccepted);
        QuestManager.instance.onQuestCompleted.AddListener(OnQuestCompleted);
    }

    void Update()
    {
        // Press Q to toggle quest list
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuestList();
        }

        UpdateActiveQuestsDisplay();
    }

    public void ShowQuestDetails(Quest quest)
    {
        currentDisplayedQuest = quest;
        questPanel.SetActive(true);

        questTitleText.text = quest.questName;
        questDescriptionText.text = quest.description +
            $"\n\nRewards:\nXP: {quest.experienceReward}\nGold: {quest.goldReward}";

        if (quest.isActive)
        {
            acceptButton.gameObject.SetActive(false);
            questProgressText.text = $"Progress: {quest.goal.currentAmount}/{quest.goal.requiredAmount}";
        }
        else if (quest.isCompleted)
        {
            acceptButton.gameObject.SetActive(false);
            questProgressText.text = "Completed!";
        }
        else
        {
            acceptButton.gameObject.SetActive(true);
            questProgressText.text = "";
        }
    }

    void AcceptCurrentQuest()
    {
        if (currentDisplayedQuest != null)
        {
            QuestManager.instance.AcceptQuest(currentDisplayedQuest);
            questPanel.SetActive(false);
        }
    }

    void ToggleQuestList()
    {
        questListPanel.SetActive(!questListPanel.activeSelf);

        if (questListPanel.activeSelf)
        {
            RefreshQuestList();
        }
    }

    void RefreshQuestList()
    {
        // Clear existing items
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // Add all available quests
        foreach (Quest quest in QuestManager.instance.allQuests)
        {
            GameObject item = Instantiate(questItemPrefab, questListContent);
            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
            text.text = quest.questName + (quest.isCompleted ? " [COMPLETED]" : quest.isActive ? " [ACTIVE]" : "");

            Button btn = item.GetComponent<Button>();
            Quest q = quest; // Capture for lambda
            btn.onClick.AddListener(() => ShowQuestDetails(q));
        }
    }

    void UpdateActiveQuestsDisplay()
    {
        // You can add a small UI element that always shows active quests
    }

    void OnQuestAccepted(Quest quest)
    {
        Debug.Log("UI: Quest accepted - " + quest.questName);
    }

    void OnQuestCompleted(Quest quest)
    {
        Debug.Log("UI: Quest completed - " + quest.questName);
    }
}