Character Shader Part
=====================
#### Character Shader is based on unity-chan, and written with hlsl because this project is on URP. Following is what I made to implement Illustrative NRP Shader
>  1. Skin Shader With Diffuse Wraping
>  2. Body Shader With Diffuse Wraping and Normal Map
>  3. Hair Shader With Diffuse Wraping and Normal Map
>  4. Unlit Eye Shader
>  5. Shadow Casting

### Skin Shader With Diffuse Wraping

To get Illustrative Shadow, I used Diffuse Wrapping with normal Dot light and normal Dot view value. For this, I made a Diffuse Ramp which maps normal Dot light value at xAxis and normal dot view value at yAxis. So that, the fragment be illuminated by standard Lambert, and will brighten up that fragnent is nearer to Main Camera's view direction.

![Alt text](/ExplainImgs/SkinDiffuseRamp.png)

And following is code for appling it

```hlsl
half4 color = lerp(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,IN.uv),_Tone,_TonePow);
    
float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);      //This converts the world coordinates into shadow map coordinates.
Light light = GetMainLight(shadowCoord);
    
 float3 normal = normalize(IN.normalWS);
float ndl = dot(normal, normalize(-1 * light.direction)) * 0.5 + 0.5;  //Map values between -1 and 1 to values between 0 and 1
float ndv = dot(normal, normalize(-IN.viewDir)) * 0.5 + 0.5;
    
//Shadowing & Retouching
half4 shadow = SAMPLE_TEXTURE2D(_DiffuseRamp,sampler_DiffuseRamp,float2(ndl,ndv));    //Sampling a Diffuse Map with ndl and ndv Values
color =  lerp(color,color * _ShadowColor,shadow);
    
color = lerp(color,color * _ShadowColor,1-(light.shadowAttenuation*0.5 +0.5));    //Apply attenuation of light to affect the shadow.
return color;
```

Using this, we can get smoother shading effect. Left image is Skin Shader with Diffuse Wrapping, and right image is Skin shader with Single Step Toon Shading.

<img src="/ExplainImgs/SkinDiffuseWrapping.png" width="35%" height="30%"> <img src="/ExplainImgs/SkinSingleToonShading.png" width="30%" height="30%">

### Body Shader With Diffuse Wraping and Normal Map

The Clothes' Shader use Diffuse Wrapping as above. but, this shader is only use normal Dot light value. because this shader is not necessary to be shown smoothly as much as Skin Shader above. And the clothes should show wrinkles, So that, I apply normal mapping to shader. 

_Diffuse Map of Clothes_

![Alt text](/ExplainImgs/ClothesDiffuseRamp.png)

_Code_
```hlsl
inline float3 UnpackNormal(half4 packednormal)        //UnpackNormal Macro from "UnityCG.cginc"
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
    float3x3 TBN = float3x3(T, B, N);        //Obtain the base of the tangent space.
    TBN = transpose(TBN);                    //calculate TBN^-1
    return mul(TBN, TangnetNormal);          //Returns the world space normal by multiplying TBN^-1 by the tangent space normal vector.
}
...
 half4 frag (Varyings IN) : SV_Target
{
    half4 color = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,IN.uv);
    
    float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
    Light light = GetMainLight(shadowCoord);
    
    //Normal Mapping
    half3 bump = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,IN.uv));
    float3 normal = TangentNormalToWorldNormal(bump, IN.tangentWS, IN.bitangentWS, IN.normalWS);    //Extract the normal from the normal map.
    float ndl = dot(normal, normalize(-1 * light.direction)) * 0.5 + 0.5;                           //Calculates ndl as the normal obtained.
    
    //Shadowing & Retouching
    half4 shadow = SAMPLE_TEXTURE2D(_DiffuseRamp,sampler_DiffuseRamp,float2(pow(ndl,_ShadowPow),0));
    color =  lerp(color,color * _ShadowColor,shadow);
    
    color *= _ShadowColor * (light.shadowAttenuation * 0.5 + 0.5);
    
    return color;
}
```

_Clothes Without Normal Mapping_

![Alt text](/ExplainImgs/ClothesWithoutNormal.png)

_Clothes With Normal Mapping_

![Alt text](/ExplainImgs/ClothesWithNormal.png)

### Hair Shader With Diffuse Wrapping and Normal Map

Hair Shader use Diffuse Warpping and Normal Mapping like Body Shader, and use Diffuse Ramp which is Body Shader's. But Hair Shader should show shadow more certainly than Body Shader, so, I remapped normal dot light value with cubic function.

_Graph of Remapping Function_

![Alt text](/ExplainImgs/GraphOfRemappingFunction.png)

Since the graph is a trigeminal function graph with inflection at (0.5,0.5) and polar values at (0,1) and (1,0), the value changes rapidly in the middle, and the value changes less as you approach (0,1) and (1,0).

_code_
```hlsl
float f_x(float x)    //remapping Function f(x) = -2(x-0.5)^3 + 1.5(x-0.5) + 0.5
{
    return -2*pow(x-0.5,3)+1.5*(x-0.5) +0.5;
}
...
half4 frag (Varyings IN) : SV_Target
{
    ...
    //Shadowing & Retouching
    half4 shadow = SAMPLE_TEXTURE2D(_DiffuseRamp,sampler_DiffuseRamp,float2(f_x(ndl),0));    //remapping ndl and sampling diffuse map
    color = lerp(color,color * _ShadowColor,shadow);
    color = lerp(color,color * _ShadowColor,1-light.shadowAttenuation);
    
    return color;
}
```

_Left is Shader with remapping, and right is non remapping. After remapping, shadow have been more distinct._

<img src="/ExplainImgs/HairShaderRemapped.png" width="35%" height="30%"> <img src="/ExplainImgs/HairShaderWithoutRemaping.png" width="37%" height="37%">

### Unlit Eye Shader

Unlit Eye Shader use two material Eye Shader and Eyeline Shader. Both of two are very simple unlit shader with one texture.

![Alt text](/ExplainImgs/EyeShader.png)

### Shadow Casting

Shadow Casting in URP is built by Adding "Shadow Casting Pass" in shader. This is final color of Character.

![Alt text](/ExplainImgs/CharacterShadowCasting.png)

Character Controller Part
=========================
