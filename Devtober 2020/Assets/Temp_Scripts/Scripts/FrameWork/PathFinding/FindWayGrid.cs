using System.Collections.Generic;
using UnityEngine;

public class FindWayGrid
{

    public int width;//格子水平方向个数
    public int height;//格子垂直方向个数
    public float nodeLength;//格子长度

    private FindWayNode[,] findWayNodes;//格子数组
    private float halfNodeLength;//格子长度的一半
    private Vector3 startPos;//场景坐标起点

    public FindWayGrid(int width, int height, float nodeLength = 1f)
    {
        this.width = width;
        this.height = height;
        this.nodeLength = nodeLength;

        findWayNodes = new FindWayNode[width, height];
        halfNodeLength = nodeLength / 2;
        startPos = new Vector3(-width / 2 * nodeLength + halfNodeLength, -height / 2 * nodeLength + halfNodeLength, 0);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = CoordinateToScenePos(x, y);
                findWayNodes[x, y] = new FindWayNode(false, pos, x, y);
            }
        }
    }

    //坐标转场景坐标
    public Vector3 CoordinateToScenePos(int x, int y)
    {
        Vector3 pos = new Vector3(startPos.x + x * nodeLength, startPos.y + y * nodeLength, 0);
        return pos;
    }

    //根据场景坐标获取节点
    public FindWayNode GetNode(Vector3 pos)
    {
        int x = (int)(Mathf.RoundToInt(pos.x - startPos.x) / nodeLength);
        int y = (int)(Mathf.RoundToInt(pos.y - startPos.y) / nodeLength);
        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);
        return GetNode(x, y);
    }

    //根据坐标获取节点
    public FindWayNode GetNode(int x, int y)
    {
        return findWayNodes[x, y];
    }

    //获取相邻节点列表
    public List<FindWayNode> GetNearbyNodeList(FindWayNode node)
    {
        List<FindWayNode> list = new List<FindWayNode>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                int x = node.x + i;
                int y = node.y + j;
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    list.Add(findWayNodes[x, y]);
                }
            }
        }
        return list;
    }

    //获取两个节点之间的距离
    int GetDistance(FindWayNode nodeA, FindWayNode nodeB)
    {
        int countX = Mathf.Abs(nodeA.x - nodeB.x);
        int countY = Mathf.Abs(nodeA.y - nodeB.y);
        if (countX > countY)
        {
            return 14 * countY + 10 * (countX - countY);
        }
        else
        {
            return 14 * countX + 10 * (countY - countX);
        }
    }

    //找出起点到终点的最短路径
    public List<FindWayNode> FindWay(Vector3 startPos, Vector3 endPos)
    {
        FindWayNode startNode = GetNode(startPos);
        FindWayNode endNode = GetNode(endPos);

        List<FindWayNode> openList = new List<FindWayNode>();
        List<FindWayNode> closeList = new List<FindWayNode>();
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            FindWayNode nowNode = openList[0];

            //选择花费最低的
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].fCost <= nowNode.fCost &&
                    openList[i].hCost < nowNode.hCost)
                {
                    nowNode = openList[i];
                }
            }

            openList.Remove(nowNode);
            closeList.Add(nowNode);

            //找到目标节点
            if (nowNode == endNode)
            {
                return GeneratePath(startNode, endNode);
            }

            List<FindWayNode> nearbyNodeList = GetNearbyNodeList(nowNode);
            for (int i = 0; i < nearbyNodeList.Count; i++)
            {
                FindWayNode node = nearbyNodeList[i];
                //如果是墙或者已经在关闭列表中
                if (node.isObstacle || closeList.Contains(node))
                {
                    continue;
                }
                //计算当前相邻节点与开始节点的距离
                int gCost = nowNode.gCost + GetDistance(nowNode, node);
                //如果距离更小，或者原来不在打开列表
                if (gCost < node.gCost || !openList.Contains(node))
                {
                    //更新与开始节点的距离
                    node.gCost = gCost;
                    //更新与结束节点的距离
                    node.hCost = GetDistance(node, endNode);
                    //更新父节点为当前选定的节点
                    node.parentNode = nowNode;
                    //加入到打开列表
                    if (!openList.Contains(node))
                    {
                        openList.Add(node);
                    }
                }
            }
        }

        return null;
    }

    //生成路径
    public List<FindWayNode> GeneratePath(FindWayNode startNode, FindWayNode endNode)
    {
        List<FindWayNode> nodeList = new List<FindWayNode>();
        if (endNode != null)
        {
            FindWayNode tempNode = endNode;
            while (tempNode != startNode)
            {
                nodeList.Add(tempNode);
                tempNode = tempNode.parentNode;
            }
            nodeList.Reverse();//反转路径
        }
        return nodeList;
    }
}