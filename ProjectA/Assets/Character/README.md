Character Shader Part
=====================
#### Character Shader is based on unity-chan, and written with hlsl because this project is on URP. Following is what I made to implement Illustrative NRP Shader
>  1. Skin Shader With Diffuse Wraping
>  2. Body Shader With Diffuse Wraping and Normal Map
>  3. Unlit Eye Shader
>  4. Hair Shader With Diffuse Wraping and Normal Map
>  5. Eyeline Shader

### Skin Shaer With Diffuse Wraping

To get Illustrative Shadow, I used Diffuse Wraping with normal Dot light and normal Dot view value. For Illustrative Shadow, I made a Diffuse Ramp. It mapped normal Dot light value at xAxis, and normal dot view value at yAxis. So that, the fragnent be illuminated by standard Lambert, and will darker that fragnent is farher at Main Camera's view direction.

![Alt text](/Assets/Character/Skin/Diffuse.png)
