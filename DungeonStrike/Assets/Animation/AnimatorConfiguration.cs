using System;
using UnityEngine;
using System.Collections.Generic;

namespace DungeonStrike
{
    public struct AnimationDescription
    {
        public string ClipName { get; set; }
        public string CallbackFunctionName { get; set; }
        public float EventTime { get; set; }
    }


    public class AnimatorConfiguration : MonoBehaviour
    {
        private bool _eventsAdded;
        private HashSet<Animator> _animators = new HashSet<Animator>();
        private HashSet<Type> _processedComponents = new HashSet<Type>();

        private Dictionary<string, List<AnimationEvent>> _animationEvents =
            new Dictionary<string, List<AnimationEvent>>();

        private void Start()
        {
            StartCoroutine(AddAllAnimationEvents());
        }

        public void AddAnimationCallback(Animator animator, Type sourceComponent, List<AnimationDescription> animations)
        {
            if (_processedComponents.Contains(sourceComponent))
            {
                return;
            }

            _processedComponents.Add(sourceComponent);
            Preconditions.CheckState(!_eventsAdded, "Animation events must be added in Start()!");

            foreach (var animationDescription in animations)
            {
                if (!_animationEvents.ContainsKey(animationDescription.ClipName))
                {
                    _animationEvents[animationDescription.ClipName] = new List<AnimationEvent>();
                }
                var animationEvent = new AnimationEvent
                {
                    functionName = animationDescription.CallbackFunctionName,
                    time = animationDescription.EventTime
                };
                _animationEvents[animationDescription.ClipName].Add(animationEvent);
            }
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
            _processedComponents = null;
        }
    }
}