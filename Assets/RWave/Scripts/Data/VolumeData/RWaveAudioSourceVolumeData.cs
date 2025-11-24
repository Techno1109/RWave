using System;

namespace RWave.Data.VolumeData
{
    /// <summary>
    /// AudioSource単位の音量設定を保存するためのデータClass
    /// </summary>
    [Serializable]
    public class RWaveAudioSourceVolumeData
    {
        /// <summary>
        /// AudioSourceの名前（識別子）
        /// </summary>
        public string audioSourceName => _audioSourceName;

        /// <summary>
        /// 音量（0～100）
        /// </summary>
        public float volume => _volume;

        private string _audioSourceName;
        private float _volume;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <param name="volume">音量（0～100）</param>
        public RWaveAudioSourceVolumeData(string audioSourceName, float volume)
        {
            _audioSourceName = audioSourceName;
            _volume = volume;
        }
    }
}
