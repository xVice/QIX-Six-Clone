using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public string Username = "QIX-Player";
    public Color TeamColor = Color.yellow;
    public TextMeshProUGUI usernameText;

    public bool TakeRandomUserName = false;

    public GameManager gameManager;

    private void Awake()
    {
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        gameManager = FindObjectOfType<GameManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (TakeRandomUserName)
        {
            // Set a random name as the Username
            int randomIndex = Random.Range(0, gameManager.GetNames().Length);
            Username = gameManager.GetNames()[randomIndex];
        }
        usernameText.text = Username;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
