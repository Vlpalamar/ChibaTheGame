using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// сущность игрока в игре
public class NetworkLobbyGame : NetworkBehaviour
{

    [SyncVar] public string DisplayName = "Loading...";

    private NetworkLobbyManager room;

    private NetworkLobbyManager Room
    {
        get
        {
            if (room != null) return room;
            return room = NetworkManager.singleton as NetworkLobbyManager;

        }
    }


    public override void OnStartClient()
    {
        DontDestroyOnLoad(this.gameObject);
        Room.GamePlayers.Add(this);

    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);

    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.DisplayName = displayName;
    }

}
