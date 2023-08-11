Shader"PostProcessing/CharacterOutlineShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _Outline("Outline", 2D) = "white" {}
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

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
                float3 normalWS       : NORMAL;
                float3 viewDir      : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            TEXTURE2D(_Outline);
            SAMPLER(sampler_Outline);

            float SampleSceneOutline(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_Outline, sampler_Outline, uv).r;
            }

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewDir = _WorldSpaceCameraPos.xyz - TransformObjectToWorld(v.positionOS.xyz);
                return
o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float outline = SampleSceneOutline(i.uv);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    
                return lerp(color,_OutlineColor,outline);
            }
            ENDHLSL
        }
    }
}
