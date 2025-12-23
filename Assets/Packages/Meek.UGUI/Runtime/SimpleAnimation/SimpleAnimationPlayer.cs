#if MEEK_ENABLE_UGUI
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Meek.UGUI
{
    [RequireComponent(typeof(Animator))]
    public class SimpleAnimationPlayer : MonoBehaviour
    {
        Animator _animator;
        PlayableGraph _graph;
        AnimationMixerPlayable _mixer;
        AnimationClipPlayable _prevPlayable;
        AnimationClipPlayable _currentPlayable;
        Coroutine _coroutine = null;
        bool _fading;
        int _playCount;

        public float DefaultFadeTime = 0f;

        public bool IsPlaying { get { return _graph.IsPlaying(); } }
        public AnimationClip CurrentClip { get { return _currentPlayable.IsValid() ? _currentPlayable.GetAnimationClip() : null; } }
        public AnimationClipPlayable CurrentPlayable { get { return _currentPlayable; } }

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _graph = PlayableGraph.Create("Simple");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            _mixer = AnimationMixerPlayable.Create(_graph, 2);

            var output = AnimationPlayableOutput.Create(_graph, "output", _animator);
            output.SetSourcePlayable(_mixer);
        }

        public Coroutine Play(AnimationClip clip, float fadeTime = -1, float speed = 1f, float startPosition = 0, Action onEnd = null)
        {
            if (fadeTime < 0)
            {
                fadeTime = DefaultFadeTime;
            }

            _coroutine = StartCoroutine(_Play(clip, fadeTime, speed, startPosition, onEnd));
            return _coroutine;
        }

        public void Set(AnimationClip clip, float time, float speed = 1f)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _fading = false;
            _playCount++;
            _prevPlayable = _currentPlayable;
            _currentPlayable = AnimationClipPlayable.Create(_graph, clip);
            _graph.Disconnect(_mixer, 0);
            _graph.Disconnect(_mixer, 1);
            if (_prevPlayable.IsValid())
            {
                _prevPlayable.Destroy();
            }

            _mixer.ConnectInput(0, _currentPlayable, 0);
            _mixer.SetInputWeight(0, 1);
            _mixer.SetInputWeight(1, 0);
            _currentPlayable.SetTime(time);
            _currentPlayable.SetSpeed(speed);
            _graph.Evaluate();
            _graph.Stop();
        }

        public void Stop()
        {
            _graph.Stop();
        }

        public void Resume()
        {
            _graph.Play();
        }

        IEnumerator _Play(AnimationClip clip, float fadeTime, float speed, float startPosition, Action onEnd)
        {
            _playCount++;
            var playId = _playCount;

            while (_fading)
            {
                if (playId != _playCount)
                {
                    // フェード終了待ち中に次のが来たら再生しないで終了
                    onEnd?.Invoke();
                    yield break;
                }

                yield return null;
            }

            if (!_graph.IsPlaying())
            {
                _graph.Play();
            }

            _prevPlayable = _currentPlayable;
            _currentPlayable = AnimationClipPlayable.Create(_graph, clip);

            if (IsReverse(speed))
            {
                _currentPlayable.SetTime(clip.length - startPosition);
            }
            else
            {
                _currentPlayable.SetTime(startPosition);
            }

            _currentPlayable.SetSpeed(speed);
            _graph.Disconnect(_mixer, 0);
            _mixer.ConnectInput(0, _currentPlayable, 0);
            if (fadeTime > 0)
            {
                if (_prevPlayable.IsValid())
                {
                    _fading = true;
                    _mixer.ConnectInput(1, _prevPlayable, 0);
                    yield return Fade(fadeTime);

                    _graph.Disconnect(_mixer, 1);
                    _prevPlayable.Destroy();

                    _mixer.SetInputWeight(1, 0);
                    _fading = false;
                }
            }

            _mixer.SetInputWeight(0, 1);

            if (IsReverse(speed))
            {
                while (playId == _playCount && (clip.isLooping || _currentPlayable.GetTime() > 0f))
                {
                    yield return null;
                }
            }
            else
            {
                while (playId == _playCount && (clip.isLooping || _currentPlayable.GetTime() < clip.length))
                {
                    yield return null;
                }
            }

            if (playId == _playCount)
            {
                _graph.Stop();
                _graph.Disconnect(_mixer, 0);
                _currentPlayable.Destroy();
            }

            onEnd?.Invoke();
        }

        IEnumerator Fade(float length)
        {
            var prevTime = (float)_currentPlayable.GetTime();
            float t = 0f;
            bool isReversePlay = _currentPlayable.GetSpeed() < 0f;

            while (t < length)
            {
                var tx = t / length;

                _mixer.SetInputWeight(0, tx);
                _mixer.SetInputWeight(1, 1 - tx);
                yield return null;

                var time = (float)_currentPlayable.GetTime();
                var delta = time - prevTime;
                if (isReversePlay)
                {
                    delta *= -1;
                }

                t += delta;
                prevTime = time;
            }
        }

        bool IsReverse(float setSpeed)
        {
            return setSpeed < 0.0f;
        }

        void OnDestroy()
        {
            _graph.Destroy();
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }
        }
    }
}
#endif