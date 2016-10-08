using UnityEngine;

namespace DungeonStrike
{
    class EnableAnimator : MonoBehaviour
    {
        private Animation _animation;
        private Animator _animator;
        private bool _updated;

        void Start()
        {
            _animation = GetComponent<Animation>();
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (_updated)
            {
                _animation.enabled = false;
                _animator.enabled = true;
            }
            _updated = true;
        }
    }
}
