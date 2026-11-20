using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace RWave.System
{
    /// <summary>
    /// 音量調整を担当するClass
    /// </summary>
    public class RWaveAudioVolumeControlSystem
    {
        public float currentAudioVolume => _volume;

        /// <summary>
        /// 音量へ一時的に掛けている倍率（0～1）
        /// </summary>
        public float currentVolumeScale => _volumeScale;
        private AudioMixer _audioMixer;
        private string _audioMixerGroupName;
        private float _volume;

        /// <summary>
        /// 音量へ一時的に掛ける倍率（0～1）。_volumeとは別に保持し、AudioMixerへは両者を掛けた値を反映する
        /// </summary>
        private float _volumeScale = 1f;
        private float _maxDB;
        private const float MIN_DB = -80f;
        private bool _isEnabled;

        /// <summary>
        /// 進行中の倍率フェードを打ち切るためのCancellationTokenSource
        /// </summary>
        private CancellationTokenSource _volumeScaleFadeCancellation;

        public void Initialize(AudioMixer audioMixer,string audioMixerGroupName,float defaultVolume = 50f, float maxDB = 0f)
        {
            _audioMixer = audioMixer;
            _audioMixerGroupName = audioMixerGroupName;
            _maxDB = maxDB;

            // Exposed Parameter Nameが空白の場合は無効化
            _isEnabled = !string.IsNullOrEmpty(_audioMixerGroupName);

            SetVolume(defaultVolume);
        }
        
        /// <summary>
        /// 音声ボリュームを設定します
        /// </summary>
        /// <param name="volume">0~100</param>
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp(volume, 0f, 100f);
            ApplyVolumeToAudioMixer();
        }

        /// <summary>
        /// 音声ボリュームへ一時的に掛ける倍率を設定します。
        /// SetVolumeで設定した音量そのものは変えないため、倍率を1に戻せば元の音量に戻ります
        /// </summary>
        /// <param name="volumeScale">音量倍率（0~1）</param>
        /// <param name="fadeSeconds">倍率を変化させる時間（秒）。0以下の場合は即座に反映します</param>
        public void SetVolumeScale(float volumeScale, float fadeSeconds = 0f)
        {
            CancelVolumeScaleFade();

            var targetVolumeScale = Mathf.Clamp01(volumeScale);
            if (fadeSeconds <= 0f)
            {
                _volumeScale = targetVolumeScale;
                ApplyVolumeToAudioMixer();
                return;
            }

            // アプリケーション終了（PlayMode終了）時にもフェードを打ち切る
            _volumeScaleFadeCancellation = CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken);
            _ = FadeVolumeScale(targetVolumeScale, fadeSeconds, _volumeScaleFadeCancellation.Token);
        }

        /// <summary>
        /// 現在の音声ボリュームを取得します（一時的な倍率は含みません）
        /// </summary>
        /// <returns>0~100</returns>
        public float GetVolume()
        {
            // AudioMixerには一時的な倍率を掛けた値が入っているため、内部で保持している値を返す
            return _volume;
        }

        /// <summary>
        /// 音量と一時的な倍率を掛けた値をAudioMixerへ反映します
        /// </summary>
        private void ApplyVolumeToAudioMixer()
        {
            // 無効の場合はAudioMixer操作をスキップ
            if (!_isEnabled)
            {
                return;
            }

            var scaledVolume = _volume * _volumeScale;

            // 0-100 を対数スケールで -80dB ~ maxDB に変換
            // dBは対数単位のため、線形変換では中間音量で急激に音が小さくなる
            // 100%→maxDB、50%→約-6dB（振幅1/2）、10%→-20dB
            float dB;
            if (scaledVolume <= 0.01f)
            {
                dB = MIN_DB;  // 実質ミュート
            }
            else
            {
                dB = Mathf.Max(_maxDB + Mathf.Log10(scaledVolume / 100f) * 20f, MIN_DB);
            }

            _audioMixer.SetFloat(_audioMixerGroupName, dB);
        }

        /// <summary>
        /// 現在の倍率から指定の倍率へ時間をかけて変化させます
        /// </summary>
        /// <param name="targetVolumeScale">目標の音量倍率（0~1）</param>
        /// <param name="duration">フェード時間（秒）</param>
        /// <param name="token">キャンセルトークン</param>
        private async Awaitable FadeVolumeScale(float targetVolumeScale, float duration, CancellationToken token)
        {
            var startVolumeScale = _volumeScale;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                await Awaitable.NextFrameAsync();

                // 新しい倍率の設定やアプリケーション終了で打ち切られた場合は終了する
                if (token.IsCancellationRequested)
                {
                    return;
                }

                elapsedTime += Time.unscaledDeltaTime;
                _volumeScale = Mathf.Lerp(startVolumeScale, targetVolumeScale, elapsedTime / duration);
                ApplyVolumeToAudioMixer();
            }
        }

        /// <summary>
        /// 進行中の倍率フェードを打ち切ります
        /// </summary>
        private void CancelVolumeScaleFade()
        {
            if (_volumeScaleFadeCancellation == null)
            {
                return;
            }

            _volumeScaleFadeCancellation.Cancel();
            _volumeScaleFadeCancellation.Dispose();
            _volumeScaleFadeCancellation = null;
        }
    }
}