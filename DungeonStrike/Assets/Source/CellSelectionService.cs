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

        public Material GreenIceMaterial;
        public Material LavaMaterial;
        public Material SciFiMaterial;
        public Material GreyWoodMaterial;
        private bool _inCellSelectionMode;
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

        public void EnterCellSelectionMode(Card card)
        {
            _inCellSelectionMode = true;
            _currentCard = card;
            _movementService.MovementEnabled = false;
            Material quadMaterial;
            switch (card.School)
            {
                case School.Aeris:
                    quadMaterial = GreyWoodMaterial;
                    break;
                case School.Aquis:
                    quadMaterial = SciFiMaterial;
                    break;
                case School.Ignis:
                    quadMaterial = LavaMaterial;
                    break;
                default: // School.Petra:
                    quadMaterial = GreenIceMaterial;
                    break;
            }
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
                    _cardService.CellSelected(_currentCard, cell, quad as GameObject);
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