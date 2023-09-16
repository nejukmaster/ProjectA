using UnityEngine;
using Unity.Netcode;
using UnityEditor;

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

    public void TestSpawn()
    {
        SpawnMob(0, 0);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MobSpawner))]
public class MobSpawnerEditor : Editor
{
    [SerializeField] Vector2 testSpawnIndexes;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MobSpawner spawner = (MobSpawner)target;

        if (GUILayout.Button("Test Spawn"))
        {
            spawner.SpawnMob(0, 0);
        }
    }
    private void OnSceneGUI()
    {
        Texture spawnPointTexture = Resources.Load("StageSpawnPoint") as Texture;
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
#endif

