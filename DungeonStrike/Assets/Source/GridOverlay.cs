using UnityEngine;
using System.Collections.Generic;
using Vectrosity;

namespace DungeonStrike {
  public class GridOverlay : MonoBehaviour {
    private GGGrid _grid;

    private void Start() {
      _grid = GetComponent<GGGrid>();
      Debug.Log("printing grid");

      for (var x = 0; x < _grid.GridWidth; ++x) {
        for (var y = 0; y < _grid.GridHeight; ++y) {
          var cell = _grid.Cells[x, y];
          var pointArray = new List<Vector3>() {
            new Vector3(cell.MinPoint3D.x, 1.0f, cell.MinPoint3D.z),
            new Vector3(cell.MaxPoint3D.x, 1.0f, cell.MinPoint3D.z), 
            new Vector3(cell.MaxPoint3D.x, 1.0f, cell.MaxPoint3D.z), 
            new Vector3(cell.MinPoint3D.x, 1.0f, cell.MaxPoint3D.z),
            new Vector3(cell.MinPoint3D.x, 1.0f, cell.MinPoint3D.z)
          };

          var color = new Color(0.0f, 0.0f, 0.0f, 1.0f);

          var points = new VectorLine("Points", pointArray, null, 2.0f, LineType.Continuous) {
              color = color
          };
          points.Draw3DAuto();
        }
      }
    }

    private void Update() {}
  }
}
