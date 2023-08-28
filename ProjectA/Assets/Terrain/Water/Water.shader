Shader"Terrain/Water"
{
    Properties
    {
        _Color("Color 1",Color) = (1,1,1,1)
        _Color2("Color 2",Color) = (1,1,1,1)
        _BumpMap("Normal Map",2D) = "white" {}
        _BumpIntensity("Normal Map Intensity",Range(0,1)) = 0.5

        [Header(Wave)]
        _Amply("Amply",Range(1.0,50.0)) = 1.0
        _Frequency("Frequency",Range(0,1)) = 1.0
        _Sharpness("Wave Sharpness",Range(0,1)) = 0.2
        _WaveNum("Numbe of Waves", Int) = 1
        [ShowAsVector2]_Direction("Direction",Vector) = (1.0,1.0,0.0,0.0)
        _Speed("Flow Speed",Range(0,100)) = 1.0
        
        [Header(Foam)]
        _Foam("Foam: Amount(x) Scale(y) Cutoff(z) Frequency(w)",Vector) = (1,120,0.5,1)
        _FoamNoiseSize("Foam Noise Cell Size", Float) = 30.0
        _FoamIntensity("Foam Intensity",Range(0,1)) = 1.0

        [Header(Refraction)]
        _Scale("Refraction Scale", Range(0,1)) = 0.1
        _RefractionSpeed("Refraction Speed",Float) = 1
        _RefractionStrength("Refraction Strength",Range(0,0.01)) = 0.002

        [Header(Fog)]
        _FogIntensity("Fog Intensity",Range(0,0.01)) = 10
        _FogBias("Fog Bias",Range(1,10)) = 1

        [IntRange] _StencilRef("Stencil Reference Value", Range(0,255)) = 0
        
    }

        SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue"="Transparent" "ForceNoShadowCasting"="True" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Stencil{
                Ref [_StencilRef]
                Comp Equal
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Utility/Shader/WhiteNoise.cginc"
            
            #define E 2.7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274274663919320030599218174135966290435729003342952605956307381323286279434907632338298807531
            #define PI 3.141592

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS       : NORMAL;
                float4 tangentOS    :TANGENT;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS       : NORMAL;
                float3 positionWS : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 tangentWS    : TEXTCOORD3;
                float3 bitangentWS      : TEXCOORD4;
                float3 viewDir      : TEXCOORD5;

                UNITY_VERTEX_OUTPUT_STEREO
            };
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _Color2;
                float _BumpIntensity;
            
                float _Amply;
                float _Frequency;
                float4 _Direction;
                float _Sharpness;
                float _Speed;
                int _WaveNum;
            
                float4 _Foam;
                float _FoamIntensity;
                float _FoamNoiseSize;

                float _CellSize;
                float _Brightness;
    
                float _Scale;
                float _RefractionSpeed;
                float _RefractionStrength;
            
                float _ReflectionStrength;

                float3 _LightDir;
                float3 _CameraWS;
                half4 _LightColor;

                float _FogIntensity;
                float _FogBias;
            CBUFFER_END

            TEXTURECUBE(_ReflMap);
            SAMPLER(sampler_ReflMap);

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            float4 _CameraOpaqueTexture_TexelSize;

            float SampleSceneDepth(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraDepthTexture,sampler_CameraDepthTexture,UnityStereoTransformScreenSpaceTex(uv)).r;
            }

            float3 SampleSceneColor(float2 uv) {
                return SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, UnityStereoTransformScreenSpaceTex(uv)).rgb;
            }

            float WaterDepthFade(float Depth, float4 ScreenPosition, float Distance)
            {
                return (Depth-ScreenPosition.w)/(Distance*2);
            }

            float gradientNoise(float value){
                float fraction = frac(value);
                float interpolator = easeInOut(fraction);
            
                float previousCellInclination = rand1dTo1d(floor(value)) * 2 - 1;
                float previousCellLinePoint = previousCellInclination * fraction;

			    float nextCellInclination = rand1dTo1d(ceil(value)) * 2 - 1;
                float nextCellLinePoint = nextCellInclination * (fraction - 1);
    
			    return lerp(previousCellLinePoint, nextCellLinePoint, interpolator);
		    }

            inline float3 UnpackNormal(half4 packednormal)
            {
            #if defined(SHADER_API_GLES) && defined(SHADER_API_MOBILE)
                return (packednormal.xyz * 2 - 1);
            #else
                float3 normal;
                normal.xy = (packednormal.wy * 2 - 1);
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

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                float k = PI/_Amply;
                //IN.positionOS.y += sin(dot(_Direction.xy,float2(IN.positionOS.x, IN.positionOS.z)) * _Frequency + _Speed * _Time.x)*_Amply;
                //Gerstner Wave vertex
                float3 ori = IN.positionOS.xyz;
                ori.x += _Sharpness * _Direction.x * cos(dot(_Frequency * _Direction.xy,IN.positionOS.xz)+ _Speed * 4 / _Frequency * _Time.x)/(_Frequency*_WaveNum);
                ori.z += _Sharpness * _Direction.y * cos(dot(_Frequency * _Direction.xy,IN.positionOS.xz)+ _Speed * 4 / _Frequency * _Time.x)/(_Frequency*_WaveNum);
                ori.y = _Amply * sin(dot(_Frequency * _Direction.xy,IN.positionOS.xz) + _Speed * 4 / _Frequency * _Time.x);
    
                IN.positionOS.xyz = ori;
                

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * IN.tangentOS.w * unity_WorldTransformParams.w;
                OUT.viewDir = _WorldSpaceCameraPos.xyz - TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = _Color;
                float3 cell = IN.positionWS.xyz/_CellSize;
                Light light = GetMainLight();
    
                //Foam
                float2 screenUVs = IN.screenPos.xy / IN.screenPos.w;
                float zRaw = SampleSceneDepth(screenUVs);
                float zEye = LinearEyeDepth(SampleSceneDepth(screenUVs), _ZBufferParams);
    
                //Water Fog
                float waterFog= zEye - IN.screenPos.w;
                waterFog = _FogIntensity * waterFog;
                waterFog = pow(waterFog,_FogBias);
                color.rgb *= pow(_Color2.rgb,waterFog);
                color.a = color.a + waterFog;
                
                //Normal Mapping
                half3 bump = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,IN.uv + _Speed/1000 * _Time));
                float3 normal = TangentNormalToWorldNormal(bump, IN.tangentWS, IN.bitangentWS, IN.normalWS);
                float ndl = dot(normal, normalize(-1*light.direction)) * _BumpIntensity + (1-_BumpIntensity);
                color.rgb *= 1-ndl;
    
                //Refraction
                float refractionmap = bump.r * 0.299 + bump.g * 0.587 + bump.b * 0.114;
                refractionmap *= _Scale;
                half3 refractionColor = SampleSceneColor(screenUVs + refractionmap);
                color = lerp(half4(refractionColor,1), color, color.a);
                
                float foam = WaterDepthFade(zEye, IN.screenPos, _Foam.x);
                foam = (sin(_Foam.w * foam) * 0.5 + _Foam.y) * 1 / foam;
                float foamValue = step(_Foam.z + gradientNoise(dot(_Direction.xy,IN.positionWS.xz/_FoamNoiseSize) + _Speed*_Time.x),foam);
                //foamValue *= _Foam.y/foam - _Foam.y/_Foam.z;
                color = lerp(color, 1, foamValue * _FoamIntensity);
                
                return color;
            }
            ENDHLSL
        }
    }
    Fallback "Vertex Lit"
}