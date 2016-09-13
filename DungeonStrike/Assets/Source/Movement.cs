using UnityEngine;
using System.Collections.Generic;

namespace DungeonStrike
{
    public class Movement : MonoBehaviour
    {
        public GGGrid grid;
        private List<GGCell> _currentPath;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var cell = GGGrid.GetCellFromRay(ray, 1000f);
                var gridObject = GetComponent<GGObject>();

                _currentPath = GGAStar.GetPath(gridObject.Cell, cell, true /* ignoredOccupiedAtDestCell */, true);

                Debug.Log("Got Path " + _currentPath.Count);
                foreach (var pathCell in _currentPath)
                {
                    Debug.Log("Cell " + pathCell.GridX + "," + pathCell.GridY);
                }
            }
        }
    }
}
