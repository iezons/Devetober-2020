using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassInstantiate : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        CachePool.GetInstance();
        EventCenter.GetInstance();
        ScenesMgr.GetInstance();
        ResMgr.GetInstance();
        AudioMgr.GetInstance();
        UIMgr.GetInstance();
    }

    private void Start()
    {
        
        
    }
}
