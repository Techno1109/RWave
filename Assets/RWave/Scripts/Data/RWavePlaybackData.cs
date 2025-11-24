using System.Threading;
using RWave.Enum;

namespace RWave.Data
{
    /// <summary>
    /// AudioSourceプール内の再生状況を管理する内部データ
    /// 固定サイズプールで再利用される
    /// </summary>
    internal class RWavePlaybackData
    {
        /// <summary>
        /// 再生ID（-1は未使用を示す）
        /// </summary>
        private int _playbackId;

        /// <summary>
        /// AudioClipのアドレス
        /// </summary>
        private string _address;

        /// <summary>
        /// 対応するUnity AudioSource（List内のインデックス）
        /// </summary>
        private int _audioSourceIndex;

        /// <summary>
        /// 使用中フラグ
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// 現在のステート状態
        /// </summary>
        private eRWaveFadeState _fadeState;

        /// <summary>
        /// 個別のCancellationTokenSource
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 再生時に指定された音量（0～100、nullの場合はBaseVolume使用）
        /// </summary>
        private float? _playVolume;

        /// <summary>
        /// 再生ID（読み取り専用）
        /// </summary>
        public int playbackId => _playbackId;

        /// <summary>
        /// AudioClipのアドレス（読み取り専用）
        /// </summary>
        public string address => _address;

        /// <summary>
        /// 対応するUnity AudioSource（List内のインデックス）（読み取り専用）
        /// </summary>
        public int audioSourceIndex => _audioSourceIndex;

        /// <summary>
        /// 使用中フラグ（読み取り専用）
        /// </summary>
        public bool isActive => _isActive;

        /// <summary>
        /// 現在のステート状態（読み取り専用）
        /// </summary>
        public eRWaveFadeState fadeState => _fadeState;

        /// <summary>
        /// 再生時に指定された音量（読み取り専用）
        /// </summary>
        public float? playVolume => _playVolume;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RWavePlaybackData()
        {
            Reset();
        }

        /// <summary>
        /// データを初期状態にリセット
        /// </summary>
        public void Reset()
        {
            _playbackId = -1;
            _address = string.Empty;
            _audioSourceIndex = -1;
            _playVolume = null;
            _isActive = false;
            _fadeState = eRWaveFadeState.None;

            // CancellationTokenSourceをクリーンアップ
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// 再生データを設定
        /// </summary>
        /// <param name="id">再生ID</param>
        /// <param name="addr">AudioClipアドレス</param>
        /// <param name="srcIndex">AudioSourceインデックス</param>
        /// <param name="playVol">再生時に指定された音量（nullの場合はBaseVolume使用）</param>
        public void Set(int id, string addr, int srcIndex, float? playVol)
        {
            _playbackId = id;
            _address = addr;
            _audioSourceIndex = srcIndex;
            _playVolume = playVol;
            _isActive = true;
            _fadeState = eRWaveFadeState.None;
        }

        /// <summary>
        /// ステート状態を設定
        /// </summary>
        /// <param name="state">設定するステート</param>
        public void SetFadeState(eRWaveFadeState state)
        {
            _fadeState = state;
        }

        /// <summary>
        /// 指定されたIDでアクティブな再生データかを確認
        /// </summary>
        /// <param name="id">確認する再生ID</param>
        /// <returns>指定IDでアクティブならtrue</returns>
        public bool IsActiveWithId(int id)
        {
            return _isActive && _playbackId == id;
        }

        /// <summary>
        /// Attack状態かを確認
        /// </summary>
        /// <returns>Attack状態ならtrue</returns>
        public bool IsInAttackState()
        {
            return _fadeState == eRWaveFadeState.Attack;
        }

        /// <summary>
        /// 指定されたアドレスでアクティブな再生データかを確認
        /// </summary>
        /// <param name="addr">確認するアドレス</param>
        /// <returns>指定アドレスでアクティブならtrue</returns>
        public bool IsActiveWithAddress(string addr)
        {
            return _isActive && _address == addr;
        }

        /// <summary>
        /// CancellationTokenSourceの取得または生成
        /// </summary>
        /// <returns>CancellationToken</returns>
        public CancellationToken GetOrCreateCancellationToken()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }
            return _cancellationTokenSource.Token;
        }

        /// <summary>
        /// フェード処理のキャンセル
        /// </summary>
        public void CancelFade()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// 再生時音量を変更
        /// </summary>
        /// <param name="volume">新しい音量（0～100）</param>
        public void SetPlayVolume(float volume)
        {
            _playVolume = UnityEngine.Mathf.Clamp(volume, 0f, 100f);
        }
    }
}
