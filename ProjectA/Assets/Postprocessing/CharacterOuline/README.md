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

### Scriptable Renderer Feature To Apply Post-Processing Shader

URP inherits and uses a class called Scriptable Render Feature when applying a postprocessing shader. The Scriptable Render Feature allows us to insert our Scriptable Render Pass into the rendering process of each camera.
