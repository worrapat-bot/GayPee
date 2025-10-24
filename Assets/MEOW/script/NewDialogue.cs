using System;
using UnityEngine;

public class NewDialogue : MonoBehaviour
{
    int index = 2;
    void Update()
    {
        if (Input.GetMouseButton(0) && transform.childCount > 1)
        {
            if (PlayerController1.dialog)
            {
                transform.GetChild(index).gameObject.SetActive(true);
                index += 1;
                if(transform.childCount == index)
                {
                    index = 2;
                    PlayerController1.dialog = false;
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
