using UnityEngine;

public class DebugManager : MonoBehaviour {

    public static DebugManager Instance;
    public bool ShowAimLines { get; private set;  }

    public bool ShowNavigationDebug { get; private set; }

    void Start () {
        Instance = this;
    }

	void Update () {

	}

	public void SetShowAimLines(bool value)
	{
        ShowAimLines = value;
    }

	public void SetShowNavigationDebug(bool value)
	{
        ShowNavigationDebug = value;
    }
}
