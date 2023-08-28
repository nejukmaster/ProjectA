Water Shader Part
=================
In this part, I will introduce my tries to make water more realistic and more cartoonic. The following is the table of contents.
> Getstner Wave
> 
> Foam
> 
> Water Fog
> 
> Normal map
> 
> Refraction effect

### Gestner Wave

Building wave is one of the most important part to express water's movement. At First, I tried to apply y-axis sin wave to vertex but it was not natural.

_This is shot of above. It's hard to call it a wave_
![Alt text](/ExplainImgs/WaveWithSin.png)

So, I found a way, and searched the "Gestner Wave" at GPU Gems(2004). It's the way of express Ocean Wave more realistic, that adds the x/z movement to vertex as well as y axis.

![Alt text](/ExplainImgs/EquationOfGestnerWave.jpg)

z is a sine wave value, and x and y are cosine values for each axis. In this equation, D is a two-dimensional vector representing the direction of the wave, A represents the amplitude of the wave, w represents the frequency, Q represents the steepness of the wave, and finally t represents the time. 

_hlsl code implemented above_
```hlsl
//Gerstner Wave vertex
float3 ori = IN.positionOS.xyz;
ori.x += _Sharpness * _Direction.x * cos(dot(_Frequency * _Direction.xy,IN.positionOS.xz)+ _Speed * 4 / _Frequency * _Time.x)/(_Frequency*_WaveNum);
ori.z += _Sharpness * _Direction.y * cos(dot(_Frequency * _Direction.xy,IN.positionOS.xz)+ _Speed * 4 / _Frequency * _Time.x)/(_Frequency*_WaveNum);
ori.y = _Amply * sin(dot(_Frequency * _Direction.xy,IN.positionOS.xz) + _Speed * 4 / _Frequency * _Time.x);
    
IN.positionOS.xyz = ori;
```

_Gestner Applied._
![Alt text](/ExplainImgs/WaveWithGestner.png)

### Foam

Foam means the expression of waves breaking off the coast. It is obtained by comparing the Scene Depth value with the depth value of the current fragment. Shader then creates a Foam at the interface where water and other objects meet. 

_code of foam implemention_
```hlsl
 float WaterDepthFade(float Depth, float4 ScreenPosition, float Distance)
{
    return (Depth-ScreenPosition.w)/(Distance*2);
}

...

//Foam
float2 screenUVs = IN.screenPos.xy / IN.screenPos.w;
float zRaw = SampleSceneDepth(screenUVs);
float zEye = LinearEyeDepth(SampleSceneDepth(screenUVs), _ZBufferParams);
float foam = WaterDepthFade(zEye, IN.screenPos, _Foam.x);
float foamValue = step(foam,_Foam.z);
color = lerp(color, _FoamColor, foamValue);
```
First, obtain the screen uv from the screen coordinates and then sample the Scene Depth. Subsequently, the sampled depth value is converted to the world scale through the LinearEye Depth macro. The world scale value is then mapped by subtracting the missing value of the current pixel from the sampled depth value and dividing it by the length of foam to be rendered. Here, _Foam is property that stores (amount, size, cut-off) information.

_the application of water to the terrain._
![Alt text](/ExplainImgs/WaterWithoutFoam.png)
![Alt text](/ExplainImgs/WaterWithFoam.png)

### Water Fog

Water has different colors depending on the depth. WaterFog was created to implement this. It is made by comparing depth and Scene Depth, and it is applied to the rest of the part that Foam hasn't been applied.
  
 _Water Fog with Foam_
 ![Alt text](/ExplainImgs/WaterFogWithFoam.png)

 As you can now see on the camera, the deeper the depth, the thicker the fog is. If you confirm that it works properly, now, apply this Fog to the color and alpha.

 ```hlsl
//Water Fog
float waterFog= zEye - IN.screenPos.w;
waterFog = _FogIntensity * waterFog;
waterFog = pow(waterFog,_FogBias);
color.rgb *= pow(_Color2.rgb,waterFog);
color.a = color.a + waterFog;
```

 _Water applied Fog. It can be seen that the color darkens and the transparency decreases in the deep._
 ![Alt text](/ExplainImgs/WaterWithWaterFog.png)

 As a result, I was able to obtain a more realistic water texture.

 ### Normal Map

In order to express the curvature of the actual water on the plan, a shadow was expressed by using a Normal Map. Normal Map used Noramap Texture which is included in Asset called Animated Water Texture in Asset Store, and shadows are expressed in simple Lambert Lighting using this normal.

_Water applied Normal Map. The curvature of the water was more emphasized._
![Alt text](/ExplainImgs/WaterWithNormal.png)

### Refraction effect

And the last thing to implement is the Refraction effect. First, a refractive texture must be created, but there was a disadvantage, that if it was created with gradient noise, it looked weird. 

_It looked like oil floating on the water._

<img src="/ExplainImgs/WaterRefractionWithGradientNoise.png" width="40%" height="40%">

To solve this problem, I tried to change pre-loaded Normal Map to grayscale, and used it as a refractive map. The sampled normal map pixels are grayscaleed and then multiplied by the _Scale property, and they are adjusted to the degree of refraction. And then sample the Scene Opaque Texture with the obtained refraction plus uv. Now the sampled Refraction Color and water color values are interpolated by the water's alpha values.

__Scale is Property to adjust refraction intensity_
```hlsl
//Refraction
float refractionmap = bump.r * 0.299 + bump.g * 0.587 + bump.b * 0.114;
refractionmap *= _Scale;
half3 refractionColor = SampleSceneColor(screenUVs + refractionmap);
color = lerp(half4(refractionColor,1), color, color.a);
```

_Refraction Applied_
![Alt text](/ExplainImgs/WaterWithRefraction.png)

### Stencil

The stencil buffer allows water to be rendered only on top of the terrain.
_Stencil Receive in Water Shader_
```hlsl
Stencil{
    Ref [_StencilRef]
    Comp Equal
}
```
_Stencil Write in Terrain Shader_
```hlsl
Stencil{
    Ref [_StencilRef]
    Comp Always
    Pass Replace
}
```
_Stencil Applied_
![Alt text](/ExplainImgs/WaterWithStencil.png)

### Final
Use two water objects to construct more realistic water shading. This is a finished copy for my water shader.

![Alt text](/ExplainImgs/WaterComplete.png)
