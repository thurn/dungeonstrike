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

        public void Start()
        {
            _selectedQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _selectedQuad.transform.position = new Vector3(0, 0.03f, 0);
            _selectedQuad.transform.eulerAngles = new Vector3(90, 0, 0);
            SetQuadEnabled(false);
        }

        public void EnterCellSelectionMode(Card card)
        {
            _inCellSelectionMode = true;
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

                SetQuadEnabled(true);
                _selectedQuad.transform.position = new Vector3(cell.CenterPoint3D.x, 0.03f, cell.CenterPoint3D.z);
            }
        }

        private void SetQuadEnabled(bool enabled)
        {
            _selectedQuad.GetComponent<Renderer>().enabled = enabled;
        }
    }
}