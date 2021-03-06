﻿using UnityEngine;
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
        public bool MovementEnabled;
        private Queue<GGCell> _currentPath;
        private GameObject _currentMover;
        private CharacterService _characterService;
        private int _maxDistance;

        void Start()
        {
            _characterService = CharacterService.Instance;
            MovementEnabled = true;
        }

        // Update is called once per frame
        void Update()
        {
            MoveOnCurrentPath();
            if (MovementEnabled)
            {
                HandleMovementClick();
            }
        }

        public void SetCurrentMover(GameObject currentMover)
        {
            if (_currentPath != null)
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
                _currentMover.transform.position =
                    Vector3.MoveTowards(_currentMover.transform.position, target, Time.deltaTime * MovementSpeed);
                if (Vector3.Distance(_currentMover.transform.position, target) < 0.001f)
                {
                    _currentPath.Dequeue();
                    if (_currentPath.Count == 0)
                    {
                        _currentPath = null;
                    }
                }
            }
        }

        private void HandleMovementClick()
        {
            if (Input.GetMouseButtonDown(0) && _currentMover != null)
            {
                var currentCharacter = _characterService.CurrentTurnCharacter();
                if (EventSystem.current.IsPointerOverGameObject()) return;

                if (!currentCharacter.CanTakeAdditionAction())
                {
                    Debug.Log("OUT OF MOVES");
                    return;
                }

                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var cell = GGGrid.GetCellFromRay(ray, 1000f);
                if (cell == null) return;

                var cellObjects = cell.Objects;
                if (cellObjects.Count > 1)
                {
                    throw new System.ArgumentException("too many objects in cell");
                }
                else if (cellObjects.Count == 1)
                {
                    var cellObject = cellObjects[0];
                    var character = cellObject.GetComponent<Character>();
                    if (character != null)
                    {
                        _characterService.SelectCharacter(character.CharacterNumber);
                    }
                }
                else
                {
                    var gridObject = _currentMover.GetComponent<GGObject>();
                    var newPath = GGAStar.GetPath(gridObject.Cell, cell, false /* ignoredOccupiedAtDestCell */);
                    if (newPath.Count > 0)
                    {
                        if (newPath.Count > MaxMoveDistance(currentCharacter) + 1)
                        {
                            Debug.Log("TOO FAR");
                            return;
                        }

                        _currentPath = new Queue<GGCell>(newPath);

                        // Remove first path entry, since it is the current location:
                        _currentPath.Dequeue();
                        currentCharacter.MovesThisRound += 1;
                    }
                    else
                    {
                        Debug.Log("NO PATH");
                    }
                }
            }
        }

        private int MaxMoveDistance(Character currentCharacter)
        {
            return currentCharacter.Agility / 2;
        }
    }
}