Mob System
=====
In this part, we will build monsters system that will be placed in the open world field. Features to be implemented are as follows.

> Mob Spawning
>
> Mob Controller

### Mob Spawner
MobSpawner scripts to handle monsters' spawn use NetworkObjectPool. The code for NetworkObjectPool can be found in [Unity Multiplayer Networking - ObjectPool](https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/object-pooling/index.html). This code can manage Spawn and Despawn by generating registered objects when the server starts and activating only as much as necessary.
MobSpawner takes the single tone of this NetworkObjectPool and implements Monster Sponning.
```c#
public class MobSpawner : MonoBehaviour
{

  public Vector3[] m_SpawnPoints;

  [SerializeField] GameObject[] m_Prefab;
  private const int max_prefab = 5;

  public void SpawnMob(int mob_index, int spawnPoint_index)
  {
      NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(m_Prefab[mob_index], m_SpawnPoints[spawnPoint_index],Quaternion.identity);
      if(!obj.IsSpawned) obj.Spawn();
  }
}
```
