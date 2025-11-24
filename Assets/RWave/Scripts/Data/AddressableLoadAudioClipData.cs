using System;
using RWave.Data.Interface;
using RWave.Enum;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RWave.Data
{
    /// <summary>
    /// Addressableで読み込まれたAudioClipを管理するためのDataClass
    /// </summary>
    public class AddressableLoadAudioClipData : IRWaveLoadAudioClipData
    {
        /// <summary>
        /// AudioClip
        /// </summary>
        public AudioClip audioClip => _handle.Result;

        /// <summary>
        /// AudioPackから読み込まれたかどうか
        /// </summary>
        public bool isLoadedFromScriptableObject => false;

        /// <summary>
        /// 読み込み元のアドレス
        /// </summary>
        private string _address;

        /// <summary>
        /// リソースに紐づいたHandle
        /// </summary>
        private AsyncOperationHandle<AudioClip> _handle;

        public AddressableLoadAudioClipData(string address, AsyncOperationHandle<AudioClip> handle)
        {
            _address = address;
            _handle = handle;
        }
        
        public void Dispose()
        {
            //解放済みの場合は何もしない
            if (!_handle.IsValid()) { return; }
            //リソースの解放
            Addressables.Release(_handle);
        }
    }
}