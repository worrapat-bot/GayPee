using UnityEngine;

public class MetalCollection : MonoBehaviour
{
    private int Metal = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Metal")
        {
            Metal++;
            Debug.Log(Metal);
            Destroy(other.gameObject);

        }
    }
}
