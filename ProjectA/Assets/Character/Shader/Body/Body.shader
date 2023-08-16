Shader"Character/Body"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _DiffuseRamp("Diffuse Ramp", 2D) = "white" {}
        _ShadowColor("Shadow Color",Color) = (0,0,0,1)
        _ShadowPow("Shadow Power",Range(0,10)) = 1
        _OutlineCutoff("Outline Cutoff",Range(0,1)) = 0.5
        _OutlineColor("Outline Color",Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
        
            Cull Off

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
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
            };

            struct Varyings
            {
                float2 uv               : TEXCOORD0;
                float4 positionHCS      : SV_POSITION;
                float3 normalWS         : NORMAL;
                float3 positionWS       : TEXCOORD1;
                float3 tangentWS        : TEXCOORD2;
                float3 bitangentWS      : TEXCOORD3;
                float3 viewDir          : TEXCOORD4;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _ShadowColor;
                float _BumpScale;
                float _ShadowPow;
                float _OutlineCutoff;
                half4 _OutlineColor;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DiffuseRamp);
            SAMPLER(sampler_DiffuseRamp);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            inline float3 UnpackNormal(half4 packednormal)
            {
            #if defined(SHADER_API_GLES) && defined(SHADER_API_MOBILE)
                return (packednormal.xyz * 2 - 1) * _BumpScale;
            #else
                float3 normal;
                normal.xy = (packednormal.wy * 2 - 1) * _BumpScale;
                normal.z = sqrt(1 - normal.x*normal.x - normal.y * normal.y);
                return normal;
            #endif
            }

            inline half3 TangentNormalToWorldNormal(half3 TangnetNormal, half3 T, half3  B, half3 N)
            {
                float3x3 TBN = float3x3(T, B, N);
                //calculate TBN^-1
                TBN = transpose(TBN);
                return mul(TBN, TangnetNormal);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * IN.tangentOS.w * unity_WorldTransformParams.w;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,IN.uv);
    
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light light = GetMainLight(shadowCoord);
    
                //Normal Mapping
                half3 bump = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,IN.uv));
                float3 normal = TangentNormalToWorldNormal(bump, IN.tangentWS, IN.bitangentWS, IN.normalWS);
                float ndl = dot(normal, normalize(-1 * light.direction)) * 0.5 + 0.5;
    
                //Shadowing & Retouching
                half4 shadow = SAMPLE_TEXTURE2D(_DiffuseRamp,sampler_DiffuseRamp,float2(pow(ndl,_ShadowPow),0));
                color =  lerp(color,color * _ShadowColor,shadow);
    
                color *= _ShadowColor * (light.shadowAttenuation * 0.5 + 0.5);
                
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
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Assets/Utility/Shader/LitDepthNormalsBatchedPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
