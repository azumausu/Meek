#if MEEK_ENABLE_UGUI
using System.Collections.Generic;
using UnityEngine;

namespace Meek.UGUI
{
    public class SingleAnimationController : MonoBehaviour
#if UNITY_2018_3_OR_NEWER
        , IAnimationClipSource
#endif
    {
        [SerializeField] AnimationClip m_Clip;
        [SerializeField] bool m_AutoPlay = true;

        SimpleAnimationPlayer _player;

        void Awake()
        {
            _player = GetComponent<SimpleAnimationPlayer>();
        }

        void Start()
        {
            if (!m_AutoPlay) return;
            Play();
        }

        public void Play(global::System.Action onComplete = null)
        {
            if (_player == null)
            {
                return;
            }

            _player.Play(m_Clip, onEnd: onComplete);
        }

        public void Stop()
        {
            if (_player == null)
            {
                return;
            }

            _player.Stop();
        }

#if UNITY_2018_3_OR_NEWER
        void IAnimationClipSource.GetAnimationClips(List<AnimationClip> results)
        {
            if (m_Clip != null)
            {
                results.Add(m_Clip);
            }
        }
#endif
    }
}
#endif