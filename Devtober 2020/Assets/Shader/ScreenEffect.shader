Shader "PS1/ScreenEffect"
{
    Properties
    {
        _BlendTex("Blend Texture", 2D) = "black" {}
        _Opacity("Blend Opacity", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARN_precision_hint_fastest
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            uniform sampler2D _BlendTex;
            fixed _Opacity;

            fixed4 frag (v2f_img i) : COLOR
            {
                fixed4 blendTex = tex2D(_BlendTex, i.uv);

                blendTex = fixed4(blendTex.r, blendTex.g, blendTex.b, blendTex.r) *_Opacity;

                return blendTex;
            }
            ENDCG
        }
    }
    FallBack off
}
