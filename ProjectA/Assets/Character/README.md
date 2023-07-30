Character Shader Part
=====================
#### Character Shader is based on unity-chan, and written with hlsl because this project is on URP. Following is what I made to implement Illustrative NRP Shader
>  1. Skin Shader With Diffuse Wraping
>  2. Body Shader With Diffuse Wraping and Normal Map
>  3. Unlit Eye Shader
>  4. Hair Shader With Diffuse Wraping and Normal Map
>  5. Eyeline Shader

### Skin Shaer With Diffuse Wraping

To get Illustrative Shadow, I used Diffuse Wraping with normal Dot light and normal Dot view value. For this, I made a Diffuse Ramp. It mapped normal Dot light value at xAxis, and normal dot view value at yAxis. So that, the fragnent be illuminated by standard Lambert, and will brighten up that fragnent is nearer to Main Camera's view direction.

![Alt text](/ExplainImgs/SkinDiffuseRamp.png)

To use this, we can get smoother shading effect.

![Alt text](/ExplainImgs/SkinDiffuseWrapping.png) ![Alt text](/ExplainImgs/SkinSingleToonShading.png)
