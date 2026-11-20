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

            // 0-100 を対数スケールで -80dB ~ maxDB に変換
            // dBは対数単位のため、線形変換では中間音量で急激に音が小さくなる
            // 100%→maxDB、50%→約-6dB（振幅1/2）、10%→-20dB
            float dB;
            if (_volume <= 0.01f)
            {
                dB = MIN_DB;  // 実質ミュート
            }
            else
            {
                dB = Mathf.Max(_maxDB + Mathf.Log10(_volume / 100f) * 20f, MIN_DB);
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
                // -80dB ~ maxDB を対数スケールの逆変換で 0-100 に戻す（SetVolumeと対になる変換）
                if (dB <= MIN_DB)
                {
                    return 0f;
                }

                float volume = Mathf.Clamp(Mathf.Pow(10f, (dB - _maxDB) / 20f) * 100f, 0f, 100f);

                // 内部状態も更新
                _volume = volume;

                return volume;
            }

            // 取得失敗時は内部で保持している値を返す
            return _volume;
        }
    }
}