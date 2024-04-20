using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class TankManager
{
    public Color m_PlayerColor;
    public Transform m_SpawnPoint;
    public string m_PlayerName;
    [HideInInspector] public TextMeshProUGUI m_PlayerNameText;
    [HideInInspector] public int m_PlayerNumber;
    [HideInInspector] public string m_ColoredPlayerText;
    [HideInInspector] public GameObject m_Instance;
    [HideInInspector] public int m_Wins;
    [HideInInspector] public bool m_IsHost = false;
    [HideInInspector] public GameStats m_GameStats;

    public TankMovement m_Movement;
    public TankShooting m_Shooting;
    private GameObject m_CanvasGameObject;
    private GameObject m_HealthSlider;
    private TankHealth m_Health;


    public void Setup()
    {
        m_Movement = m_Instance.GetComponent<TankMovement>();
        m_Shooting = m_Instance.GetComponent<TankShooting>();
        m_Shooting.m_PlayerName = m_PlayerName;
        m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;
        m_PlayerNameText = m_CanvasGameObject.GetComponentInChildren<TextMeshProUGUI>();
        m_HealthSlider = m_CanvasGameObject.GetComponentInChildren<Slider>().gameObject;
        m_Health = m_Instance.GetComponent<TankHealth>();
        m_Health.m_TankManager = this;

        if (m_IsHost)
        {
            m_PlayerNameText.text = m_PlayerName + "*";
        }
        else
        {
            m_PlayerNameText.text = m_PlayerName;
        }

        m_Movement.m_PlayerNumber = m_PlayerNumber;
        m_Shooting.m_PlayerNumber = m_PlayerNumber;

        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">" + m_PlayerName + "</color>";

        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = m_PlayerColor;
        }
    }

    public void Move(string controllerStateString)
    {
        string[] controllerStateStrings = controllerStateString.Split(',');
        int[] controllerState = new int[2];
        for (int i = 0; i < 2; i++)
        {
            controllerState[i] = int.Parse(controllerStateStrings[i]);
        }

        float distance = Mathf.Sqrt(controllerState[1] * controllerState[1] + controllerState[0] * controllerState[0]);
        distance = distance / 100f;
        m_Movement.m_MovementInputValue = distance;

        if (distance > 0.5f)
        {
            float angleRadians = Mathf.Atan2(controllerState[0], controllerState[1]);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            m_Movement.m_AngleInputValue = angleDegrees;
        }
    }


    public void DisableControl()
    {
        if (m_IsHost)
        {
            m_PlayerNameText.text = m_PlayerName;
        }
        m_Movement.enabled = false;
        m_Shooting.enabled = false;

        m_HealthSlider.SetActive(false);

        m_Shooting.m_AimSlider.value = m_Shooting.m_MinLaunchForce;
    }

    public void EnableMovement()
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = false;

        m_HealthSlider.SetActive(false);
    }


    public void EnableControl()
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = true;

        m_HealthSlider.SetActive(true);
    }


    public void Reset()
    {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;

        m_Instance.SetActive(false);
        m_Instance.SetActive(true);
    }

    public void PauseControl()
    {
        m_Movement.enabled = false;
        m_Shooting.enabled = false;
    }

    public void ResumeControl()
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = true;
    }
}
