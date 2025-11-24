using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using RWave.System;
using RWave.Data;
using RWave.Extensions;

namespace RWave.Demo
{
    /// <summary>
    /// アクティブなHandlerの情報
    /// </summary>
    public class HandlerInfo
    {
        public RWaveAudioHandler handler;
        public string address;
        public int playbackId;

        public string DisplayName => $"Address: {address} (ID: {playbackId})";
    }

    /// <summary>
    /// RWaveデモUI用のコントローラー
    /// UIToolKitを使用してオーディオのロード、再生、音量制御を行う
    /// </summary>
    public class RWaveDemoUIController : MonoBehaviour
    {
        [SerializeField, Header("UIDocument")]
        private UIDocument _uiDocument;

        private TextField _addressField;
        private TextField _resourceGroupField;
        private Button _loadButton;
        private Button _syncLoadButton;
        private Label _loadStatus;

        private TextField _playAddressField;
        private TextField _audioSourceNameField;
        private TextField _playResourceGroupField;
        private Button _playButton;
        private Button _stopButton;
        private Button _forceStopButton;

        private TextField _mixerGroupNameField;
        private Slider _mixerVolumeSlider;
        private Button _getMixerVolumeButton;
        private Button _setMixerVolumeButton;

        private TextField _sourceNameField;
        private Slider _sourceVolumeSlider;
        private Button _getSourceVolumeButton;
        private Button _setSourceVolumeButton;

        private CancellationTokenSource _cancellationTokenSource;

        // 再生制御セクション（playVolume関連）
        private Toggle _usePlayVolumeToggle;
        private Slider _playVolumeSlider;
        private Label _handlerStatusLabel;

        // Handler制御セクション
        private Slider _handlerVolumeSlider;
        private TextField _handlerFadeDurationField;
        private Button _setHandlerVolumeButton;
        private Button _checkHandlerPlayingButton;
        private Button _stopHandlerButton;
        private Label _handlerPlayingLabel;

        // 複数Handler管理用
        private Dictionary<int, HandlerInfo> _activeHandlers = new Dictionary<int, HandlerInfo>();
        private int _selectedHandlerId = -1;
        private ListView _handlerListView;
        private Label _selectedHandlerLabel;

        // タブ制御用
        private Button _tabAudioPlaybackButton;
        private Button _tabVolumeHandlerButton;
        private VisualElement _tabContentAudioPlayback;
        private VisualElement _tabContentVolumeHandler;

        private void OnEnable()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("UIDocument is not assigned.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            SetupUI();
        }

        private void OnDisable()
        {
            CleanupUI();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private void OnDestroy()
        {
            CleanupUI();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        /// <summary>
        /// 定期的にHandler一覧を更新する
        /// </summary>
        private void Update()
        {
            // 1秒ごとにリフレッシュ（負荷軽減）
            if (Time.frameCount % 60 == 0)
            {
                RefreshHandlerList();
            }
        }

        /// <summary>
        /// UIの初期設定を行う
        /// </summary>
        private void SetupUI()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("UIDocument is null in SetupUI");
                return;
            }

            var root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("rootVisualElement is null");
                return;
            }

            // Audio Loading Section
            _addressField = root.Q<TextField>("address-field");
            _resourceGroupField = root.Q<TextField>("resource-group-field");
            _loadButton = root.Q<Button>("load-button");
            _syncLoadButton = root.Q<Button>("sync-load-button");
            _loadStatus = root.Q<Label>("load-status");

            _loadButton.clicked += OnLoadButtonClicked;
            _syncLoadButton.clicked += OnSyncLoadButtonClicked;

            // Playback Control Section
            _playAddressField = root.Q<TextField>("play-address-field");
            _audioSourceNameField = root.Q<TextField>("audiosource-name-field");
            _playResourceGroupField = root.Q<TextField>("play-resource-group-field");
            _playButton = root.Q<Button>("play-button");
            _stopButton = root.Q<Button>("stop-button");
            _forceStopButton = root.Q<Button>("force-stop-button");

            _playButton.clicked += OnPlayButtonClicked;
            _stopButton.clicked += OnStopButtonClicked;
            _forceStopButton.clicked += OnForceStopButtonClicked;

            // Volume Control Section - AudioMixerGroup
            _mixerGroupNameField = root.Q<TextField>("mixer-group-name-field");
            _mixerVolumeSlider = root.Q<Slider>("mixer-volume-slider");
            _getMixerVolumeButton = root.Q<Button>("get-mixer-volume-button");
            _setMixerVolumeButton = root.Q<Button>("set-mixer-volume-button");

            _getMixerVolumeButton.clicked += OnGetMixerVolumeButtonClicked;
            _setMixerVolumeButton.clicked += OnSetMixerVolumeButtonClicked;

            // Volume Control Section - AudioSource
            _sourceNameField = root.Q<TextField>("source-name-field");
            _sourceVolumeSlider = root.Q<Slider>("source-volume-slider");
            _getSourceVolumeButton = root.Q<Button>("get-source-volume-button");
            _setSourceVolumeButton = root.Q<Button>("set-source-volume-button");

            _getSourceVolumeButton.clicked += OnGetSourceVolumeButtonClicked;
            _setSourceVolumeButton.clicked += OnSetSourceVolumeButtonClicked;

            // Playback Control Section - playVolume
            _usePlayVolumeToggle = root.Q<Toggle>("use-play-volume-toggle");
            _playVolumeSlider = root.Q<Slider>("play-volume-slider");

            if (_usePlayVolumeToggle != null)
            {
                _usePlayVolumeToggle.RegisterValueChangedCallback(OnUsePlayVolumeToggleChanged);
            }

            // Handler Status Label
            _handlerStatusLabel = root.Q<Label>("handler-status-label");

            // Handler Control Section - ListView
            _handlerListView = root.Q<ListView>("handler-list-view");
            _selectedHandlerLabel = root.Q<Label>("selected-handler-label");

            if (_handlerListView != null)
            {
                SetupHandlerListView();
            }

            // Handler Control Section - 制御UI
            _handlerVolumeSlider = root.Q<Slider>("handler-volume-slider");
            _handlerFadeDurationField = root.Q<TextField>("handler-fade-duration-field");
            _setHandlerVolumeButton = root.Q<Button>("set-handler-volume-button");
            _checkHandlerPlayingButton = root.Q<Button>("check-handler-playing-button");
            _stopHandlerButton = root.Q<Button>("stop-handler-button");
            _handlerPlayingLabel = root.Q<Label>("handler-playing-label");

            if (_setHandlerVolumeButton != null)
                _setHandlerVolumeButton.clicked += OnSetHandlerVolumeButtonClicked;
            if (_checkHandlerPlayingButton != null)
                _checkHandlerPlayingButton.clicked += OnCheckHandlerPlayingButtonClicked;
            if (_stopHandlerButton != null)
                _stopHandlerButton.clicked += OnStopHandlerButtonClicked;

            // スライダーの入力フィールドの幅を設定
            SetSliderInputFieldWidth(_mixerVolumeSlider, 150);
            SetSliderInputFieldWidth(_sourceVolumeSlider, 150);
            SetSliderInputFieldWidth(_playVolumeSlider, 150);
            SetSliderInputFieldWidth(_handlerVolumeSlider, 150);

            // タブ制御の初期化
            _tabAudioPlaybackButton = root.Q<Button>("tab-audio-playback");
            _tabVolumeHandlerButton = root.Q<Button>("tab-volume-handler");
            _tabContentAudioPlayback = root.Q<VisualElement>("tab-content-audio-playback");
            _tabContentVolumeHandler = root.Q<VisualElement>("tab-content-volume-handler");

            if (_tabAudioPlaybackButton != null)
                _tabAudioPlaybackButton.clicked += () => SwitchTab(0);
            if (_tabVolumeHandlerButton != null)
                _tabVolumeHandlerButton.clicked += () => SwitchTab(1);

            // 初期タブ表示
            SwitchTab(0);

            // 初期状態設定
            if (_playVolumeSlider != null)
                _playVolumeSlider.SetEnabled(false);
            if (_handlerStatusLabel != null)
                _handlerStatusLabel.text = "No handler";
            if (_handlerPlayingLabel != null)
                _handlerPlayingLabel.text = "-";
            UpdateHandlerControlsEnabled();
        }

        /// <summary>
        /// UIのクリーンアップを行う
        /// </summary>
        private void CleanupUI()
        {
            // UIDocumentまたはrootVisualElementが既に破棄されている場合はスキップ
            if (_uiDocument == null)
            {
                return;
            }

            try
            {
                if (_loadButton != null) _loadButton.clicked -= OnLoadButtonClicked;
                if (_syncLoadButton != null) _syncLoadButton.clicked -= OnSyncLoadButtonClicked;
                if (_playButton != null) _playButton.clicked -= OnPlayButtonClicked;
                if (_stopButton != null) _stopButton.clicked -= OnStopButtonClicked;
                if (_forceStopButton != null) _forceStopButton.clicked -= OnForceStopButtonClicked;
                if (_getMixerVolumeButton != null) _getMixerVolumeButton.clicked -= OnGetMixerVolumeButtonClicked;
                if (_setMixerVolumeButton != null) _setMixerVolumeButton.clicked -= OnSetMixerVolumeButtonClicked;
                if (_getSourceVolumeButton != null) _getSourceVolumeButton.clicked -= OnGetSourceVolumeButtonClicked;
                if (_setSourceVolumeButton != null) _setSourceVolumeButton.clicked -= OnSetSourceVolumeButtonClicked;
                if (_usePlayVolumeToggle != null) _usePlayVolumeToggle.UnregisterValueChangedCallback(OnUsePlayVolumeToggleChanged);
                if (_setHandlerVolumeButton != null) _setHandlerVolumeButton.clicked -= OnSetHandlerVolumeButtonClicked;
                if (_checkHandlerPlayingButton != null) _checkHandlerPlayingButton.clicked -= OnCheckHandlerPlayingButtonClicked;
                if (_stopHandlerButton != null) _stopHandlerButton.clicked -= OnStopHandlerButtonClicked;
                if (_tabAudioPlaybackButton != null) _tabAudioPlaybackButton.clicked -= () => SwitchTab(0);
                if (_tabVolumeHandlerButton != null) _tabVolumeHandlerButton.clicked -= () => SwitchTab(1);

                // ListView イベント削除
                if (_handlerListView != null)
                {
                    _handlerListView.selectionChanged -= OnHandlerSelectionChanged;
                }
            }
            catch (Exception e)
            {
                // UIDocumentが既に破棄されている場合は例外を無視
                Debug.LogWarning($"Exception during UI cleanup (this is usually safe to ignore): {e.Message}");
            }
        }

        #region Audio Loading Handlers

        /// <summary>
        /// 非同期ロードボタンクリック時の処理
        /// </summary>
        private void OnLoadButtonClicked()
        {
            _ = LoadAudioClipAsync();
        }

        /// <summary>
        /// AudioClipを非同期で読み込む
        /// </summary>
        private async Awaitable LoadAudioClipAsync()
        {
            if (RWaveSoundManager.Instance == null)
            {
                _loadStatus.text = "Error: RWaveSoundManager is not initialized";
                return;
            }

            var address = _addressField.value;
            if (string.IsNullOrEmpty(address))
            {
                _loadStatus.text = "Error: Address is empty";
                return;
            }

            var resourceGroup = _resourceGroupField.value;
            _loadStatus.text = "Loading...";

            try
            {
                bool success;
                if (string.IsNullOrEmpty(resourceGroup))
                {
                    success = await RWaveSoundManager.Instance.LoadAudioClip(address, _cancellationTokenSource.Token);
                }
                else
                {
                    success = await RWaveSoundManager.Instance.LoadAudioClip(address, resourceGroup, _cancellationTokenSource.Token);
                }

                _loadStatus.text = success ? $"Loaded: {address}" : $"Failed to load: {address}";
            }
            catch (Exception e)
            {
                _loadStatus.text = $"Error: {e.Message}";
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 同期ロードボタンクリック時の処理
        /// </summary>
        private void OnSyncLoadButtonClicked()
        {
            if (RWaveSoundManager.Instance == null)
            {
                _loadStatus.text = "Error: RWaveSoundManager is not initialized";
                return;
            }

            var address = _addressField.value;
            if (string.IsNullOrEmpty(address))
            {
                _loadStatus.text = "Error: Address is empty";
                return;
            }

            var resourceGroup = _resourceGroupField.value;
            _loadStatus.text = "Loading (Sync)...";

            try
            {
                bool success;
                if (string.IsNullOrEmpty(resourceGroup))
                {
                    success = RWaveSoundManager.Instance.SyncLoadAudioClip(address);
                }
                else
                {
                    success = RWaveSoundManager.Instance.SyncLoadAudioClip(address, resourceGroup);
                }

                _loadStatus.text = success ? $"Loaded (Sync): {address}" : $"Failed to load: {address}";
            }
            catch (Exception e)
            {
                _loadStatus.text = $"Error: {e.Message}";
                Debug.LogError(e);
            }
        }

        #endregion

        #region Playback Control Handlers

        /// <summary>
        /// 再生ボタンクリック時の処理
        /// </summary>
        private void OnPlayButtonClicked()
        {
            if (RWaveSoundManager.Instance == null)
            {
                Debug.LogWarning("RWaveSoundManager is not initialized");
                return;
            }

            var address = _playAddressField.value;
            var audioSourceName = _audioSourceNameField.value;

            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning("Play address is empty");
                return;
            }

            if (string.IsNullOrEmpty(audioSourceName))
            {
                Debug.LogWarning("AudioSource name is empty");
                return;
            }

            var resourceGroup = _playResourceGroupField.value;

            // playVolume指定があるかチェック
            float? playVolume = null;
            if (_usePlayVolumeToggle != null && _usePlayVolumeToggle.value)
            {
                playVolume = _playVolumeSlider.value;
            }

            // 再生実行とハンドラー保持
            RWaveAudioHandler handler;
            if (string.IsNullOrEmpty(resourceGroup))
            {
                handler = playVolume.HasValue
                    ? RWaveSoundManager.Instance.Play(address, audioSourceName, playVolume.Value)
                    : RWaveSoundManager.Instance.Play(address, audioSourceName);
            }
            else
            {
                handler = playVolume.HasValue
                    ? RWaveSoundManager.Instance.Play(address, audioSourceName, resourceGroup, playVolume.Value)
                    : RWaveSoundManager.Instance.Play(address, audioSourceName, resourceGroup);
            }

            // ハンドラーをリストに追加
            if (handler.IsValid())
            {
                var handlerInfo = new HandlerInfo
                {
                    handler = handler,
                    address = address,
                    playbackId = handler.playbackId
                };

                _activeHandlers[handler.playbackId] = handlerInfo;
                RefreshHandlerList();

                // 新しく再生したHandlerを自動選択
                _selectedHandlerId = handler.playbackId;
                var handlerList = _activeHandlers.Values.ToList();
                var index = handlerList.FindIndex(h => h.playbackId == handler.playbackId);
                if (index >= 0 && _handlerListView != null)
                {
                    _handlerListView.selectedIndex = index;
                }

                // Playback Control Sectionのステータスラベルを更新
                if (_handlerStatusLabel != null)
                {
                    _handlerStatusLabel.text = $"Playing: {address} (ID: {handler.playbackId})";
                }
            }

            Debug.Log($"Played: {address} on {audioSourceName}" +
                      (playVolume.HasValue ? $" with volume {playVolume.Value}" : ""));
        }

        /// <summary>
        /// 停止ボタンクリック時の処理
        /// </summary>
        private void OnStopButtonClicked()
        {
            if (RWaveSoundManager.Instance == null)
            {
                Debug.LogWarning("RWaveSoundManager is not initialized");
                return;
            }

            var audioSourceName = _audioSourceNameField.value;

            if (string.IsNullOrEmpty(audioSourceName))
            {
                Debug.LogWarning("AudioSource name is empty");
                return;
            }

            RWaveSoundManager.Instance.Stop(audioSourceName);
            Debug.Log($"Stopping: {audioSourceName}");
        }

        /// <summary>
        /// 強制停止ボタンクリック時の処理
        /// </summary>
        private void OnForceStopButtonClicked()
        {
            if (RWaveSoundManager.Instance == null)
            {
                Debug.LogWarning("RWaveSoundManager is not initialized");
                return;
            }

            var audioSourceName = _audioSourceNameField.value;

            if (string.IsNullOrEmpty(audioSourceName))
            {
                Debug.LogWarning("AudioSource name is empty");
                return;
            }

            RWaveSoundManager.Instance.ForceStop(audioSourceName);
            Debug.Log($"Force stopping: {audioSourceName}");
        }

        #endregion

        #region Handler List Management

        /// <summary>
        /// Handler ListViewのセットアップ
        /// </summary>
        private void SetupHandlerListView()
        {
            // 項目生成関数
            _handlerListView.makeItem = () =>
            {
                var label = new Label();
                label.style.fontSize = 22;
                label.style.paddingLeft = 8;
                label.style.paddingRight = 8;
                label.style.paddingTop = 6;
                label.style.paddingBottom = 6;
                label.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.95f));
                return label;
            };

            // データバインディング関数
            _handlerListView.bindItem = (element, index) =>
            {
                var label = element as Label;
                if (label != null && _handlerListView.itemsSource is List<HandlerInfo> items && index < items.Count)
                {
                    label.text = items[index].DisplayName;
                }
            };

            // 選択変更イベント
            _handlerListView.selectionChanged += OnHandlerSelectionChanged;

            // 初期状態
            RefreshHandlerList();
        }

        /// <summary>
        /// Handler選択変更時の処理
        /// </summary>
        private void OnHandlerSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selected = selectedItems.FirstOrDefault() as HandlerInfo;

            if (selected != null)
            {
                _selectedHandlerId = selected.playbackId;

                if (_selectedHandlerLabel != null)
                {
                    _selectedHandlerLabel.text = $"Selected: {selected.DisplayName}";
                }

                // Playback Control Sectionのステータスラベルも更新
                if (_handlerStatusLabel != null)
                {
                    _handlerStatusLabel.text = $"Selected: {selected.address} (ID: {selected.playbackId})";
                }

                UpdateHandlerControlsEnabled();
            }
            else
            {
                _selectedHandlerId = -1;

                if (_selectedHandlerLabel != null)
                {
                    _selectedHandlerLabel.text = "No handler selected";
                }

                // Playback Control Sectionのステータスラベルもリセット
                if (_handlerStatusLabel != null)
                {
                    _handlerStatusLabel.text = "No handler";
                }

                UpdateHandlerControlsEnabled();
            }
        }

        /// <summary>
        /// Handler一覧を更新
        /// </summary>
        private void RefreshHandlerList()
        {
            if (_handlerListView == null)
                return;

            // 再生中でないHandlerを自動削除
            var toRemove = new List<int>();
            foreach (var kvp in _activeHandlers)
            {
                if (!kvp.Value.handler.IsValid() || !kvp.Value.handler.IsPlaying())
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var id in toRemove)
            {
                _activeHandlers.Remove(id);
            }

            // ListViewに反映
            _handlerListView.itemsSource = _activeHandlers.Values.ToList();
            _handlerListView.Rebuild();
        }

        /// <summary>
        /// Handlerをリストから削除
        /// </summary>
        private void RemoveHandler(int playbackId)
        {
            if (_activeHandlers.Remove(playbackId))
            {
                RefreshHandlerList();

                // 削除したHandlerが選択中だった場合
                if (_selectedHandlerId == playbackId)
                {
                    _selectedHandlerId = -1;
                    _handlerListView.ClearSelection();

                    if (_selectedHandlerLabel != null)
                    {
                        _selectedHandlerLabel.text = "No handler selected";
                    }

                    // Playback Control Sectionのステータスラベルもリセット
                    if (_handlerStatusLabel != null)
                    {
                        _handlerStatusLabel.text = "No handler";
                    }

                    UpdateHandlerControlsEnabled();
                }
            }
        }

        #endregion

        #region Slider Helper Methods

        /// <summary>
        /// スライダーの入力フィールドの幅を設定する
        /// </summary>
        /// <param name="slider">対象のスライダー</param>
        /// <param name="width">設定する幅</param>
        private void SetSliderInputFieldWidth(Slider slider, float width)
        {
            if (slider == null) return;

            // スライダー内のTextFieldを取得
            var textField = slider.Q<TextField>();
            if (textField != null)
            {
                textField.style.width = width;
                textField.style.minWidth = width;
                textField.style.maxWidth = width;

                // 入力フィールド部分も設定
                var input = textField.Q(className: "unity-text-field__input");
                if (input != null)
                {
                    input.style.width = width;
                    input.style.minWidth = width;
                }
            }
        }

        #endregion

        #region Volume Control Handlers

        /// <summary>
        /// AudioMixerGroup現在音量取得ボタンクリック時の処理
        /// </summary>
        private void OnGetMixerVolumeButtonClicked()
        {
            if (RWaveSoundManager.Instance == null)
            {
                Debug.LogWarning("RWaveSoundManager is not initialized");
                return;
            }

            var mixerGroupName = _mixerGroupNameField.value;

            if (string.IsNullOrEmpty(mixerGroupName))
            {
                Debug.LogWarning("AudioMixerGroup name is empty");
                return;
            }

            var currentVolume = RWaveSoundManager.Instance.GetAudioMixerGroupVolume(mixerGroupName);
            _mixerVolumeSlider.value = currentVolume;
            Debug.Log($"Get AudioMixerGroup '{mixerGroupName}' volume: {currentVolume}");
        }

        /// <summary>
        /// AudioMixerGroup音量設定ボタンクリック時の処理
        /// </summary>
        private void OnSetMixerVolumeButtonClicked()
        {
            if (RWaveSoundManager.Instance == null)
            {
                Debug.LogWarning("RWaveSoundManager is not initialized");
                return;
            }

            var mixerGroupName = _mixerGroupNameField.value;

            if (string.IsNullOrEmpty(mixerGroupName))
            {
                Debug.LogWarning("AudioMixerGroup name is empty");
                return;
            }

            var volume = _mixerVolumeSlider.value;
            RWaveSoundManager.Instance.SetAudioMixerGroupVolume(mixerGroupName, volume);
            Debug.Log($"Set AudioMixerGroup '{mixerGroupName}' volume to {volume}");
        }

        /// <summary>
        /// AudioSource現在音量取得ボタンクリック時の処理
        /// </summary>
        private void OnGetSourceVolumeButtonClicked()
        {
            if (RWaveSoundManager.Instance == null)
            {
                Debug.LogWarning("RWaveSoundManager is not initialized");
                return;
            }

            var sourceName = _sourceNameField.value;

            if (string.IsNullOrEmpty(sourceName))
            {
                Debug.LogWarning("AudioSource name is empty");
                return;
            }

            var currentVolume = RWaveSoundManager.Instance.GetAudioSourceVolume(sourceName);
            _sourceVolumeSlider.value = currentVolume;
            Debug.Log($"Get AudioSource '{sourceName}' volume: {currentVolume}");
        }

        /// <summary>
        /// AudioSource音量設定ボタンクリック時の処理
        /// </summary>
        private void OnSetSourceVolumeButtonClicked()
        {
            if (RWaveSoundManager.Instance == null)
            {
                Debug.LogWarning("RWaveSoundManager is not initialized");
                return;
            }

            var sourceName = _sourceNameField.value;

            if (string.IsNullOrEmpty(sourceName))
            {
                Debug.LogWarning("AudioSource name is empty");
                return;
            }

            var volume = _sourceVolumeSlider.value;
            RWaveSoundManager.Instance.SetAudioSourceVolume(sourceName, volume);
            Debug.Log($"Set AudioSource '{sourceName}' volume to {volume}");
        }

        #endregion

        #region Handler Control Handlers

        /// <summary>
        /// playVolume使用トグルの変更時
        /// </summary>
        private void OnUsePlayVolumeToggleChanged(ChangeEvent<bool> evt)
        {
            if (_playVolumeSlider != null)
            {
                _playVolumeSlider.SetEnabled(evt.newValue);
            }
        }

        /// <summary>
        /// Handler音量設定ボタンクリック時
        /// </summary>
        private void OnSetHandlerVolumeButtonClicked()
        {
            if (_selectedHandlerId == -1 || !_activeHandlers.ContainsKey(_selectedHandlerId))
            {
                Debug.LogWarning("No valid handler selected");
                return;
            }

            var handlerInfo = _activeHandlers[_selectedHandlerId];

            if (!handlerInfo.handler.IsValid())
            {
                Debug.LogWarning("Selected handler is no longer valid");
                RemoveHandler(_selectedHandlerId);
                return;
            }

            float volume = _handlerVolumeSlider.value;
            float fadeDuration = 0f;

            if (_handlerFadeDurationField != null &&
                !string.IsNullOrEmpty(_handlerFadeDurationField.value))
            {
                if (float.TryParse(_handlerFadeDurationField.value, out float parsedDuration))
                {
                    fadeDuration = parsedDuration;
                }
            }

            handlerInfo.handler.SetVolume(volume, fadeDuration);
            Debug.Log($"Set handler {_selectedHandlerId} volume to {volume} with fade duration {fadeDuration}s");
        }

        /// <summary>
        /// 再生状態確認ボタンクリック時
        /// </summary>
        private void OnCheckHandlerPlayingButtonClicked()
        {
            if (_selectedHandlerId == -1 || !_activeHandlers.ContainsKey(_selectedHandlerId))
            {
                if (_handlerPlayingLabel != null)
                    _handlerPlayingLabel.text = "No handler selected";
                return;
            }

            var handlerInfo = _activeHandlers[_selectedHandlerId];

            if (!handlerInfo.handler.IsValid())
            {
                if (_handlerPlayingLabel != null)
                    _handlerPlayingLabel.text = "Invalid handler";
                RemoveHandler(_selectedHandlerId);
                return;
            }

            bool isPlaying = handlerInfo.handler.IsPlaying();

            if (_handlerPlayingLabel != null)
                _handlerPlayingLabel.text = isPlaying ? "Playing" : "Stopped";

            // 停止している場合は自動削除
            if (!isPlaying)
            {
                RemoveHandler(_selectedHandlerId);
            }

            Debug.Log($"Handler {_selectedHandlerId} playing status: {isPlaying}");
        }

        /// <summary>
        /// Handler経由停止ボタンクリック時
        /// </summary>
        private void OnStopHandlerButtonClicked()
        {
            if (_selectedHandlerId == -1 || !_activeHandlers.ContainsKey(_selectedHandlerId))
            {
                Debug.LogWarning("No valid handler selected");
                return;
            }

            var handlerInfo = _activeHandlers[_selectedHandlerId];

            if (!handlerInfo.handler.IsValid())
            {
                Debug.LogWarning("Selected handler is no longer valid");
                RemoveHandler(_selectedHandlerId);
                return;
            }

            var audioSourceName = _audioSourceNameField.value;
            if (string.IsNullOrEmpty(audioSourceName))
            {
                Debug.LogError("Audio source name is empty");
                return;
            }

            RWaveSoundManager.Instance.Stop(handlerInfo.handler, audioSourceName);
            Debug.Log($"Stopped handler {_selectedHandlerId} on {audioSourceName}");

            // 停止後は自動削除
            RemoveHandler(_selectedHandlerId);
        }

        /// <summary>
        /// Handler制御UIの有効/無効を更新
        /// </summary>
        private void UpdateHandlerControlsEnabled()
        {
            bool hasSelection = _selectedHandlerId != -1 && _activeHandlers.ContainsKey(_selectedHandlerId);

            if (_setHandlerVolumeButton != null)
                _setHandlerVolumeButton.SetEnabled(hasSelection);
            if (_checkHandlerPlayingButton != null)
                _checkHandlerPlayingButton.SetEnabled(hasSelection);
            if (_stopHandlerButton != null)
                _stopHandlerButton.SetEnabled(hasSelection);
            if (_handlerVolumeSlider != null)
                _handlerVolumeSlider.SetEnabled(hasSelection);
            if (_handlerFadeDurationField != null)
                _handlerFadeDurationField.SetEnabled(hasSelection);
        }

        #endregion

        #region Tab Control

        /// <summary>
        /// タブを切り替える
        /// </summary>
        /// <param name="tabIndex">タブインデックス（0: Audio&Playback, 1: Volume&Handler）</param>
        private void SwitchTab(int tabIndex)
        {
            if (_tabContentAudioPlayback == null || _tabContentVolumeHandler == null)
                return;

            // タブコンテンツの表示切り替え
            if (tabIndex == 0)
            {
                _tabContentAudioPlayback.style.display = DisplayStyle.Flex;
                _tabContentVolumeHandler.style.display = DisplayStyle.None;

                // タブボタンのアクティブ状態切り替え
                if (_tabAudioPlaybackButton != null)
                {
                    _tabAudioPlaybackButton.AddToClassList("tab-button-active");
                }
                if (_tabVolumeHandlerButton != null)
                {
                    _tabVolumeHandlerButton.RemoveFromClassList("tab-button-active");
                }
            }
            else if (tabIndex == 1)
            {
                _tabContentAudioPlayback.style.display = DisplayStyle.None;
                _tabContentVolumeHandler.style.display = DisplayStyle.Flex;

                // タブボタンのアクティブ状態切り替え
                if (_tabAudioPlaybackButton != null)
                {
                    _tabAudioPlaybackButton.RemoveFromClassList("tab-button-active");
                }
                if (_tabVolumeHandlerButton != null)
                {
                    _tabVolumeHandlerButton.AddToClassList("tab-button-active");
                }
            }
        }

        #endregion
    }
}
