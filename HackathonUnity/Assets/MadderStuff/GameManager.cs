using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Canvas canvas;
    public GameObject nameObject; // Ensure this has a NameScript attached for setting the name
    public TextMeshProUGUI roomCodeText; // This will hold the reference to the TextMeshProUGUI component for the room code
    private List<PlayerJoin> playerJoins = new List<PlayerJoin>();

    [DllImport("__Internal")]
    private static extern void MessageToPlayer(string userName, string message);
    [DllImport("__Internal")]
    private static extern void MessageToAllPlayers(string message);
    [DllImport("__Internal")]
    private static extern void Exit();
    [DllImport("__Internal")]
    private static extern void UpdateStats(string userName, string stats);

    void Start()
    {
        playerJoins = new List<PlayerJoin>();
    }

    void Update()
    {
        // Your testing code or game logic here
    }

    public void RoomCode(string roomCode)
    {
        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + roomCode;
        }
        else
        {
            Debug.LogError("RoomCodeText not set in the inspector.");
        }
    }

    public void PlayerJoined(string jsonPlayerJoin)
    {
        PlayerJoin playerJoin = JsonUtility.FromJson<PlayerJoin>(jsonPlayerJoin);
        if (playerJoin.stats == null)
        {
            playerJoin.stats = new GameStats();
        }
        playerJoins.Add(playerJoin);
        UpdatePlayerNamesDisplay();
    }

    private void UpdatePlayerNamesDisplay()
    {
        // Clear existing player name objects
        foreach (Transform child in canvas.transform)
        {
            if (child.gameObject.name != "RoomCodeText") // Make sure not to destroy the room code text GameObject
            {
                Destroy(child.gameObject);
            }
        }

        // Create new name objects for each player
        foreach (PlayerJoin player in playerJoins)
        {
            GameObject name = Instantiate(nameObject, canvas.transform);
            // You may need to adjust the position of the nameObject instantiation
            name.GetComponent<NameScript>().SetName(player.name);
        }
    }

    public void PlayerLeft(string playerName)
    {
        playerJoins.RemoveAll(player => player.name == playerName);
        UpdatePlayerNamesDisplay();

        // Additional logic if needed when a player leaves
    }

    public void PlayerControllerState(string jsonControllerState)
    {
        ControllerState controllerState = JsonUtility.FromJson<ControllerState>(jsonControllerState);

        // Find the player with the matching name and move their displayed name
        foreach (Transform child in canvas.transform)
        {
            NameScript nameScript = child.GetComponent<NameScript>();
            if (nameScript != null && nameScript.GetName() == controllerState.name)
            {
                // Assuming the NameScript has a method called UpdateXY to handle the movement
                nameScript.UpdateXY(controllerState.joystick.x, controllerState.joystick.y);
                break; // Assuming only one GameObject per player name
            }
        }
    }

    // ...

    [System.Serializable]
    public class ControllerState
    {
        public string name;
        public Joystick joystick;
        public bool circle;
        public bool triangle;
        public bool plus;
    }

    [System.Serializable]
    public class Joystick
    {
        public float x;
        public float y;

        public Joystick(float initX, float initY)
        {
            x = initX;
            y = initY;
        }
    }

    [System.Serializable]
    public class PlayerJoin
    {
        public string name;
        public GameStats stats;
    }

    [System.Serializable]
    public class GameStats
    {
        public Stat gamesPlayed;

        public GameStats()
        {
            gamesPlayed = new Stat("Games Played", 0);
        }

        public void addGamePlayed()
        {
            gamesPlayed.value++;
        }
    }

    [System.Serializable]
    public class Stat
    {
        public string title;
        public int value;

        public Stat(string initTitle, int initValue)
        {
            title = initTitle;
            value = initValue;
        }
    }

    // ...
}