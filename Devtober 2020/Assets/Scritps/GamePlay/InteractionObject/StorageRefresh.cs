using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageRefresh : MonoBehaviour
{
    public float RefreshTime = 300;
    public Item_SO.ItemType itemType = Item_SO.ItemType.MedicalKit;
    float Timer = 0;
    StoragePos storage;

    // Start is called before the first frame update
    void Awake()
    {
        storage = GetComponent<StoragePos>();
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        if(Timer >= RefreshTime)
        {
            Timer = 0;
            if(storage.StorageItem.Count < storage.MaxStorage)
            {
                storage.StorageItem.Add(itemType);
            }
        }
    }
}
