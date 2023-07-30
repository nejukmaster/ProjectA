Shader"Character/Skin"
{
    Properties
    {
        _BaseMap("BaseMap",2D) = "white"{}
        _DiffuseRamp("Diffuse Ramp",2D) = "white"{}
        _Tone("Skin Tone",Color) = (1,1,1,1)
        _TonePow("Tone Power", Range(0,1)) = 0.5
        _ShadowColor("Shadow Color",Color) = (0,0,0,1)
        _ShadowStrength("Shadow Strength", Range(0,3)) = 0.5
    }

        SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE 
            #pragma multi_compile _ _SHADOWS_SOFT 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS       : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS       : NORMAL;
                float3 positionWS : TEXCOORD1;  
                float3 viewDir      : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Tone;
                float _TonePow;
                half4 _ShadowColor;
                float _ShadowStrength;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DiffuseRamp);
            SAMPLER(sampler_DiffuseRamp);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDir = _WorldSpaceCameraPos.xyz - TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = lerp(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,IN.uv),_Tone,_TonePow);
    
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light light = GetMainLight(shadowCoord);
    
                float3 normal = normalize(IN.normalWS);
                float ndl = dot(normal, normalize(-1 * light.direction)) * 0.5 + 0.5;
                float ndv = dot(normal, normalize(-IN.viewDir)) * 0.5 + 0.5;
    
                //Shadowing & Retouching
                half4 shadow = SAMPLE_TEXTURE2D(_DiffuseRamp,sampler_DiffuseRamp,float2(ndl,ndv));
                color =  lerp(color,color * _ShadowColor,shadow);
    
                color = lerp(color,color * _ShadowColor,1-light.shadowAttenuation);
                return color;
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // -------------------------------------
            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Assets/Utility/Shader/LitDepthNormalsBatchedPass.hlsl"
            ENDHLSL
        }
    }
    Fallback "Vertex Lit"
}