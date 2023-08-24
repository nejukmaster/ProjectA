Character Outline
=================
The character outline effect plays a role in highlighting the character in NPR. This project, which aims at rendering such as illustration, has particularly focused on drawing the outline as naturally as possible. Accordingly, the target points for this outline shader has been set as follows
>  Depth Normal Outline
>
> Draw Character Only

To achieve this, I have set up an implementation plan as the following.
![Alt text](/ExplainImgs/ShaderImplementionPlanMap.png)

### Character Camera Setting

As I wanted to render the outline of the character only, I divided the character layer, and set up a character camera.

First, set the rendering layer. In Inspector > Layer > Add Layer, add Layer named "Character" and Assign to the character.
![Alt text](/ExplainImgs/AddCharacterLayer.png)
![Alt text](/ExplainImgs/AssignLayerToCharacter.png)

After that, create another camera as children of the main camera. And set it up as the following.
![Alt text](/ExplainImgs/AddCharacterCam.png)
![Alt text](/ExplainImgs/CharacterCamSettings.png)

Explain of Settings

Priority: Set the rendering order of the camera. Set it to -1 to render before the main camera.

Depth Texture: Decide whether to use Depth Texture. Since I will use DepthNormal to draw the outline, so I will choose "On".

Culling Mask: Determines which layer to render. Let's choose the "Character" layer.

Now, CharacterCam has been set up to create a character outline mask. What we're going to do now is creating an outline mask with CharacterCam's DepthNormal and a post-processing shader that applies it to the main camera.

### Outline Mask

What we are going to produce this time is a post-processing shader that creates an outline mask to be applied to the CharacterCam. This shader gets the Scene Depth Normal from CharacterCam and turn it into Outline Mask. 

This shader receives and saves Scene's Depth and Normal Texture. It also declares a variable which stores values compared to neighboring pixels.

```hlsl
float3 normal = SampleSceneNormals(IN.uv);
float depth = SampleSceneDepth(IN.uv);
float normalDifference = 0;
float depthDifference = 0;
```

Drawing an outline using Depth Normal is a technique that compares the current Depth value and normal value with the surrounding pixels, and generates a darker outline as the difference increases. To do this, I'm going to use a simple algorithm called Sobel Filter.

Sobel Filter is one of the Edge detection algorithms, which is a method of multiplying the kernel of 3x3 by the original image to obtain an approximate value of the rate of change.

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

Then, this Outline Mask should be applied to the Main Camera. In order to produce a post-processing shader for this, an Outline Mask is required to be produced and processed by RenderFeature. Therefore, we write a shader which receives and processes an Outline Mask.
```hlsl
Properties
{
  _MainTex("Texture", 2D) = "white" {}
  _OutlineColor("Outline Color", Color) = (0,0,0,1)
  _Outline("Outline", 2D) = "white" {}
}
```
This is the property of the Character Outline Shader that I'm going to write this time. Here, _MainTex will receive a temporary render texture from Main Camera, and _Outline Properties will receive an Outline Mask from CharacterCam. After that, it can be seen that the _Outline Color, which will determine the color of the outline. Now, the _OutlineColor is declared.

_Apply in fragment shader_
```hlsl
float SampleSceneOutline(float2 uv)
{
  return SAMPLE_TEXTURE2D_X(_Outline, sampler_Outline, uv).r;
}
...
half4 frag (v2f i) : SV_Target
{
  float outline = SampleSceneOutline(i.uv);
  half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    
  return lerp(color,_OutlineColor,outline);
}
```
All we have to do is to provide an Outline Mask to the _Outline property of the material associated with this shader.

### Scriptable Renderer Feature To Apply Post-Processing Shader

URP inherits and uses a class called Scriptable Render Feature when applying a postprocessing shader. The Scriptable Render Feature allows us to insert our Scriptable Render Pass into the rendering process of each camera. 

First, write a Scriptable Render Pass. Declare a colorBuffer to store temporary textures and a shaderBuffer to store the post-processed textures. It also declares the ShaderPropertyID to access this shaderBuffer.
```hlsl
class CustomRenderPass : ScriptableRenderPass
{
  private RenderTargetIdentifier colorBuffer, shaderBuffer;
  private int shaderBufferID = Shader.PropertyToID("_ShaderBuffer");
```
And, Creates a constructor with various properties. The description of each property is annotated.
```hlsl
private Material material;          //Specify the material associated with the postprocessing shader.
private Material outlineMapping;    //Specify the material of the shader that creates the outline mask
private RenderTexture outlineMap;   //Custom Lander Texture to Store OutlineMask
public CustomRenderPass(CustomPassSettings settings) : base()
{
  this.renderPassEvent = settings.renderPassEvent;
  this.material = settings.mat;
  this.outlineMapping = settings.outlineMapping;

  ConfigureInput(ScriptableRenderPassInput.Normal);
}
```
The next step is to complete the necessary settings in OnCameraSetup.
```hlsl
//OnCameraSetup runs before rendering the camera.
public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
{
  //Lander DepthNormal on camera.
  renderingData.cameraData.camera.depthTextureMode = DepthTextureMode.DepthNormals;
  //Connect the rendering screen of the current camera to the colorBuffer.
  colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
  //Gets the RenderTextureDescriptor from the current camera to use when creating the rendering texture.
  RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

  //Gets the current temporary render texture and binds it to shaderBufferID.
  cmd.GetTemporaryRT(shaderBufferID, descriptor, FilterMode.Point);
  shaderBuffer = new RenderTargetIdentifier(shaderBufferID);
}
```
Then We should Execute this pass. At this time, the outline mask is configured and mapped. Since we use two cameras, it is necessary to separate them and apply different processes. They are distinguished by using the Tag function to Unity. The following is an Execute block that divides each camera into tags and configures an Outline Mask in the character cam, and applies it in the main camera.
```hlsl
//The Execute block is executed when this Pass is actually executed.
public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
{
  //Get one new CommandBuffer from the CommandBufferPool.
  CommandBuffer cmd = CommandBufferPool.Get();

  //Create a new CustomRenderTexture if there is no texture in the Outline Map.
  if (outlineMap == null) outlineMap = new CustomRenderTexture(renderingData.cameraData.camera.pixelWidth, renderingData.cameraData.camera.pixelHeight);

  //Main camera rendering
  if (renderingData.cameraData.camera.CompareTag("MainCamera"))
  {
    //ProfilingScope makes it easier to debug by making the rendering process visible in Unity Profiler.
    using (new ProfilingScope(cmd, new ProfilingSampler("Character Outline Pass")))
    {
      //Saves the generated OutlineMask to the Material's _OutlineMap property.
      material.SetTexture("_Outline", outlineMap);
      //After that, apply the material that draws the outline of the character created earlier and blit the camera.
      cmd.Blit(colorBuffer, shaderBuffer, material);
      cmd.Blit(shaderBuffer, colorBuffer);
    } 
  }
  //Character Camera Rendering
  else if (renderingData.cameraData.camera.CompareTag("CharacterCam"))
  {
    using (new ProfilingScope(cmd, new ProfilingSampler("Outline Mapping")))
    {
      //Release the previously used Outline Mask texture.
      outlineMap.Release();
      //Blit the CharacterCam by applying the shader that creates the Outline Mask.
      cmd.Blit(colorBuffer, shaderBuffer, outlineMapping);
      cmd.Blit(shaderBuffer, colorBuffer);
      //Saves the generated Outline Mask to the Outline Map.
      cmd.Blit(colorBuffer, outlineMap);
    }
  }
  //Run the CommandBuffer you created earlier.
  context.ExecuteCommandBuffer(cmd);
  //Release the used CommandBuffer.
  CommandBufferPool.Release(cmd);
}
```
Finally, it releases the resources used by the OnCleanupCamera block.
```hlsl
  public override void OnCameraCleanup(CommandBuffer cmd)
  {
    if (cmd == null) throw new System.ArgumentNullException("cmd");
    cmd.ReleaseTemporaryRT(shaderBufferID);
  }
}
```
Now, the rendering pass is complete. All we have to do is to configure Scriptable RenderFeature to apply this rendering pass. It is very easy and simple.

First, it inherits Scriptable RenderFeature to the script, and for convenience, it creates a modified CustomPassSetting class in the inspector.
```hlsl
public class CharacterOutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CustomPassSettings
    {
        //RenderPassEvent determines when RenderPass runs.
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        //Material to draw the outline
        public Material mat;
        //Material to create an outline mask
        public Material outlineMapping;
    }
```
Then, in AddRenderPass, insert the RenderPass we created into the rendering pipeline.
```hlsl
  private CustomRenderPass m_ScriptablePass;
    [SerializeField] CustomPassSettings m_Settings;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(m_Settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
```
Finally, add the feature you just created to the Universal Render Data you are currently using.
![Alt text](/ExplainImgs/InsertRendererFeatureAndSetting.png)
### Result
![Alt text](/ExplainImgs/OutlineResult.png)
It can be seen that the outline is applied only to the character and not to the terrain behind it.
