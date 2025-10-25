using UnityEngine;
using TMPro;
using System.Collections;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed = 0.05f;

    private int index;

    public void StartDialogue()
    {
        index = 0;
        textComponent.text = string.Empty;
        gameObject.SetActive(true);
        if (lines.Length > 0)
            StartCoroutine(TypeLine());
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    IEnumerator TypeLine()
    {
        textComponent.text = string.Empty;
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
