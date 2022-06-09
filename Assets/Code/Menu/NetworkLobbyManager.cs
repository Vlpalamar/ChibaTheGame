using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;
using UnityEngine.SceneManagement;


//наш кастомный NetworkManager
public class NetworkLobbyManager : NetworkManager //кастомный NetworkManager
{
    //ссылка на нужную ссцену
    [SerializeField] private int minPlayers = 2;
     

    [Scene]
    [SerializeField] private  string menuScene= string.Empty;

    [Scene]
    [SerializeField] private string gameScene = string.Empty;

    //сылка на коммнату 
    [Header("Room")]
    [SerializeField] private  NetworkLobbyRoom roomPlayerPrefab= null;

    [Header("Game")]
    [SerializeField] private NetworkLobbyGame gamePlayerPrefab= null;
    [SerializeField] private GameObject playerSpawnSystem= null;

    public GameObject PlayerSpawnSystem
    {
        get { return playerSpawnSystem; }
    }
    public static event Action OnClientConnected;// ивент, что должно сработать когда кто то подключаеться 
    public static event Action OnClientDisconected;//ивент, что должно сработать когда кто то отключаеться  
    public static event Action<NetworkConnection> onServerReadied;//

    public List<NetworkLobbyRoom> RoomPlayers { get; } = new List<NetworkLobbyRoom>();
    public List<NetworkLobbyGame> GamePlayers { get; } = new List<NetworkLobbyGame>();


    private const string SpawnablePrefabsFolderName = "SpawnablePregabs"; // название папки ресурсов с префабами 
 
    //если я правильно понял, что бы пользоватся префабами и что б их было видно всем их нужно сначало зарегистрировать в старте мы это и делаем
    //берем префабы сразу из папки
    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>(SpawnablePrefabsFolderName).ToList();
       
    }

    //срабатывает когда загружается клиент
    public override void OnStartClient()
    {

        var spawnablePrefabs = Resources.LoadAll<GameObject>(SpawnablePrefabsFolderName);
      
        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }

       
    }

    // если правильно понимаю, мы не вызываем метод, он сам срабатывает внутри оригинального NetworkManager срабатывает когда клиент подключается
    // мы туда дописали, что бы события выпонились  
    public override void OnClientConnect( )
    {
        
        base.OnClientConnect();
        //вызывает все подписанные события, не особо понимаю зачем это нужно в данной  реализации, возможно в  дальнейшем пригодится 
        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        OnClientDisconected?.Invoke();
    }

    //когда на сервер подключается игрок, работаем с ним через соединение
    public override void OnServerConnect(NetworkConnection conn)
    {
        //если лобби полное выкинуть его нахуйЯ
        if (numPlayers>=maxConnections)
        {
            conn.Disconnect();
            return;
        }

        //если игра уже началась нельзя заходить
        if ((SceneManager.GetActiveScene().path!=menuScene))
        {
            conn.Disconnect();
            return;
        }

    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity !=null)
        {
            //identity = обьект по подключению (conn) ->берем его скрипт NetworkLobbyGame
            var player = conn.identity.GetComponent<NetworkLobbyRoom>();
            
            //убрать его с лобби
            RoomPlayers.Remove(player);

             NotifyPlayersOfReadyState();
        }
        base.OnServerDisconnect(conn);
    }

    //создать лобби
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        //если игра еще не началась path - путь + имя как и в ссылке menuScene
        if (SceneManager.GetActiveScene().path==menuScene)
        {
            Debug.Log("RoomInstantiated");
            bool isLeader = RoomPlayers.Count == 0; //кто первый подключился(создал лобби) тот и хост(лиддер)
            
            //создаем комнату для игроков 
            NetworkLobbyRoom lobbyRoomInstance = Instantiate(roomPlayerPrefab);
            
            lobbyRoomInstance.IsLeader = isLeader;
            //говорим серверу добавить этого человека
            NetworkServer.AddPlayerForConnection(conn, lobbyRoomInstance.gameObject);
           
        }
        
    }


    public override void OnStopServer()
    {
        RoomPlayers.Clear();
    }

   //сообщить хосту, можно ли начинать игру 
    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }
    //проверка может хост начать игру 
    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) return false;
        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) return false;
        }

        return true;


    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().path==menuScene)
        {
            if (!IsReadyToStart()) return;
            ServerChangeScene(gameScene);
                
            
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        //Frome Meny to Scene
        if (SceneManager.GetActiveScene().path==menuScene && newSceneName==gameScene)
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                //получаем соединение
                var conn = RoomPlayers[i].connectionToClient;
                //создаем обьект из префаба
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);
                
                gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                //сервер уничтожает старый обькт нашего соединения (localPlayer)
                NetworkServer.Destroy(conn.identity.gameObject);
                //local player у каждого клиента может быть только один, он связан с игровым обьектом
                //с помощью этого метода мы перемещаем localPlayer в нужный обьект (если я правильно понял)
                NetworkServer.ReplacePlayerForConnection(conn,gamePlayerInstance.gameObject,true);
            }
        }
        base.ServerChangeScene(newSceneName);
        
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.StartsWith(gameScene))
        {
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem.gameObject);
            NetworkServer.Spawn(playerSpawnSystemInstance);
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        onServerReadied?.Invoke(conn);
       
    }
}
