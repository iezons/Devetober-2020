using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ShaderReplacement : MonoBehaviour
{
    public Shader XRayShader;

    void Awake()
    {
        Setup();
    }

    void OnEnable()
    {
        Setup();
    }

    void Setup()
    {
        GetComponent<Camera>().SetReplacementShader(XRayShader, "XRay");
    }
}