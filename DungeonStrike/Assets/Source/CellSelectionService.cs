using UnityEngine;
using UnityEngine.EventSystems;

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
        private GameObject _selectedQuad;
        private MeshRenderer _quadRenderer;
        private Card _currentCard;
        private CardService _cardService;
        private MovementService _movementService;

        public void Start()
        {
            _selectedQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _selectedQuad.transform.position = new Vector3(0, 0.03f, 0);
            _selectedQuad.transform.eulerAngles = new Vector3(90, 0, 0);
            _quadRenderer = _selectedQuad.GetComponent<MeshRenderer>();
            _cardService = CardService.Instance;
            _movementService = MovementService.Instance;
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
                        _cardService.AreaSelected(_currentCard, null, quad as GameObject);
                    }
                    else
                    {
                        _cardService.CellSelected(_currentCard, cell, quad as GameObject);
                    }

                    SetQuadEnabled(false);
                    _movementService.MovementEnabled = true;
                    _inCellSelectionMode = false;
                }
                else
                {
                    SetQuadEnabled(true);
                    _selectedQuad.transform.position = new Vector3(cell.CenterPoint3D.x, 0.03f, cell.CenterPoint3D.z);
                }
            }
        }

        private void SetQuadEnabled(bool enabled)
        {
            _selectedQuad.GetComponent<Renderer>().enabled = enabled;
        }
    }
}