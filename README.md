# RWave - Audio Management Framework

RWaveはUnity向けの包括的なオーディオ管理フレームワークです。
Addressablesを使用したオーディオリソース管理、音量制御、フェード機能、クロスフェード機能を提供します。

## 特徴

- **AddressablesFriendly**:Addressablesを使用したオーディオクリップの読み込みと管理
- **オーディオソース管理**: BGM、SE、Voiceなど目的に合わせた複数のオーディオソースを一元管理
- **AudioMixerGroupFriendlyな音量制御システム**: AudioMixerGroup単位、AudioSource単位での細かい音量制御
- **フェード機能**: カスタマイズ可能な継続時間でのフェードイン/フェードアウト
- **クロスフェード**: シームレスなオーディオトランジション機能
- **ScriptableObjectベースのAudioClip管理**:Addressablesを使用できない環境下やあえて使用したくない場合にも対応可能なAudioPack機能
- **音量データのエクスポート/インポート**: 音量設定の保存と復元

## インストール

### Unity Package Manager (UPM) 経由

1. Unity エディタで `Window > Package Manager` を開く
2. `+` ボタンをクリック
3. `Add package from git URL` を選択
4. 以下の URL を入力:
   ```
   https://github.com/Techno1109/RWave.git?path=Assets/RWave
   ```

## 必要要件

- **Unity**: 6000.2.13f1 以降
- **依存パッケージ**:
  - Unity Addressables (2.6.0)

## クイックスタート

### デモのインポート

Package Manager > RWave > Samples からデモをインポートしてください。

### デモシーンで試す

1. `Assets/RWave/Demo/Scenes/RWaveDemo.unity` を開く
2. Play ボタンを押す

### プロジェクトに組み込む

`Assets/RWave/Demo/Prefabs/RWave.prefab` をシーンに配置するだけで使用できます。

詳細は「デモシーン」セクションと「基本的な使い方」セクションを参照してください。

## デモシーン

Package Manager > RWave > Samples からデモをインポートしてください。

### 含まれるファイル

- **RWaveDemo.unity**: デモシーン
- **RWave.prefab**: 設定済みのRWaveSoundManager（シーンに配置するだけで使用可能）
- **SampleSetting.asset**: サンプル設定ファイル
- **SampleAudioMixer.mixer**: AudioMixer設定例
- **RWaveDemoUIController.cs**: デモUI実装（CancellationToken管理、Handler管理、エラーハンドリングの参考例）

### プレハブの使い方

`RWave.prefab` をシーンに配置すると、以下が設定済みの状態で使用できます：
- RWaveSoundManager コンポーネント
- SampleSetting.asset の参照
- Auto Initialize On Awake が有効

必要に応じて Inspector で RWaveSetting を独自のものに差し替えてください。

## 基本的な使い方

**注意**: RWaveを初めて使用する場合は、まず「クイックスタート」と「デモシーン」セクションを確認することをお勧めします。デモシーンには実践的な例がすべて揃っています。

以下では、RWaveをゼロから設定する方法を説明します。

### 1. 設定の作成

1. Unity エディタで `Assets > Create > RWave > RWaveSetting` を選択
2. AudioMixer、AudioSource 設定、AudioMixerGroup 設定を構成

### 2. RWaveSoundManager の配置

1. シーンに空の GameObject を作成
2. `RWaveSoundManager` コンポーネントをアタッチ
3. 作成した RWaveSetting を割り当て

### 3. オーディオクリップの読み込み

```csharp
using UnityEngine;
using RWave.System;
using System.Threading;

public class AudioLoader : MonoBehaviour
{
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _cts = new CancellationTokenSource();
    }

    private async void Start()
    {
        // オーディオクリップを読み込む
        await RWaveSoundManager.Instance.LoadAudioClip("audio_address", _cts.Token);

        // ラベルで読み込む
        await RWaveSoundManager.Instance.LoadAudioClipWithAddressableLabel("bgm_label", _cts.Token);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

#### 同期読み込み

同期的にオーディオクリップを読み込むこともできます。

```csharp
// 単一のオーディオクリップを同期読み込み
bool success = RWaveSoundManager.Instance.SyncLoadAudioClip("audio_address");

// ResourceGroupを指定して同期読み込み
bool success = RWaveSoundManager.Instance.SyncLoadAudioClip("audio_address", "AudioResourceGroupName");

// 複数のオーディオクリップを同期読み込み
var addressList = new List<string> { "audio_address1", "audio_address2" };
bool success = RWaveSoundManager.Instance.SyncLoadAudioClip(addressList);
```

### 4. オーディオの再生

```csharp
// オーディオを再生(ResourceGroup指定無し)
RWaveSoundManager.Instance.Play("audio_address", "AudioSourceName");

// オーディオを再生(ResourceGroup指定あり)
RWaveSoundManager.Instance.Play("audio_address", "AudioSourceName", "AudioResourceGroupName");

// 再生時音量を指定して再生
RWaveSoundManager.Instance.Play("audio_address", "AudioSourceName", playVolume: 50f);

// 停止（フェードアウト付き）
RWaveSoundManager.Instance.Stop("AudioSourceName");

// 即座に停止
RWaveSoundManager.Instance.ForceStop("AudioSourceName");
```

#### 再生制御用ハンドラー

`Play()`メソッドは`RWaveAudioHandler`を返します。これを使って個別の再生を制御できます。

```csharp
using RWave.Data;

// ハンドラーを取得
RWaveAudioHandler handler = RWaveSoundManager.Instance.Play("audio_address", "AudioSourceName");

// ハンドラーを使った音量制御
handler.SetVolume(80f, fadeDuration: 2.0f);

// 再生状態の確認
bool isPlaying = handler.IsPlaying();

// ハンドラーを使った停止
RWaveSoundManager.Instance.Stop(handler, "AudioSourceName");
```

### 5. 音量制御

```csharp
// AudioMixerGroup の音量を設定（0～100）
RWaveSoundManager.Instance.SetAudioMixerGroupVolume("AudioMixerGroupName", 80f);

// AudioSource の音量を設定（0～100）
RWaveSoundManager.Instance.SetAudioSourceVolume("AudioSourceName", 70f);

// AudioMixerGroupの現在の音量を取得（AudioMixerから実際の値を取得し、0～100に変換）
float volume = RWaveSoundManager.Instance.GetAudioMixerGroupVolume("AudioMixerGroupName");

// AudioSourceの現在の音量を取得
float volume = RWaveSoundManager.Instance.GetAudioSourceVolume("AudioSourceName");
```

**注意**: RWaveSettingまたはRWaveAudioMixerGroupSettingでExposed Parameter Nameが空白の場合、AudioMixerへの反映は無効化され、内部状態のみが管理されます。

## 高度な機能

**ヒント**: これらの機能の実装例は `Assets/RWave/Demo/Scripts/RWaveDemoUIController.cs` で確認できます。

### オーディオグループ管理

```csharp
private CancellationTokenSource _cts = new CancellationTokenSource();

// グループを指定してオーディオクリップを読み込む
await RWaveSoundManager.Instance.LoadAudioClip("audio_address", "AudioResourceGroupName", _cts.Token);

// グループ単位で開放
RWaveSoundManager.Instance.ReleaseAudioClipWithAudioGroup("AudioResourceGroupName");
```

### 音量データの保存と復元

```csharp
// 現在の音量設定をエクスポート
RWaveVolumeData volumeData = RWaveSoundManager.Instance.ExportVolumeData();

// 音量設定を復元
RWaveSoundManager.Instance.ApplyVolumeData(volumeData);
```

### AudioPack の使用

1. `Assets > Create > RWave > AudioPack` でオーディオパックを作成
2. オーディオクリップを登録
3. RWaveSetting の AudioPacks リストに追加
4. 初期化時に自動的に読み込まれ、AudioGroupを指定せずに再生可能

### AudioMixer 設定

RWaveはAudioMixerと連携して音量を管理します。

#### Exposed Parameter の設定

1. AudioMixerウィンドウで各AudioMixerGroupのVolumeパラメータを右クリック
2. "Expose 'Volume' to script" を選択
3. Exposed Parametersビューで名前を設定（例: "MasterVolume", "BGMVolume"）
4. RWaveSettingのInspectorで対応するExposed Parameter名を入力

#### 最大dB値の設定

各AudioMixerGroupの最大dB値を個別に設定できます（デフォルト: 0dB）。

- **RWaveSetting**: `masterMaxDB`（-80 ~ 20の範囲）
- **RWaveAudioMixerGroupSetting**: `maxDB`（-80 ~ 20の範囲）

音量値（0-100）は内部で **-80dB ~ maxDB** に線形変換されます。

```csharp
// 音量 0   → -80dB（ミュート）
// 音量 50  → -80dB + (maxDB - (-80)) × 0.5
// 音量 100 → maxDB
```

## API リファレンス

主要なクラスとメソッドの詳細については、コード内のドキュメントコメントを参照してください。

### 主要クラス

- `RWaveSoundManager`: オーディオシステム全体を統括するシングルトンマネージャー
- `RWaveAudioSource`: オーディオソースの操作を定義するインターフェース実装
- `RWaveAudioHandler`: 個別の音声再生を制御するハンドラー構造体
- `RWaveAudioResourceContainer`: オーディオリソースの読み込みと管理
- `RWaveAudioVolumeControlSystem`: 音量制御システム

## ライセンス

MIT License - 詳細は [LICENSE](LICENSE) を参照してください。

## 作者

- **Techno1109**
- GitHub: [https://github.com/Techno1109](https://github.com/Techno1109)
---

# RWave - Audio Management Framework

RWave is a comprehensive audio management framework for Unity that provides audio resource management using Unity Addressables, BGM/SE/Voice audio source management, audio volume control, fade functionality, and cross-fade capabilities.

## Features

- **Addressables Friendly**: Audio clip loading and management using Addressables
- **Audio Source Management**: Centralized management of multiple audio sources tailored to specific purposes such as BGM, SE, and Voice
- **AudioMixerGroup Friendly Volume Control System**: Fine-grained volume control at AudioMixerGroup and AudioSource levels
- **Fade Functionality**: Fade-in/fade-out with customizable duration
- **Cross-Fade**: Seamless audio transition capabilities
- **ScriptableObject-Based AudioClip Management**: AudioPack feature that supports environments where Addressables cannot be used or when you prefer not to use them
- **Volume Data Export/Import**: Save and restore volume settings

## Installation

### Via Unity Package Manager (UPM)

1. Open `Window > Package Manager` in Unity Editor
2. Click the `+` button
3. Select `Add package from git URL`
4. Enter the following URL:
   ```
   https://github.com/Techno1109/RWave.git?path=Assets/RWave
   ```

## Requirements

- **Unity**: 6000.2.13f1 or later
- **Dependencies**:
  - Unity Addressables (2.6.0)

## Quick Start

### Import Demo

Import the demo from Package Manager > RWave > Samples.

### Try the Demo Scene

1. Open `Assets/RWave/Demo/Scenes/RWaveDemo.unity`
2. Press the Play button

### Integration into Your Project

Simply place `Assets/RWave/Demo/Prefabs/RWave.prefab` in your scene and you're ready to use.

See the "Demo Scene" and "Basic Usage" sections below for details.

## Demo Scene

Import the demo from Package Manager > RWave > Samples.

### Included Files

- **RWaveDemo.unity**: Demo scene
- **RWave.prefab**: Pre-configured RWaveSoundManager (ready to use by placing in scene)
- **SampleSetting.asset**: Sample configuration file
- **SampleAudioMixer.mixer**: AudioMixer configuration example
- **RWaveDemoUIController.cs**: Demo UI implementation (reference for CancellationToken, Handler, and error handling)

### Using the Prefab

By placing `RWave.prefab` in your scene, you get the following pre-configured:
- RWaveSoundManager component
- Reference to SampleSetting.asset
- Auto Initialize On Awake enabled

Replace RWaveSetting with your own in the Inspector as needed.

## Basic Usage

**Note**: If you're new to RWave, we recommend reviewing the "Quick Start" and "Demo Scene" sections first. The demo scene contains all practical examples.

The following explains how to configure RWave from scratch.

### 1. Create Settings

1. Select `Assets > Create > RWave > RWaveSetting` in Unity Editor
2. Configure AudioMixer, AudioSource settings, and AudioMixerGroup settings

### 2. Place RWaveSoundManager

1. Create an empty GameObject in the scene
2. Attach the `RWaveSoundManager` component
3. Assign the created RWaveSetting

### 3. Load Audio Clips

```csharp
using UnityEngine;
using RWave.System;
using System.Threading;

public class AudioLoader : MonoBehaviour
{
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _cts = new CancellationTokenSource();
    }

    private async void Start()
    {
        // Load audio clip
        await RWaveSoundManager.Instance.LoadAudioClip("audio_address", _cts.Token);

        // Load by label
        await RWaveSoundManager.Instance.LoadAudioClipWithAddressableLabel("bgm_label", _cts.Token);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

#### Synchronous Loading

You can also load audio clips synchronously.

```csharp
// Synchronously load a single audio clip
bool success = RWaveSoundManager.Instance.SyncLoadAudioClip("audio_address");

// Synchronously load with ResourceGroup specification
bool success = RWaveSoundManager.Instance.SyncLoadAudioClip("audio_address", "AudioResourceGroupName");

// Synchronously load multiple audio clips
var addressList = new List<string> { "audio_address1", "audio_address2" };
bool success = RWaveSoundManager.Instance.SyncLoadAudioClip(addressList);
```

### 4. Play Audio

```csharp
// Play audio (without ResourceGroup specification)
RWaveSoundManager.Instance.Play("audio_address", "AudioSourceName");

// Play audio (with ResourceGroup specification)
RWaveSoundManager.Instance.Play("audio_address", "AudioSourceName", "AudioResourceGroupName");

// Play with specified volume
RWaveSoundManager.Instance.Play("audio_address", "AudioSourceName", playVolume: 50f);

// Stop (with fade-out)
RWaveSoundManager.Instance.Stop("AudioSourceName");

// Force stop immediately
RWaveSoundManager.Instance.ForceStop("AudioSourceName");
```

#### Audio Handler for Playback Control

The `Play()` method returns `RWaveAudioHandler`, which you can use to control individual playback instances.

```csharp
using RWave.Data;

// Get handler
RWaveAudioHandler handler = RWaveSoundManager.Instance.Play("audio_address", "AudioSourceName");

// Volume control via handler
handler.SetVolume(80f, fadeDuration: 2.0f);

// Check playback status
bool isPlaying = handler.IsPlaying();

// Stop using handler
RWaveSoundManager.Instance.Stop(handler, "AudioSourceName");
```

### 5. Volume Control

```csharp
// Set AudioMixerGroup volume (0-100)
RWaveSoundManager.Instance.SetAudioMixerGroupVolume("AudioMixerGroupName", 80f);

// Set AudioSource volume (0-100)
RWaveSoundManager.Instance.SetAudioSourceVolume("AudioSourceName", 70f);

// Get current AudioMixerGroup volume (retrieves actual value from AudioMixer and converts to 0-100)
float volume = RWaveSoundManager.Instance.GetAudioMixerGroupVolume("AudioMixerGroupName");

// Get current AudioSource volume
float volume = RWaveSoundManager.Instance.GetAudioSourceVolume("AudioSourceName");
```

**Note**: If the Exposed Parameter Name is left blank in RWaveSetting or RWaveAudioMixerGroupSetting, AudioMixer integration is disabled and only internal state is managed.

## Advanced Features

**Tip**: Implementation examples of these features can be found in `Assets/RWave/Demo/Scripts/RWaveDemoUIController.cs`.

### Audio Group Management

```csharp
private CancellationTokenSource _cts = new CancellationTokenSource();

// Load audio clip with group specification
await RWaveSoundManager.Instance.LoadAudioClip("audio_address", "AudioResourceGroupName", _cts.Token);

// Release by group
RWaveSoundManager.Instance.ReleaseAudioClipWithAudioGroup("AudioResourceGroupName");
```

### Save and Restore Volume Data

```csharp
// Export current volume settings
RWaveVolumeData volumeData = RWaveSoundManager.Instance.ExportVolumeData();

// Restore volume settings
RWaveSoundManager.Instance.ApplyVolumeData(volumeData);
```

### Using AudioPack

1. Create an audio pack with `Assets > Create > RWave > AudioPack`
2. Register audio clips
3. Add to the AudioPacks list in RWaveSetting
4. Audio clips are automatically loaded on initialization and can be played without specifying an AudioGroup

### AudioMixer Settings

RWave integrates with AudioMixer for volume management.

#### Setting Up Exposed Parameters

1. Right-click the Volume parameter of each AudioMixerGroup in the AudioMixer window
2. Select "Expose 'Volume' to script"
3. Set names in the Exposed Parameters view (e.g., "MasterVolume", "BGMVolume")
4. Enter the corresponding Exposed Parameter name in the RWaveSetting Inspector

#### Maximum dB Configuration

You can configure the maximum dB value for each AudioMixerGroup individually (default: 0dB).

- **RWaveSetting**: `masterMaxDB` (range: -80 to 20)
- **RWaveAudioMixerGroupSetting**: `maxDB` (range: -80 to 20)

Volume values (0-100) are internally converted linearly to **-80dB ~ maxDB**.

```csharp
// Volume 0   → -80dB (mute)
// Volume 50  → -80dB + (maxDB - (-80)) × 0.5
// Volume 100 → maxDB
```

## API Reference

For details on main classes and methods, refer to the documentation comments in the code.

### Main Classes

- `RWaveSoundManager`: Singleton manager coordinating all audio systems
- `RWaveAudioSource`: Interface implementation defining audio source operations
- `RWaveAudioHandler`: Handler structure for controlling individual audio playback
- `RWaveAudioResourceContainer`: Audio resource loading and management
- `RWaveAudioVolumeControlSystem`: Volume control system

## License

MIT License - See [LICENSE](LICENSE) for details.

## Author

- **Techno1109**
- GitHub: [https://github.com/Techno1109](https://github.com/Techno1109)