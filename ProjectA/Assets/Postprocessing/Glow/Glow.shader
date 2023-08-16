Shader"PostProcessing/Glow"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS       : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                return
o;
            }

            half4 frag (v2f i, out half4 bloom : SV_Target1) : SV_Target
            {
                // sample the texture
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                bloom = 1;
                return color.a;
            }
            ENDHLSL
        }
    }
}
