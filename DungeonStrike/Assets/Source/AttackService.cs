using System;
using System.Collections.Generic;
using UnityEngine;
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

        public bool InTargetMode;
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
            if (Input.GetKeyDown(KeyCode.T))
            {
                var currentCharacter = _characterService.CurrentTurnCharacter();
                if (InTargetMode)
                {
                    ExitTargetMode();
                }
                else
                {
                    if (currentCharacter.CanTakeAdditionAction())
                    {
                        EnterTargetMode();
                    }
                    else
                    {
                        Debug.Log("Out of actions");
                    }
                }
            }

            if (InTargetMode)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    _currentEnemyIndex = (_currentEnemyIndex + 1) % _enemies.Count;
                    SelectEnemy(_enemies[_currentEnemyIndex]);
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    // fire!
                    MakeAttack(true, () => RollDice(2, 10));
                }

                if (_line != null)
                {
                    _line.Draw();
                }
            }
        }

        private IEnumerator<WaitForSeconds> ExitTargetModeAfterDelay()
        {
            yield return new WaitForSeconds(1.0f);
            ExitTargetMode();
        }

        private void ExitTargetMode()
        {
            VectorLine.Destroy(ref _line);
            _currentEnemyIndex = 0;
            InTargetMode = false;
            _characterService.SelectCharacter(_characterService.CurrentTurnCharacterNumber());
        }

        public Character CurrentTarget()
        {
            return _characterService.GetCharacter(_enemies[_currentEnemyIndex]);
        }

        public int RollDice(int quantity, int sides, int modifier = 0)
        {
            int result = 0;
            for (int i = 0; i < quantity; ++i)
            {
                result += UnityEngine.Random.Range(1, sides + 1) + modifier;
            }
            return result;
        }

        public bool RollForHit()
        {
            var attacker = _characterService.CurrentTurnCharacter();
            var target = CurrentTarget();
            var attackerCell = attacker.GetComponent<GGObject>().Cell;
            var targetCell = target.GetComponent<GGObject>().Cell;
            var distance = Mathf.Floor(Vector3.Distance(attackerCell.CenterPoint3D, targetCell.CenterPoint3D));
            var bonus = Mathf.Max(0, 8 - distance);
            var cover = 0;
            var agilityTarget = attacker.Agility + bonus + cover;
            var roll = RollD20();
            return roll <= agilityTarget;
        }

        public void MakeAttack(bool isAction, Func<int> damageFunction)
        {
            if (!InTargetMode)
            {
                throw new System.ArgumentException("You must select a target first.");
            }

            bool hit = RollForHit();
            if (hit)
            {
                var damage = damageFunction();
                ApplyDamage(damage);
                Debug.Log("ATTACK - Hit for " + damage);
            }
            else
            {
                Debug.Log("ATTACK - Miss");
            }

            if (isAction)
            {
                var currentCharacter = _characterService.CurrentTurnCharacter();
                currentCharacter.ActionsThisRound++;
                StartCoroutine(ExitTargetModeAfterDelay());
            }
        }

        public void ApplyDamage(int damage)
        {
            var target = CurrentTarget();
            target.CurrentHealth -= damage;
        }

        public int RollD20()
        {
            return UnityEngine.Random.Range(1, 21);
        }

        public void EnterTargetMode()
        {
            InTargetMode = true;
            _enemies = _characterService.EnemiesOfCharacter(_characterService.CurrentTurnCharacter());
            _currentEnemyIndex = 0;
            SelectEnemy(_enemies[0]);
        }

        private void SelectEnemy(int enemyNumber)
        {
            if (_line != null)
            {
                VectorLine.Destroy(ref _line);
            }
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