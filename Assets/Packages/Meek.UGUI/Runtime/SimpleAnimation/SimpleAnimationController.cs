#if MEEK_ENABLE_UGUI
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Meek.UGUI
{
    public class SimpleAnimationController : MonoBehaviour
#if UNITY_2018_3_OR_NEWER
        , IAnimationClipSource
#endif
    {
        [SerializeField] SimpleAnimationEntry[] m_Entries;
        public SimpleAnimationEntry[] Entries { get { return m_Entries; } }

        SimpleAnimationPlayer _player;

        public SimpleAnimationPlayer Player
        {
            get
            {
                if (_player == null)
                {
                    _player = GetComponent<SimpleAnimationPlayer>();
                }

                return _player;
            }
        }

        SimpleAnimationEntry _currentEntry = null;

        void Awake()
        {
            _player = GetComponent<SimpleAnimationPlayer>();
        }

        public Coroutine Play(string name, Action onEnd = null)
        {
            SimpleAnimationEntry entry = null;
            if (TryFindEntry(name, out entry))
            {
                _currentEntry = entry;
                var c = entry.config;
                return Player?.Play(entry.clip, c.FadeTime, c.PlaySpeed, c.StartPosition * entry.clip.length, onEnd: onEnd);
            }

            onEnd?.Invoke();
            return null;
        }

        /// <summary>
        /// 最後に指定したEntryに対して最終結果にする
        /// </summary>
        public void ImmediateEnd()
        {
            if (_currentEntry == null)
            {
                return;
            }

            ImmediateEnd(_currentEntry.name, _currentEntry.config.PlaySpeed < 0f);
        }

        void ImmediateEnd(string name, bool isReverse = false)
        {
            if (Player == null)
            {
                return;
            }

            foreach (var e in m_Entries)
            {
                if (e.name == name)
                {
                    float time = isReverse ? 0f : e.clip.length;
                    Player.Set(e.clip, time);
                }
            }
        }

        public bool IsPlaying(string name = null)
        {
            if (Player == null)
            {
                return false;
            }

            var currentClip = Player.CurrentClip;
            if (currentClip == null) return false;
            if (string.IsNullOrEmpty(name)) name = _currentEntry.name;
            foreach (var e in m_Entries)
            {
                if (e.name == name)
                {
                    return currentClip == e.clip;
                }
            }

            return false;
        }

        bool TryFindEntry(string name, out SimpleAnimationEntry entry)
        {
            entry = null;
            foreach (var e in m_Entries)
            {
                if (e.name == name)
                {
                    entry = e;
                    return true;
                }
            }

            return false;
        }

#if UNITY_2018_3_OR_NEWER
        void IAnimationClipSource.GetAnimationClips(List<AnimationClip> results)
        {
            if (m_Entries != null)
            {
                results.AddRange(m_Entries.Select(x => x.clip));
            }
        }
#endif
    }

    [Serializable]
    public class SimpleAnimationEntry
    {
        public string name;
        public AnimationClip clip;
        public SimpleAnimationConfig config;
    }

    /// <summary>
    /// アニメーション再生時の設定
    /// </summary>
    [Serializable]
    public class SimpleAnimationConfig
    {
        public float FadeTime = 0f;

        [Range(-5f, 5f)] public float PlaySpeed = 1f;

        [Range(0f, 1f)] public float StartPosition = 0f;
    }
}
#endif