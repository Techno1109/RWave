using System;
using System.Collections.Generic;

namespace RWave.Data.VolumeData
{
    /// <summary>
    /// 全体の音量設定を保存するためのデータClass
    /// AudioMixerGroup単位の音量（マスター含む）、AudioSource単位の音量を統合して管理します
    /// </summary>
    [Serializable]
    public class RWaveVolumeData
    {
        /// <summary>
        /// AudioMixerGroup単位の音量設定リスト（マスター音量も含む）
        /// </summary>
        public IReadOnlyList<RWaveAudioMixerGroupVolumeData> audioMixerGroupVolumes => _audioMixerGroupVolumes;

        /// <summary>
        /// AudioSource単位の音量設定リスト
        /// </summary>
        public IReadOnlyList<RWaveAudioSourceVolumeData> audioSourceVolumes => _audioSourceVolumes;

        private RWaveAudioMixerGroupVolumeData[] _audioMixerGroupVolumes;
        private RWaveAudioSourceVolumeData[] _audioSourceVolumes;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="audioMixerGroupVolumes">AudioMixerGroup単位の音量設定リスト（マスター音量も含む）</param>
        /// <param name="audioSourceVolumes">AudioSource単位の音量設定リスト</param>
        public RWaveVolumeData(
            RWaveAudioMixerGroupVolumeData[] audioMixerGroupVolumes,
            RWaveAudioSourceVolumeData[] audioSourceVolumes)
        {
            _audioMixerGroupVolumes = audioMixerGroupVolumes ?? Array.Empty<RWaveAudioMixerGroupVolumeData>();
            _audioSourceVolumes = audioSourceVolumes ?? Array.Empty<RWaveAudioSourceVolumeData>();
        }
    }
}
