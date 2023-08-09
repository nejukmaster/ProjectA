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
    
color = lerp(color,color * _ShadowColor,1-light.shadowAttenuation);    //Apply attenuation of light to affect the shadow.
return color;
```

Using this, we can get smoother shading effect. Left image is Skin Shader with Diffuse Wrapping, and right image is Skin shader with Single Step Toon Shading.

<img src="/ExplainImgs/SkinDiffuseWrapping.png" width="35%" height="30%"> <img src="/ExplainImgs/SkinSingleToonShading.png" width="30%" height="30%">

### Body Shader With Diffuse Wraping and Normal Map

The Clothes' Shader use Diffuse Wrapping as above. but, this shader is only use normal Dot light value. because this shader is not necessary to be shown smoothly as much as Skin Shader above. And the clothes should show wrinkles, So that, I apply normal mapping to shader. 

_Diffuse Map of Clothes_

![Alt text](/ExplainImgs/ClothesDiffuseRamp.png)

_Clothes Without Normal Mapping_

![Alt text](/ExplainImgs/ClothesWithoutNormal.png)

_Clothes With Normal Mapping_

![Alt text](/ExplainImgs/ClothesWithNormal.png)

### Hair Shader With Diffuse Wrapping and Normal Map

Hair Shader use Diffuse Warpping and Normal Mapping like Body Shader, and use Diffuse Ramp which is Body Shader's. But Hair Shader should show shadow more certainly than Body Shader, so, I remapped normal dot light value with cubic function.

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
