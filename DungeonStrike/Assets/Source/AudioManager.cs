using System.Collections.Generic;
using UnityEngine;

namespace DungeonStrike
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        public static AudioManager Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<AudioManager>()); }
        }

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        public void PreloadClip(string bundleName, string clipName)
        {
            if (!_clips.ContainsKey(clipName))
            {
                AssetLoaderService.Instance.InstantiateObject(bundleName, clipName, (AudioClip clip) =>
                {
                    _clips[clipName] = clip;
                });
            }
        }

        public AudioClip GetClip(string clipName)
        {
            Preconditions.CheckState(_clips.ContainsKey(clipName));
            return _clips[clipName];
        }
    }
}