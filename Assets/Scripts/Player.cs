using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MobilePlayerController))]
public class Player : MonoBehaviour
{
    public TextMeshProUGUI PlayerNameGUI;

    MobilePlayerController Movement;

    [SerializeField]
    PlayerPreferences Prefs;

    [Serializable]
    public class PlayerPreferences
    {
        public string PlayerName = "Player";
        public string TeamName = "Team";
        public Color TeamColor = Color.blue;
    }

    private void Awake()
    {
        Movement = GetComponent<MobilePlayerController>();
        PlayerNameGUI.text = Prefs.PlayerName;
    }

    /// <summary>
    /// Returns a object containing a bunch of player data.
    /// </summary>
    /// <returns></returns>
    public PlayerPreferences GetPreferences()
    {
        return Prefs;
    }

    /// <summary>
    /// Gets the movement controller.
    /// </summary>
    /// <returns></returns>
    public MobilePlayerController GetMovementController()
    {
        return Movement;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
