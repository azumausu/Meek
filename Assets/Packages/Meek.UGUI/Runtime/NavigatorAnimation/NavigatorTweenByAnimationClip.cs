using System;
using System.Collections.Generic;
using Meek.NavigationStack;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meek.UGUI
{
    [RequireComponent(typeof(SimpleAnimationPlayer))]
    public class NavigatorTweenByAnimationClip : MonoBehaviour, INavigatorTween
#if UNITY_2018_3_OR_NEWER
        , IAnimationClipSource
#endif
    {
        #region Serialized Fields

        [SerializeField] [HideInInspector] private bool _enabledFromScreenName;
        [SerializeField] [HideInInspector] private bool _enabledToScreenName;
        [SerializeField] private NavigatorAnimationType transitionType;
        [SerializeField] private AnimationClip _animationClip;
        [SerializeField] [HideInInspector] private string _fromScreenName;
        [SerializeField] [HideInInspector] private string _toScreenName;

        #endregion

        #region Fields

        private SimpleAnimationPlayer _simpleAnimationPlayer;

        #endregion

        #region Unity events

        private void Awake() => _simpleAnimationPlayer =
            GetComponent<SimpleAnimationPlayer>();

        #endregion

        #region Interface Implementations

        public NavigatorAnimationType NavigatorAnimationType => transitionType;

        public string FromScreenName => _enabledFromScreenName
            ? _fromScreenName
            : null;

        public string ToScreenName => _enabledToScreenName
            ? _toScreenName
            : null;

        public float Length => _animationClip.length;

        public void Evaluate(float t) =>
            _simpleAnimationPlayer.Set(_animationClip, t);

        public void Play(Action onComplete = null) =>
            _simpleAnimationPlayer.Play(
                _animationClip,
                onEnd: () => { onComplete?.Invoke(); }
            );

        public void Stop() => _simpleAnimationPlayer.Stop();

        #endregion
        
#if UNITY_2018_3_OR_NEWER
        void IAnimationClipSource.GetAnimationClips(List<AnimationClip> results)
        {
            if (_animationClip != null)
            {
                results.Add(_animationClip);
            }
        }
#endif
    }
}