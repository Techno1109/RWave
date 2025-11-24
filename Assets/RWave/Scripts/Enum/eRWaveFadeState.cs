namespace RWave.Enum
{
    /// <summary>
    /// 再生のステート状態
    /// </summary>
    public enum eRWaveFadeState
    {
        /// <summary>
        /// ステートなし（通常再生中）
        /// </summary>
        None = 0,

        /// <summary>
        /// Attack中（FadeIn中）- 再生開始ステート
        /// </summary>
        Attack = 1,

        /// <summary>
        /// Release中（FadeOut中）- 再生終了ステート
        /// </summary>
        Release = 2
    }
}
