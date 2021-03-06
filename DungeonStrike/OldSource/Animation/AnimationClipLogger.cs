﻿using UnityEngine;

namespace DungeonStrike
{
    public class AnimationClipLogger : MonoBehaviour
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
                Debug.Log("clip: <" + clipName + "> " + clipInfo[0].clip.events.Length);
                _currentClipName = clipName;
            }
        }
    }
}