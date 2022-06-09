using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour //что бы ввести имя типа имя пользователя 
{
    [Header("UI")]
    [SerializeField] private  TMP_InputField nameInputField= null; //тмп инпут хз чем отличается от обычного

    [SerializeField] private  Button continueButton= null; //ссылка на кнопку
   public static string DisplayName { get; private set; } //имя игрока
    public const string PlayerDefoultNameKey = "PlayerName"; //ключ что б получить имя в  PlayerPrefs

    void Start()
    {
       
            SetUpInputField(); // со старта идет настройка
    }
    void SetUpInputField()
    {

        //если ключ не устоновлен выйти return
        if (!PlayerPrefs.HasKey(PlayerDefoultNameKey)) return;
        //если есть имя вставить его в инпут
        string defoultName = PlayerPrefs.GetString(PlayerDefoultNameKey);
        nameInputField.text = defoultName;
        SetPlayerName(defoultName);
    }

    public void SetPlayerName(string name)
    {
        //если у нас уже есть имя сюда прийдет дэфолт, если нет то оно перехватывает инпут 
        if (name==null || String.IsNullOrEmpty(name))
            name = nameInputField.text;
        
        continueButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        //наше имя в лобби или не только
        DisplayName = nameInputField.text;
        //установить имя в PlayerPrefs
        PlayerPrefs.SetString(PlayerDefoultNameKey, DisplayName);
    }

   

}
