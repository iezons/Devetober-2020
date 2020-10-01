using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestFindWay : MonoBehaviour
{

    public Transform startTra;//起点tra
    public Transform endTra;//终点tra
    public Transform floorTra;//地板tra

    public GameObject obstacleGridPrefab;//障碍物格子
    public GameObject pathGridPrefab;//路径格子
    public LayerMask obstacleLayer;//障碍物所在的层

    private FindWayGrid findWayGrid;

    private GameObject obstacleRootGo;//障碍物格子的父go
    private GameObject pathRootGo;//路径格子的父go
    private List<GameObject> pathGridGoList;//路径格子go列表

    void Start()
    {
        MeshFilter meshFilter = floorTra.GetComponent<MeshFilter>();
        int width = Mathf.CeilToInt(meshFilter.mesh.bounds.size.x) * (int)floorTra.localScale.x;
        int height = Mathf.CeilToInt(meshFilter.mesh.bounds.size.y) * (int)floorTra.localScale.y;

        findWayGrid = new FindWayGrid(width, height);

        obstacleRootGo = new GameObject("ObstacleRoot");
        pathRootGo = new GameObject("PathRoot");
        pathGridGoList = new List<GameObject>();

        ShowObstacle();
    }

    void Update()
    {
        List<FindWayNode> nodeList = findWayGrid.FindWay(startTra.position, endTra.position);
        if (nodeList != null)
        {
            ShowPath(nodeList);
        }
    }

    //展示路径
    public void ShowPath(List<FindWayNode> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (i < pathGridGoList.Count)
            {
                pathGridGoList[i].transform.position = list[i].scenePos;
                pathGridGoList[i].SetActive(true);
            }
            else
            {
                GameObject go = Instantiate(pathGridPrefab);
                go.transform.SetParent(pathRootGo.transform);
                go.transform.position = list[i].scenePos;
                pathGridGoList.Add(go);
            }
        }

        for (int i = list.Count; i < pathGridGoList.Count; i++)
        {
            pathGridGoList[i].SetActive(false);
        }
    }

    //展示障碍物
    public void ShowObstacle()
    {
        int width = findWayGrid.width;
        int height = findWayGrid.height;
        float halfNodeLength = findWayGrid.nodeLength / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                FindWayNode node = findWayGrid.GetNode(x, y);
                Collider2D col = Physics2D.OverlapBox(node.scenePos, new Vector2(halfNodeLength, halfNodeLength), 0, obstacleLayer);
                bool isObstacle = col == null ? false : true;

                if (isObstacle)
                {
                    GameObject go = GameObject.Instantiate(obstacleGridPrefab, node.scenePos, Quaternion.identity) as GameObject;
                    go.transform.SetParent(obstacleRootGo.transform);
                }
                node.isObstacle = isObstacle;
            }
        }
    }
}