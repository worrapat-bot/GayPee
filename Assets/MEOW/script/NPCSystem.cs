using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCSystem : MonoBehaviour
{
    public GameObject d_template;
    public GameObject canva;
    public Button continueButton;
    bool player_detection = false;
    private int currentDialogueIndex = 0;
    private string[] dialogs = { "IM GAY" };

    void Start()
    {
        // Debug checks
        if (d_template == null) Debug.LogError("NPC: d_template is not assigned!");
        if (canva == null) Debug.LogError("NPC: canva is not assigned!");

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            continueButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("NPC: continueButton is not assigned. You need to assign a UI Button!");
        }

        // Make sure canvas starts disabled
        if (canva != null)
        {
            canva.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player_detection && Input.GetKeyDown(KeyCode.F) && !PlayerController1.dialog)
        {
            Debug.Log("NPC: F key pressed! Starting dialogue...");
            StartDialogue();
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
        currentDialogueIndex = 0;

        // Clear previous dialogue (skip first 2 children if they're UI elements)
        int childCount = canva.transform.childCount;
        Debug.Log($"NPC: Canvas has {childCount} children");

        for (int i = childCount - 1; i >= 2; i--)
        {
            Destroy(canva.transform.GetChild(i).gameObject);
        }

        ShowNextDialogue();
    }

    void ShowNextDialogue()
    {
        Debug.Log($"NPC: ShowNextDialogue - Index: {currentDialogueIndex}/{dialogs.Length}");

        if (currentDialogueIndex < dialogs.Length)
        {
            NewDialogue(dialogs[currentDialogueIndex]);

            // Show the UI element at index 1
            if (canva.transform.childCount > 1)
            {
                canva.transform.GetChild(1).gameObject.SetActive(true);
            }

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
            }
        }
        else
        {
            EndDialogue();
        }
    }

    void OnContinueButtonClicked()
    {
        Debug.Log("NPC: Continue button clicked!");
        currentDialogueIndex++;
        ShowNextDialogue();
    }

    void EndDialogue()
    {
        Debug.Log("NPC: Ending dialogue");

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
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