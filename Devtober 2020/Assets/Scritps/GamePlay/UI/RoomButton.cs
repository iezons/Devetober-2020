using GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomButton : MonoBehaviour
{
    public NpcController ctrl;
    public RoomTracker tracker;

    public void Click()
    {
        ctrl.ReadyForDispatch(tracker.tempWayPoints[0].transform.position);
    }
}
