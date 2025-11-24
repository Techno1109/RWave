using System;
using UnityEngine;
using UnityEngine.Audio;

namespace RWave.Data.Setting
{
    /// <summary>
    /// AudioMixerGroup単位の音量設定を定義するClass
    /// </summary>
    [Serializable]
    public class RWaveAudioMixerGroupSetting
    {
        /// <summary>
        /// 紐づけるAudioMixerGroup
        /// </summary>
        public AudioMixerGroup audioMixerGroup => _audioMixerGroup;

        /// <summary>
        /// AudioMixerのExposed Parameter名
        /// （AudioMixerGroupの名前とは異なる場合があります）
        /// </summary>
        public string exposedParameterName => _exposedParameterName;

        /// <summary>
        /// AudioMixerGroupの最大dB値
        /// </summary>
        public float maxDB => _maxDB;

        /// <summary>
        /// AudioMixerGroupの初期ボリューム（0～100）
        /// </summary>
        public int defaultVolume => _defaultVolume;

        [SerializeField, Header("紐づけるAudioMixerGroup")]
        private AudioMixerGroup _audioMixerGroup;

        [SerializeField, Header("AudioMixerのExposed Parameter名")]
        private string _exposedParameterName = "";

        [SerializeField, Header("AudioMixerGroupの最大dB値"), Range(-80f, 20f)]
        private float _maxDB = 0f;

        [SerializeField, Header("AudioMixerGroupの初期ボリューム"), Range(0, 100)]
        private int _defaultVolume = 50;
    }
}
