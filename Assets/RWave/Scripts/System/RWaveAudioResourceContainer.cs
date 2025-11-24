using System.Collections.Generic;
using System.Threading;
using RWave.Data.Setting;
using RWave.System.Interface;
using RWave.Utility;
using UnityEngine;

namespace RWave.System
{
    internal class RWaveAudioResourceContainer
    {
        private readonly Dictionary<string, IRWaveAudioResourceGroup> _audioResourceContainers = new Dictionary<string, IRWaveAudioResourceGroup>();
        private readonly RWaveAudioResourceGroup _commonAudioResourceGroup = new RWaveAudioResourceGroup();
        
        /// <summary>
        /// 指定したAddressでAudioClipを読み込む
        /// </summary>
        /// <param name="address"></param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(string address,CancellationToken token)
        {
            if (string.IsNullOrEmpty(address))
            {
                DrawInvalidAudioResourceGroupLog();
                return false;
            }
            
            //読み込み
            return await GetAudioResourceGroup("").LoadAudioClip(address, token);
        }
        
        /// <summary>
        /// 指定したAddressでAudioClipを読み込む
        /// </summary>
        /// <param name="address"></param>
        /// <param name="audioResourceGroup">指定したAddressに結びつけるグループ名</param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(string address,string audioResourceGroup,CancellationToken token)
        {
            if(!IsValid(address, audioResourceGroup)) { return false; }
            //読み込み
            return await GetAudioResourceGroup(audioResourceGroup).LoadAudioClip(address, token);
        }

        /// <summary>
        /// 指定したAddressでAudioClipを読み込む
        /// </summary>
        /// <param name="addressList"></param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(IReadOnlyList<string> addressList, CancellationToken token)
        {
            return await GetAudioResourceGroup("").LoadAudioClip(addressList, token);
        }
        
        /// <summary>
        /// 指定したAddressでAudioClipを読み込む
        /// </summary>
        /// <param name="addressList"></param>
        /// <param name="audioResourceGroup">読み込んだAudioClipに結びつけるグループ名</param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioClip(IReadOnlyList<string> addressList, string audioResourceGroup, CancellationToken token)
        {
            if (string.IsNullOrEmpty(audioResourceGroup))
            {
                DrawInvalidAudioResourceGroupLog();
                return false;
            }
            return await GetAudioResourceGroup(audioResourceGroup).LoadAudioClip(addressList, token);
        }


        /// <summary>
        /// 指定したAddressでAudioClipを同期読み込み
        /// </summary>
        /// <param name="address"></param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                DrawInvalidAudioResourceGroupLog();
                return false;
            }
            
            //読み込み
            return GetAudioResourceGroup("").SyncLoadAudioClip(address);
        }
        
        /// <summary>
        /// 指定したAddressでAudioClipを同期読み込み
        /// </summary>
        /// <param name="address"></param>
        /// <param name="audioResourceGroup">読み込んだAudioClipに結びつけるグループ名</param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(string address, string audioResourceGroup)
        {
            if(!IsValid(address, audioResourceGroup)) { return false; }
            //読み込み
            return GetAudioResourceGroup(audioResourceGroup).SyncLoadAudioClip(address);
        }
        
        /// <summary>
        /// 指定したAddressでAudioClipを同期読み込み
        /// </summary>
        /// <param name="addressList"></param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(IReadOnlyList<string> addressList)
        {
            return GetAudioResourceGroup("").SyncLoadAudioClip(addressList);
        }
        
        /// <summary>
        /// 指定したAddressでAudioClipを同期読み込み
        /// </summary>
        /// <param name="addressList"></param>
        /// <param name="audioResourceGroup">読み込んだAudioClipに結びつけるグループ名</param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(IReadOnlyList<string> addressList, string audioResourceGroup)
        {
            if (string.IsNullOrEmpty(audioResourceGroup))
            {
                DrawInvalidAudioResourceGroupLog();
                return false;
            }
            return GetAudioResourceGroup(audioResourceGroup).SyncLoadAudioClip(addressList);
        }
        
        /// <summary>
        /// 指定したLabelでAudioClipを読み込みます
        /// </summary>
        /// <param name="label"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioAudioClipWithAddressableLabel(string label,CancellationToken token)
        {
            if (string.IsNullOrEmpty(label))
            {
                DrawInvalidLabelLog();
                return false;
            }
            return await GetAudioResourceGroup("").LoadAudioAudioClipWithAddressableLabel(label, token);
        }
        
        /// <summary>
        /// 指定したLabelでAudioClipを読み込みます
        /// </summary>
        /// <param name="label"></param>
        /// <param name="audioResourceGroup">指定したLabelで読み込むアセットに結びつけるグループ名</param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public async Awaitable<bool> LoadAudioAudioClipWithAddressableLabel(string label,string audioResourceGroup,CancellationToken token)
        {
            if (string.IsNullOrEmpty(audioResourceGroup))
            {
                DrawInvalidAudioResourceGroupLog();
                return false;
            }
            if (string.IsNullOrEmpty(label))
            {
                DrawInvalidLabelLog();
                return false;
            }
            return await GetAudioResourceGroup(audioResourceGroup).LoadAudioAudioClipWithAddressableLabel(label, token);
        }
        
        public AudioClip GetAudioClip(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                DrawInvalidAddressLog();
                return null;
            }
            return GetAudioResourceGroup("").GetAudioClip(address);
        }
        
        public AudioClip GetAudioClip(string address,string audioResourceGroup)
        {
            if(!IsValid(address, audioResourceGroup)) { return null; }
            return GetAudioResourceGroup(audioResourceGroup).GetAudioClip(address);
        }
        
        /// <summary>
        /// AudioPackを登録する
        /// </summary>
        /// <param name="audioPack">登録するAudioPack</param>
        /// <param name="audioResourceGroup">登録先のAudioResourceGroup(空文字列の場合は_commonAudioResourceGroupに追加)</param>
        public void RegisterAudioPack(RWaveAudioPack audioPack, string audioResourceGroup = "")
        {
            if (audioPack == null)
            {
                RWaveLogUtility.LogWarning("RWaveAudioPackがnullです。");
                return;
            }

            GetAudioResourceGroup(audioResourceGroup).LoadFromAudioPack(audioPack);
        }

        /// <summary>
        /// AudioResourceGroupに紐づけられていないAudioClipをすべて開放します
        /// </summary>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void Release(bool forceReleaseScriptableObject = false)
        {
            GetAudioResourceGroup("").ReleaseAll(forceReleaseScriptableObject);
        }

        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="address"></param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void Release(string address, bool forceReleaseScriptableObject = false)
        {
            if (string.IsNullOrEmpty(address))
            {
                DrawInvalidAddressLog();
                return;
            }
            GetAudioResourceGroup("").Release(address, forceReleaseScriptableObject);
        }

        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="address"></param>
        /// <param name="audioResourceGroup"></param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void Release(string address, string audioResourceGroup, bool forceReleaseScriptableObject = false)
        {
            if (!IsValid(address, audioResourceGroup)) { return; }
            GetAudioResourceGroup(audioResourceGroup).Release(address, forceReleaseScriptableObject);
        }

        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="removeAddressList"></param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void Release(List<string> removeAddressList, bool forceReleaseScriptableObject = false)
        {
            GetAudioResourceGroup("").Release(removeAddressList, forceReleaseScriptableObject);
        }

        /// <summary>
        /// 指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="removeAddressList"></param>
        /// <param name="audioResourceGroup"></param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void Release(List<string> removeAddressList, string audioResourceGroup, bool forceReleaseScriptableObject = false)
        {
            if(string.IsNullOrEmpty(audioResourceGroup))
            {
                DrawInvalidAudioResourceGroupLog();
                return;
            }
            GetAudioResourceGroup(audioResourceGroup).Release(removeAddressList, forceReleaseScriptableObject);
        }
        
        /// <summary>
        /// 指定したLabelでAudioClipを開放します
        /// </summary>
        /// <param name="label"></param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns></returns>
        public async Awaitable ReleaseWithAddressableLabel(string label, CancellationToken token)
        {
            if (string.IsNullOrEmpty(label))
            {
                DrawInvalidLabelLog();
                return;
            }
            await GetAudioResourceGroup("").ReleaseWithAddressableLabel(label, token);
        }

        /// <summary>
        /// 指定したLabelでAudioClipを開放します
        /// </summary>
        /// <param name="label"></param>
        /// <param name="audioResourceGroup">開放する予定のAudioClipに紐付けられているグループ名</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns></returns>
        public async Awaitable ReleaseWithAddressableLabel(string label, string audioResourceGroup, CancellationToken token)
        {
            if (string.IsNullOrEmpty(audioResourceGroup))
            {
                DrawInvalidAudioResourceGroupLog();
                return;
            }
            if (string.IsNullOrEmpty(label))
            {
                DrawInvalidLabelLog();
                return;
            }
            await GetAudioResourceGroup(audioResourceGroup).ReleaseWithAddressableLabel(label, token);
        }
        
        /// <summary>
        /// 指定したAudioResourceGroupに紐付けられているAudioClipをすべて開放します
        /// </summary>
        /// <param name="audioResourceGroup"></param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseWithAudioResourceGroup(string audioResourceGroup, bool forceReleaseScriptableObject = false)
        {
            if (string.IsNullOrEmpty(audioResourceGroup))
            {
                DrawInvalidAudioResourceGroupLog();
                return;
            }
            if (_audioResourceContainers.TryGetValue(audioResourceGroup, out var value))
            {
                value.ReleaseAll(forceReleaseScriptableObject);
            }
            else
            {
                RWaveLogUtility.LogWarning($"指定されたAudioResourceGroup: {audioResourceGroup} は存在しません。");
            }
        }

        /// <summary>
        /// 読み込んでいるAudioClipをすべて開放します
        /// </summary>
        /// <param name="ignoreCommon">Commonに読み込まれているAudioClipも開放するかどうか</param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void AllRelease(bool ignoreCommon = false,bool forceReleaseScriptableObject = false)
        {
            foreach (var audioResourceGroup in _audioResourceContainers.Values)
            {
                audioResourceGroup.ReleaseAll(forceReleaseScriptableObject);
            }
            if (!ignoreCommon)
            {
                _commonAudioResourceGroup.ReleaseAll(forceReleaseScriptableObject);   
            }

            RemoveEmptyAudioResourceGroups();
        }
        
        /// <summary>
        /// 要素数が0のAudioResourceGroupを削除します
        /// </summary>
        public void RemoveEmptyAudioResourceGroups()
        {
            var emptyGroups = new List<string>();
            foreach (var kvp in _audioResourceContainers)
            {
                if (kvp.Value.GetAudioClipCount() == 0)
                {
                    emptyGroups.Add(kvp.Key);
                }
            }

            foreach (var groupName in emptyGroups)
            {
                _audioResourceContainers.Remove(groupName);
                RWaveLogUtility.DrawAudioClipContainerLog($"空のAudioResourceGroupを削除しました: {groupName}");
            }
        }
        
        private IRWaveAudioResourceGroup GetAudioResourceGroup(string audioResourceGroup)
        {
            //共通のAudioResourceGroupを返す
            if (string.IsNullOrEmpty(audioResourceGroup))
            {
                return _commonAudioResourceGroup;
            }
            
            //指定されているaudioResourceGroupが登録されていない場合は新規に登録
            if (_audioResourceContainers.TryGetValue(audioResourceGroup, out var value)){ return value;}
            value = new RWaveAudioResourceGroup();
            _audioResourceContainers.Add(audioResourceGroup, value);
            return value;
        }
        
        private bool IsValid(string address,string audioResourceGroup)
        {
            if(string.IsNullOrEmpty(audioResourceGroup)) 
            {
                DrawInvalidAudioResourceGroupLog();
                return false;
            }

            if (string.IsNullOrEmpty(address))
            {
                DrawInvalidAddressLog();
                return false;
            }

            return true;
        }

        private static void DrawInvalidAudioResourceGroupLog()
        {
            RWaveLogUtility.LogAssertion($"audioResourceGroupが指定されていません。");
        }
        
        private static void DrawInvalidAddressLog()
        {
            RWaveLogUtility.LogAssertion($"Addressが指定されていません。");
        }
        
        private static void DrawInvalidLabelLog()
        {
            RWaveLogUtility.LogAssertion($"Labelが指定されていません。");
        }
    }
}