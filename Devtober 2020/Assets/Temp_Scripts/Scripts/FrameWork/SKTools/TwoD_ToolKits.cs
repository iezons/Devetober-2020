using UnityEditor;
using UnityEngine;

public class TwoD_ToolKits : EditorWindow
{
    bool AutoSortingTurnON = false;
    float SpaceOfEachLayer = 0;

    //场景物体自动图层排序
    [MenuItem("深空工具箱/2D工具箱")]
    static void EnableSorting()
    {
        GetWindow<TwoD_ToolKits>("2D工具箱By深空");
    }

    private void OnInspectorUpdate()
    {
        if(AutoSortingTurnON)
        {
            SpriteRenderer[] objs = FindObjectsOfType<SpriteRenderer>();
            for (int i = 0; i < objs.Length; i++)
            {
                if (!objs[i].CompareTag("Player"))
                {
                    objs[i].sortingOrder = -1 * Mathf.FloorToInt(objs[i].transform.position.y * SpaceOfEachLayer);
                }
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("图层相关", EditorStyles.largeLabel);
        AutoSortingTurnON = GUILayout.Toggle(AutoSortingTurnON, "图层自动排序", EditorStyles.toggle);
        GUILayout.Label("自动排序精度 (" + SpaceOfEachLayer.ToString() + "/10)", EditorStyles.label);
        SpaceOfEachLayer = GUILayout.HorizontalSlider(SpaceOfEachLayer, 0, 10);
        GUILayout.Label("", EditorStyles.label);
        SpaceOfEachLayer = float.Parse(GUILayout.TextField(SpaceOfEachLayer.ToString()));
    }
}
