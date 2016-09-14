using UnityEngine;
using System.Collections.Generic;
using Vectrosity;

namespace DungeonStrike
{
    public class AttackService : MonoBehaviour
    {
        private static AttackService _instance;

        public static AttackService Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<AttackService>()); }
        }

        private bool _inTargetMode;
        private CharacterService _characterService;
        private List<int> _enemies;
        private int _currentEnemyIndex;
        private VectorLine _line;

        // Use this for initialization
        void Start()
        {
            _characterService = CharacterService.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            if (_inTargetMode)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {

                }
            }
            _line.Draw();
        }

        public void EnterTargetMode()
        {
            _inTargetMode = true;
            _enemies = _characterService.EnemiesOfCharacter(_characterService.SelectedCharacter());
            _currentEnemyIndex = 0;
            SelectEnemy(_enemies[0]);
        }

        private void SelectEnemy(int enemyNumber)
        {
            Debug.Log("Targeting " + enemyNumber);
            var enemy = _characterService.SelectCharacter(enemyNumber);
            var linePoints = new List<Vector3>(60);
            _line = new VectorLine("selected", linePoints, null, 5.0f, LineType.Continuous)
            {
                color = Color.red
            };
            var gridObject = enemy.GetComponent<GGObject>();
            _line.MakeCircle(gridObject.CachedTransform.position, Vector3.up, 0.75f);
        }
    }
}