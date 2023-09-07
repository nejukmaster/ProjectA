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
This allows you to refer to the Mob Controller regardless of the type of monster.

And then, implement the virtual method defined first on the Mob Controller by overriding it.
```c#
public class SlimeController : MobController
{
  //The method responsible for the mob's movement.
  protected override void Move()
  {
      GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
      if (players.Length > 0)  //Run only when the player is connected
      {
          //Locate the nearest player and set it to target.
          target = players[0];
          for (int i = 0; i < players.Length; i++)
          {
              if (Vector3.Distance(players[i].transform.position, transform.position) < Vector3.Distance(target.transform.position, transform.position))
              {
                  target = players[i];
              }
          }
          float distanceToNearest = Vector3.Distance(target.transform.position, transform.position);
          //Trace if the player is between "attack_range" and "detect_range".
          if (distanceToNearest <= status.GetStatus("detect_range") && distanceToNearest > status.GetStatus("attack_range"))
          {
              agent.SetDestination(target.transform.position);
          }
          //Attack if the player is closer than "attack_range".
          else if(distanceToNearest <= status.GetStatus("attack_range"))
          {
              agent.SetDestination(transform.position);
              animator.SetTrigger("Attack");
          }
          transform.LookAt(new Vector3(target.transform.position.x,transform.position.y,target.transform.position.z));
      }
      else  //Stop tracking if it is further than that.
      {
          agent.SetDestination(transform.position);
      }
      animator.SetBool("Move", Vector3.Distance(transform.position, agent.destination) > status.GetStatus("attack_range"));
  }

  public override void Damaged(float damage)
  {
      animator.SetTrigger("Damaged");  //Play animation on attack
      
  }
}
```
