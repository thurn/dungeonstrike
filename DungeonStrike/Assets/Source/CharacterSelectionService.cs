using UnityEngine;
using Vectrosity;
using System.Collections.Generic;

namespace DungeonStrike
{
    public class CharacterSelectionService
    {
        private VectorLine _selectionCircle;

        public void SelectCharacter(Transform characterTransform)
        {
            if (_selectionCircle != null)
            {
                VectorLine.Destroy(ref _selectionCircle);
            }
            var linePoints = new List<Vector3>(60);
            _selectionCircle = new VectorLine("selectionCircle", linePoints, null /* texture */,
                    5.0f /* width */, LineType.Continuous)
            {
                color = Color.green
            };
            _selectionCircle.MakeCircle(Vector3.zero, Vector3.up, 0.75f /* radius */);
            _selectionCircle.drawTransform = characterTransform;
        }

        public void Update()
        {
            if (_selectionCircle != null)
            {
                _selectionCircle.Draw3D();
            }
        }
    }
}