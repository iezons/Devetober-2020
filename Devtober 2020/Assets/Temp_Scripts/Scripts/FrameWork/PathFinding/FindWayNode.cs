using UnityEngine;

public class FindWayNode
{

    public bool isObstacle;//是否是障碍物
    public Vector3 scenePos;//场景位置
    public int x, y;//坐标

    public int gCost;//与起始点的距离
    public int hCost;//与目标点的距离
    public int fCost
    {
        get { return gCost + hCost; }
    }//总距离

    public FindWayNode parentNode;//父节点

    public FindWayNode(bool isObstacle, Vector3 scenePos, int x, int y)
    {
        this.isObstacle = isObstacle;
        this.scenePos = scenePos;
        this.x = x;
        this.y = y;
    }
}