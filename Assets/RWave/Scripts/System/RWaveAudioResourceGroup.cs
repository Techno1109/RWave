using System;
using System.Collections.Generic;
using System.Threading;
using RWave.Data;
using RWave.Data.Interface;
using RWave.Data.Setting;
using RWave.System.Interface;
using RWave.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RWave.System
{
    /// <summary>
    /// AudioClipを読み込み、管理するClass
    /// </summary>
    public class RWaveAudioResourceGroup:IDisposable,IRWaveAudioResourceGroup
    {
        /// <summary>
        /// 読み込み済みのAudioClipを保持するDictionary
        /// </summary>
        private readonly Dictionary<string, IRWaveLoadAudioClipData> _loadAudioClipDictionary = new();

        /// <summary>
        /// 指定したAddressでAudioClipを読み込みます
        /// </summary>
        /// <param name="address"></param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(string address, CancellationToken token)
        {
            if (string.IsNullOrEmpty(address)) { return false;}
            //読み込み済みの場合は即座に終了
            if (_loadAudioClipDictionary.TryGetValue(address, out var value))
            {
                return true;
            }
            
            //読み込み
            try
            {
                var handle = Addressables.LoadAssetAsync<AudioClip>(address);
                
                //Awaitableを利用する都合上、Whileループで読み込み完了を待つ
                while(!handle.IsDone)
                {
                    //読み込みがキャンセルされた場合はここでReturn
                    if (token.IsCancellationRequested)
                    {
                        Addressables.Release(handle);
                        token.ThrowIfCancellationRequested();
                    }
                    await Awaitable.NextFrameAsync(token);
                }
                
                var addressableAudioClipData = new AddressableLoadAudioClipData(address, handle);
                //読み込み済みのDictionaryに追加
                _loadAudioClipDictionary.Add(address, addressableAudioClipData);
                RWaveLogUtility.DrawAudioClipContainerLog($"AudioClipを読み込みました。 Address:{address}");
                return true;
            }
            catch (OperationCanceledException)
            {
                // キャンセル例外はスルー
                if (token.IsCancellationRequested)
                {
                    RWaveLogUtility.LogWarning($"AudioClipの読み込みがキャンセルされました。 Address:{address}");
                    throw;
                }
                else
                {
                    RWaveLogUtility.LogError($"AudioClipの読み込み中に予期しないエラーが発生しました。 Address:{address}");
                    throw;
                }
            }
            catch (Exception e)
            {
                RWaveLogUtility.LogError($"AudioClipの読み込み中に予期しないエラーが発生しました。 Address:{address}\n{e}");
                throw;
            }
        }

        /// <summary>
        /// 指定したAddressでAudioClipを即時読み込みします
        /// </summary>
        /// <param name="addressList"></param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(IReadOnlyList<string> addressList, CancellationToken token)
        {
            if (addressList == null || addressList.Count == 0) { return false; }
            //全てのAddressを読み込む
            foreach (var address in addressList)
            {
                //キャンセルが要求された場合はここでReturn
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                //個別のAudioClip読み込み
                await LoadAudioClip(address, token);
            }
            return true;
        }

        /// <summary>
        /// 指定したAddressでAudioClipを同期読み込みします
        /// </summary>
        /// <param name="address"></param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(string address)
        {
            if (string.IsNullOrEmpty(address)) { return false;}
            //読み込み済みの場合は即座に終了
            if (_loadAudioClipDictionary.TryGetValue(address, out var value))
            {
                return true;
            }
            
            //読み込み
            try
            {
                var handle = Addressables.LoadAssetAsync<AudioClip>(address);
                handle.WaitForCompletion();
                var addressableAudioClipData = new AddressableLoadAudioClipData(address, handle);
                //読み込み済みのDictionaryに追加
                _loadAudioClipDictionary.Add(address, addressableAudioClipData);
                RWaveLogUtility.DrawAudioClipContainerLog($"AudioClipを読み込みました。 Address:{address}");
                return true;
            }
            catch (Exception e)
            {
                RWaveLogUtility.LogError($"AudioClipの読み込み中に予期しないエラーが発生しました。 Address:{address}/n{e}");
                throw;
            }
        }

        /// <summary>
        /// Addressable上のAddress指定で同期読み込みします
        /// </summary>
        /// <param name="addressList"></param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(IReadOnlyList<string> addressList)
        {
            if (addressList == null || addressList.Count == 0) { return false; }
            //全てのAddressを読み込む
            foreach (var address in addressList)
            {
                //個別のAudioClip読み込み
                if (!SyncLoadAudioClip(address))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 指定したLabelでAudioClipを同期読み込みします
        /// </summary>
        /// <param name="label"></param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioAudioClipWithAddressableLabel(string label, CancellationToken token)
        {
            var handle = Addressables.LoadResourceLocationsAsync(label, typeof(AudioClip));
            
            try
            {
                // 非同期でResource Locationsの読み込み完了を待つ
                while (!handle.IsDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        Addressables.Release(handle);
                        token.ThrowIfCancellationRequested();
                    }
                    await Awaitable.NextFrameAsync(token);
                }
                
                var resourceLocations = handle.Result;
                foreach (var resourceLocation in resourceLocations)
                {
                    // 個別のAudioClip読み込みでもキャンセルをチェック
                    token.ThrowIfCancellationRequested();
                    await LoadAudioClip(resourceLocation.PrimaryKey, token);
                }
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合はハンドルを解放して例外を再スロー
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                throw;
            }
            finally
            {
                // 正常完了時もハンドルを解放
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            return true;
        }
        
        public AudioClip GetAudioClip(string address)
        {
            //読み込まれていない場合はここでReturn
            if (!_loadAudioClipDictionary.TryGetValue(address, out var value))
            {
                RWaveLogUtility.LogAssertion($"指定したAddressのAudioClipは読み込まれていません。 Address{address}");
                return null;
            }

            return value.audioClip;
        }

        /// <summary>
        /// 読み込まれているAudioClipの数を取得します
        /// </summary>
        /// <returns>AudioClipの数</returns>
        public int GetAudioClipCount()
        {
            return _loadAudioClipDictionary?.Count ?? 0;
        }

        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="removeAddressList"></param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void Release(List<string> removeAddressList, bool forceReleaseScriptableObject = false)
        {
            foreach (var address in removeAddressList)
            {
                Release(address, forceReleaseScriptableObject);
            }
        }
        
        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="address"></param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void Release(string address, bool forceReleaseScriptableObject = false)
        {
            if(_loadAudioClipDictionary == null) { return; }

            //読み込みされていない場合、そのまま終了
            if (!_loadAudioClipDictionary.TryGetValue(address, out var releaseTarget)) { return; }

            //AudioPackから読み込まれている場合は開放できない（forceReleaseScriptableObjectがfalseの場合）
            if (releaseTarget.isLoadedFromScriptableObject && !forceReleaseScriptableObject)
            {
                RWaveLogUtility.LogWarning($"AudioPackから読み込まれたAudioClipは開放できません。 Address:{address}");
                return;
            }

            releaseTarget?.Dispose();
            _loadAudioClipDictionary.Remove(address);
            RWaveLogUtility.DrawAudioClipContainerLog($"AudioClipを開放しました。 Address:{address}");
        }

        /// <summary>
        /// 指定したLabelでAudioClipを開放します
        /// </summary>
        /// <param name="label"></param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns></returns>
        public async Awaitable ReleaseWithAddressableLabel(string label, CancellationToken token)
        {
            var handle = Addressables.LoadResourceLocationsAsync(label,typeof(AudioClip));

            try
            {
                // 非同期でResource Locationsの読み込み完了を待つ
                while (!handle.IsDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    await Awaitable.NextFrameAsync(token);
                }

                var resourceLocations = handle.Result;
                foreach (var resourceLocation in resourceLocations)
                {
                    Release(resourceLocation.PrimaryKey);
                }
            }
            finally
            {
                // 常にハンドルを解放
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }
        
        /// <summary>
        /// AudioPackから読み込む
        /// </summary>
        /// <param name="audioPack">読み込むAudioPack</param>
        public void LoadFromAudioPack(RWaveAudioPack audioPack)
        {
            if (audioPack == null)
            {
                RWaveLogUtility.LogWarning("RWaveAudioPackがnullです。");
                return;
            }

            foreach (var entity in audioPack.audioClips)
            {
                if (string.IsNullOrEmpty(entity.address))
                {
                    RWaveLogUtility.LogWarning("addressが空のデータがあります。");
                    continue;
                }

                if (entity.audioClip == null)
                {
                    RWaveLogUtility.LogWarning($"AudioClipがnullです。Address: {entity.address}");
                    continue;
                }

                if (_loadAudioClipDictionary.ContainsKey(entity.address))
                {
                    RWaveLogUtility.LogWarning($"重複するアドレスが検出されました: {entity.address}");
                    continue;
                }

                var data = new ScriptableObjectLoadAudioClipData(entity.audioClip);
                _loadAudioClipDictionary.Add(entity.address, data);
                RWaveLogUtility.DrawAudioClipContainerLog($"ScriptableObjectからAudioClipを登録しました。Address: {entity.address}");
            }
        }

        /// <summary>
        /// 読み込んでいるAudioClipをすべて開放します
        /// </summary>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseAll(bool forceReleaseScriptableObject = false)
        {
            if(_loadAudioClipDictionary == null) { return; }

            //AudioPackから読み込まれたもの以外を開放（forceReleaseScriptableObjectがfalseの場合）
            var keysToRemove = new List<string>();
            foreach (var kvp in _loadAudioClipDictionary)
            {
                if (kvp.Value.isLoadedFromScriptableObject && !forceReleaseScriptableObject)
                {
                    continue;
                }
                kvp.Value?.Dispose();
                keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                _loadAudioClipDictionary.Remove(key);
            }
        }

        public void Dispose()
        {
            ReleaseAll();
        }
    }
}