using System;
using RWave.Enum;
using UnityEngine;
using UnityEngine.Audio;

namespace RWave.Data.Setting
{
    /// <summary>
    /// RWaveAudioSourceの設定を定義するClass
    /// </summary>
    [Serializable]
    public class RWaveAudioSourceSetting
    {
        /// <summary>
        /// AudioSourceName
        /// </summary>
        public string audioSourceName => _audioSourceName;

        /// <summary>
        /// 紐づけるAudioMixerGroup
        /// </summary>
        public AudioMixerGroup audioMixerGroup => _audioMixerGroup;
        
        /// <summary>
        /// 再生モード
        /// </summary>
        public eRWavePlayBackMode playBackMode => _playBackMode;
        
        /// <summary>
        /// AudioSource単位の初期ボリューム（0～100）
        /// AudioMixerGroup全体の音量とは独立して設定されます
        /// </summary>
        public int defaultVolume => _defaultVolume;
        
        /// <summary>
        /// 最大同時発音数
        /// </summary>
        public int maxSoundCount => _maxSoundCount;
        
        /// <summary>
        /// 再生開始時のAttack(ms)
        /// </summary>
        public int attackTime => _attackTime;

        /// <summary>
        /// 再生停止時のRelease(ms)
        /// </summary>
        public int releaseTime => _releaseTime;

        /// <summary>
        /// Attack秒数(float)
        /// </summary>
        public float attackTimeInSeconds => _attackTime / 1000f;

        /// <summary>
        /// Release秒数(float)
        /// </summary>
        public float releaseTimeInSeconds => _releaseTime / 1000f;

        /// <summary>
        /// CrossFadeの設定
        /// </summary>
        public RWaveCrossFadeSetting crossFadeSetting => _crossFadeSetting;

        /// <summary>
        /// 再生中の音声と同じ音声の重複再生を許可するかどうか
        /// </summary>
        public bool allowDuplicatePlay => _allowDuplicatePlay;

        [SerializeField,Header("AudioSourceName"),Tooltip("任意の名称")]
        private string _audioSourceName;
        [SerializeField,Header("紐づけるAudioMixerGroup")]
        private AudioMixerGroup _audioMixerGroup;
        [SerializeField,Header("同じ音声の重複再生を許可"),Tooltip("falseの場合、再生中と同じAudioClipの再生リクエストは無視されます")] 
        private bool _allowDuplicatePlay = true;
        [SerializeField,Header("再生モード")]
        private eRWavePlayBackMode _playBackMode = eRWavePlayBackMode.OneShot;
        [SerializeField,Header("AudioSource単位の初期ボリューム"),Range(0,100)]
        private int _defaultVolume = 50;
        [SerializeField, Header("最大同時発音数"),Range(1,100)]
        private int _maxSoundCount = 1;
        [SerializeField,Header("再生開始時のAttack(ms)")]
        private int _attackTime = 100;
        [SerializeField,Header("再生停止時のRelease(ms)")]
        private int _releaseTime = 100;
        [SerializeField,Header("CrossFadeの設定")]
        private RWaveCrossFadeSetting _crossFadeSetting;
    }
}