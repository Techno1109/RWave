using System;
using UnityEngine;

namespace RWave.Data.Setting
{
    [Serializable]
    public class RWaveAudioPackEntity
    {
        /// <summary>
        /// 識別用のアドレス
        /// </summary>
        public string address
        {
            get
            {
                if (string.IsNullOrEmpty(_address))
                {
                    return _audioClip != null ? _audioClip.name : string.Empty;
                }
                return _address;
            }
        }

        /// <summary>
        /// AudioClip
        /// </summary>
        public AudioClip audioClip => _audioClip;

        [SerializeField, Header("識別用のアドレス")]
        private string _address;

        [SerializeField, Header("AudioClip")]
        private AudioClip _audioClip;
    }
}
