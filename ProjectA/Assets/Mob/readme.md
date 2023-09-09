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

  public Vector3[] m_SpawnPoints;  //Array to store the mob's sponge points

  [SerializeField] GameObject[] m_Prefab;  //Mob Prefab Array

  public void SpawnMob(int mob_index, int spawnPoint_index)
  {
      NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(m_Prefab[mob_index], m_SpawnPoints[spawnPoint_index],Quaternion.identity);  //GetNetworkObject gets the mob of that prefab from the Pool.
      if(!obj.IsSpawned) obj.Spawn();  //Sponsor Mob. NetworkObjectPool causes registered NetworkObjects to be pulled out of the ObjectPool if sponged.
  }
}
```
And create Spawner's Custom Editor to facilitate SpawnPoint setup.
```c#
[CustomEditor(typeof(MobSpawner))]
public class MobSpawnerEditor : Editor
{

  public override void OnInspectorGUI()
  {
      base.OnInspectorGUI();

      MobSpawner spawner = (MobSpawner)target;

      string mob_index = GUILayout.TextField("Index of Mob");
      string spawnpoint_index = GUILayout.TextField("Index of Spawn Point");
      if (GUILayout.Button("Test Spawn"))
      {
          spawner.SpawnMob(0, 0);
      }
  }
  private void OnSceneGUI()
  {
      Texture spawnPointTexture = Resources.Load("Mob/StageSpawnPoint") as Texture;
      MobSpawner spawner = (MobSpawner)target;

      EditorGUI.BeginChangeCheck();
      Handles.color = Color.magenta;
      Vector3[] newPos = new Vector3[spawner.m_SpawnPoints.Length];
      for (int i = 0; i < newPos.Length; i++) {
          newPos[i] = Handles.PositionHandle(spawner.m_SpawnPoints[i], Quaternion.identity);
          Handles.Label(newPos[i], spawnPointTexture);
      }
      if (EditorGUI.EndChangeCheck())
      {
          Undo.RecordObject(spawner, "Changed Spawner GUI");
          for (int i = 0; i < newPos.Length; i++)
          {
              spawner.m_SpawnPoints[i] = newPos[i];
          }
      }
  }
}
```
