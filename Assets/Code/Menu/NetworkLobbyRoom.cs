using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// сущность игрока в Лобби
public class NetworkLobbyRoom : NetworkBehaviour
{
    
    [Header("UI")]
    [SerializeField] private  GameObject lobbyUI= null;
   


    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
    [SerializeField] private Button startButton = null;




    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady= false;

    private bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;
            startButton.gameObject.SetActive(value); 
        }
    }

    private NetworkLobbyManager room;

    private NetworkLobbyManager Room
    {
        get
        {
            if (room != null) return room;
            return room = NetworkManager.singleton as NetworkLobbyManager;

        }
    }

    //This is invoked on behaviours that have authority,
    //based on context and the LocalPlayerAuthority
    //value on the NetworkIdentity.
    //This is called after OnStartServer and OnStartClient.
    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerNameInput.DisplayName);
        lobbyUI.SetActive(true);
    }

    public override void OnStartClient( )
    {
        Room.RoomPlayers.Add(this);
        UpdateDisplay();
        // RpcAddButtons(); переделать 
       // CmdAddButtons();
    }

   


    public override void OnStopClient()
    {
        Room.RoomPlayers.Remove(this);
        UpdateDisplay();
    }

    //обновить дисплей, что бы у всех
    //пользователей показывало кто готов, кто подключился 
    public void UpdateDisplay()
    {//не понял зачем это скорее всего, что бы у каждого вызывалось индивидуально
        if (!hasAuthority)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;

                }
            }
            return;

        }

        //обновляет дисплей
        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player ...";
            playerReadyTexts[i].text = string.Empty;
        }
        //ставит где что нужно 
        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady?
                "<color=green>Ready</color>":
                "<color=red>Not Ready</color>";
        }
    }


    //дополнительный метод который говорит хосту можно ли начинать 
    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) return;

        startButton.interactable = readyToStart;
    }

    //сказать серверу, что это твое имя 
    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        //имя в лобби 
        DisplayName = displayName;
    }

    //метод который вызывается по клику на кнопку 
    //показать что ты(localPlayer) готов
    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;
        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayers[0].connectionToClient !=connectionToClient){return;}
        
        Room.StartGame();
    }



    //методы которые закидываются в hook(аргумент SyncVar)
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
}
