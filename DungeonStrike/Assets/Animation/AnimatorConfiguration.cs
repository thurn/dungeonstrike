using UnityEngine;
using System;
using System.Collections.Generic;

namespace DungeonStrike
{
    public class AnimatorConfiguration : MonoBehaviour
    {
        private bool _eventsAdded;
        private HashSet<Animator> _animators = new HashSet<Animator>();
        private Dictionary<string, List<AnimationEvent>> _animationEvents =
            new Dictionary<string, List<AnimationEvent>>();

        private void Start()
        {
            StartCoroutine(AddAllAnimationEvents());
        }

        public void AddAnimationCallback(Animator animator, string clipName, string callbackFunctionName,
            float eventTime)
        {
            Preconditions.CheckState(!_eventsAdded, "Animation events must be added in Start()!");
            if (!_animationEvents.ContainsKey(clipName))
            {
                _animationEvents[clipName] = new List<AnimationEvent>();
            }
            var animationEvent = new AnimationEvent
            {
                functionName = callbackFunctionName,
                time = eventTime
            };
            _animationEvents[clipName].Add(animationEvent);
            _animators.Add(animator);
        }

        private IEnumerator<YieldInstruction> AddAllAnimationEvents()
        {
            yield return new WaitForSeconds(1);

            var animationClips = new Dictionary<string, AnimationClip>();
            foreach (var animator in _animators)
            {
                var runtimeController = animator.runtimeAnimatorController;
                foreach (var animationClip in runtimeController.animationClips)
                {
                    animationClips[animationClip.name] = animationClip;
                }
            }

            foreach (var item in _animationEvents)
            {
                var animationClip = animationClips[item.Key];
                foreach (var animationEvent in _animationEvents[item.Key])
                {
                    animationClip.AddEvent(animationEvent);
                }
            }

            _eventsAdded = true;
            _animationEvents = null;
            _animators = null;
        }
    }
}