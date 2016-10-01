using UnityEngine;
using System.Collections.Generic;

public class AnimatorStateLogger : MonoBehaviour {

    private Dictionary<int, string> _states;
    private int _currentStateHash;
    private Animator _animator;

    // Use this for initialization
    void Start () {
        var baseLayerStates = new List<string>()
        {
            "Idle",
			"Running",
			"Walking",
			"TurnL180",
			"TurnL90",
			"TurnR90",
			"TurnR180"
        };

        _states = new Dictionary<int, string>();
        foreach (var stateName in baseLayerStates)
		{
            _states[Animator.StringToHash(stateName)] = stateName;
        }

        _animator = GetComponent<Animator>();
    }

	// Update is called once per frame
	void Update () {
        var stateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
		if (stateHash != _currentStateHash)
		{
			Debug.Log(_states[stateHash]);
            _currentStateHash = stateHash;
        }
    }
}
