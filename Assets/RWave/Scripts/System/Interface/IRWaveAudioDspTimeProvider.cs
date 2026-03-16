namespace RWave.System.Interface
{
    /// <summary>
    /// メインのAudioSourceのDSPTimeを提供するReadOnlyインターフェース。
    /// 音楽ゲームやリズム同期での利用を想定する。
    /// </summary>
    public interface IRWaveAudioDspTimeProvider
    {
        /// <summary>
        /// 現在メインで使用されているAudioSourceの再生開始DSPTime。
        /// AudioSettings.dspTimeとの差分で精密な再生経過時間を計算可能。
        /// 再生が開始されていない場合は0を返す。
        /// </summary>
        double dspTime { get; }

        /// <summary>
        /// 現在メインで使用されているAudioSourceの再生開始からの経過DSP時間（秒）。
        /// AudioSettings.dspTime - dspTime で算出される。
        /// 再生が開始されていない場合は0を返す。
        /// </summary>
        double elapsedDspTime { get; }
    }
}
