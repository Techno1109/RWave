using System.Collections.Generic;
using System.Threading;
using RWave.Data;
using RWave.Data.Setting;
using UnityEngine;
using UnityEngine.Audio;

namespace RWave.System.Interface
{
    /// <summary>
    /// SoundPlayerの操作系定義するInterface;
    /// </summary>
    public interface IRWaveAudioSource
    {
        /// <summary>
        /// 同時再生数を指定し、初期化を行う。
        /// </summary>
        /// <param name="mixerGroup">結び付けるMixerGroup</param>
        /// <param name="audioSourceSetting">AudioSourceの設定</param>
        public void Initialize(AudioMixerGroup mixerGroup, RWaveAudioSourceSetting audioSourceSetting);

        /// <summary>
        /// 再生を実行
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip);

        /// <summary>
        /// 再生を実行（アドレス指定あり）
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="address">AudioClipのアドレス</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, string address);

        /// <summary>
        /// 再生を実行（再生時音量指定あり）
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="playVolume">再生時の音量（0～100、nullの場合はBaseVolumeを使用）</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, float? playVolume);

        /// <summary>
        /// 再生を実行（アドレス・再生時音量指定あり）
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="address">AudioClipのアドレス</param>
        /// <param name="playVolume">再生時の音量（0～100、nullの場合はBaseVolumeを使用）</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, string address, float? playVolume);

        /// <summary>
        /// 現在再生中の音をすべてFadeOutさせて停止する。
        /// </summary>
        public void Stop();

        /// <summary>
        /// ハンドラー指定で再生を停止する
        /// </summary>
        /// <param name="handler">停止する再生のハンドラー</param>
        public void Stop(RWaveAudioHandler handler);

        /// <summary>
        /// アドレス指定で該当する全再生を停止する
        /// </summary>
        /// <param name="address">AudioClipのアドレス</param>
        public void Stop(string address);

        /// <summary>
        /// 現在再生中の音をすべて即座に停止する。
        /// </summary>
        public void ForceStop();

        /// <summary>
        /// AudioSource単位の音量を設定する
        /// 設定した音量は次回再生時やFade処理の最大値として適用されます
        /// 再生中のAudioSourceにも即座に反映されます
        /// </summary>
        /// <param name="volume">音量（0～100）</param>
        public void SetVolume(float volume);

        /// <summary>
        /// AudioSource単位の現在の音量を取得する
        /// </summary>
        /// <returns>音量（0～100）</returns>
        public float GetVolume();

        /// <summary>
        /// 指定したplaybackIdに対応する再生が現在再生中かどうかを判定
        /// </summary>
        /// <param name="playbackId">確認する再生ID</param>
        /// <returns>再生中の場合true、停止済み・存在しない場合false</returns>
        public bool IsPlaying(int playbackId);

        /// <summary>
        /// 指定したplaybackIdの再生の音量をフェード付きで変更する
        /// </summary>
        /// <param name="playbackId">対象の再生ID</param>
        /// <param name="volume">新しい音量（0～100）</param>
        /// <param name="fadeDuration">フェード時間（秒）</param>
        public void SetPlaybackVolume(int playbackId, float volume, float fadeDuration);
    }
}