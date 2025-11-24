using System;
using RWave.Data.Interface;
using UnityEngine;

namespace RWave.Data
{
    /// <summary>
    /// ScriptableObjectで読み込まれたAudioClipを管理するためのDataClass
    /// </summary>
    public class ScriptableObjectLoadAudioClipData : IRWaveLoadAudioClipData
    {
        /// <summary>
        /// AudioClip
        /// </summary>
        public AudioClip audioClip { get; }

        /// <summary>
        /// AudioPackから読み込まれたかどうか
        /// </summary>
        public bool isLoadedFromScriptableObject => true;

        public ScriptableObjectLoadAudioClipData(AudioClip audioClip)
        {
            this.audioClip = audioClip;
        }

        public void Dispose()
        {
            // ScriptableObjectの場合は解放処理不要
        }
    }
}
