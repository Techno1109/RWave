using System.Collections.Generic;
using System.Threading;
using RWave.Data.Setting;
using UnityEngine;

namespace RWave.System.Interface
{
    public interface IRWaveAudioResourceGroup
    {
        /// <summary>
        /// 指定したAddressでAudioClipを読み込みます
        /// </summary>
        /// <param name="address"></param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public Awaitable<bool> LoadAudioClip(string address, CancellationToken token);

        /// <summary>
        /// 指定したAddressでAudioClipを即時読み込みします
        /// </summary>
        /// <param name="addressList"></param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public Awaitable<bool> LoadAudioClip(IReadOnlyList<string> addressList, CancellationToken token);

        /// <summary>
        /// 指定したAddressでAudioClipを同期読み込みします
        /// </summary>
        /// <param name="address"></param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(string address);

        /// <summary>
        /// Addressable上のAddress指定で同期読み込みします
        /// </summary>
        /// <param name="addressList"></param>
        /// <returns>読み込み成功時:True</returns>
        public bool SyncLoadAudioClip(IReadOnlyList<string> addressList);
        
        /// <summary>
        /// 指定したLabelでAudioClipを同期読み込みします
        /// </summary>
        /// <param name="label"></param>
        /// <param name="token"></param>
        /// <returns>読み込み成功時:True</returns>
        public Awaitable<bool> LoadAudioAudioClipWithAddressableLabel(string label, CancellationToken token);
        

        public AudioClip GetAudioClip(string address);

        /// <summary>
        /// 読み込まれているAudioClipの数を取得します
        /// </summary>
        /// <returns>AudioClipの数</returns>
        public int GetAudioClipCount();

        /// <summary>
        /// AudioPackから読み込む
        /// </summary>
        /// <param name="audioPack">読み込むAudioPack</param>
        public void LoadFromAudioPack(RWaveAudioPack audioPack);

        /// <summary>
        /// Addressable上の指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="removeAddressList"></param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void Release(List<string> removeAddressList, bool forceReleaseScriptableObject = false);

        /// <summary>
        /// Addressable上の指定したAddressで読み込まれているAudioClipを開放します
        /// </summary>
        /// <param name="address"></param>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void Release(string address, bool forceReleaseScriptableObject = false);

        /// <summary>
        /// Addressable上の指定したLabelでAudioClipを開放します
        /// </summary>
        /// <param name="label"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Awaitable ReleaseWithAddressableLabel(string label, CancellationToken token);

        /// <summary>
        /// 読み込んでいるAddressable上のAudioClipをすべて開放します
        /// </summary>
        /// <param name="forceReleaseScriptableObject">AudioPackから読み込まれたAudioClipも開放するかどうか</param>
        public void ReleaseAll(bool forceReleaseScriptableObject = false);
    }
}