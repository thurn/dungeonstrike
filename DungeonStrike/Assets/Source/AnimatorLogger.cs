using com.ootii.Actors.AnimationControllers;
using UnityEngine;

namespace DungeonStrike
{
    public class AnimatorLogger : MonoBehaviour
    {
        private Animator _animator;
        private string _currentStateName;
        private MotionController _motionController;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _motionController = GetComponent<MotionController>();
        }

        void Update()
        {
            var state = _motionController.GetAnimatorStateAndTransitionName();
            if (state != _currentStateName)
            {
                var clipInfo = _animator.GetCurrentAnimatorClipInfo(0);
                var clipName = clipInfo[0].clip.name;
                Debug.Log(">>> " + state + "    [" + clipName + "]");
                _currentStateName = state;
            }
        }
    }
}