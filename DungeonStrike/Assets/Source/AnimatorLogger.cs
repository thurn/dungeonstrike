using UnityEngine;

namespace DungeonStrike
{
    public class AnimatorLogger : MonoBehaviour
    {
        private Animator _animator;
        private string _currentClipName;
        private string _currentStateName;

        void Start()
        {
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
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