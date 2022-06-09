using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

//ПЕРЕПИСАТЬ
public class GameSpawnSystem : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab = null;
    public GameObject PlayerPrefab
    {
        set { playerPrefab = value; }
        get { return playerPrefab; }
    }

    [Header("UI")]
    [SerializeField] private  GameObject heroPanel=null;
    

    private static List<Transform> spawnPoints = new List<Transform>();

    private int newxtIndex = 0;

    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();

    }

  

    public static void RemoveSpawnPoint(Transform transform) { spawnPoints.Remove(transform); }
    
    //когда обьект начинает существовать подписываем его на событие
    public override void OnStartServer() { NetworkLobbyManager.onServerReadied += ChooseHero; }

    //когда обьект перестает существовать- отписываем от событий
    [ServerCallback]
    private void OnDestroy() => NetworkLobbyManager.onServerReadied -= SpawnPlayer;

    public override void OnStartClient()
    {
        GameObject hdd = Instantiate(heroPanel.gameObject);

        hdd.SetActive(true);

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



    public void Select(int id)
    { 
        GameObject spawnSystem = GameObject.Find("SpawnSystem(Clone)");
        spawnSystem.GetComponent<GameSpawnSystem>().CmdSelect(id);
        GameObject panel = GameObject.Find("ChooseHero(Clone)");
        panel.SetActive(false);


    }

    [Command(requiresAuthority = false)]
    public void CmdSelect(int CharracterIndex, NetworkConnectionToClient conn = null)
    {
        int i = 0;
        foreach (var prefab in Room.spawnPrefabs)
        {
            //if (prefab.gameObject.GetComponent<Hero>())
            //{
            //    if (i == CharracterIndex)
            //    {
            //        playerPrefab = prefab;
            //        SpawnPlayer(conn);
            //        // heroPanel.SetActive(false);
            //        return;

            //    }

            //    i++;
            //}


        }
    }


    [Server]
    public void ChooseHero(NetworkConnection conn)
    {
        //GameObject hdd = Instantiate(heroPanel.gameObject);

        //hdd.SetActive(true);
        
        //NetworkServer.Spawn(hdd, conn);
       // SpawnPlayer(conn); работает 


    }

    //сервер выполняет метод у каждого из клиентов
    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        //если бы был  if(!isLocalPlayer) return; ничего не вызвалось бы потому что оно в локал плэер не заходит даже с хоста изза отметки [Server] 
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(newxtIndex);
        if (spawnPoint==null)
        {
            Debug.LogError($"Missing spawn point for player{newxtIndex}");
                return;
        }

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[newxtIndex].position, spawnPoints[newxtIndex].rotation);
        newxtIndex++;
        if (newxtIndex>spawnPoints.Count) newxtIndex = 0;
            //спавнит gameobject для всех клиентов который ready то есть,
        //кидает localPlayer на этот обьект по соединению
        //и каждый игрок его видит 
        NetworkServer.Spawn(playerInstance,conn);
        //сервер уничтожает старый обькт нашего соединения (localPlayer)
        NetworkServer.Destroy(conn.identity.gameObject);
        //local player у каждого клиента может быть только один, он связан с игровым обьектом
        //с помощью этого метода мы перемещаем localPlayer в нужный обьект (если я правильно понял)
        NetworkServer.ReplacePlayerForConnection(conn, playerInstance.gameObject, true);
        

    }

}
