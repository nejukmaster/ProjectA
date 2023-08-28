using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Status
{
    Dictionary<string,float> status = new Dictionary<string,float>();

    public Status(Dictionary<string,float> status) 
    {
        this.status = status;
    }

    public void AddStat(KeyValuePair<string,float> b)
    {
        if (status.ContainsKey(b.Key))
            status[b.Key] += b.Value;
    }
    
    public void SetStat(KeyValuePair<string,float> b)
    {
        if(status.ContainsKey(b.Key))
            status[(b.Key)] = b.Value;
    }

    public void InstanceStat(KeyValuePair<string,float> b)
    {
        if (!status.ContainsKey(b.Key))
            status.Add(b.Key,b.Value);
    }

    public float GetStat(string key)
    {
        return status[key];
    }
}
public class StatusManager : NetworkBehaviour
{
    public static StatusManager instance;

    Dictionary<ulong,Status> playerStatusDic = new Dictionary<ulong,Status>();

    private void Start()
    {
        instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void InstanceStatusServerRpc(ulong playerId)
    {
        if (!playerStatusDic.ContainsKey(playerId))
        {
            Dictionary<string, float> status = new Dictionary<string,float>();
            status.Add("hp", 1.0f);
            status.Add("attackRange", 100.0f);
            status.Add("max_hp", 1.0f);
            playerStatusDic.Add(playerId, new Status(status));
        }
    }

    public void ChangeStatus(ulong playerId, string s, float v)
    {
        if(playerStatusDic.ContainsKey(playerId))
        {
            playerStatusDic[playerId].SetStat(new KeyValuePair<string, float>(s,v));
            NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStatus>().StatusChangeClientRpc(s, v);
        }
    }

    public void AddStatus(ulong playerId, string s, float v)
    {
        if(playerStatusDic.ContainsKey(playerId))
        {
            playerStatusDic[playerId].AddStat(new KeyValuePair<string, float>(s, v));
            NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStatus>().StatusChangeClientRpc(s, playerStatusDic[playerId].GetStat(s));
        }
    }

    public Status GetStatus(ulong playerId)
    {
        if (playerStatusDic.ContainsKey(playerId))
        {
            return playerStatusDic[playerId];
        }
        else
            return null;
    }
    [ServerRpc]
    public void RemoveStatusServerRpc(ulong playerId) 
    {
        if(playerStatusDic.ContainsKey(playerId))
        {
            playerStatusDic.Remove(playerId);
        }
    }
}

[CustomEditor(typeof(StatusManager))]
public class StatusManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

