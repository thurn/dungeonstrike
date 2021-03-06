﻿using UnityEngine;

namespace DungeonStrike
{
    public class CharacterHit : MonoBehaviour, IOnProjectileHit
    {
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void OnProjectileHit()
        {
            _animator.applyRootMotion = false;
            var hitNumber = Random.Range(1, 5);
            _animator.SetFloat("HitNumber", hitNumber);
            _animator.SetTrigger("GetHit");
        }
    }
}