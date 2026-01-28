using System.Collections.Generic;
using System.Threading;
using RWave.Data;
using RWave.Data.Setting;
using RWave.Data.VolumeData;
using RWave.System.Interface;
using RWave.Utility;
using UnityEngine;

namespace RWave.System
{
    /// <summary>
    /// 全般の機能を纏めるStaticClass
    /// </summary>
    public class RWaveSoundManager:MonoBehaviour
    {
        /// <summary>
        /// RWaveSoundManagerのシングルトンインスタンス
        /// </summary>
        public static RWaveSoundManager Instance => s_instance;

        public static bool isInitialized => s_instance && s_instance._isInitialized;
        private static RWaveSoundManager s_instance;

        [SerializeField,Header("Start時に自動的に初期化処理を行います")]
        private bool _autoInitializeOnStart = true;
        [SerializeField,Header("Setting")]
        private RWaveSetting _setting;
        private bool _isValid = false;
        private bool _isInitialized = false;
        private RWaveAudioSourceContainer _audioSourceContainer;
        private RWaveAudioResourceContainer _audioResourceContainer;

        /// <summary>
        /// AudioMixerGroup名とRWaveAudioVolumeControlSystemのマッピング
        /// </summary>
        private Dictionary<string, RWaveAudioVolumeControlSystem> _audioMixerGroupVolumeControllers;

        #region Lifecycle Methods

        /// <summary>
        /// PlayMode突入時に初期化する必要のあるStaticFieldを初期化する
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeOnStartPlayMode()
        {
            s_instance = null;
        }

        private void Start()
        {
            if (s_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            _isValid = true;
            if (!_autoInitializeOnStart) { return; }
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized) { return; }
            _audioSourceContainer = this.gameObject.AddComponent<RWaveAudioSourceContainer>();
            _audioSourceContainer.Initialize(_setting);
            _audioResourceContainer = new RWaveAudioResourceContainer();

            // AudioPacksから自動読み込み
            if (_setting.audioPacks != null && _setting.audioPacks.Count > 0)
            {
                foreach (var audioPack in _setting.audioPacks)
                {
                    _audioResourceContainer.RegisterAudioPack(audioPack);
                }
            }

            // AudioMixerGroup単位の音量管理システムを初期化
            _audioMixerGroupVolumeControllers = new Dictionary<string, RWaveAudioVolumeControlSystem>();

            // Master音量を追加
            var masterVolumeController = new RWaveAudioVolumeControlSystem();
            masterVolumeController.Initialize(_audioSourceContainer.audioMixer, _setting.masterExposedParameterName, _setting.masterDefaultVolume, _setting.masterMaxDB);
            _audioMixerGroupVolumeControllers.Add(_setting.masterMixerGroup.name, masterVolumeController);

            // その他のAudioMixerGroupを追加
            if (_setting.audioMixerGroupSettings != null)
            {
                foreach (var mixerGroupSetting in _setting.audioMixerGroupSettings)
                {
                    if (mixerGroupSetting.audioMixerGroup == null) continue;

                    var volumeController = new RWaveAudioVolumeControlSystem();
                    volumeController.Initialize(
                        mixerGroupSetting.audioMixerGroup.audioMixer,
                        mixerGroupSetting.exposedParameterName,
                        mixerGroupSetting.defaultVolume,
                        mixerGroupSetting.maxDB
                    );
                    _audioMixerGroupVolumeControllers.Add(mixerGroupSetting.audioMixerGroup.name, volumeController);
                }
            }

            //シーン移行後に破棄されないようにする
            DontDestroyOnLoad(this.gameObject);
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_isValid)
            {
                s_instance = null;
            }
        }

        #endregion

        #region Playback Control Methods

        /// <summary>
        /// 再生を実行
        /// </summary>
        /// <param name="audioAddress">再生するAudioのAddress</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(string audioAddress, string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return RWaveAudioHandler.Invalid;
            }
            var audioClip = _audioResourceContainer.GetAudioClip(audioAddress);
            if (audioClip == null)
            {
                RWaveLogUtility.LogAssertion($"AudioClipが読み込まれていません。Address:{audioAddress}");
                return RWaveAudioHandler.Invalid;
            }
            return _audioSourceContainer.Play(audioClip, audioSourceName, audioAddress);
        }

        /// <summary>
        /// 再生を実行
        /// </summary>
        /// <param name="audioAddress">再生するAudioのAddress</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <param name="audioResourceGroup">AudioResourceのグループ名</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(string audioAddress, string audioSourceName, string audioResourceGroup)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return RWaveAudioHandler.Invalid;
            }
            var audioClip = _audioResourceContainer.GetAudioClip(audioAddress, audioResourceGroup);
            if (audioClip == null)
            {
                RWaveLogUtility.LogAssertion($"AudioClipが読み込まれていません。Address:{audioAddress}");
                return RWaveAudioHandler.Invalid;
            }
            return _audioSourceContainer.Play(audioClip, audioSourceName, audioAddress);
        }

        /// <summary>
        /// 再生を実行（再生時音量指定あり）
        /// </summary>
        /// <param name="audioAddress">再生するAudioのAddress</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <param name="playVolume">再生時の音量（0～100、nullの場合はBaseVolumeを使用）</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(string audioAddress, string audioSourceName, float? playVolume)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return RWaveAudioHandler.Invalid;
            }
            var audioClip = _audioResourceContainer.GetAudioClip(audioAddress);
            if (audioClip == null)
            {
                RWaveLogUtility.LogAssertion($"AudioClipが読み込まれていません。Address:{audioAddress}");
                return RWaveAudioHandler.Invalid;
            }
            return _audioSourceContainer.Play(audioClip, audioSourceName, audioAddress, playVolume);
        }

        /// <summary>
        /// 再生を実行（再生時音量指定あり）
        /// </summary>
        /// <param name="audioAddress">再生するAudioのAddress</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <param name="audioResourceGroup">AudioResourceのグループ名</param>
        /// <param name="playVolume">再生時の音量（0～100、nullの場合はBaseVolumeを使用）</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(string audioAddress, string audioSourceName, string audioResourceGroup, float? playVolume)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return RWaveAudioHandler.Invalid;
            }
            var audioClip = _audioResourceContainer.GetAudioClip(audioAddress, audioResourceGroup);
            if (audioClip == null)
            {
                RWaveLogUtility.LogAssertion($"AudioClipが読み込まれていません。Address:{audioAddress}");
                return RWaveAudioHandler.Invalid;
            }
            return _audioSourceContainer.Play(audioClip, audioSourceName, audioAddress, playVolume);
        }

        /// <summary>
        /// 指定したAudioSourceの再生を止める
        /// </summary>
        /// <param name="audioSourceName"></param>
        public void Stop(string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return;
            }
            _audioSourceContainer.Stop(audioSourceName);
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
            _audioSourceContainer.Stop(handler, audioSourceName);
        }

        /// <summary>
        /// アドレス指定で該当する全再生を停止する
        /// </summary>
        /// <param name="audioAddress">AudioClipのアドレス</param>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        public void Stop(string audioAddress, string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return;
            }
            _audioSourceContainer.Stop(audioAddress, audioSourceName);
        }

        /// <summary>
        /// 指定したAudioSourceの現在再生中の音をすべて即座に停止する。
        /// </summary>
        /// <param name="audioSourceName"></param>
        public void ForceStop(string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("AudioSourceContainerが初期化されていません。");
                return;
            }
            _audioSourceContainer.ForceStop(audioSourceName);
        }

        #endregion

        #region AudioSource Methods

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
            var audioSource = _audioSourceContainer.GetAudioSource(audioSourceName);
            return audioSource;
        }

        #endregion

        #region Volume Control Methods

        /// <summary>
        /// 指定したAudioMixerGroupの音量を設定する
        /// </summary>
        /// <param name="audioMixerGroupName">AudioMixerGroupの名前</param>
        /// <param name="volume">音量（0～100）</param>
        public void SetAudioMixerGroupVolume(string audioMixerGroupName, float volume)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }

            if (_audioMixerGroupVolumeControllers.TryGetValue(audioMixerGroupName, out var controller))
            {
                controller.SetVolume(volume);
                return;
            }

            RWaveLogUtility.LogAssertion($"指定したAudioMixerGroupは存在しません。AudioMixerGroupName:{audioMixerGroupName}");
        }

        /// <summary>
        /// 指定したAudioMixerGroupの現在の音量を取得する
        /// </summary>
        /// <param name="audioMixerGroupName">AudioMixerGroupの名前</param>
        /// <returns>音量（0～100）、存在しない場合は0</returns>
        public float GetAudioMixerGroupVolume(string audioMixerGroupName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return 0f;
            }

            if (_audioMixerGroupVolumeControllers.TryGetValue(audioMixerGroupName, out var controller))
            {
                return controller.GetVolume();
            }

            RWaveLogUtility.LogAssertion($"指定したAudioMixerGroupは存在しません。AudioMixerGroupName:{audioMixerGroupName}");
            return 0f;
        }

        /// <summary>
        /// 指定したAudioSourceの音量を設定する
        /// 再生中のAudioSourceにも即座に反映されます
        /// </summary>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <param name="volume">音量（0～100）</param>
        public void SetAudioSourceVolume(string audioSourceName, float volume)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }

            var audioSource = _audioSourceContainer.GetAudioSource(audioSourceName);
            if (audioSource == null)
            {
                RWaveLogUtility.LogAssertion($"指定したAudioSourceは存在しません。AudioSourceName:{audioSourceName}");
                return;
            }

            audioSource.SetVolume(volume);
        }

        /// <summary>
        /// 指定したAudioSourceの現在の音量を取得する
        /// </summary>
        /// <param name="audioSourceName">AudioSourceの名前</param>
        /// <returns>音量（0～100）、存在しない場合は0</returns>
        public float GetAudioSourceVolume(string audioSourceName)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return 0f;
            }

            var audioSource = _audioSourceContainer.GetAudioSource(audioSourceName);
            if (audioSource == null)
            {
                RWaveLogUtility.LogAssertion($"指定したAudioSourceは存在しません。AudioSourceName:{audioSourceName}");
                return 0f;
            }

            return audioSource.GetVolume();
        }

        #endregion

        #region Volume Data Methods

        /// <summary>
        /// 現在の音量設定をRWaveVolumeDataとして出力する
        /// </summary>
        /// <returns>現在の音量設定データ</returns>
        public RWaveVolumeData ExportVolumeData()
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return null;
            }

            // AudioMixerGroup単位の音量を収集（マスター音量も含む）
            var mixerGroupVolumes = new RWaveAudioMixerGroupVolumeData[_audioMixerGroupVolumeControllers.Count];
            var index = 0;
            foreach (var kvp in _audioMixerGroupVolumeControllers)
            {
                mixerGroupVolumes[index] = new RWaveAudioMixerGroupVolumeData(kvp.Key, kvp.Value.currentAudioVolume);
                index++;
            }

            // AudioSource単位の音量を収集（AudioSourceContainerから出力）
            var audioSourceVolumes = _audioSourceContainer.ExportAudioSourceVolumes();

            return new RWaveVolumeData(mixerGroupVolumes, audioSourceVolumes);
        }

        /// <summary>
        /// RWaveVolumeDataから音量設定を反映する
        /// </summary>
        /// <param name="volumeData">適用する音量設定データ</param>
        public void ApplyVolumeData(RWaveVolumeData volumeData)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }

            if (volumeData == null)
            {
                RWaveLogUtility.LogAssertion("volumeDataがNullです。");
                return;
            }

            // AudioMixerGroup単位の音量を設定（マスター音量も含む）
            if (volumeData.audioMixerGroupVolumes != null)
            {
                for (int i = 0; i < volumeData.audioMixerGroupVolumes.Count; i++)
                {
                    var mixerGroupVolume = volumeData.audioMixerGroupVolumes[i];
                    if (_audioMixerGroupVolumeControllers.ContainsKey(mixerGroupVolume.audioMixerGroupName))
                    {
                        SetAudioMixerGroupVolume(mixerGroupVolume.audioMixerGroupName, mixerGroupVolume.volume);
                    }
                }
            }

            // AudioSource単位の音量を設定（AudioSourceContainerで反映）
            _audioSourceContainer.ApplyAudioSourceVolumes(volumeData.audioSourceVolumes);
        }

        #endregion

        #region AudioResource Methods

        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを取得する
        /// </summary>
        /// <param name="address">AudioClipのAddress</param>
        /// <returns>読み込まれているAudioClip、存在しない場合はnull</returns>
        public AudioClip GetAudioClip(string address)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return null;
            }
            return _audioResourceContainer.GetAudioClip(address);
        }

        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを取得する
        /// </summary>
        /// <param name="address">AudioClipのAddress</param>
        /// <param name="audioResourceGroup">AudioResourceGroup名</param>
        /// <returns>読み込まれているAudioClip、存在しない場合はnull</returns>
        public AudioClip GetAudioClip(string address, string audioResourceGroup)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return null;
            }
            return _audioResourceContainer.GetAudioClip(address, audioResourceGroup);
        }

        /// <summary>
        /// 指定したAddressでAudioClipを読み込む
        /// </summary>
        /// <param name="address">AudioClipのAddress</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(string address, CancellationToken token)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return await _audioResourceContainer.LoadAudioClip(address, token);
        }

        /// <summary>
        /// 指定したAddressでAudioClipを読み込む
        /// </summary>
        /// <param name="address">AudioClipのAddress</param>
        /// <param name="audioResourceGroup">指定したAddressに結びつけるグループ名</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(string address, string audioResourceGroup, CancellationToken token)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return await _audioResourceContainer.LoadAudioClip(address, audioResourceGroup, token);
        }

        /// <summary>
        /// 指定したAddressリストでAudioClipを読み込む
        /// </summary>
        /// <param name="addressList">AudioClipのAddressリスト</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(IReadOnlyList<string> addressList, CancellationToken token)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return await _audioResourceContainer.LoadAudioClip(addressList, token);
        }

        /// <summary>
        /// 指定したAddressリストでAudioClipを読み込む
        /// </summary>
        /// <param name="addressList">AudioClipのAddressリスト</param>
        /// <param name="audioResourceGroup">読み込んだAudioClipに結びつけるグループ名</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(IReadOnlyList<string> addressList, string audioResourceGroup, CancellationToken token)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return await _audioResourceContainer.LoadAudioClip(addressList, audioResourceGroup, token);
        }

        /// <summary>
        /// 指定したAddressでAudioClipを同期読み込み
        /// </summary>
        /// <param name="address">AudioClipのAddress</param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(string address)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return _audioResourceContainer.SyncLoadAudioClip(address);
        }

        /// <summary>
        /// 指定したAddressでAudioClipを同期読み込み
        /// </summary>
        /// <param name="address">AudioClipのAddress</param>
        /// <param name="audioResourceGroup">読み込んだAudioClipに結びつけるグループ名</param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(string address, string audioResourceGroup)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return _audioResourceContainer.SyncLoadAudioClip(address, audioResourceGroup);
        }

        /// <summary>
        /// 指定したAddressリストでAudioClipを同期読み込み
        /// </summary>
        /// <param name="addressList">AudioClipのAddressリスト</param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(IReadOnlyList<string> addressList)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return _audioResourceContainer.SyncLoadAudioClip(addressList);
        }

        /// <summary>
        /// 指定したAddressリストでAudioClipを同期読み込み
        /// </summary>
        /// <param name="addressList">AudioClipのAddressリスト</param>
        /// <param name="audioResourceGroup">読み込んだAudioClipに結びつけるグループ名</param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(IReadOnlyList<string> addressList, string audioResourceGroup)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return _audioResourceContainer.SyncLoadAudioClip(addressList, audioResourceGroup);
        }

        /// <summary>
        /// 指定したLabelでAudioClipを読み込みます
        /// </summary>
        /// <param name="label">Addressable Label</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClipWithAddressableLabel(string label, CancellationToken token)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return await _audioResourceContainer.LoadAudioAudioClipWithAddressableLabel(label, token);
        }

        /// <summary>
        /// 指定したLabelでAudioClipを読み込みます
        /// </summary>
        /// <param name="label">Addressable Label</param>
        /// <param name="audioResourceGroup">指定したLabelで読み込むアセットに結びつけるグループ名</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClipWithAddressableLabel(string label, string audioResourceGroup, CancellationToken token)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return false;
            }
            return await _audioResourceContainer.LoadAudioAudioClipWithAddressableLabel(label, audioResourceGroup, token);
        }

        /// <summary>
        /// AudioPackを登録する
        /// </summary>
        /// <param name="audioPack">登録するAudioPack</param>
        /// <param name="audioResourceGroup">登録先のAudioResourceGroup(空文字列の場合は共通グループに追加)</param>
        public void RegisterAudioPack(RWaveAudioPack audioPack, string audioResourceGroup = "")
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            _audioResourceContainer.RegisterAudioPack(audioPack, audioResourceGroup);
        }

        /// <summary>
        /// AudioResourceGroupに紐づけられていないAudioClipをすべて開放します
        /// </summary>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseAudioClip(bool forceReleaseScriptableObject = false)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            _audioResourceContainer.Release(forceReleaseScriptableObject);
        }

        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="address">AudioClipのAddress</param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseAudioClip(string address, bool forceReleaseScriptableObject = false)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            _audioResourceContainer.Release(address, forceReleaseScriptableObject);
        }

        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="address">AudioClipのAddress</param>
        /// <param name="audioResourceGroup">AudioResourceGroup名</param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseAudioClip(string address, string audioResourceGroup, bool forceReleaseScriptableObject = false)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            _audioResourceContainer.Release(address, audioResourceGroup, forceReleaseScriptableObject);
        }

        /// <summary>
        /// 指定したAddressリストで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="removeAddressList">AudioClipのAddressリスト</param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseAudioClip(List<string> removeAddressList, bool forceReleaseScriptableObject = false)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            _audioResourceContainer.Release(removeAddressList, forceReleaseScriptableObject);
        }

        /// <summary>
        /// 指定したAddressリストで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="removeAddressList">AudioClipのAddressリスト</param>
        /// <param name="audioResourceGroup">AudioResourceGroup名</param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseAudioClip(List<string> removeAddressList, string audioResourceGroup, bool forceReleaseScriptableObject = false)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            _audioResourceContainer.Release(removeAddressList, audioResourceGroup, forceReleaseScriptableObject);
        }

        /// <summary>
        /// 指定したLabelでAudioClipを開放します
        /// </summary>
        /// <param name="label">Addressable Label</param>
        /// <param name="token">キャンセルトークン</param>
        public async Awaitable ReleaseAudioClipWithAddressableLabel(string label, CancellationToken token)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            await _audioResourceContainer.ReleaseWithAddressableLabel(label, token);
        }

        /// <summary>
        /// 指定したLabelでAudioClipを開放します
        /// </summary>
        /// <param name="label">Addressable Label</param>
        /// <param name="audioResourceGroup">開放する予定のAudioClipに紐付けられているグループ名</param>
        /// <param name="token">キャンセルトークン</param>
        public async Awaitable ReleaseAudioClipWithAddressableLabel(string label, string audioResourceGroup, CancellationToken token)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            await _audioResourceContainer.ReleaseWithAddressableLabel(label, audioResourceGroup, token);
        }

        /// <summary>
        /// 指定したAudioResourceGroupに紐付けられているAudioClipをすべて開放します
        /// </summary>
        /// <param name="audioResourceGroup">AudioResourceGroup名</param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseAudioClipWithAudioResourceGroup(string audioResourceGroup, bool forceReleaseScriptableObject = false)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            _audioResourceContainer.ReleaseWithAudioResourceGroup(audioResourceGroup, forceReleaseScriptableObject);
        }

        /// <summary>
        /// 読み込んでいるAudioClipをすべて開放します
        /// </summary>
        /// <param name="ignoreCommon">Commonに読み込まれているAudioClipも開放するかどうか</param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseAllAudioClip(bool ignoreCommon = false, bool forceReleaseScriptableObject = false)
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            _audioResourceContainer.AllRelease(ignoreCommon, forceReleaseScriptableObject);
        }

        /// <summary>
        /// 要素数が0のAudioResourceGroupを削除します
        /// </summary>
        public void RemoveEmptyAudioResourceGroups()
        {
            if (!isInitialized)
            {
                RWaveLogUtility.LogAssertion("RWaveSoundManagerが初期化されていません。");
                return;
            }
            _audioResourceContainer.RemoveEmptyAudioResourceGroups();
        }

        #endregion
    }
}
