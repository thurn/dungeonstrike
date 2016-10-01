using com.ootii.Actors.AnimationControllers;
using UnityEngine;

namespace DungeonStrike
{
    public class AnimatorLogger : MonoBehaviour
    {
        private Animator _animator;
        private string _currentClipName;
        private string _currentStateName;
        private MotionController _motionController;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _motionController = GetComponent<MotionController>();
        }

        void Update()
        {
            var state = _motionController.GetAnimatorStateName();
            if (state != _currentStateName)
            {
                //Debug.Log("animator state: <" + state + ">");
                _currentStateName = state;
            }

            var clipInfo = _animator.GetCurrentAnimatorClipInfo(0);
            var clipName = clipInfo[0].clip.name;
            if (clipName != _currentClipName)
            {
                Debug.Log("clip: <" + clipName + ">");
                _currentClipName = clipName;
            }
        }
    }
}