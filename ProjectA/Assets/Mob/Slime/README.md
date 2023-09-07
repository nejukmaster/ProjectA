Slime
======
In this chapter, we will make mobs. The mob production process will first take on the mob's concept, modeling, and implement it in games. 

### Make a model of slime.
Create a model of Slime using Blender 3D. First, we create sphere, sculpting it.
![Alt text](/ExplainImgs/MobMaking1.png)
![Alt text](/ExplainImgs/MobMaking2.png)
![Alt text](/ExplainImgs/MobMaking3.png)
And then, use the Texture Paint mode, we create Texture to apply to model.
![Alt text](/ExplainImgs/MobMaking4.png)
And set frame and wieghts the mesh so the frame allows the mesh to move.
![Alt text](/ExplainImgs/MobMaking5.png)
![Alt text](/ExplainImgs/MobMaking6.png)
And import model in Unity.
![Alt text](/ExplainImgs/MobMaking7.png)
![Alt text](/ExplainImgs/MobMaking8.png)
### SlimeController
Then create a controller that moves the slime. Before writing the script, add the NavMesh Agent component to the slime object.
![Alt text](/ExplainImgs/MobMaking9.png)
The NavMesh Agent is an AI system provided by Unity that is useful for implementing movements such as moving toward a goal or avoiding obstacles on a baked NavMesh. 

Then We create a Slime Controller script that inherits the Mob Controller.
```c#
public class SlimeController : MobController
{
}
```
