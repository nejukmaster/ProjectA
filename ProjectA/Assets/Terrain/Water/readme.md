Water Shader Part
=================
In this part, I will introduce my tries to make water more realistic and more cartoonic. The Following is passing through to goal. The table of contents is as follows.
> Getstner Wave
> Foam
> Water Fog
> Normal map
> Refraction effect

### Gestner Wave

Building wave is most important part to express water's movement. At First, I tried to apply y-axis sin wave to vertex. but it was not natural.

_This is shot of above. It's hard to call it a wave_
![Alt text](/ExplainImgs/WaveWithSin.png)

So, I Find a way, and search the "Gestner Wave" at GPU Gems(2004). It's the way of express Ocean Wave more realistic, that add the x/z movement to vertex as well as y axis.
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

_the application of water to the terrain._
![Alt text](/ExplainImgs/WaterWithoutFoam.png)
![Alt text](/ExplainImgs/WaterWithFoam.png)

### Water Fog

Water has different colors depending on the depth. WaterFog was created to implement this. It is composed by comparing depth and Scene Depth in the rest of the part other than Foam with the application of Foam.

 _Water Fog with Foam_
 ![Alt text](/ExplainImgs/WaterFogWithFoam.png)

 From the camera, you can see that the deeper the depth, the thicker the fog. If you have confirmed that it works properly, now, apply this Fog to the color and alpha.

 _Water applied Fog. It can be seen that the color darkens and the transparency decreases in the deep._
 ![Alt text](/ExplainImgs/WaterWithWaterFog.png)

 As a result, I was able to obtain a more realistic water texture.

 ### Normal Map

In order to express the curvature of the actual water on the plan, a shadow was expressed using a Normal Map. Normal Map used Noramap Texture included in Asset called Animated Water Texture in Asset Store, and shadows are expressed in simple Lambert Lighting using this normal.

_Water applied Normal Map. The curvature of the water was more emphasized._
![Alt text](/ExplainImgs/WaterWithNormal.png)

### Refraction effect

And the last thing to implement is the Refraction effect. First, a refractive texture must be created, but there was a disadvantage that if it was treated with gradient noise, it would look weird. 

_The result seems to be oil floating on the water._

<img src="/ExplainImgs/WaterRefractionWithGradientNoise.png" width="40%" height="40%">

To solve this problem, I tried to change pre-loaded Normal Map to grayscale and use it as a refractive map. To this, the sampled normal map pixels are grayscaleed and then multiplied by the _Scale property to adjust the degree of refraction. And Samples the Scene Opaque Texture with the obtained refraction plus uv. Then, the sampled Refraction Color and water color values are interpolated into the water's alpha values.

__Scale is Property to adjust refraction intensity_
![Alt text](/ExplainImgs/WaterShaderCodefragmentRefraction.png)
