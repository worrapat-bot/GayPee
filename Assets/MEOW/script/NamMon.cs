using UnityEngine;

public class NamMon : MonoBehaviour
{
    public int value;

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            NamMonManager namMonManager = GameObject.FindAnyObjectByType<NamMonManager>();
            namMonManager.currentNamMon += value;


            Destroy(gameObject);
        }
    }
}
