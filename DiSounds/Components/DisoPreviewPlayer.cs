﻿using Zenject;
using System.Linq;
using UnityEngine;
using SiraUtil.Tools;

namespace DiSounds.Components
{
    public class DisoPreviewPlayer : SongPreviewPlayer
    {
        private bool _active = true;
        private bool _transitionDidEnd;
        private SiraLog _siraLog = null!;
        private float _lastTrueAudioLength = 0f;
        private AudioClip _realDefaultAudioClip = null!;

        public bool Active
        {
            get => _active;
            set
            {
                if (_active)
                {
                    _lastTrueAudioLength = CurrentAudioTime;
                }
                _active = value;
                if (_active)
                {
                    if (_audioSources != null)
                    {
                        CrossfadeTo(_defaultAudioClip, _lastTrueAudioLength, -1f, _ambientVolumeScale);
                    }
                }
                else CrossfadeTo(null!, 0f, -1f, _volumeScale);
            }
        }

        public float Volume
        {
            get => _volume;
            set => _volume = value;
        }
        
        public float CurrentAudioTime
        {
            get
            {
                var audioSource = _audioSources[_activeChannel];
                if (!audioSource.isPlaying)
                {
                    return 0f;
                }
                return audioSource.time;
            }
        }

        public AudioClip DefaultClip => _defaultAudioClip;

        public float DefaultAudioLength => _defaultAudioClip.length;

        public bool PlayingDefault => _audioSources != null && _audioSources.Length != 0 && _audioSources.ElementAtOrDefault(_activeChannel) != null && _audioSources[_activeChannel] != null && _defaultAudioClip == _audioSources[_activeChannel].clip;

        [Inject]
        public void Construct(SiraLog siraLog)
        {
            _siraLog = siraLog;
            _realDefaultAudioClip = _defaultAudioClip;
        }

        public void SetDefault(AudioClip clip)
        {
            _defaultAudioClip = clip;
            if (_active)
            {
                CrossfadeTo(clip, 0f, -1f, _ambientVolumeScale);
            }
        }

        #region Overrides

        public override void CrossfadeToDefault()
        {
            if (!_active)
            {
                base.CrossfadeTo(null!, _lastTrueAudioLength, -1f, _ambientVolumeScale);
                return;
            }
            if (_audioSources == null) return;
            if (!_transitionAfterDelay && _activeChannel > 0 && _audioSources[_activeChannel].clip == _defaultAudioClip) return;
            CrossfadeTo(_defaultAudioClip, _lastTrueAudioLength, -1f, _ambientVolumeScale);
        }

        public override void CrossfadeTo(AudioClip audioClip, float startTime, float duration, float volumeScale = 1)
        {
            if (audioClip != _defaultAudioClip && _audioSources[_activeChannel].clip == _defaultAudioClip) _lastTrueAudioLength = CurrentAudioTime;
            if (_transitionDidEnd)
            {
                _transitionDidEnd = false;
                startTime = _lastTrueAudioLength;
                audioClip = _active ? _defaultAudioClip : null!;
            }
            base.CrossfadeTo(audioClip, startTime, duration, volumeScale);
        }

        public override void Update()
        {
            var fut = _timeToDefaultAudioTransition - Time.deltaTime;
            if (_transitionAfterDelay)
            {
                if (fut <= 0f)
                {
                    _transitionDidEnd = true;
                    CrossfadeTo(_defaultAudioClip, 0f, -1f, _ambientVolumeScale);
                }
            }
            base.Update();
        }

        #endregion
    }
}