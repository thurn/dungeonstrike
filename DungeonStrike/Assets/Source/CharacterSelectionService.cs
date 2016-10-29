using UnityEngine;
using Vectrosity;
using System.Collections.Generic;

namespace DungeonStrike
{
    public class CharacterSelectionService
    {
        private Dictionary<string, VectorLine> _selectionCircles;

        public CharacterSelectionService()
        {
            _selectionCircles = new Dictionary<string, VectorLine>();
        }

        public void SelectCharacter(string selectionKey, Transform characterTransform, Color color)
        {
            if (_selectionCircles.ContainsKey(selectionKey))
            {
                var circle = _selectionCircles[selectionKey];
                _selectionCircles.Remove(selectionKey);
                VectorLine.Destroy(ref circle);
            }
            var linePoints = new List<Vector3>(60);
            var selectionCircle = new VectorLine("selectionCircle", linePoints, null /* texture */,
                    5.0f /* width */, LineType.Continuous)
            {
                color = color
            };
            selectionCircle.MakeCircle(Vector3.zero, Vector3.up, 0.75f /* radius */);
            selectionCircle.drawTransform = characterTransform;
            _selectionCircles[selectionKey] = selectionCircle;
        }

        public void Update()
        {
            foreach (var circle in _selectionCircles.Values)
            {
                circle.Draw3D();
            }
        }
    }
}