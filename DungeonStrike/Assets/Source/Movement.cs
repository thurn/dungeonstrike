using UnityEngine;
using System.Collections.Generic;

public class Movement : MonoBehaviour {
	private List<GGCell> _currentPath;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			var cell = GGGrid.GetCellFromRay(ray, 1000f);
			var gridObject = GetComponent<GGObject>();
			Debug.Log(gridObject.Cell);
			Debug.Log(cell);
			_currentPath = GGAStar.GetPath(gridObject.Cell, cell, false);

			Debug.Log("Got Path " + _currentPath.Count);
			foreach (var pathCell in _currentPath) {
				Debug.Log("Cell " + pathCell);
			}			
		}
	}
}
