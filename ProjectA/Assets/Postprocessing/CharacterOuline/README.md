Character Outline
=================
The character outline effect plays a role in highlighting and highlighting the character in NPR. In particular, this project, which aims at rendering such as illustration, has focused on drawing the outline as naturally as possible. Accordingly, the target point for this outline shader has been set as follows
>  Depth Normal Outline
>
> Draw Character Only

To achieve this, I have set up an implementation plan as follows.
![Alt text](/ExplainImgs/ShaderImplementionPlanMap.png)

### Character Camera Setting

First, after dividing the character layers, set up a character camera that will render only the characters. Because I will render only the outline of the character. 

First, set the rendering layer. In Inspector > Layer > Add Layer, add Layer named "Character" and Assign to the character.
![Alt text](/ExplainImgs/AddCharacterLayer.png)
![Alt text](/ExplainImgs/AssignLayerToCharacter.png)

After that, create another camera as children of the main camera. And set it up as follows.
![Alt text](/ExplainImgs/AddCharacterCam.png)
![Alt text](/ExplainImgs/CharacterCamSettings.png)
Explain of Settings
Priority: Sets the rendering order of the camera. Set it to -1 to render before the main camera.
Depth Texture: Decide whether to use Depth Texture. I will use DepthNormal to draw the outline, so I will choose "On".
Culling Mask: Determines which layer to render. Let's choose the "Character" layer.

Now, CharacterCam has been set up to create a character outline mask. What we're going to do now is create an outline mask with CharacterCam's DepthNormal and a post-processing shader that applies it to the main camera.

### Outline Mask

What we are going to produce this time is a post-processing shader that creates an outline mask to be applied to the CharacterCam. This shader will take the Scene Depth Normal from CharacterCam and turn it into CharacterMask. 

First, receive and save Scene's Depth and Normal Texture. It also declares a variable that will store values compared to neighboring pixels.

```hlsl
float3 normal = SampleSceneNormals(IN.uv);
float depth = SampleSceneDepth(IN.uv);
float normalDifference = 0;
float depthDifference = 0;
```

Drawing an outline using Depth Normal is a technique that compares the current Depth value with the normal value with the surrounding pixels, and generates a darker outline as the difference increases. To do this, I'm going to use a simple algorithm called Sobel Filter.

Sobel Filter is one of the Edge detection algorithms, which is a method of multiplying the kernel of 3x3 by the original image to obtain an approximate value of the rate of change, usually expressed as follows.
![Alt text](/ExplainImgs/SobelOperator.png)
And this is a function of finding the outline using Sobel Filter.
```hlsl
void Compare(inout float depthOutline, inout float normalOutline,float2 uv) {

  float3x3 horizontalOutlineConv = {1,0,-1,
                                  2,0,-2,
                                  1,0,-1};    //Horizontal axis kernel
  float3x3 verticallOutlineConv = {1,2,1,
                                    0,0,0,
                                    -1,-2,-1};  //Vertical axis kernel

  //Horizontal Outline

  float depthDifferency_horizon = 0;
  float3 normalDifferency_horizon = 0;

  for(uint i = 0; i < 9; i ++){
    int x = i/3;
    int y = i%3;

    depthDifferency_horizon += horizontalOutlineConv[x][y] * SampleSceneDepth(uv + _MainTex_TexelSize.xy * float2(x-2,y-2));
    normalDifferency_horizon += horizontalOutlineConv[x][y] * SampleSceneNormals(uv + _MainTex_TexelSize.xy * float2(x-2,y-2));  //The normal and depth of the 3x3 area centered on the current pixel are sampled and multiplied by the kernel
  }

  depthDifferency_horizon = abs(depthDifferency_horizon);
  normalDifferency_horizon = abs(normalDifferency_horizon);  //Since it is to find the difference, not the displacement, it gives an absolute value.

  //Vertical Outline

  float depthDifferency_vert = 0;
  float3 normalDifferency_vert = 0;

  for(uint i = 0; i < 9; i ++){
    int x = i/3;
    int y = i%3;

    depthDifferency_vert += verticallOutlineConv[x][y] * SampleSceneDepth(uv + _MainTex_TexelSize.xy * float2(x-2,y-2));
    normalDifferency_vert += verticallOutlineConv[x][y] * SampleSceneNormals(uv + _MainTex_TexelSize.xy * float2(x-2,y-2));
  }

  depthDifferency_vert = abs(depthDifferency_vert);
  normalDifferency_vert = abs(normalDifferency_vert);

  depthOutline = (depthDifferency_vert + depthDifferency_horizon) / 2.0;   
  normalOutline = (normalDifferency_horizon.r + normalDifferency_horizon.g + normalDifferency_horizon.b + normalDifferency_vert.r + normalDifferency_vert.g + normalDifferency_vert.b)/6.0;  //The horizontal outline and the vertical outline are added to give the average.
}
```
_Apply to Fragment Shader_
```hlsl
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
```
_Outline Mask Rendered_

![Alt text](/ExplainImgs/OutlineMask.png)

### Character Outline Shader

Then, this Outline Mask should be applied to the Main Camera. In order to produce a post-processing shader for this, an Outline Mask is required, which will then be produced and processed by RenderFeature. Therefore, here, we write a shader to receive and process an Outline Mask.
```hlsl
Properties
{
  _MainTex("Texture", 2D) = "white" {}
  _OutlineColor("Outline Color", Color) = (0,0,0,1)
  _Outline("Outline", 2D) = "white" {}
}
```
This is the property of the Character Outline Shader that I'm going to write this time. Here, _MainTex will receive a temporary render texture from Main Camera, and _Outline Properties will receive an Outline Mask from CharacterCam. After that, it can be seen that the _Outline Color, which will determine the color of the outline, is also declared.
```hlsl
float SampleSceneOutline(float2 uv)
{
  return SAMPLE_TEXTURE2D_X(_Outline, sampler_Outline, UnityStereoTransformScreenSpaceTex(uv)).r;
}
...
half4 frag (v2f i) : SV_Target
{
  float outline = SampleSceneOutline(i.uv);
  half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    
  return lerp(color,_OutlineColor,outline);
}
```

### Scriptable Renderer Feature To Apply Post-Processing Shader

URP inherits and uses a class called Scriptable Render Feature when applying a postprocessing shader. The Scriptable Render Feature allows us to insert our Scriptable Render Pass into the rendering process of each camera.

