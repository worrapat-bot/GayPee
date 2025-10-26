using System.Collections.Generic;
using UnityEngine;

public class ItemStateTracker : MonoBehaviour
{
    public static ItemStateTracker Instance;
    private HashSet<string> collectedItems = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void MarkCollected(string itemID)
    {
        collectedItems.Add(itemID);
    }

    public bool IsCollected(string itemID)
    {
        return collectedItems.Contains(itemID);
    }

    public List<string> GetCollectedItems()
    {
        return new List<string>(collectedItems);
    }

    public void RestoreItemStates(List<string> savedItems)
    {
        collectedItems = new HashSet<string>(savedItems);
        foreach (var item in GameObject.FindGameObjectsWithTag("Collectible"))
        {
            if (collectedItems.Contains(item.name))
                item.SetActive(false);
        }
    }
}
