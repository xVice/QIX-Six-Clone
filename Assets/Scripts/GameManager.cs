using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class NamesData
{
    public string[] names;
}

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;

    [SerializeField]
    private GameObject EnemyPrefab;

    private NamesData names;

    [SerializeField]
    private TextMeshProUGUI ScoreboardText;

    private Player Player;


    [SerializeField]
    private HexagonGridObject Grid;

    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera VCAM;


    public HexagonGridObject GetGrid()
    {
        return Grid;
    }

    public void ReSetupCam(Vector3 targetPosition, int size)
    {
        Transform targetTransform = new GameObject().transform;
        targetTransform.position = targetPosition;

        VCAM.LookAt = targetTransform;
        VCAM.Follow = targetTransform;

        // Calculate desired FOV based on list size
        float desiredFOV = Mathf.Clamp(size * 10f, 20f, 60f); // Adjust the values as needed

        // Apply the desired FOV to the camera
        VCAM.m_Lens.FieldOfView = desiredFOV;

        // Destroy the temporary targetTransform object
        Destroy(targetTransform.gameObject);
    }



    public Player GetPlayer()
    {
        return Player;
    }

    /// <summary>
    /// Update the fake name list from the json at -> Resources -> names.json, used for fake bots, fetched only once since its 9k entrys.
    /// </summary>
    public void FetchNamesFromJson()
    {
        // Load the names.json file
        TextAsset textAsset = Resources.Load<TextAsset>("names");

        // Parse the JSON data
        NamesData data = JsonUtility.FromJson<NamesData>(textAsset.text);

        names = data;
    }

    /// <summary>
    /// Updates the scoreboard text.
    /// </summary>
    public void UpdateScoreBoard()
    {
        ScoreboardText.text = Grid.GetScoreboardString();
    }

    private void Awake()
    {
        VCAM = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        Grid = FindObjectOfType<HexagonGridObject>();
        FetchNamesFromJson();
    }

    /// <summary>
    /// Get a string array containing a bunch of names.
    /// </summary>
    /// <returns></returns>
    public string[] GetNames()
    {
        return names.names;
    }

    // Start is called before the first frame update
    void Start()
    {
        var player = Grid.InitilzePrefabOnHexagonWithStartingArea(new Vector2Int(15, 15), PlayerPrefab, Color.yellow);
        Player = player.GetComponent<Player>();
        VCAM.Follow = player.transform;
        VCAM.LookAt = player.transform;

        Grid.InitilzePrefabOnHexagonWithStartingArea(new Vector2Int(50, 50), EnemyPrefab, Color.green);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public static class ColorNameConverter
{
    private static readonly Dictionary<string, Color32> colorNames = new Dictionary<string, Color32>
    {
        { "Green", new Color32(0, 255, 0, 255) },
        { "Yellow", new Color32(255, 255, 0, 255) },
        { "Red", new Color32(255, 0, 0, 255) },
        { "Blue", new Color32(0, 0, 255, 255) },
        { "White", new Color32(255, 255, 255, 255) },
        { "Black", new Color32(0, 0, 0, 255) },
        { "Cyan", new Color32(0, 255, 255, 255) },
        { "Magenta", new Color32(255, 0, 255, 255) },
        { "Gray", new Color32(128, 128, 128, 255) },
        { "LightGray", new Color32(192, 192, 192, 255) },
        { "DarkGray", new Color32(64, 64, 64, 255) },
        { "Purple", new Color32(128, 0, 128, 255) },
        { "Orange", new Color32(255, 165, 0, 255) },
        { "Brown", new Color32(165, 42, 42, 255) },
        { "Pink", new Color32(255, 192, 203, 255) },
        { "Lime", new Color32(0, 255, 0, 255) },
    };

    public static string GetColorName(Color32 color)
    {
        foreach (var kvp in colorNames)
        {
            if (kvp.Value.Equals(color))
                return kvp.Key;
        }

        return "Unknown";
    }
}
