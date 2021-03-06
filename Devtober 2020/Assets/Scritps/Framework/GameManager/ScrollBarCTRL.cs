﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Test
{
    public string FunctionName;
    public UnityAction Action;
}

public class ScrollBarCTRL : MonoBehaviour
{
    public List<Test> test = new List<Test>();
    public Scrollbar scrollbar;
    public bool KeepLowest;
    public bool dragging = false;
    public float MinValue = 0.07f;

    // Start is called before the first frame update
    void Awake()
    {
        scrollbar.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (scrollbar.value <= MinValue)
        {
            KeepLowest = true;
        }

        if(KeepLowest & !dragging)
        {
            scrollbar.value = 0;
        }
    }

    public void OnBeginDrag()
    {
        dragging = true;
        KeepLowest = false;
    }

    public void OnEndDrag()
    {
        dragging = false;
        KeepLowest = false;
    }

    public void OnScroll()
    {
        KeepLowest = false;
    }
}
