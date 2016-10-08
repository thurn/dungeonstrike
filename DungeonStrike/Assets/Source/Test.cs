﻿using UnityEngine;

namespace DungeonStrike
{
    public class Test : MonoBehaviour
    {
        private Animation _animation;
        private Animator _animator;

        void Start()
        {
            _animation = GetComponent<Animation>();
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("space");
                _animator.SetTrigger("Shoot");
            }
        }
    }
}
