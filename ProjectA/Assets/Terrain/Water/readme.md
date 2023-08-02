Water Shader Part
=================
In this part, I will introduce my tries to make water more realistic and more cartoonic. The Following is passing through to goal.

### Gestner Wave

Building wave is most important part to express water's movement. At First, I tried to apply y-axis sin wave to vertex. but it was not natural.

_This is shot of above. It is unnatural to be called "Wave"_
![Alt text](/ExplainImgs/WaveWithSin.png)

So, I Find a way, and search the "Gestner Wave" at GPU Gems(2004). It's the way of express Ocean Wave more realistic, that add the x/z movement to vertex as well as y axis. So that, I could get below movement of Plane.

_Gestner Applied._
![Alt text](/ExplainImgs/WaveWithGestner.png)
