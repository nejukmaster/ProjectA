Shader"Terrain/Water"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        [Header(Wave)]
        _Amply("Amply",Range(1.0,50.0)) = 1.0
        _Frequency("Frequency",Range(1.0,50.0)) = 1.0
        _Sharpness("Wave Sharpness",Range(1.0,3.14)) = 1.0
        [ShowAsVector2]_Direction("Direction",Vector) = (1.0,1.0,0.0,0.0)
        _Speed("Flow Speed",Range(0,100)) = 1.0
        
        [Header(Foam)]
        _Foam("Foam: Amount(x) Scale(y) Cutoff(z) Speed(w)",Vector) = (1,120,0.5,1)
        _FoamColor("Foam Color",Color) = (1.0,1.0,1.0,1.0)

        [Header(Voronoi)]
        _CellSize("Voronoi Cell Size",Range(0,100)) = 1
        _Brightness("Voronoi Brightness", Range(0,10)) = 1

        [Header(Refraction)]
        _Scale("Refraction Scale", Range(0,10)) = 1
        _RefractionSpeed("Refraction Speed",Float) = 1
        _RefractionStrength("Refraction Strength",Range(0,0.01)) = 0.002
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

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
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

                UNITY_VERTEX_OUTPUT_STEREO
            };
            half4 _Color;
            
            float _Amply;
            float _Frequency;
            float4 _Direction;
            float _Sharpness;
            float _Speed;
            
            float4 _Foam;
            half4 _FoamColor;

            float _CellSize;
            float _Brightness;

            float _Scale;
            float _RefractionSpeed;
            float _RefractionStrength;
            
            float _ReflectionStrength;

            float3 _LightDir;
            float3 _CameraWS;
            half4 _LightColor;

            TEXTURECUBE(_ReflMap);
            SAMPLER(sampler_ReflMap);

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

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
                return saturate((Depth-ScreenPosition.w)/Distance);
            }

            float3 VoronoiNoise(float3 value){
            float3 baseCell = floor(value);
    
            float minDistToCell = 10;
            float3 toClosestCell;
            float3 closestCell;
            [unroll]
            for(int x1=-1; x1<=1; x1++){
                [unroll]
                for(int y1=-1; y1<=1; y1++){
                    [unroll]
                    for(int z1=-1; z1<=1; z1++){
                        float3 cell = baseCell + float3(x1, y1, z1);
                        float3 cellPosition = cell + rand3dTo3d(cell);
                        float3 toCell = cellPosition - value;
                        float distToCell = length(toCell);
                        if(distToCell < minDistToCell){
                            minDistToCell = distToCell;
                            closestCell = cell;
                            toClosestCell = toCell;
                        }
                    }
                }
            }
    
            float minEdgeDistance = 10;
            [unroll]
            for(int x2=-1; x2<=1; x2++){
                [unroll]
                for(int y2=-1; y2<=1; y2++){
                    [unroll]
                    for(int z2=-1; z2<=1; z2++){
                        float3 cell = baseCell + float3(x2, y2, z2);
                        float3 cellPosition = cell + rand3dTo3d(cell);
                        float3 toCell = cellPosition - value;

                        float3 diffToClosestCell = abs(closestCell - cell);
                        bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y + diffToClosestCell.z < 0.1;
                        if(!isClosestCell){
                            float3 toCenter = (toClosestCell + toCell) * 0.5;
                            float3 cellDifference = normalize(toCell - toClosestCell);
                            float edgeDistance = dot(toCenter, cellDifference);
                            minEdgeDistance = min(minEdgeDistance, edgeDistance);
                        }
                    }
                }
            }

            float random = rand3dTo1d(closestCell);
            return float3(minDistToCell, random, minEdgeDistance);
        }
    
            float2 GradientNoiseDir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float GradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(GradientNoiseDir(ip), fp);
                float d01 = dot(GradientNoiseDir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(GradientNoiseDir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(GradientNoiseDir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
            }
            

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                float k = PI/_Amply;
                //IN.positionOS.y += sin(dot(_Direction.xy,float2(IN.positionOS.x, IN.positionOS.z)) + _Progress)*_Amply;
                //Gerstner Wave vertex
                float3 ori = IN.positionOS.xyz;
                IN.positionOS.x += _Direction.x * cos(dot(_Sharpness * _Direction.xy,ori.xz) * _Frequency + _Speed * _Time.x) / _Sharpness;
                IN.positionOS.z += _Direction.z * cos(dot(_Sharpness * _Direction.xy,ori.xz) * _Frequency + _Speed * _Time.x) / _Sharpness;
                IN.positionOS.y +=sin(dot(_Sharpness * _Direction.xy,ori.xz)  * _Frequency+ _Speed * _Time.x)*_Amply;
    
                float3 normal;
    
                normal.x = -1 * _Direction.x * _Sharpness * _Amply * cos(dot(_Direction.xy, ori.xz) + _Speed * _Time.x);
                normal.z = -1 * _Direction.z * _Sharpness * _Amply * cos(dot(_Direction.xy, ori.xz) + _Speed * _Time.x);
                normal.y = 1-sin(dot(_Direction.xy, ori.xz) + _Speed * _Time.x);
                

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);
                OUT.normalWS = TransformObjectToWorldNormal(normal);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = _Color;
                float3 cell = IN.positionWS.xyz/_CellSize;
    
                //Foam
                float2 screenUVs = IN.screenPos.xy / IN.screenPos.w;
                float zRaw = SampleSceneDepth(screenUVs);
                //float z01 = Linear01Depth(SampleSceneDepth(screenUVs), _ZBufferParams);
                float zEye = LinearEyeDepth(SampleSceneDepth(screenUVs), _ZBufferParams);
                float foam = WaterDepthFade(zEye, IN.screenPos, _Foam.x);
                float foamValue = step(foam,_Foam.z);
                color = lerp(color,_FoamColor,foamValue);
    
                //Voronoi
                float3 voronoi = VoronoiNoise(cell - _Speed * _Time.x);
                voronoi.x = pow(voronoi.x,_Brightness);
                color = lerp(color,_FoamColor,voronoi.x);
    
                //Refraction
                float2 tiledAndOffesettedUV = screenUVs * _Scale + (_Time.y * _RefractionSpeed);
                float2 refractionUVs = screenUVs +  GradientNoise(tiledAndOffesettedUV) * 2 * _RefractionStrength;
                half3 refractionColor = SampleSceneColor(refractionUVs);
                color = lerp(half4(refractionColor,1), color, color.a);
                
                return color;
            }
            ENDHLSL
        }
    }
    Fallback "Vertex Lit"
}