using System.Collections.Generic;
using System.Threading;
using RWave.Data;
using RWave.Data.Setting;
using RWave.Enum;
using RWave.System.Interface;
using RWave.Utility;
using UnityEngine;
using UnityEngine.Audio;

namespace RWave.System
{
    /// <summary>
    /// AudioSourceの管理系を纏めたClass
    /// </summary>
    public class RWaveAudioSource : MonoBehaviour,IRWaveAudioSource
    {
        /// <summary>
        /// 結び付けられているAudioSourceのリスト
        /// </summary>
        private List<AudioSource> _audioSourceList;

        /// <summary>
        /// 次に使用するAudioSourceのIndex
        /// </summary>
        private int _nextUseAudioSourceIndex;

        /// <summary>
        /// 最後に使用したAudioSourceのIndex
        /// </summary>
        private int _lastUseAudioSourceIndex;

        private RWaveAudioSourceSetting _audioSourceSetting;

        /// <summary>
        /// AudioSource単位の基準音量（0～100）
        /// </summary>
        private float _audioSourceBaseVolume;

        /// <summary>
        /// AudioSource単位の基準音量の正規化値（0f～1f）
        /// _audioSourceBaseVolume / 100f のキャッシュ
        /// </summary>
        private float _audioSourceBaseVolumeNormalized;

        private int _maxSoundCount;
        private RWaveCrossFadeSetting _crossFadeSetting;
        private bool _allowDuplicatePlay;

        /// <summary>
        /// 再生ID生成用カウンター（インクリメント）
        /// </summary>
        private int _nextPlaybackId = 0;

        /// <summary>
        /// 再生データプール（固定サイズ配列）
        /// </summary>
        private RWavePlaybackData[] _playbackDataPool;

        /// <summary>
        /// 最後に使用されたプールスロットのインデックス
        /// </summary>
        private int _lastUsedPlaybackSlotIndex = -1;

        /// <summary>
        /// 同時再生数を指定し、初期化を行う。
        /// </summary>
        /// <param name="mixerGroup">結び付けるMixerGroup</param>
        /// <param name="audioSourceSetting">AudioSourceの設定</param>
        public void Initialize(AudioMixerGroup mixerGroup, RWaveAudioSourceSetting audioSourceSetting)
        {
            _audioSourceList ??= new List<AudioSource>();
            _audioSourceList.Clear();

            _audioSourceSetting = audioSourceSetting;

            // AudioSource単位の基準音量を保持
            _audioSourceBaseVolume = audioSourceSetting.defaultVolume;
            _audioSourceBaseVolumeNormalized = _audioSourceBaseVolume / 100f;

            _crossFadeSetting = audioSourceSetting.crossFadeSetting;
            _allowDuplicatePlay = audioSourceSetting.allowDuplicatePlay;
            _maxSoundCount = _crossFadeSetting.isCrossFadeActive ? 2 : audioSourceSetting.maxSoundCount;
            //カウントが0以下の場合は1にする
            if (_maxSoundCount <= 0)
            {
                _maxSoundCount = 1;
            }

            for (int i = 0; i < _maxSoundCount; i++)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = mixerGroup;
                _audioSourceList.Add(audioSource);
                audioSource.loop = _audioSourceSetting.playBackMode == eRWavePlayBackMode.Loop;
            }

            _nextUseAudioSourceIndex = 0;
            _lastUseAudioSourceIndex = _maxSoundCount - 1;

            // プール初期化
            _playbackDataPool = new RWavePlaybackData[_maxSoundCount];
            _lastUsedPlaybackSlotIndex = -1;

            for (int i = 0; i < _maxSoundCount; i++)
            {
                _playbackDataPool[i] = new RWavePlaybackData();
            }
        }
        
        /// <summary>
        /// 指定したAudioClipを再生する。
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip)
        {
            return Play(audioClip, string.Empty, null);
        }

        /// <summary>
        /// 指定したAudioClipを再生する（アドレス指定あり）
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="address">AudioClipのアドレス</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, string address)
        {
            return Play(audioClip, address, null);
        }

        /// <summary>
        /// 指定したAudioClipを再生する（再生時音量指定あり）
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="playVolume">再生時の音量（0～100、nullの場合はBaseVolumeを使用）</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, float? playVolume)
        {
            return Play(audioClip, string.Empty, playVolume);
        }

        /// <summary>
        /// 指定したAudioClipを再生する（アドレス・再生時音量指定あり）
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="address">AudioClipのアドレス</param>
        /// <param name="playVolume">再生時の音量（0～100、nullの場合はBaseVolumeを使用）</param>
        /// <returns>再生制御用ハンドラー</returns>
        public RWaveAudioHandler Play(AudioClip audioClip, string address, float? playVolume)
        {
            if (audioClip == null)
            {
                RWaveLogUtility.LogAssertion("AudioClipがNullです。");
                return RWaveAudioHandler.Invalid;
            }

            // 再生時音量の検証とクランプ（0-100）
            float effectivePlayVolume = playVolume.HasValue
                ? Mathf.Clamp(playVolume.Value, 0f, 100f)
                : _audioSourceBaseVolume;

            // playVolumeをクランプ後の値に更新（null許容型のまま）
            float? clampedPlayVolume = playVolume.HasValue
                ? Mathf.Clamp(playVolume.Value, 0f, 100f)
                : (float?)null;

            // 重複再生チェック
            if (!_allowDuplicatePlay && IsAudioClipPlaying(audioClip))
            {
                RWaveLogUtility.LogWarning($"同じAudioClip '{audioClip.name}' が既に再生中のため、再生リクエストを無視しました。");
                return RWaveAudioHandler.Invalid;
            }

            // 再生ID生成
            int currentPlaybackId = _nextPlaybackId++;

            // CrossFadeが有効かどうかで処理を分岐
            if (_crossFadeSetting.isCrossFadeActive)
            {
                PlayWithCrossFade(audioClip, currentPlaybackId, address, effectivePlayVolume, clampedPlayVolume);
            }
            else
            {
                PlayNormal(audioClip, currentPlaybackId, address, effectivePlayVolume, clampedPlayVolume);
            }

            // ハンドラーを返す
            return new RWaveAudioHandler(currentPlaybackId, address, this);
        }

        /// <summary>
        /// 指定したAudioClipが現在再生中かどうかをチェックする
        /// </summary>
        /// <param name="audioClip">チェックするAudioClip</param>
        /// <returns>再生中の場合true</returns>
        private bool IsAudioClipPlaying(AudioClip audioClip)
        {
            if (audioClip == null || _audioSourceList == null)
            {
                return false;
            }

            foreach (var audioSource in _audioSourceList)
            {
                if (audioSource.isPlaying && audioSource.clip == audioClip)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 通常再生を行う
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="playbackId">再生ID</param>
        /// <param name="address">AudioClipのアドレス</param>
        /// <param name="playVolume">再生時の音量（0～100）</param>
        /// <param name="originalPlayVolume">元の再生時音量（null許容型、nullの場合はBaseVolume使用）</param>
        private void PlayNormal(AudioClip audioClip, int playbackId, string address, float playVolume, float? originalPlayVolume)
        {
            var audioSourceIndex = _nextUseAudioSourceIndex;
            var isFadeIn = 0f < _audioSourceSetting.attackTimeInSeconds;

            // 再生時音量を正規化（BaseVolumeでキャップしない）
            var playVolumeNormalized = playVolume / 100f;

            //FadeInする場合は最初の音量を0にする、しない場合は再生時音量を設定
            _audioSourceList[audioSourceIndex].volume = isFadeIn ? 0f : playVolumeNormalized;

            //再生に関わる設定を反映
            _audioSourceList[audioSourceIndex].clip = audioClip;

            //再生開始
            _audioSourceList[audioSourceIndex].Play();

            // プールデータに登録（先に行う）
            RegisterPlaybackData(playbackId, address, audioSourceIndex, originalPlayVolume);

            //FadeInする場合、実行（目標音量として再生時音量を使用）
            if (isFadeIn)
            {
                // PlaybackDataから個別のCancellationTokenを取得
                var cancellationToken = GetPlaybackDataCancellationToken(playbackId);
                _ = DoFade(_audioSourceList[audioSourceIndex], playVolumeNormalized, _audioSourceSetting.attackTimeInSeconds, cancellationToken, playbackId, eRWaveFadeState.Attack);
            }

            _lastUseAudioSourceIndex = _nextUseAudioSourceIndex;
            _nextUseAudioSourceIndex++;
            if (_nextUseAudioSourceIndex >= _maxSoundCount)
            {
                _nextUseAudioSourceIndex = 0;
            }
        }

        /// <summary>
        /// CrossFade再生を行う
        /// </summary>
        /// <param name="audioClip">再生するAudioClip</param>
        /// <param name="playbackId">再生ID</param>
        /// <param name="address">AudioClipのアドレス</param>
        /// <param name="playVolume">再生時の音量（0～100）</param>
        /// <param name="originalPlayVolume">元の再生時音量（null許容型、nullの場合はBaseVolume使用）</param>
        private void PlayWithCrossFade(AudioClip audioClip, int playbackId, string address, float playVolume, float? originalPlayVolume)
        {
            var audioSourceIndex = _nextUseAudioSourceIndex;
            var crossFadeDuration = _crossFadeSetting.attackTimeInSeconds;

            // 再生時音量を正規化（BaseVolumeでキャップしない）
            var playVolumeNormalized = playVolume / 100f;

            //クロスフェードする場合は最初の音量を0にする、しない場合は再生時音量を設定
            _audioSourceList[audioSourceIndex].volume = 0f < crossFadeDuration ? 0f : playVolumeNormalized;

            //再生に関わる設定を反映
            _audioSourceList[audioSourceIndex].clip = audioClip;

            //再生開始
            _audioSourceList[audioSourceIndex].Play();

            // プールデータに登録（先に行う）
            RegisterPlaybackData(playbackId, address, audioSourceIndex, originalPlayVolume);

            //クロスフェードする場合、実行
            if (0f < crossFadeDuration)
            {
                // 古い音のフェードアウト処理
                if (_lastUseAudioSourceIndex >= 0)
                {
                    var fadeOutAudioSource = _audioSourceList[_lastUseAudioSourceIndex];
                    // 前の再生のplaybackIdを取得
                    int previousPlaybackId = GetPlaybackIdByAudioSourceIndex(_lastUseAudioSourceIndex);
                    if (previousPlaybackId >= 0)
                    {
                        var previousToken = GetPlaybackDataCancellationToken(previousPlaybackId);
                        _ = ExecuteCrossFadeOut(fadeOutAudioSource, _crossFadeSetting.releaseTimeInSeconds, previousToken, previousPlaybackId);
                    }
                }

                // 新しい音のフェードイン処理（目標音量として再生時音量を使用）
                var newToken = GetPlaybackDataCancellationToken(playbackId);
                _ = DoFade(_audioSourceList[audioSourceIndex], playVolumeNormalized, crossFadeDuration, newToken, playbackId, eRWaveFadeState.Attack);
            }

            _lastUseAudioSourceIndex = _nextUseAudioSourceIndex;
            _nextUseAudioSourceIndex++;
            if (_nextUseAudioSourceIndex >= _maxSoundCount)
            {
                _nextUseAudioSourceIndex = 0;
            }
        }
        
        /// <summary>
        /// 再生されているAudioSourceをすべて停止する
        /// </summary>
        public void Stop()
        {
            // 全てのPlaybackDataを処理
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                var playbackData = _playbackDataPool[i];

                if (playbackData.isActive)
                {
                    // Attack中の場合、Releaseフェードに切り替え
                    if (playbackData.IsInAttackState())
                    {
                        SwitchToReleaseFade(playbackData);
                    }
                    else
                    {
                        // 通常の停止処理
                        StopAudioSourceByIndex(playbackData.audioSourceIndex, playbackData.playbackId);
                    }
                }
            }

            _nextUseAudioSourceIndex = 0;
            _lastUseAudioSourceIndex = _nextUseAudioSourceIndex - 1;
        }

        /// <summary>
        /// ハンドラー指定で再生を停止する
        /// </summary>
        /// <param name="handler">停止する再生のハンドラー</param>
        public void Stop(RWaveAudioHandler handler)
        {
            if (!handler.IsValid())
            {
                RWaveLogUtility.LogWarning("無効なハンドラーが指定されました。");
                return;
            }

            // プールから該当する再生データを検索
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                var playbackData = _playbackDataPool[i];

                if (playbackData.IsActiveWithId(handler.playbackId))
                {
                    // Attack中の場合、Releaseフェードに切り替え
                    if (playbackData.IsInAttackState())
                    {
                        SwitchToReleaseFade(playbackData);
                    }
                    else
                    {
                        // 通常の停止処理
                        StopAudioSourceByIndex(playbackData.audioSourceIndex, playbackData.playbackId);
                    }

                    // playbackData.Reset()を削除
                    // Releaseフェード中もPlaybackDataを維持し、フェード完了後にResetする
                    return;
                }
            }

            // 見つからなかった場合（既に停止済み、または無効なID）
            RWaveLogUtility.LogWarning($"指定された再生ID {handler.playbackId} が見つかりません（既に停止済みの可能性があります）。");
        }

        /// <summary>
        /// アドレス指定で該当する全再生を停止する
        /// </summary>
        /// <param name="address">AudioClipのアドレス</param>
        public void Stop(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                RWaveLogUtility.LogWarning("空のアドレスが指定されました。");
                return;
            }

            bool foundAny = false;

            for (int i = _playbackDataPool.Length - 1; i >= 0; i--)
            {
                var playbackData = _playbackDataPool[i];

                if (playbackData.IsActiveWithAddress(address))
                {
                    // Attack中の場合、Releaseフェードに切り替え
                    if (playbackData.IsInAttackState())
                    {
                        SwitchToReleaseFade(playbackData);
                    }
                    else
                    {
                        // 通常の停止処理
                        StopAudioSourceByIndex(playbackData.audioSourceIndex, playbackData.playbackId);
                    }

                    // playbackData.Reset()を削除
                    // Releaseフェード完了後にResetされる
                    foundAny = true;
                }
            }

            if (!foundAny)
            {
                RWaveLogUtility.LogWarning($"指定されたアドレス '{address}' の再生が見つかりません。");
            }
        }

        /// <summary>
        /// AudioSourceインデックス指定で停止
        /// </summary>
        /// <param name="audioSourceIndex">AudioSourceのインデックス</param>
        /// <param name="playbackId">再生ID</param>
        private void StopAudioSourceByIndex(int audioSourceIndex, int playbackId)
        {
            if (audioSourceIndex < 0 || audioSourceIndex >= _audioSourceList.Count)
            {
                return;
            }

            var audioSource = _audioSourceList[audioSourceIndex];

            if (_audioSourceSetting.releaseTimeInSeconds <= 0f)
            {
                audioSource.Stop();
            }
            else
            {
                var token = GetPlaybackDataCancellationToken(playbackId);
                _ = ExecuteReleaseStop(audioSource, _audioSourceSetting.releaseTimeInSeconds, token, playbackId);
            }
        }

        /// <summary>
        /// 現在再生中の音をすべて即座に停止する
        /// </summary>
        public void ForceStop()
        {
            // 全てのPlaybackDataをキャンセルしてリセット
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                var playbackData = _playbackDataPool[i];
                if (playbackData.isActive)
                {
                    // フェードをキャンセル
                    playbackData.CancelFade();

                    // AudioSourceを停止
                    if (playbackData.audioSourceIndex >= 0 && playbackData.audioSourceIndex < _audioSourceList.Count)
                    {
                        _audioSourceList[playbackData.audioSourceIndex].Stop();
                    }

                    // PlaybackDataをリセット
                    playbackData.Reset();
                }
            }

            _nextUseAudioSourceIndex = 0;
            _lastUseAudioSourceIndex = _nextUseAudioSourceIndex - 1;
        }

        /// <summary>
        /// AudioSource単位の音量を設定する
        /// 設定した音量は次回再生時やFade処理の最大値として適用されます
        /// 再生中のAudioSourceにも即座に反映されます
        /// </summary>
        /// <param name="volume">音量（0～100）</param>
        public void SetVolume(float volume)
        {
            _audioSourceBaseVolume = Mathf.Clamp(volume, 0f, 100f);
            _audioSourceBaseVolumeNormalized = _audioSourceBaseVolume / 100f;
            UpdatePlayingVolumes();
        }

        /// <summary>
        /// AudioSource単位の現在の音量を取得する
        /// </summary>
        /// <returns>音量（0～100）</returns>
        public float GetVolume()
        {
            return _audioSourceBaseVolume;
        }

        /// <summary>
        /// 指定したplaybackIdに対応する再生が現在再生中かどうかを判定
        /// </summary>
        /// <param name="playbackId">確認する再生ID</param>
        /// <returns>再生中の場合true、停止済み・存在しない場合false</returns>
        public bool IsPlaying(int playbackId)
        {
            // プールから該当する再生データを検索
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                var playbackData = _playbackDataPool[i];

                // playbackIdが一致するアクティブなデータを発見
                if (playbackData.IsActiveWithId(playbackId))
                {
                    var audioSourceIndex = playbackData.audioSourceIndex;

                    // AudioSourceインデックスが有効範囲内かチェック
                    if (audioSourceIndex >= 0 && audioSourceIndex < _audioSourceList.Count)
                    {
                        // Release中も再生中として扱う
                        return _audioSourceList[audioSourceIndex].isPlaying;
                    }

                    return false;
                }
            }

            // 見つからなかった場合は再生していない
            return false;
        }

        /// <summary>
        /// 指定したplaybackIdの再生の音量をフェード付きで変更する
        /// </summary>
        /// <param name="playbackId">対象の再生ID</param>
        /// <param name="volume">新しい音量（0～100）</param>
        /// <param name="fadeDuration">フェード時間（秒）</param>
        public void SetPlaybackVolume(int playbackId, float volume, float fadeDuration)
        {
            // 1. 音量を検証・クランプ（0-100）
            float clampedVolume = Mathf.Clamp(volume, 0f, 100f);
            float normalizedVolume = clampedVolume / 100f;

            // 2. playbackIdで対応するPlaybackDataを検索
            RWavePlaybackData targetData = null;
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                if (_playbackDataPool[i].IsActiveWithId(playbackId))
                {
                    targetData = _playbackDataPool[i];
                    break;
                }
            }

            // 3. 見つからない、または非アクティブなら何もしない
            if (targetData == null || !targetData.isActive)
            {
                return;
            }

            // 4. 対応するAudioSourceを取得
            int audioSourceIndex = targetData.audioSourceIndex;
            if (audioSourceIndex < 0 || audioSourceIndex >= _audioSourceList.Count)
            {
                return;
            }
            var audioSource = _audioSourceList[audioSourceIndex];

            // 5. playVolumeを更新（BaseVolume変更に追従しないようにする）
            targetData.SetPlayVolume(clampedVolume);

            // 6. fadeDuration = 0 なら即座に反映
            if (fadeDuration <= 0f)
            {
                audioSource.volume = normalizedVolume;
                return;
            }

            // 7. フェード処理を実行
            // 既存のフェードをキャンセルして新しいフェードを開始
            targetData.CancelFade();
            var newToken = targetData.GetOrCreateCancellationToken();

            // playbackVolume変更用のフェードを実行（BaseVolumeキャップなし）
            _ = DoFadeForPlaybackVolume(audioSource, normalizedVolume, fadeDuration, newToken, playbackId);
        }

        /// <summary>
        /// 再生中のすべてのAudioSourceの音量を更新する
        /// playVolume未指定の再生のみBaseVolumeに追従します
        /// </summary>
        private void UpdatePlayingVolumes()
        {
            for (int i = 0; i < _audioSourceList.Count; i++)
            {
                var audioSource = _audioSourceList[i];
                if (!audioSource.isPlaying) continue;

                // このAudioSourceに対応するPlaybackDataを検索
                RWavePlaybackData matchedData = null;
                for (int j = 0; j < _playbackDataPool.Length; j++)
                {
                    var playbackData = _playbackDataPool[j];
                    if (playbackData.isActive && playbackData.audioSourceIndex == i)
                    {
                        matchedData = playbackData;
                        break;
                    }
                }

                if (matchedData == null) continue;

                if (matchedData.playVolume.HasValue)
                {
                    // playVolume指定あり → 何もしない（指定音量を維持）
                    continue;
                }
                else
                {
                    // playVolume未指定（BaseVolume使用） → BaseVolumeに更新
                    audioSource.volume = _audioSourceBaseVolumeNormalized;
                }
            }
        }

        /// <summary>
        /// 指定したAudioSourceの現在の音量から指定の音量へとFadeする処理
        /// Fade中にSetVolume()が呼ばれた場合、BaseVolumeにキャップされます
        /// 経過時間または目標音量到達のいずれかで終了します
        /// </summary>
        /// <param name="audioSource">対象のAudioSource</param>
        /// <param name="targetVolume">目標音量</param>
        /// <param name="duration">フェード時間</param>
        /// <param name="token">キャンセルトークン</param>
        /// <param name="playbackId">再生ID</param>
        /// <param name="state">フェードステート</param>
        private async Awaitable DoFade(AudioSource audioSource, float targetVolume, float duration, CancellationToken token, int playbackId, eRWaveFadeState state)
        {
            if (audioSource == null) return;

            // ステート開始: playbackIdに対応するPlaybackDataのステートを設定
            SetPlaybackDataState(playbackId, state);

            // 目標音量をBaseVolumeでキャップ
            var cappedTargetVolume = Mathf.Min(targetVolume, _audioSourceBaseVolumeNormalized);

            if (duration <= 0f)
            {
                audioSource.volume = cappedTargetVolume;
                // 即座に完了
                SetPlaybackDataState(playbackId, eRWaveFadeState.None);
                return;
            }

            // 既に目標音量に到達している場合は即座に終了
            if (Mathf.Abs(audioSource.volume - cappedTargetVolume) < 0.001f)
            {
                audioSource.volume = cappedTargetVolume;
                // 即座に完了
                SetPlaybackDataState(playbackId, eRWaveFadeState.None);
                return;
            }

            var startVolume = audioSource.volume;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                // キャンセル要求をチェック
                if (token.IsCancellationRequested)
                {
                    // キャンセル時: ステートをクリア
                    SetPlaybackDataState(playbackId, eRWaveFadeState.None);
                    return;
                }

                // 目標音量に到達したかチェック（早期終了）
                if (Mathf.Abs(audioSource.volume - cappedTargetVolume) < 0.001f)
                {
                    break;
                }

                // 現在の目標音量を計算（BaseVolumeが変わる可能性があるため毎フレーム）
                cappedTargetVolume = Mathf.Min(targetVolume, _audioSourceBaseVolumeNormalized);

                // 音量を補間
                var normalizedTime = elapsedTime / duration;
                var uncappedVolume = Mathf.Lerp(startVolume, targetVolume, normalizedTime);
                audioSource.volume = Mathf.Min(uncappedVolume, _audioSourceBaseVolumeNormalized);

                elapsedTime += Time.unscaledDeltaTime;

                // 次のフレームまで待機
                await Awaitable.NextFrameAsync(token);
            }

            // 最終的な音量を確実に設定（BaseVolumeでキャップ）
            audioSource.volume = Mathf.Min(targetVolume, _audioSourceBaseVolumeNormalized);

            // フェード完了: ステートをNoneに戻す
            SetPlaybackDataState(playbackId, eRWaveFadeState.None);
        }

        /// <summary>
        /// playbackVolume変更用のフェード処理
        /// BaseVolumeキャップを適用しない
        /// </summary>
        /// <param name="audioSource">対象のAudioSource</param>
        /// <param name="targetVolume">目標音量</param>
        /// <param name="duration">フェード時間</param>
        /// <param name="token">キャンセルトークン</param>
        /// <param name="playbackId">再生ID</param>
        private async Awaitable DoFadeForPlaybackVolume(AudioSource audioSource, float targetVolume, float duration, CancellationToken token, int playbackId)
        {
            if (audioSource == null) return;

            // ステート開始
            SetPlaybackDataState(playbackId, eRWaveFadeState.Attack);

            if (duration <= 0f)
            {
                audioSource.volume = targetVolume;
                SetPlaybackDataState(playbackId, eRWaveFadeState.None);
                return;
            }

            // 既に目標音量に到達している場合は即座に終了
            if (Mathf.Abs(audioSource.volume - targetVolume) < 0.001f)
            {
                audioSource.volume = targetVolume;
                SetPlaybackDataState(playbackId, eRWaveFadeState.None);
                return;
            }

            var startVolume = audioSource.volume;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                // キャンセル要求をチェック
                if (token.IsCancellationRequested)
                {
                    SetPlaybackDataState(playbackId, eRWaveFadeState.None);
                    return;
                }

                // 目標音量に到達したかチェック（早期終了）
                if (Mathf.Abs(audioSource.volume - targetVolume) < 0.001f)
                {
                    break;
                }

                // 音量を補間（BaseVolumeキャップなし）
                var normalizedTime = elapsedTime / duration;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, normalizedTime);

                elapsedTime += Time.unscaledDeltaTime;

                // 次のフレームまで待機
                await Awaitable.NextFrameAsync(token);
            }

            // 最終的な音量を確実に設定（BaseVolumeキャップなし）
            audioSource.volume = targetVolume;

            // フェード完了
            SetPlaybackDataState(playbackId, eRWaveFadeState.None);
        }

        /// <summary>
        /// クロスフェード処理：指定したAudioSourceをフェードアウト後に停止する
        /// </summary>
        /// <param name="audioSource">対象のAudioSource</param>
        /// <param name="duration">フェード時間</param>
        /// <param name="token">キャンセルトークン</param>
        /// <param name="playbackId">再生ID</param>
        private async Awaitable ExecuteCrossFadeOut(AudioSource audioSource, float duration, CancellationToken token, int playbackId)
        {
            await DoFade(audioSource, 0f, duration, token, playbackId, eRWaveFadeState.Release);
            if (!token.IsCancellationRequested && audioSource != null)
            {
                audioSource.Stop();

                // Releaseフェード完了後にPlaybackDataをReset
                ResetPlaybackDataByPlaybackId(playbackId);
            }
        }

        /// <summary>
        /// Release処理：指定したAudioSourceをフェードアウト後に停止する
        /// </summary>
        /// <param name="audioSource">対象のAudioSource</param>
        /// <param name="duration">フェード時間</param>
        /// <param name="token">キャンセルトークン</param>
        /// <param name="playbackId">再生ID</param>
        private async Awaitable ExecuteReleaseStop(AudioSource audioSource, float duration, CancellationToken token, int playbackId)
        {
            await DoFade(audioSource, 0f, duration, token, playbackId, eRWaveFadeState.Release);
            if (!token.IsCancellationRequested && audioSource != null)
            {
                audioSource.Stop();

                // Releaseフェード完了後にPlaybackDataをReset
                ResetPlaybackDataByPlaybackId(playbackId);
            }
        }
        
        /// <summary>
        /// 指定したplaybackIdのPlaybackDataからCancellationTokenを取得
        /// </summary>
        /// <param name="playbackId">再生ID</param>
        /// <returns>CancellationToken</returns>
        private CancellationToken GetPlaybackDataCancellationToken(int playbackId)
        {
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                if (_playbackDataPool[i].IsActiveWithId(playbackId))
                {
                    return _playbackDataPool[i].GetOrCreateCancellationToken();
                }
            }
            // 見つからない場合は新しいトークンを返す（エラーケース）
            return new CancellationTokenSource().Token;
        }

        /// <summary>
        /// Attack中のフェードをReleaseフェードに切り替える
        /// </summary>
        /// <param name="playbackData">対象のPlaybackData</param>
        private void SwitchToReleaseFade(RWavePlaybackData playbackData)
        {
            if (playbackData.audioSourceIndex < 0 || playbackData.audioSourceIndex >= _audioSourceList.Count)
            {
                return;
            }

            var audioSource = _audioSourceList[playbackData.audioSourceIndex];

            // 既存のAttackフェードをキャンセル
            playbackData.CancelFade();

            // 新しいCancellationTokenを取得
            var token = playbackData.GetOrCreateCancellationToken();

            // 現在の音量からReleaseフェードを開始
            if (_audioSourceSetting.releaseTimeInSeconds > 0f)
            {
                _ = ExecuteReleaseStop(audioSource, _audioSourceSetting.releaseTimeInSeconds,
                    token, playbackData.playbackId);
            }
            else
            {
                // Release時間が0の場合は即座に停止
                audioSource.Stop();
                playbackData.Reset();
            }
        }

        /// <summary>
        /// 再生データをプールに登録
        /// </summary>
        /// <param name="playbackId">再生ID</param>
        /// <param name="address">AudioClipアドレス</param>
        /// <param name="audioSourceIndex">AudioSourceインデックス</param>
        /// <param name="playVolume">再生時に指定された音量（nullの場合はBaseVolume使用）</param>
        private void RegisterPlaybackData(int playbackId, string address, int audioSourceIndex, float? playVolume)
        {
            // 空きスロットを探す（既に停止済みのスロットを再利用）
            int slotIndex = FindAvailablePlaybackSlot();

            if (slotIndex >= 0)
            {
                _playbackDataPool[slotIndex].Set(playbackId, address, audioSourceIndex, playVolume);
                _lastUsedPlaybackSlotIndex = slotIndex;
            }
        }

        /// <summary>
        /// 使用可能なプールスロットを検索
        /// </summary>
        /// <returns>使用可能なスロットのインデックス（見つからない場合は0）</returns>
        private int FindAvailablePlaybackSlot()
        {
            // 非アクティブスロットを探す
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                if (!_playbackDataPool[i].isActive)
                {
                    return i;
                }
            }

            // 再生が完了したスロットを探す
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                var audioSourceIndex = _playbackDataPool[i].audioSourceIndex;
                if (audioSourceIndex >= 0 && audioSourceIndex < _audioSourceList.Count)
                {
                    if (!_audioSourceList[audioSourceIndex].isPlaying)
                    {
                        _playbackDataPool[i].Reset();
                        return i;
                    }
                }
            }

            // すべて使用中の場合、最後に使用されたスロットの次を上書き
            int nextSlot = (_lastUsedPlaybackSlotIndex + 1) % _playbackDataPool.Length;
            var oldPlaybackData = _playbackDataPool[nextSlot];

            // 古い再生を強制停止
            if (oldPlaybackData.isActive)
            {
                // フェードをキャンセル
                oldPlaybackData.CancelFade();

                // AudioSourceを即座に停止
                if (oldPlaybackData.audioSourceIndex >= 0 && oldPlaybackData.audioSourceIndex < _audioSourceList.Count)
                {
                    _audioSourceList[oldPlaybackData.audioSourceIndex].Stop();
                }

                // PlaybackDataをリセット
                oldPlaybackData.Reset();
            }

            return nextSlot;
        }

        /// <summary>
        /// 指定したplaybackIdに対応するPlaybackDataのステートを設定
        /// </summary>
        /// <param name="playbackId">再生ID</param>
        /// <param name="state">設定するステート</param>
        private void SetPlaybackDataState(int playbackId, eRWaveFadeState state)
        {
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                if (_playbackDataPool[i].IsActiveWithId(playbackId))
                {
                    _playbackDataPool[i].SetFadeState(state);
                    return;
                }
            }
        }

        /// <summary>
        /// AudioSourceIndexから対応するplaybackIdを取得
        /// </summary>
        /// <param name="audioSourceIndex">AudioSourceのインデックス</param>
        /// <returns>playbackId（見つからない場合は-1）</returns>
        private int GetPlaybackIdByAudioSourceIndex(int audioSourceIndex)
        {
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                if (_playbackDataPool[i].isActive && _playbackDataPool[i].audioSourceIndex == audioSourceIndex)
                {
                    return _playbackDataPool[i].playbackId;
                }
            }
            return -1;
        }

        /// <summary>
        /// 指定したplaybackIdに対応するPlaybackDataをReset
        /// </summary>
        /// <param name="playbackId">再生ID</param>
        private void ResetPlaybackDataByPlaybackId(int playbackId)
        {
            for (int i = 0; i < _playbackDataPool.Length; i++)
            {
                if (_playbackDataPool[i].IsActiveWithId(playbackId))
                {
                    _playbackDataPool[i].Reset();
                    return;
                }
            }
        }

        /// <summary>
        /// オブジェクト破棄時のクリーンアップ
        /// </summary>
        private void OnDestroy()
        {
            // 全てのPlaybackDataをクリーンアップ
            if (_playbackDataPool != null)
            {
                for (int i = 0; i < _playbackDataPool.Length; i++)
                {
                    _playbackDataPool[i]?.Reset();
                }
            }
        }
    }
}