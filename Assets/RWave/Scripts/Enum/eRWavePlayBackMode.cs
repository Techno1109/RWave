namespace RWave.Enum
{
    /// <summary>
    /// 再生モードを表すEnum
    /// </summary>
    public enum eRWavePlayBackMode
    {
        /// <summary>
        /// 最後まで再生したら自動的に停止します
        /// </summary>
        OneShot = 0,
        
        /// <summary>
        ///　最後まで再生が完了した場合、先頭から再生を再開します。
        /// </summary>
        Loop = 1,
    }
}