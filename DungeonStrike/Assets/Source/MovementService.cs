using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace DungeonStrike
{
    public class MovementService : MonoBehaviour
    {
        private static MovementService _instance;

        public static MovementService Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<MovementService>()); }
        }

        public float MovementSpeed;
        private Queue<GGCell> _currentPath;
        private Transform _currentMover;
        private GGObject _gridObject;

        // Use this for initialization
        void Start()
        {
            _gridObject = GetComponent<GGObject>();
        }

        // Update is called once per frame
        void Update()
        {
            MoveOnCurrentPath();
            HandleMovementClick();
        }

        public void SetCurrentMover(Transform currentMover)
        {
            if (_currentMover != null)
            {
                Debug.Log("Error: already have a current mover");
            }
            else
            {
                _currentMover = currentMover;
            }
        }

        private void MoveOnCurrentPath()
        {
            if (_currentPath != null && _currentMover != null)
            {
                var currentTargetCell = _currentPath.Peek();
                var target = currentTargetCell.CenterPoint3D;
                _currentMover.position = Vector3.MoveTowards(_currentMover.position, target, Time.deltaTime * MovementSpeed);
                if (Vector3.Distance(_currentMover.position, target) < 0.001f)
                {
                    _currentPath.Dequeue();
                    if (_currentPath.Count == 0)
                    {
                        _currentPath = null;
                        _currentMover = null;
                    }
                }
            }
        }

        private void HandleMovementClick()
        {
            if (Input.GetMouseButtonDown(0) && _currentMover != null)
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var cell = GGGrid.GetCellFromRay(ray, 1000f);

                var newPath = GGAStar.GetPath(_gridObject.Cell, cell, false /* ignoredOccupiedAtDestCell */);
                if (newPath.Count > 0)
                {
                    _currentPath = new Queue<GGCell>(newPath);
                    // Remove first path entry, since it is the current location:
                    _currentPath.Dequeue();
                }
                else
                {
                    Debug.Log("NO PATH");
                }
            }
        }
    }
}