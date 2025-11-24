using System;
using UnityEngine;

namespace RWave.Data.Interface
{
    /// <summary>
    /// 読み込まれたAudioClipを管理するための共通インターフェース
    /// </summary>
    public interface IRWaveLoadAudioClipData : IDisposable
    {
        /// <summary>
        /// AudioClip
        /// </summary>
        AudioClip audioClip { get; }

        /// <summary>
        /// AudioPackから読み込まれたかどうか
        /// </summary>
        bool isLoadedFromScriptableObject { get; }
    }
}
