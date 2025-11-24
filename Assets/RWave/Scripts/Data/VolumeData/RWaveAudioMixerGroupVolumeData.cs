using System;

namespace RWave.Data.VolumeData
{
    /// <summary>
    /// AudioMixerGroup単位の音量設定を保存するためのデータClass
    /// </summary>
    [Serializable]
    public class RWaveAudioMixerGroupVolumeData
    {
        /// <summary>
        /// AudioMixerGroupの名前（識別子）
        /// </summary>
        public string audioMixerGroupName => _audioMixerGroupName;

        /// <summary>
        /// 音量（0～100）
        /// </summary>
        public float volume => _volume;

        private string _audioMixerGroupName;
        private float _volume;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="audioMixerGroupName">AudioMixerGroupの名前</param>
        /// <param name="volume">音量（0～100）</param>
        public RWaveAudioMixerGroupVolumeData(string audioMixerGroupName, float volume)
        {
            _audioMixerGroupName = audioMixerGroupName;
            _volume = volume;
        }
    }
}
