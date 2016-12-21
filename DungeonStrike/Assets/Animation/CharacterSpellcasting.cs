using System.Collections.Generic;
using UnityEngine;

namespace DungeonStrike
{
    public class CharacterSpellcasting : MonoBehaviour
    {
        public GameObject RootObject;
        public SpellType SpellType;
        private GameObject _target;
        private Vector3 _targetPosition;
        private CharacterTurning _characterTurning;
        private Animator _animator;
        private bool _casting;
        private float? _spineAngle;
        private Transform _spineBone;
        private bool _hitTarget;
        private EffectSettings _effectSettings;
        private AudioSource _audioSource;

        private void Start()
        {
            _characterTurning = GetComponent<CharacterTurning>();
            _animator = GetComponent<Animator>();
            _spineBone = Transforms.FindChildTransformWithTag(this.transform, Tags.SpineBone);
            AddAnimationEvents();
            _audioSource = GetComponent<AudioSource>();
            SpellConstants.PreloadAudioForSpell(SpellType);
        }

        private void Update()
        {
        }

        private void LateUpdate()
        {
            if (AnimationTransitions.IsInTransition(_animator, AnimationTransitions.CastToIdle))
            {
                _casting = false;
            }

            if (_casting)
            {
                _spineBone.eulerAngles = new Vector3(
                    _spineBone.eulerAngles.x,
                    _spineBone.eulerAngles.y + _spineAngle.Value,
                    _spineBone.eulerAngles.z);
            }
        }

        public void CastSpellWithTarget(Transform target)
        {
            _target = Transforms.FindChildTransformWithTag(target, Tags.TargetPoint).gameObject;
            _targetPosition = target.transform.position;
            var angle = Transforms.AngleToTarget(transform, _targetPosition);
            _characterTurning.TurnToAngle(angle, BeginCast);
        }

        private void BeginCast()
        {
            _spineAngle = Transforms.AngleToTarget(this.transform, _targetPosition);
            _casting = true;
            _animator.SetTrigger("Casting");
            StartCasting();
        }

        private void OnHitTarget(object sender, CollisionInfo collisionInfo)
        {
            if (_hitTarget) return;
            if (collisionInfo.Hit.transform == null) return;
            _hitTarget = true;
            var raycastHit = collisionInfo.Hit;

            var characterHit = raycastHit.transform.GetComponent<CharacterHit>();
            if (characterHit != null)
            {
                characterHit.OnProjectileHit();
            }
        }

        private void OnCastSpell()
        {
            _effectSettings.MoveSpeed = 10;
        }

        private void StartCasting()
        {
            _hitTarget = false;
            var clip = SpellConstants.AudioClipForSpell(SpellType);
            _audioSource.PlayOneShot(clip);

            AssetLoaderService.Instance.LoadAsset("spell_effects",
                SpellConstants.AssetNameForSpell(SpellType), (GameObject instance) =>
                {
                    instance.transform.SetParent(this.transform, false);
                    instance.transform.localPosition = new Vector3(0.0f, 1.5f, 1.0f);
                    _effectSettings = instance.GetComponent<EffectSettings>();
                    _effectSettings.Target = _target;
                    _effectSettings.IsHomingMove = true;
                    _effectSettings.MoveSpeed = 0;
                    _effectSettings.CollisionEnter += OnHitTarget;
                });
        }

        private void AddAnimationEvents()
        {
            var animationConfiguration = RootObject.GetComponent<AnimatorConfiguration>();
            animationConfiguration.AddAnimationCallback(_animator, GetType(),
                new List<AnimationDescription>
                {
                    new AnimationDescription()
                    {
                        ClipName = AnimationClips.CastSpell,
                        CallbackFunctionName = "OnCastSpell",
                        EventTime = 1.12f
                    },
                });
        }
    }
}