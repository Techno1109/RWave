# Changelog

## [0.1.3] - 2026-11-20

### 新規
- 再生・停止時のフェード時間を呼び出しごとに指定できるようにした（`Play(..., fadeInSeconds)` / `Stop(..., fadeOutSeconds)`）。指定しない場合は従来どおり設定のAttack / Releaseを使用
- AudioMixerGroup単位の一時的な音量倍率を追加（`SetAudioMixerGroupVolumeScale` / `GetAudioMixerGroupVolumeScale` / `ResetAllAudioMixerGroupVolumeScales`）。設定した音量とは別に保持され、倍率を1に戻せば元の音量に戻る

### 変更
- 音量（0～100）からdBへの変換を線形から対数スケールに変更（50%が約-6dB）。同じ音量値でも以前より大きく聞こえる
- `GetAudioMixerGroupVolume` はAudioMixerから逆算せず、設定した音量をそのまま返すように変更（一時的な音量倍率は含まない）
- `IRWaveAudioSource` にフェード時間指定の `Play` / `Stop` を追加。独自に実装している場合は実装の追加が必要

### 修正
- フェードアウト中のAudioSourceを次の再生が使用したとき、残っていたフェードが新しい音を停止させてしまう問題を修正
- CrossFadeのAttackが0のとき、前の音が停止されない問題を修正

### Added
- Fade time can now be specified per call for playback and stop (`Play(..., fadeInSeconds)` / `Stop(..., fadeOutSeconds)`). When omitted, the Attack / Release settings are used as before
- Added a temporary volume scale per AudioMixerGroup (`SetAudioMixerGroupVolumeScale` / `GetAudioMixerGroupVolumeScale` / `ResetAllAudioMixerGroupVolumeScales`). It is kept separately from the configured volume, so setting the scale back to 1 restores the original volume

### Changed
- Volume (0-100) to dB conversion changed from linear to logarithmic scale (50% is about -6dB). The same volume value now sounds louder than before
- `GetAudioMixerGroupVolume` now returns the configured volume as is instead of calculating it back from the AudioMixer (the temporary volume scale is not included)
- Added `Play` / `Stop` overloads with fade time to `IRWaveAudioSource`. Custom implementations need to implement them

### Fixed
- Fixed an issue where a leftover fade stopped the new audio when the next playback reused an AudioSource that was still fading out
- Fixed an issue where the previous audio was not stopped when the CrossFade Attack was 0

## [0.1.2] - 2026-03-16

### 新規
- 再生開始時のDSPTimeと経過DSP時間を取得できるようにした（`GetDspTimeProvider` / `IRWaveAudioDspTimeProvider`）

### Added
- Playback start DSP time and elapsed DSP time can now be retrieved (`GetDspTimeProvider` / `IRWaveAudioDspTimeProvider`)

## [0.1.1] - 2026-01-28

### 修正
- Initialize時、AudioMixerに音量設定が反映されていなかった問題を修正

### Fixed
- Fixed an issue where volume settings were not applied to the AudioMixer on Initialize

## [0.1.0] - 2025-01-25

### 新規
- 初回リリース

### Added
- Initial release
