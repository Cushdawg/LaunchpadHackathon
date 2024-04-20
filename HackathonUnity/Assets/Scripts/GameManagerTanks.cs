using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public TextMeshProUGUI m_MessageText;
    public TextMeshProUGUI m_RoomCodePrefaceText;
    public TextMeshProUGUI m_RoomCodeText;
    public TextMeshProUGUI m_HostStartText;
    public TextMeshProUGUI m_RequiredPlayersText;
    public Image m_TitleImage;
    public TextMeshProUGUI m_HostPlayAgainText;
    public TextMeshProUGUI m_HostExitText;
    public TextMeshProUGUI m_WinnerText;
    public Image m_WinnerAnnouncementImage;
    public string m_RoomCode;
    public GameObject m_TankPrefab;
    public TankManager[] m_Tanks;
    public GameObject[] m_SpawnPoints;
    public GameObject m_StartingSpawnPoint;
    public Color[] colors = new Color[10];

    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private bool m_RoundActive = false;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;
    private bool m_GameStarted = false;
    private PlayerJoin[] m_PlayerJoins = new PlayerJoin[0];
    private int m_PlayerCount = 0;
    private int m_MaxPlayers = 10;

    private bool handlingPlus = false;
    private bool handlingMinus = false;

    [DllImport("__Internal")]
    private static extern void MessageToPlayer(string userName, string message);
    [DllImport("__Internal")]
    private static extern void MessageToAllPlayers(string message);
    [DllImport("__Internal")]
    private static extern void Exit();
    [DllImport("__Internal")]
    private static extern void UpdateStats(string userName, string stats);



    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        StartCoroutine(GameLoop());
    }

    public void RoomCode(string roomCode)
    {
        m_RoomCode = roomCode;
        m_RoomCodeText.text = roomCode;
    }

    public void PlayerJoined(string jsonPlayerJoin)
    {
        PlayerJoin playerJoin = JsonUtility.FromJson<PlayerJoin>(jsonPlayerJoin);

        Debug.Log("Player " + playerJoin.name + " joined");
        Debug.Log("Player count: " + m_PlayerCount);

        GameStats expectedStats = new GameStats();
        if (playerJoin.stats == null)
        {
            playerJoin.stats = expectedStats;
        }
        else
        {
            if (playerJoin.stats.gamesPlayed == null)
            {
                playerJoin.stats.gamesPlayed = expectedStats.gamesPlayed;
            }
            if (playerJoin.stats.roundWins == null)
            {
                playerJoin.stats.roundWins = expectedStats.roundWins;
            }
            if (playerJoin.stats.gameWins == null)
            {
                playerJoin.stats.gameWins = expectedStats.gameWins;
            }
        }

        PlayerJoin[] newPlayerJoins = new PlayerJoin[m_PlayerJoins.Length + 1];
        for (int i = 0; i < m_PlayerJoins.Length; i++)
        {
            newPlayerJoins[i] = m_PlayerJoins[i];
        }
        newPlayerJoins[newPlayerJoins.Length - 1] = playerJoin;
        m_PlayerJoins = newPlayerJoins;
        Debug.Log("New player count: " + m_PlayerJoins.Length);
    }

    public void PlayerLeft(string playerName)
    {
        if (playerName == m_PlayerJoins[0].name)
        {
            HandleExit();
            return;
        }
        PlayerJoin[] newPlayerJoins = new PlayerJoin[m_PlayerJoins.Length - 1];
        int j = 0;
        for (int i = 0; i < m_PlayerJoins.Length; i++)
        {
            if (m_PlayerJoins[i].name != playerName)
            {
                newPlayerJoins[j] = m_PlayerJoins[i];
                j++;
            }
        }
        m_PlayerCount = newPlayerJoins.Length;
        m_PlayerJoins = newPlayerJoins;
        TankManager[] newTanks = new TankManager[m_Tanks.Length - 1];
        TankManager tankToDelete = null;
        j = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_PlayerName != playerName)
            {
                newTanks[j] = m_Tanks[i];
                j++;
            }
            else
            {
                tankToDelete = m_Tanks[i];
            }
        }
        m_Tanks = newTanks;
        if (tankToDelete != null)
        {
            Color[] remainingColors = new Color[colors.Length + 1];
            for (int i = 0; i < colors.Length; i++)
            {
                remainingColors[i] = colors[i];
            }
            remainingColors[remainingColors.Length - 1] = tankToDelete.m_PlayerColor;
            tankToDelete.m_Instance.SetActive(false);
        }
    }

    public void PlayerControllerState(string stateString)
    {
        // destructure controller states
        string name = stateString.Split(',')[0];
        int x = int.Parse(stateString.Split(',')[1]);
        int y = int.Parse(stateString.Split(',')[2]);
        int fire = int.Parse(stateString.Split(',')[3]);
        int boost = int.Parse(stateString.Split(',')[4]);
        float boostValue = float.Parse(stateString.Split(',')[5]);
        int minus = int.Parse(stateString.Split(',')[6]);
        int plus = int.Parse(stateString.Split(',')[7]);

        // find tank
        TankManager tank = null;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_PlayerName == name)
            {
                tank = m_Tanks[i];
                break;
            }
        }
        // if tank not found, return
        if (tank == null)
        {
            return;
        }

        // handle movement speed
        float distance = Mathf.Sqrt(x * x + y * y);
        distance = distance / 100f;
        tank.m_Movement.m_MovementInputValue = distance;

        // handle rotation
        if (distance > 0f)
        {
            float angleRadians = Mathf.Atan2(x, y);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            tank.m_Movement.m_AngleInputValue = angleDegrees;
        }

        // handle fire
        if (fire == 1)
        {
            tank.m_Shooting.m_Charging = true;
        }
        else
        {
            tank.m_Shooting.m_Charging = false;
        }

        // handle boost
        if (boost == 1 && boostValue > 0 && tank.m_Movement.m_BoostReleased)
        {
            tank.m_Movement.m_BoostReleased = false;
            tank.m_Movement.Boost(name, boostValue);
        }
        else if (boost == 0)
        {
            tank.m_Movement.m_BoostReleased = true;
        }

        if (name == m_Tanks[0].m_PlayerName)
        {
            HandleHostControls(minus, plus);
        }
    }

    private void HandleHostControls(int minus, int plus)
    {
        // handle minus
        if (minus == 1 && !handlingMinus && m_GameWinner != null)
        {
            handlingMinus = true;
            HandleExit();
        }
        else if (minus == 0)
        {
            handlingMinus = false;
        }

        // handle plus
        if (plus == 1 && !handlingPlus && (m_GameWinner != null || m_GameStarted == false) && m_PlayerCount > 1)
        {
            handlingPlus = true;
            if (m_GameStarted == false)
            {
                HandleBegin();
            }
            else
            {
                PlayAgain();
            }
        }
        else if (plus == 0)
        {
            handlingPlus = false;
        }
    }

    public void HandleBegin()
    {
        m_GameStarted = true;
#if UNITY_WEBGL == true && UNITY_EDITOR == false
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].m_GameStats.addGamePlayed();
                UpdateStats(m_Tanks[i].m_PlayerName, JsonUtility.ToJson(m_Tanks[i].m_GameStats));
            }
#endif
    }

    private void Update()
    {
        if (m_PlayerJoins.Length > m_PlayerCount && m_PlayerCount < m_MaxPlayers)
        {
            Debug.Log("Adding the player to the game.");
            PlayerJoin playerJoin = m_PlayerJoins[m_PlayerJoins.Length - 1];
            AddTank(playerJoin);
            m_PlayerCount = m_PlayerJoins.Length;
            // if the game has started and a round is active, enable full control for the tank
            // otherwise, enable movement only
            if (m_GameStarted && m_RoundActive)
            {
                m_Tanks[m_Tanks.Length - 1].EnableControl();
            }
            else
            {
                m_Tanks[m_Tanks.Length - 1].EnableMovement();
            }
            SetCameraTargets();
        }
    }

    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_SpawnPoint = m_SpawnPoints[i].transform;
        }
    }

    public void HandleExit()
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false
        Exit();
#endif
        m_RoomCodePrefaceText.gameObject.SetActive(true);
        m_RoomCodeText.gameObject.SetActive(true);
        m_HostStartText.gameObject.SetActive(true);
        m_RequiredPlayersText.gameObject.SetActive(true);
        m_TitleImage.gameObject.SetActive(true);
        m_MessageText.text = string.Empty;
        m_HostPlayAgainText.gameObject.SetActive(false);
        m_HostExitText.gameObject.SetActive(false);
        m_WinnerText.text = string.Empty;
        m_WinnerAnnouncementImage.gameObject.SetActive(false);
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            Destroy(m_Tanks[i].m_Instance);
        }
        m_Tanks = new TankManager[0];
        m_PlayerJoins = new PlayerJoin[0];
        m_PlayerCount = 0;
        m_GameStarted = false;
        m_RoundNumber = 0;
        Transform[] targets = new Transform[1];
        targets[0] = m_StartingSpawnPoint.transform;
        m_CameraControl.m_Targets = targets;
        StartCoroutine(GameLoop());
    }


    private IEnumerator WaitingForPlayersLoop()
    {
        while (!m_GameStarted)
        {
            yield return null;
        }
    }

    private IEnumerator EndOfGameLoop()
    {
        m_WinnerText.text = string.Empty;
        m_WinnerAnnouncementImage.gameObject.SetActive(false);
        m_MessageText.text = string.Empty;
        m_TitleImage.gameObject.SetActive(true);
        m_HostPlayAgainText.gameObject.SetActive(true);
        m_HostExitText.gameObject.SetActive(true);
        while (true)
        {
            yield return null;
        }
    }

    private IEnumerator GameLoop()
    {
        if (m_GameStarted == false)
        {
            yield return StartCoroutine(WaitingForPlayersLoop());

            m_HostStartText.gameObject.SetActive(false);
            m_RequiredPlayersText.gameObject.SetActive(false);
            m_TitleImage.gameObject.SetActive(false);

            SpawnAllTanks();

        }

        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameStarted)
        {
            if (m_GameWinner != null)
            {
                StartCoroutine(EndOfGameLoop());
            }
            else
            {
                StartCoroutine(GameLoop());
            }
        }
    }

    public void PlayAgain()
    {
        m_HostPlayAgainText.gameObject.SetActive(false);
        m_HostExitText.gameObject.SetActive(false);
        m_TitleImage.gameObject.SetActive(false);
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Wins = 0;
#if UNITY_WEBGL == true && UNITY_EDITOR == false
                m_Tanks[i].m_GameStats.addGamePlayed();
                UpdateStats(m_Tanks[i].m_PlayerName, JsonUtility.ToJson(m_Tanks[i].m_GameStats));
#endif
        }
        m_RoundNumber = 0;
        StartCoroutine(GameLoop());
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        m_RoundActive = true;
        EnableTankControl();

        m_MessageText.text = string.Empty;

        while (!OneTankLeft())
        {
            yield return null;
        }
    }


    private IEnumerator RoundEnding()
    {
        m_RoundActive = false;
        DisableTankControl();

        m_RoundWinner = null;

        m_RoundWinner = GetRoundWinner();

#if UNITY_WEBGL == true && UNITY_EDITOR == false
        if (m_RoundWinner != null)
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i].m_PlayerName == m_RoundWinner.m_PlayerName)
                {
                    m_Tanks[i].m_GameStats.addRoundWin();
                    UpdateStats(m_Tanks[i].m_PlayerName, JsonUtility.ToJson(m_Tanks[i].m_GameStats));
                    break;
                }
            }
        }
#endif

        if (m_RoundWinner != null)
            m_RoundWinner.m_Wins++;

        m_GameWinner = GetGameWinner();

        string message = EndMessage();
        m_MessageText.text = message;

        yield return m_EndWait;
    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }


    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }


    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
            {
#if UNITY_WEBGL == true && UNITY_EDITOR == false
                    m_Tanks[i].m_GameStats.addGameWin();
                    UpdateStats(m_Tanks[i].m_PlayerName, JsonUtility.ToJson(m_Tanks[i].m_GameStats));
#endif
                return m_Tanks[i];
            }
        }

        return null;
    }


    private string EndMessage()
    {
        if (m_GameStarted == false)
        {
            return string.Empty;
        }

        string message = "DRAW!";

        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        message += "\n\n";

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS";
            if (i < m_Tanks.Length - 1)
            {
                if (i % 2 == 1)
                {
                    message += "\n";
                }
                else
                {
                    message += "   |   ";
                }
            }
        }

        if (m_GameWinner != null)
        {
            message = "";
            m_WinnerText.text = m_GameWinner.m_ColoredPlayerText;
            m_WinnerAnnouncementImage.gameObject.SetActive(true);
        }

        return message;
    }


    private void ResetAllTanks()
    {
        // randomize spawn points
        for (int i = 0; i < m_SpawnPoints.Length; i++)
        {
            int randomIndex = Random.Range(i, m_SpawnPoints.Length);
            GameObject temp = m_SpawnPoints[i];
            m_SpawnPoints[i] = m_SpawnPoints[randomIndex];
            m_SpawnPoints[randomIndex] = temp;
        }
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // assign new spawn point
            m_Tanks[i].m_SpawnPoint = m_SpawnPoints[i].transform;
            m_Tanks[i].Reset();
        }
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }

    private void AddTank(PlayerJoin playerJoin)
    {
        TankManager newTank = new TankManager();
        int random_color = Random.Range(0, colors.Length);
        newTank.m_PlayerColor = colors[random_color];
        Color[] remainingColors = new Color[colors.Length - 1];
        for (int i = 0; i < colors.Length; i++)
        {
            if (i < random_color)
            {
                remainingColors[i] = colors[i];
            }
            else if (i > random_color)
            {
                remainingColors[i - 1] = colors[i];
            }
        }
        colors = remainingColors;
        newTank.m_PlayerName = playerJoin.name;
        newTank.m_GameStats = playerJoin.stats;
        Debug.Log("Before spawn point added");
        if (m_GameStarted)
        {
            Debug.Log("Game started spawn point added");
            newTank.m_SpawnPoint = m_SpawnPoints[m_Tanks.Length].transform;
        }
        else
        {
            Debug.Log("Game not started spawn point added");
            newTank.m_SpawnPoint = m_StartingSpawnPoint.transform;
        }
        Debug.Log("Spawn point added");
        newTank.m_PlayerNumber = m_Tanks.Length + 1;
        newTank.m_Instance = Instantiate(m_TankPrefab, newTank.m_SpawnPoint.position, newTank.m_SpawnPoint.rotation) as GameObject;
        if (m_Tanks.Length == 0)
        {
            newTank.m_IsHost = true;
        }
        newTank.Setup();

        TankManager[] newTanks = new TankManager[m_Tanks.Length + 1];
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            newTanks[i] = m_Tanks[i];
        }
        newTanks[newTanks.Length - 1] = newTank;
        m_Tanks = newTanks;
    }
}

public class Message
{
    public string name;
    public string message;
}


[System.Serializable]
public class GameStats
{
    public Stat gamesPlayed;
    public Stat roundWins;
    public Stat gameWins;

    public GameStats()
    {
        gamesPlayed = new Stat("Games Played", 0);
        roundWins = new Stat("Round Wins", 0);
        gameWins = new Stat("Game Wins", 0);
    }

    // Methods to increment stats
    public void addGamePlayed()
    {
        gamesPlayed.value++;
    }
    public void addRoundWin()
    {
        roundWins.value++;
    }
    public void addGameWin()
    {
        gameWins.value++;
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

public class PlayerJoin
{
    public string name;
    public GameStats stats;
}