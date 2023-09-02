using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Status
{
    Dictionary<string,int> status = new Dictionary<string,int>();

    public Status(Dictionary<string,int> status) 
    {
        this.status = status;
    }

    public void AddStat(KeyValuePair<string,int> b)
    {
        if (status.ContainsKey(b.Key))
            status[b.Key] += b.Value;
    }
    
    public void SetStat(KeyValuePair<string,int> b)
    {
        if(status.ContainsKey(b.Key))
            status[(b.Key)] = b.Value;
    }

    public void InstanceStat(KeyValuePair<string,int> b)
    {
        if (!status.ContainsKey(b.Key))
            status.Add(b.Key,b.Value);
    }

    public int GetStat(string key)
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
            Dictionary<string, int> status = new Dictionary<string,int>();
            status.Add("hp", 10);
            status.Add("attackRange", 100);
            status.Add("max_hp", 10);
            playerStatusDic.Add(playerId, new Status(status));
        }
    }

    public void ChangeStatus(ulong playerId, string s, int v)
    {
        if(playerStatusDic.ContainsKey(playerId))
        {
            playerStatusDic[playerId].SetStat(new KeyValuePair<string, int>(s,v));
            if(s == "hp") 
            {
                NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStatus>().HPChangeClientRpc(playerStatusDic[playerId].GetStat("hp"), playerStatusDic[playerId].GetStat("max_hp"));
            }
        }
    }

    public void AddStatus(ulong playerId, string s, int v)
    {
        if(playerStatusDic.ContainsKey(playerId))
        {
            playerStatusDic[playerId].AddStat(new KeyValuePair<string, int>(s, v));
            if (s == "hp")
            {
                NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<PlayerStatus>().HPChangeClientRpc(playerStatusDic[playerId].GetStat("hp"), playerStatusDic[playerId].GetStat("max_hp"));
            }
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

