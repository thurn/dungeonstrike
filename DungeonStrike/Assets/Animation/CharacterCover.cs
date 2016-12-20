using UnityEngine;

namespace DungeonStrike
{
    public class CharacterCover : MonoBehaviour
    {
        private Animator _animator;
        private bool _inCover;

        void Start()
        {
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName(AnimationStates.BackToCoverL))
            {
                _animator.SetBool("ShootFromCoverL", false);
            }

            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                if (_inCover)
                {
                    _animator.applyRootMotion = true;
                    _animator.SetBool("CoverL", false);
                    _inCover = false;
                }
                else
                {
                    _animator.applyRootMotion = true;
                    _animator.SetBool("CoverL", true);
                    _inCover = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                _animator.applyRootMotion = true;
                _animator.SetBool("ShootFromCoverL", true);
            }
        }
    }
}