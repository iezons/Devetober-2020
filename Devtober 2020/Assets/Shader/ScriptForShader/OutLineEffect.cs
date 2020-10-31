//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class OutLineEffect : MonoBehaviour
//{
//    public Color OutlineColor
//    {
//        get { return outlineColor; }
//        set
//        {
//            outlineColor = value;
//            needsUpdate = true;
//        }
//    }

//    public float OutlineWidth
//    {
//        get { return outlineWidth; }
//        set
//        {
//            outlineWidth = value;
//            needsUpdate = true;
//        }
//    }

//    private Color outlineColor = Color.red;
//    private float outlineWidth = 0f;
//    bool needsUpdate = true;

//    private List<Mesh> bakeKeys = new List<Mesh>();

//    private List<ListVector3> bakeValues = new List<ListVector3>();

//    private Renderer[] renderers;
//    private Material outlineMaskMaterial;
//    private Material outlineFillMaterial;

//    [Serializable]
//    private class ListVector3
//    {
//        public List<Vector3> data;
//    }

//    public void SetOutline(bool IsOutline)
//    {
//        if (IsOutline)
//        {
//            Enable();
//        }
//        else
//        {
//            Disable();
//        }
//    }

//    // Start is called before the first frame update
//    void Awake()
//    {
//        renderers = GetComponentsInChildren<Renderer>();

//        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
//        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

//        outlineMaskMaterial.name = "OutlineMask (Instance)";
//        outlineFillMaterial.name = "OutlineFill (Instance)";

//        LoadSmoothNormals();

//        needsUpdate = true;
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }

//    void Enable()
//    {
//        foreach (var renderer in renderers)
//        {
//            var materials = renderer.sharedMaterials.ToList();

//            materials.Add(outlineMaskMaterial);
//            materials.Add(outlineFillMaterial);

//            renderer.materials = materials.ToArray();
//        }
//    }

//    void Disable()
//    {
//        foreach (var renderer in renderers)
//        {

//            // Remove outline shaders
//            var materials = renderer.sharedMaterials.ToList();

//            materials.Remove(outlineMaskMaterial);
//            materials.Remove(outlineFillMaterial);

//            renderer.materials = materials.ToArray();
//        }
//    }

//    void OnDestroy()
//    {
//        Destroy(outlineMaskMaterial);
//        Destroy(outlineFillMaterial);
//    }

//    void Bake()
//    {
//        var bakedMeshes = new HashSet<Mesh>();

//        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
//        {

//            if (!bakedMeshes.Add(meshFilter.sharedMesh))
//            {
//                continue;
//            }

//            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

//            bakeKeys.Add(meshFilter.sharedMesh);
//            bakeValues.Add(new ListVector3() { data = smoothNormals });
//        }
//    }

//    List<Vector3> SmoothNormals(Mesh mesh)
//    {

//        // Group vertices by location
//        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

//        // Copy normals to a new list
//        var smoothNormals = new List<Vector3>(mesh.normals);

//        // Average normals for grouped vertices
//        foreach (var group in groups)
//        {

//            // Skip single vertices
//            if (group.Count() == 1)
//            {
//                continue;
//            }

//            // Calculate the average normal
//            var smoothNormal = Vector3.zero;

//            foreach (var pair in group)
//            {
//                smoothNormal += mesh.normals[pair.Value];
//            }

//            smoothNormal.Normalize();

//            // Assign smooth normal to each vertex
//            foreach (var pair in group)
//            {
//                smoothNormals[pair.Value] = smoothNormal;
//            }
//        }

//        return smoothNormals;
//    }

//    void LoadSmoothNormals()
//    {

//        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
//        {

//            if (!registeredMeshes.Add(meshFilter.sharedMesh))
//            {
//                continue;
//            }

//            var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
//            var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

//            // Store smooth normals in UV3
//            meshFilter.sharedMesh.SetUVs(3, smoothNormals);
//        }

//        // Clear UV3 on skinned mesh renderers
//        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
//        {
//            if (registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
//            {
//                skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
//            }
//        }
//    }
//}
