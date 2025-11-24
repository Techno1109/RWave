using System.Collections.Generic;
using UnityEngine;

namespace RWave.Data.Setting
{
    /// <summary>
    /// AudioClipをまとめたパック
    /// </summary>
    [CreateAssetMenu(fileName = "RWaveAudioPack", menuName = "RWave/AudioPack")]
    public class RWaveAudioPack : ScriptableObject
    {
        /// <summary>
        /// AudioClipのリスト
        /// </summary>
        public IReadOnlyList<RWaveAudioPackEntity> audioClips => _audioClips;

        [SerializeField, Header("AudioClipのリスト")]
        private List<RWaveAudioPackEntity> _audioClips = new();
    }
}
