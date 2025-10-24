using UnityEngine;
using TMPro;

public class NPCSystem1 : MonoBehaviour
{
    public GameObject d_template;
    public GameObject canva;
    bool player_detection = false;
    private int currentDialogueIndex = 0;
    private string[] dialogues = { "Somsri's Special Stick :333" };
    private bool dialogueActive = false;

    void Start()
    {
        if (d_template == null) Debug.LogError("NPC: d_template is not assigned!");
        if (canva == null) Debug.LogError("NPC: canva is not assigned!");

        if (canva != null)
        {
            canva.SetActive(false);
        }
    }

    void Update()
    {
        if (player_detection && Input.GetKeyDown(KeyCode.F) && !PlayerController1.dialog)
        {
            Debug.Log("NPC: F key pressed! Starting dialogue...");
            StartDialogue();
        }

        // Click to advance dialogue
        if (dialogueActive && Input.GetMouseButtonDown(0))
        {
            Debug.Log("NPC: Mouse clicked! Advancing dialogue...");
            AdvanceDialogue();
        }
    }

    void StartDialogue()
    {
        Debug.Log("NPC: StartDialogue called");

        if (canva == null)
        {
            Debug.LogError("NPC: Canvas is null! Cannot start dialogue.");
            return;
        }

        canva.SetActive(true);
        PlayerController1.dialog = true;
        dialogueActive = true;
        currentDialogueIndex = 0;

        ClearDialogueBoxes();
        ShowNextDialogue();
    }

    void AdvanceDialogue()
    {
        ClearDialogueBoxes();
        currentDialogueIndex++;

        if (currentDialogueIndex < dialogues.Length)
        {
            ShowNextDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    void ShowNextDialogue()
    {
        Debug.Log($"NPC: ShowNextDialogue - Index: {currentDialogueIndex}/{dialogues.Length}");

        if (currentDialogueIndex < dialogues.Length)
        {
            NewDialogue(dialogues[currentDialogueIndex]);

            if (canva.transform.childCount > 1)
            {
                canva.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    void ClearDialogueBoxes()
    {
        int childCount = canva.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = canva.transform.GetChild(i);
            // Don't destroy the background UI element at index 1
            if (i > 1)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void EndDialogue()
    {
        Debug.Log("NPC: Ending dialogue");

        dialogueActive = false;
        PlayerController1.dialog = false;

        if (canva != null)
        {
            canva.SetActive(false);
        }
    }

    void NewDialogue(string text)
    {
        if (d_template == null)
        {
            Debug.LogError("NPC: d_template is null!");
            return;
        }

        GameObject template_clone = Instantiate(d_template, canva.transform);
        template_clone.SetActive(true);

        if (template_clone.transform.childCount > 1)
        {
            TextMeshProUGUI textComponent = template_clone.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
                Debug.Log($"NPC: Created dialogue with text: {text}");
            }
            else
            {
                Debug.LogError("NPC: TextMeshProUGUI component not found on template child!");
            }
        }
        else
        {
            Debug.LogError("NPC: Template doesn't have enough children!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            player_detection = true;
            Debug.Log("NPC: Player detected! Press F to talk.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            player_detection = false;
            Debug.Log("NPC: Player left trigger area.");
        }
    }
}