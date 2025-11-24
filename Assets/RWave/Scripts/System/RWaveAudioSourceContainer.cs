using System;
using System.Collections.Generic;
using RWave.Data;
using RWave.Data.Setting;
using RWave.Data.VolumeData;
using RWave.System.Interface;
using RWave.Utility;
using UnityEngine;
using UnityEngine.Audio;

namespace RWave.System
{
    public class RWaveAudioSourceContainer : MonoBehaviour
    {
        internal bool isInitialized => _audioSourceDictionary != null;
        internal AudioMixer audioMixer => _audioMixer;
        private AudioMixer _audioMixer;

        private Dictionary<string,IRWaveAudioSource> _audioSourceDictionary = null;
        public void Initialize(RWaveSetting setting)
        {
            _audioMixer = setting.audioMixer;

            // AudioSource単位の管理を初期化
            _audioSourceDictionary = new Dictionary<string, IRWaveAudioSource>(setting.audioSourceSettings.Count);
            foreach (var audioSourceSetting in setting.audioSourceSettings)
            {
                // 子GameObjectを作成し、名前をAudioSource名に設定
                var childGameObject = new GameObject(audioSourceSetting.audioSourceName);
                childGameObject.transform.SetParent(gameObject.transform);

                // 子GameObjectにRWaveAudioSourceコンポーネントを追加
                IRWaveAudioSource audioSource = childGameObject.AddComponent<RWaveAudioSource>();
                audioSource.Initialize(audioSourceSetting.audioMixerGroup, audioSourceSetting);
                _audioSourceDictionary.Add(audioSourceSetting.audioSourceName, audioSource);
            }
        }
        
        /// <summary>
        /// 再生を実行
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, string audioSourceName)
        {
            return Play(audioClip, audioSourceName, string.Empty);
        }

        /// <summary>
        /// 再生を実行（アドレス指定あり）
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <param name="address">AudioClipのアドレス</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, string audioSourceName, string address)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return RWaveAudioHandler.Invalid;
            }

            var soundPlayer = GetAudioSource(audioSourceName);
            if (soundPlayer == null)
            {
                return RWaveAudioHandler.Invalid;
            }

            return soundPlayer.Play(audioClip, address);
        }

        /// <summary>
        /// 再生を実行（再生時音量指定あり）
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <param name="playVolume">再生時の音量（0～100、nullの場合はBaseVolumeを使用）</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, string audioSourceName, float? playVolume)
        {
            return Play(audioClip, audioSourceName, string.Empty, playVolume);
        }

        /// <summary>
        /// 再生を実行（アドレス・再生時音量指定あり）
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <param name="address">AudioClipのアドレス</param>
        /// <param name="playVolume">再生時の音量（0～100、nullの場合はBaseVolumeを使用）</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, string audioSourceName, string address, float? playVolume)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return RWaveAudioHandler.Invalid;
            }

            var soundPlayer = GetAudioSource(audioSourceName);
            if (soundPlayer == null)
            {
                return RWaveAudioHandler.Invalid;
            }

            return soundPlayer.Play(audioClip, address, playVolume);
        }

        /// <summary>
        /// 指定したAudioSourceNameの再生を止める
        /// </summary>
        /// <param name="audioSourceName"></param>
        public void Stop(string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return;
            }

            var soundPlayer = GetAudioSource(audioSourceName);
            soundPlayer?.Stop();
        }

        /// <summary>
        /// ハンドラー指定で再生を停止する
        /// </summary>
        /// <param name="handler">停止する再生のハンドラー</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        public void Stop(RWaveAudioHandler handler, string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return;
            }

            var soundPlayer = GetAudioSource(audioSourceName);
            soundPlayer?.Stop(handler);
        }

        /// <summary>
        /// アドレス指定で該当する全再生を停止する
        /// </summary>
        /// <param name="address">AudioClipのアドレス</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        public void Stop(string address, string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return;
            }

            var soundPlayer = GetAudioSource(audioSourceName);
            soundPlayer?.Stop(address);
        }

        /// <summary>
        /// 指定したAudioClipTypeの再生を強制的に止める
        /// </summary>
        /// <param name="audioSourceName"></param>
        public void ForceStop(string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return;
            }
            
            var soundPlayer = GetAudioSource(audioSourceName);
            soundPlayer?.ForceStop();
        }
        
        /// <summary>
        /// AudioSourceを取得する
        /// </summary>
        /// <param name="audioSourceName"></param>
        /// <returns></returns>
        public IRWaveAudioSource GetAudioSource(string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return null;
            }

            if (_audioSourceDictionary.TryGetValue(audioSourceName, value: out var source)){ return source;}
            RWaveLogUtility.LogAssertion($"指定したAudioSourceは存在しません。AudioSourceName:{audioSourceName}");
            return null;
        }

        /// <summary>
        /// 登録されているすべてのAudioSource名を取得する
        /// </summary>
        /// <returns>AudioSource名のコレクション</returns>
        public IEnumerable<string> GetAllAudioSourceNames()
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return Array.Empty<string>();
            }

            return _audioSourceDictionary.Keys;
        }

        /// <summary>
        /// すべてのAudioSourceの音量データを出力する
        /// </summary>
        /// <returns>AudioSource音量データの配列</returns>
        public RWaveAudioSourceVolumeData[] ExportAudioSourceVolumes()
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return Array.Empty<RWaveAudioSourceVolumeData>();
            }

            var volumeDataList = new RWaveAudioSourceVolumeData[_audioSourceDictionary.Count];
            var index = 0;

            foreach (var kvp in _audioSourceDictionary)
            {
                var volume = kvp.Value?.GetVolume() ?? 0f;
                volumeDataList[index] = new RWaveAudioSourceVolumeData(kvp.Key, volume);
                index++;
            }

            return volumeDataList;
        }

        /// <summary>
        /// AudioSourceの音量データを反映する
        /// </summary>
        /// <param name="audioSourceVolumes">適用するAudioSource音量データのリスト</param>
        public void ApplyAudioSourceVolumes(IReadOnlyList<RWaveAudioSourceVolumeData> audioSourceVolumes)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return;
            }

            if (audioSourceVolumes == null)
            {
                return;
            }

            for (int i = 0; i < audioSourceVolumes.Count; i++)
            {
                var audioSourceVolume = audioSourceVolumes[i];
                if (_audioSourceDictionary.TryGetValue(audioSourceVolume.audioSourceName, out var audioSource))
                {
                    audioSource.SetVolume(audioSourceVolume.volume);
                }
            }
        }
    }
}