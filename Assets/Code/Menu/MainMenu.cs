using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour //что бы начать хостить
{

    //ссылка на наш кастомный NetworkManager
    [SerializeField] private NetworkLobbyManager lobbyManager = null;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;


    //начинаем хостить, внутренняя кухня нам не важна
    public void HostLobby()
    {
        lobbyManager.StartHost();
        landingPagePanel.SetActive(false);
    }
}
