using UnityEngine;

public class Metal : MonoBehaviour
{
    public int value;
    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            MetalManager metalManager = GameObject.FindAnyObjectByType<MetalManager>();
            metalManager.currentMetal += value;
            Destroy(gameObject);
        }
    }
}
