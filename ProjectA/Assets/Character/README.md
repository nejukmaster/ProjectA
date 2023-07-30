Character Shader Part
=====================
#### Character Shader is based on unity-chan, and written with hlsl because this project is on URP. Following is what I made to implement Illustrative NRP Shader
>  1. Skin Shader With Diffuse Wraping
>  2. Body Shader With Diffuse Wraping and Normal Map
>  3. Unlit Eye Shader
>  4. Hair Shader With Diffuse Wraping and Normal Map
>  5. Eyeline Shader

### Skin Shader With Diffuse Wraping

To get Illustrative Shadow, I used Diffuse Wrapping with normal Dot light and normal Dot view value. For this, I made a Diffuse Ramp. It mapped normal Dot light value at xAxis, and normal dot view value at yAxis. So that, the fragnent be illuminated by standard Lambert, and will brighten up that fragnent is nearer to Main Camera's view direction.

![Alt text](/ExplainImgs/SkinDiffuseRamp.png)

To use this, we can get smoother shading effect. Left image is Skin Shader with Diffuse Wrapping, and right image is Skin shader with Single Step Toon Shading.

<img src="/ExplainImgs/SkinDiffuseWrapping.png" width="35%" height="30%"> <img src="/ExplainImgs/SkinSingleToonShading.png" width="30%" height="30%">

### Body Shader With Diffuse Wraping and Normal Map

The Clothes's Shader use Diffuse Wrapping as above. but, this shader is only use normal Dot light value. because this shader is not necessary to be shown smoothly as much as Skin Shader above. And the clothes should show wrinkles, So that, I apply normal mapping to shader. 

![Alt text](/ExplainImgs/ClothesDiffuseRamp.png)

_Diffuse Map of Clothes_

![Alt text](/ExplainImgs/ClothesWithoutNormal.png)

_Clothes Without Normal Mapping_

![Alt text](/ExplainImgs/ClothesWithNormal.png)

_Clothes With Normal Mapping_
