using System;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{

    public static DebugManager Instance;
    public Toggle ShowAimLinesToggle;
    public Toggle ShowNavigationDebugToggle;

    void Start()
    {
        Instance = this;
        ShowAimLinesToggle.isOn = ShowAimLines;
        ShowNavigationDebugToggle.isOn = ShowNavigationDebug;
    }

    public void SetShowAimLines(bool value)
    {
        PlayerPrefs.SetInt("ShowAimLines", Convert.ToInt32(value));
    }

    public void SetShowNavigationDebug(bool value)
    {
        PlayerPrefs.SetInt("ShowNavigationDebug", Convert.ToInt32(value));
    }

    public bool ShowAimLines
    {
        get
        {
            return Convert.ToBoolean(PlayerPrefs.GetInt("ShowAimLines"));
        }
    }

    public bool ShowNavigationDebug
    {
        get
        {
            return Convert.ToBoolean(PlayerPrefs.GetInt("ShowNavigationDebug"));
        }
    }
}
