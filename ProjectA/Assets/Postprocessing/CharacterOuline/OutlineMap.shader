Shader"PostProcessing/OutlineMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _NormalMult("Normal Outline Multiplier", Range(0,100)) = 1
        _NormalBias("Normal Outline Bias", Range(1,10)) = 1
        _DepthMult("Depth Outline Multiplier", Range(0,100)) = 1
        _DepthBias("Depth Outline Bias", Range(1,10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Name "OutlineMap"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                float4 _MainTex_ST;
                float _NormalMult;
                float _NormalBias;
                float _DepthMult;
                float _DepthBias;
            CBUFFER_END

            void Compare(inout float depthOutline, inout float normalOutline,float2 uv) {

                float3x3 verticalOutlineConv = {1,0,-1,
                                                2,0,-2,
                                                1,0,-1};
                float3x3 horizontalOutlineConv = {1,2,1,
                                                0,0,0,
                                                -1,-2,-1};

                float depthDifferency_vert = 0;
                float3 normalDifferency_vert = 0;

                for(uint i = 0; i < 9; i ++){
                    int x = i/3;
                    int y = i%3;

                    depthDifferency_vert += verticalOutlineConv[x][y] * SampleSceneDepth(uv + _MainTex_TexelSize.xy * float2(x-2,y-2));
                    normalDifferency_vert += verticalOutlineConv[x][y] * SampleSceneNormals(uv + _MainTex_TexelSize.xy * float2(x-2,y-2));
                }

                depthDifferency_vert = abs(depthDifferency_vert);
                normalDifferency_vert = abs(normalDifferency_vert);

                float depthDifferency_horizon = 0;
                float3 normalDifferency_horizon = 0;

                for(uint i = 0; i < 9; i ++){
                    int x = i/3;
                    int y = i%3;

                    depthDifferency_horizon += horizontalOutlineConv[x][y] * SampleSceneDepth(uv + _MainTex_TexelSize.xy * float2(x-2,y-2));
                    normalDifferency_horizon += horizontalOutlineConv[x][y] * SampleSceneNormals(uv + _MainTex_TexelSize.xy * float2(x-2,y-2));
                }

                depthDifferency_horizon = abs(depthDifferency_horizon);
                normalDifferency_horizon = abs(normalDifferency_horizon);

                depthOutline = depthDifferency_horizon + depthDifferency_vert / 2.0;
                normalOutline = (normalDifferency_horizon.r + normalDifferency_horizon.g + normalDifferency_horizon.b + normalDifferency_vert.r + normalDifferency_vert.g + normalDifferency_vert.b)/6.0;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                float3 normal = SampleSceneNormals(IN.uv);
                float depth = SampleSceneDepth(IN.uv);
                float normalDifference = 0;
                float depthDifference = 0;

                Compare(depthDifference, normalDifference, IN.uv);
                normalDifference = normalDifference * _NormalMult;
                normalDifference = saturate(normalDifference);
                normalDifference = pow(normalDifference, _NormalBias);
    
                depthDifference = depthDifference * _DepthMult;
                depthDifference = saturate(depthDifference);
                depthDifference = pow(depthDifference, _DepthBias);

                float outline = (normalDifference + depthDifference);

                half4 color = lerp(0, 1, outline);
                return color;
            }
            ENDHLSL
        }
    }
}
