using System;
using UnityEngine;

public class NewDialogue : MonoBehaviour
{
    int index = 2;
    void Update()
    {
        if (Input.GetMouseButton(0) && transform.childCount > 1)
        {
            if (PlayerController1.dialogue)
            {
                transform.GetChild(index).gameObject.SetActive(true);
                index += 1;
                if(transform.childCount == index)
                {
                    index = 2;
                    PlayerController1.dialogue = false;
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
