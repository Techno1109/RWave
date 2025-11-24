using UnityEngine;
using UnityEngine.Audio;

namespace RWave.System
{
    /// <summary>
    /// 音量調整を担当するClass
    /// </summary>
    public class RWaveAudioVolumeControlSystem
    {
        public float currentAudioVolume => _volume;
        private AudioMixer _audioMixer;
        private string _audioMixerGroupName;
        private float _volume;
        private float _maxDB;
        private const float MIN_DB = -80f;
        private bool _isEnabled;
        
        public void Initialize(AudioMixer audioMixer,string audioMixerGroupName,float defaultVolume = 50f, float maxDB = 0f)
        {
            _audioMixer = audioMixer;
            _audioMixerGroupName = audioMixerGroupName;
            _maxDB = maxDB;

            // Exposed Parameter Nameが空白の場合は無効化
            _isEnabled = !string.IsNullOrEmpty(_audioMixerGroupName);

            SetVolume(defaultVolume);
        }
        
        /// <summary>
        /// 音声ボリュームを設定します
        /// </summary>
        /// <param name="volume">0~100</param>
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp(volume, 0f, 100f);

            // 無効の場合はAudioMixer操作をスキップ
            if (!_isEnabled)
            {
                return;
            }

            // 0-100 を -80dB ~ maxDB に変換
            float dB;
            if (_volume <= 0.01f)
            {
                dB = MIN_DB;  // 実質ミュート
            }
            else
            {
                float normalizedVolume = _volume / 100f;  // 0.0001~1.0
                dB = MIN_DB + (_maxDB - MIN_DB) * normalizedVolume;
            }

            _audioMixer.SetFloat(_audioMixerGroupName, dB);
        }

        /// <summary>
        /// 現在の音声ボリュームをAudioMixerから取得します
        /// </summary>
        /// <returns>0~100</returns>
        public float GetVolume()
        {
            // 無効の場合は内部保持値を返す
            if (!_isEnabled)
            {
                return _volume;
            }

            if (_audioMixer.GetFloat(_audioMixerGroupName, out float dB))
            {
                // -80dB ~ maxDB を 0-100 に変換
                if (dB <= MIN_DB)
                {
                    return 0f;
                }

                float normalizedVolume = (dB - MIN_DB) / (_maxDB - MIN_DB);
                float volume = Mathf.Clamp(normalizedVolume * 100f, 0f, 100f);

                // 内部状態も更新
                _volume = volume;

                return volume;
            }

            // 取得失敗時は内部で保持している値を返す
            return _volume;
        }
    }
}