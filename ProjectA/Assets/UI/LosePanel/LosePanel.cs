using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LosePanel : MonoBehaviour
{
    public static LosePanel instance;

    public PlayerController player;

    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] GameObject losePanel;

    private void Start()
    {
        instance = this;
    }

    public void SetVisible(bool visible)
    {
        losePanel.SetActive(visible);
    }

    public void Respawn()
    {
        player.RespawnCharacterServerRpc();
    }
}
