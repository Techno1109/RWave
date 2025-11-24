using RWave.System.Interface;

namespace RWave.Data
{
    /// <summary>
    /// 再生中のオーディオを個別制御するためのハンドラー構造体
    /// 生成後は全フィールドReadOnly
    /// </summary>
    public readonly struct RWaveAudioHandler
    {
        /// <summary>
        /// 再生ID（インクリメント方式: 0, 1, 2, ...）
        /// </summary>
        public readonly int playbackId;

        /// <summary>
        /// 再生するAudioClipのアドレス
        /// </summary>
        public readonly string address;

        /// <summary>
        /// このハンドラーが紐づくAudioSource
        /// </summary>
        internal readonly IRWaveAudioSource audioSource;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playbackId">再生ID</param>
        /// <param name="address">AudioClipアドレス</param>
        /// <param name="audioSource">紐づくAudioSource</param>
        public RWaveAudioHandler(int playbackId, string address, IRWaveAudioSource audioSource)
        {
            this.playbackId = playbackId;
            this.address = address;
            this.audioSource = audioSource;
        }

        /// <summary>
        /// 無効なハンドラーを表す定数
        /// </summary>
        public static readonly RWaveAudioHandler Invalid = new (-1, string.Empty, null);

        /// <summary>
        /// ハンドラーが有効かどうかを判定
        /// </summary>
        /// <returns>有効な場合true</returns>
        public bool IsValid() => playbackId >= 0 && audioSource != null;
    }
}
