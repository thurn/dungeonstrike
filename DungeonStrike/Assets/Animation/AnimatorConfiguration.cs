using UnityEngine;
using System;
using System.Collections.Generic;

namespace DungeonStrike
{
    public class AnimatorConfiguration : MonoBehaviour
    {
        private bool _eventsAdded = false;
        // Clip Name -> Function Name -> Event Time:
        private Dictionary<string, Dictionary<string, float>> _animationEvents =
            new Dictionary<string, Dictionary<string, float>>();

        private HashSet<Animator> _animators = new HashSet<Animator>();
        private HashSet<Type> _eventCallers;

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
                _animationEvents[clipName] = new Dictionary<string, float>();
            }
            _animationEvents[clipName][callbackFunctionName] = eventTime;
            _animators.Add(animator);
        }

        private IEnumerator<WaitForSeconds> AddAllAnimationEvents()
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
                foreach (var eventTimeItem in _animationEvents[item.Key])
                {
                    var animationEvent = new AnimationEvent();
                    animationEvent.functionName = eventTimeItem.Key;
                    animationEvent.time = eventTimeItem.Value;
                    animationClip.AddEvent(animationEvent);
                }
            }

            _eventsAdded = true;
            _animationEvents = null;
            _animators = null;
        }
    }
}