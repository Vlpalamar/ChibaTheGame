using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour // что бы присоединится к текущей игре
{
    //ссылка на наш кастомный NetworkManager
    [SerializeField] private NetworkLobbyManager networkManager = null;

    //ссылки на UI обьекты, ничего интерессного
    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;

    //когда при активации обьекта на котором скрипт идет подписка и отписка на события 
    private void OnEnable()
    {
        NetworkLobbyManager.OnClientConnected += HandleClientConnected;
        NetworkLobbyManager.OnClientDisconected += HandleClientDisconnected;
    }
    private void OnDisable()
    {
        NetworkLobbyManager.OnClientConnected -= HandleClientConnected;
        NetworkLobbyManager.OnClientDisconected -= HandleClientDisconnected;
    }

   
    public void JoinLobby()
    {

        //пишем IP в InputField, куда хотим подключиться, потом если успеем будем думать как перевести игру на сервер, пока так 
        string ipAdress = ipAddressInputField.text;
        //получаем ip
        networkManager.networkAddress = ipAdress;
        //заходим в игру
        networkManager.StartClient();

        joinButton.interactable = false;

    }


    //дополнительные методы скрывающие/показывающие UI обьекты
    private void HandleClientConnected()
    {
        joinButton.interactable = true;
        this.gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }
    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;

    }

}
