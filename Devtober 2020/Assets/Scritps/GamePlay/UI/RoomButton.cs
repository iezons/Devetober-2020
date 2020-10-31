using GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    public NpcController ctrl;
    public RoomTracker tracker;

    public void AfterInstantiate()
    {
        GetComponentInChildren<Text>().text = "Goto: " + tracker.RoomName();
    }

    public void Click()
    {
        ctrl.ReadyForDispatch(tracker.tempWayPoints[0].transform.position);
        transform.parent.gameObject.SetActive(false);
    }
}
