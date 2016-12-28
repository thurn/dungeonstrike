using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace DungeonStrike
{
    public class WorldPointSelectionService
    {
        private bool _active;
        private Action<Vector3> _selectionCallback;

        public void GetUserSelectedWorldPoint(Action<Vector3> callback)
        {
            Preconditions.CheckState(_selectionCallback == null);
            _selectionCallback = callback;
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0) && _selectionCallback != null)
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit raycastHit;
                var raycastHitCollider = Physics.Raycast(ray, out raycastHit);
                if (raycastHitCollider)
                {
                    _selectionCallback(raycastHit.point);
                    _selectionCallback = null;
                }
            }
        }
    }
}