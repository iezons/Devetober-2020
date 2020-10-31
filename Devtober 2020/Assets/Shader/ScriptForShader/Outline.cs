using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]

public class Outline : MonoBehaviour
{
    public Color OutlineColor
    {
        get { return outlineColor; }
        set
        {
            outlineColor = value;
            needsUpdate = true;
        }
    }

    public float OutlineWidth
    {
        get { return outlineWidth; }
        set
        {
            outlineWidth = value;
            needsUpdate = true;
        }
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    [SerializeField]
    private Color outlineColor = Color.white;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 0f;

    private Renderer[] renderers;
    private Material outlineMaskMaterial;
    private Material outlineFillMaterial;

    private bool needsUpdate;

    public void SetOutline(bool IsOutline)
    {
        if (IsOutline)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        //LoadSmoothNormals();

        needsUpdate = true;
    }

    void OnValidate()
    {

    }

    void Update()
    {
        if (needsUpdate)
        {
            needsUpdate = false;

            UpdateMaterialProperties();
        }
    }

    void OnDisable()
    {
        Disable();
    }

    void Enable()
    {
        foreach (var renderer in renderers)
        {

            // Append outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Add(outlineMaskMaterial);
            materials.Add(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void Disable()
    {
        foreach (var renderer in renderers)
        {

            // Remove outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Remove(outlineMaskMaterial);
            materials.Remove(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void OnDestroy()
    {
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
    }

    void UpdateMaterialProperties()
    {
        outlineFillMaterial.SetColor("_OutlineColor", outlineColor);

        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
    }
}
