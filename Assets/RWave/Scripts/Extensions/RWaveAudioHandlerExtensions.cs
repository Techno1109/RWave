using RWave.Data;

namespace RWave.Extensions
{
    /// <summary>
    /// RWaveAudioHandlerの拡張メソッド
    /// </summary>
    public static class RWaveAudioHandlerExtensions
    {
        /// <summary>
        /// ハンドラーに紐づく再生が現在再生中かどうかを判定
        /// </summary>
        /// <param name="handler">確認するハンドラー</param>
        /// <returns>再生中の場合true、停止済み・無効なハンドラーの場合false</returns>
        public static bool IsPlaying(this RWaveAudioHandler handler)
        {
            if (!handler.IsValid())
            {
                return false;
            }

            return handler.audioSource.IsPlaying(handler.playbackId);
        }

        /// <summary>
        /// このハンドラーが管理する再生の音量をフェード付きで変更する
        /// </summary>
        /// <param name="handler">対象のハンドラー</param>
        /// <param name="volume">新しい音量（0～100）</param>
        /// <param name="fadeDuration">フェード時間（秒）デフォルトは0（即座に変更）</param>
        public static void SetVolume(this RWaveAudioHandler handler, float volume, float fadeDuration = 0f)
        {
            if (!handler.IsValid())
            {
                return;
            }

            handler.audioSource.SetPlaybackVolume(handler.playbackId, volume, fadeDuration);
        }
    }
}
