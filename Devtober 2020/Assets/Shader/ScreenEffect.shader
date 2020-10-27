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

            /*struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }*/

            fixed4 frag (v2f_img i) : COLOR
            {
                //Get the colors from the RenderTexture and the uv's
                //from the v2f_img struct
                //fixed4 renderTex = tex2D(_MainTex, i.uv);
                fixed4 blendTex = tex2D(_BlendTex, i.uv);

                //Perform a multiply Blend mode
                //fixed4 blendedAdd = renderTex + blendTex;

                //Adjust amount of Blend Mode with a lerp
                //renderTex = lerp(renderTex, blendedAdd, _Opacity);
                blendTex = fixed4(blendTex.r, blendTex.g, blendTex.b, blendTex.r) *_Opacity;

                return blendTex;
            }
            ENDCG
        }
    }
    FallBack off
}
