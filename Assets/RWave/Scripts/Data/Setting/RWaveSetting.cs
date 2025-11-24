using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace RWave.Data.Setting
{
    [CreateAssetMenu(fileName = "RWaveSetting", menuName = "RWave/RWaveSetting", order = 0)]
    public class RWaveSetting : ScriptableObject
    {
        /// <summary>
        /// 紐づけるAudioMixer
        /// </summary>
        public AudioMixer audioMixer => _audioMixer;
        
        /// <summary>
        /// マスター初期ボリューム
        /// </summary>
        public int masterDefaultVolume => _masterDefaultVolume;

        /// <summary>
        /// マスターボリュームのAudioGroupName
        /// </summary>
        public AudioMixerGroup masterMixerGroup => _masterMixerGroup;

        /// <summary>
        /// マスターボリュームのExposed Parameter名
        /// </summary>
        public string masterExposedParameterName => _masterExposedParameterName;

        /// <summary>
        /// マスターボリュームの最大dB値
        /// </summary>
        public float masterMaxDB => _masterMaxDB;

        /// <summary>
        /// AudioSourceの設定
        /// </summary>
        public IReadOnlyCollection<RWaveAudioSourceSetting> audioSourceSettings => _audioSourceSettings;

        /// <summary>
        /// ScriptableObjectから読み込むAudioPackのリスト
        /// </summary>
        public IReadOnlyList<RWaveAudioPack> audioPacks => _audioPacks;

        /// <summary>
        /// AudioMixerGroup単位の音量設定
        /// </summary>
        public IReadOnlyCollection<RWaveAudioMixerGroupSetting> audioMixerGroupSettings => _audioMixerGroupSettings;

        [SerializeField,Header("紐づけるAudioMixer")]
        private AudioMixer _audioMixer;
        [SerializeField,Header("初期マスターボリューム"),Range(0,100)]
        private int _masterDefaultVolume = 50;
        [SerializeField,Header("マスターボリュームのAudioGroupName")]
        private AudioMixerGroup _masterMixerGroup;
        [SerializeField,Header("マスターボリュームのExposed Parameter名")]
        private string _masterExposedParameterName = "";
        [SerializeField,Header("マスターボリュームの最大dB値"),Range(-80f, 20f)]
        private float _masterMaxDB = 0f;
        [SerializeField,Header("AudioMixerGroup単位の音量設定")]
        private RWaveAudioMixerGroupSetting[] _audioMixerGroupSettings;
        [SerializeField,Header("AudioSourceの設定")]
        private RWaveAudioSourceSetting[] _audioSourceSettings;
        [SerializeField,Header("AudioPack(オプション)")]
        private List<RWaveAudioPack> _audioPacks = new();
    }
}