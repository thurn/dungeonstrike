using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Vectrosity;

namespace DungeonStrike
{
    public class CellSelectionService : MonoBehaviour
    {
        private static CellSelectionService _instance;
        public static CellSelectionService Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<CellSelectionService>()); }
        }

        private bool _inCellSelectionMode;
        private bool _inAreaSelectionMode;
        private int _areaSelectionRadius;
        private GGCell _hoverCell;
        private GGGrid _grid;
        private GameObject _selectedQuad;
        private MeshRenderer _quadRenderer;
        private Card _currentCard;
        private CardService _cardService;
        private MovementService _movementService;
        private List<VectorLine> _lines;
        private List<GGCell> _selectedCells;

        public void Start()
        {
            _selectedQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _selectedQuad.transform.position = new Vector3(0, 0.03f, 0);
            _selectedQuad.transform.eulerAngles = new Vector3(90, 0, 0);
            _quadRenderer = _selectedQuad.GetComponent<MeshRenderer>();
            _cardService = CardService.Instance;
            _movementService = MovementService.Instance;
            _grid = GetComponent<GGGrid>();
            _lines = new List<VectorLine>();
            _selectedCells = new List<GGCell>();
            SetQuadEnabled(false);
        }

        public void EnterCellSelectionMode(Material quadMaterial, Card card)
        {
            _inCellSelectionMode = true;
            _currentCard = card;
            _movementService.MovementEnabled = false;
            _quadRenderer.material = quadMaterial;
        }

        public void EnterAreaSelectionMode(Material quadMaterial, Card card, int radius)
        {
            _inCellSelectionMode = true;
            _inAreaSelectionMode = true;
            _areaSelectionRadius = radius;
            _currentCard = card;
            _movementService.MovementEnabled = false;
            _quadRenderer.material = quadMaterial;
        }

        public void Update()
        {
            if (_inCellSelectionMode)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    SetQuadEnabled(false);
                    return;
                }
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var cell = GGGrid.GetCellFromRay(ray, 1000f);

                if (cell == null)
                {
                    SetQuadEnabled(false);
                    return;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    var quad = GameObject.Instantiate(_selectedQuad,
                        _selectedQuad.transform.parent,
                        true /* worldPositionStays */);
                    if (_inAreaSelectionMode)
                    {
                        _cardService.AreaSelected(_currentCard, _selectedCells, quad as GameObject);
                        VectorLine.Destroy(_lines);
                        _lines = new List<VectorLine>();
                    }
                    else
                    {
                        _cardService.CellSelected(_currentCard, cell, quad as GameObject);
                    }

                    SetQuadEnabled(false);
                    _movementService.MovementEnabled = true;
                    _inCellSelectionMode = false;
                }
                else if (cell != _hoverCell)
                {
                    SetQuadEnabled(true);
                    _selectedQuad.transform.position = new Vector3(cell.CenterPoint3D.x, 0.03f, cell.CenterPoint3D.z);
                    _hoverCell = cell;

                    if (_inAreaSelectionMode)
                    {
                        UpdateAreaSelectionPosition();
                    }
                }
            }
        }

        private void UpdateAreaSelectionPosition()
        {
            _selectedCells = new List<GGCell>();
            GGGrid.GetCellsInRange(_selectedCells, _hoverCell, _areaSelectionRadius,
                false /* onlyPathable */, true /* allowOccupied */, true /* allowDiagonals */,
                true /* allowSelf */, false /* useLinkDirections */);

            while (_selectedCells.Count > _lines.Count)
            {
                _lines.Add(new VectorLine("Square", new List<Vector3>(), null, 10.0f, LineType.Continuous)
                {
                    color = Color.red,
                    joins = Joins.Fill
                });
            }

            var index = 0;
            foreach (var cell in _selectedCells)
            {
                var line = _lines[index++];
                line.points3 = SquareForCell(cell);
                line.Draw3D();
            }
        }

        private List<Vector3> SquareForCell(GGCell cell)
        {
            return new List<Vector3>() {
                new Vector3(cell.MinPoint3D.x, 1.0f, cell.MinPoint3D.z),
                new Vector3(cell.MaxPoint3D.x, 1.0f, cell.MinPoint3D.z),
                new Vector3(cell.MaxPoint3D.x, 1.0f, cell.MaxPoint3D.z),
                new Vector3(cell.MinPoint3D.x, 1.0f, cell.MaxPoint3D.z),
                new Vector3(cell.MinPoint3D.x, 1.0f, cell.MinPoint3D.z)
            };
        }

        private void SetQuadEnabled(bool enabled)
        {
            _selectedQuad.GetComponent<Renderer>().enabled = enabled;
        }
    }
}