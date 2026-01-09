using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShortcutRestore.Models;
using ShortcutRestore.Services;

namespace ShortcutRestore.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DesktopIconService _desktopIconService;
        private readonly PresetService _presetService;
        private readonly SystemUtilitiesService _systemUtilitiesService;
        private AppData _appData;

        [ObservableProperty]
        private ObservableCollection<PresetViewModel> _presets = new();

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isStatusSuccess = true;

        [ObservableProperty]
        private bool _showStatus = false;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _newPresetName = string.Empty;

        [ObservableProperty]
        private bool _showNewPresetDialog = false;

        [ObservableProperty]
        private int _scannedIconCount = 0;

        private System.Collections.Generic.List<IconPosition>? _scannedPositions;

        public MainViewModel()
        {
            _desktopIconService = new DesktopIconService();
            _presetService = new PresetService();
            _systemUtilitiesService = new SystemUtilitiesService();
            _appData = new AppData();

            LoadPresets();
        }

        private void LoadPresets()
        {
            _appData = _presetService.Load();
            Presets.Clear();
            foreach (var preset in _appData.Presets)
            {
                Presets.Add(new PresetViewModel(preset, this));
            }
        }

        [RelayCommand]
        private void ScanCurrentLayout()
        {
            try
            {
                IsLoading = true;
                _scannedPositions = _desktopIconService.GetAllIconPositions();
                ScannedIconCount = _scannedPositions.Count;

                if (_scannedPositions.Count > 0)
                {
                    ShowNewPresetDialog = true;
                    NewPresetName = $"Layout {DateTime.Now:yyyy-MM-dd HH:mm}";
                    ShowStatusMessage($"Scanned {_scannedPositions.Count} icons", true);
                }
                else
                {
                    ShowStatusMessage("No desktop icons found", false);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error scanning: {ex.Message}", false);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveNewPresetAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPresetName) || _scannedPositions == null)
                return;

            try
            {
                IsLoading = true;
                var preset = new Preset(NewPresetName.Trim(), _scannedPositions);
                await _presetService.AddPresetAsync(preset, _appData);

                Presets.Add(new PresetViewModel(preset, this));
                ShowNewPresetDialog = false;
                NewPresetName = string.Empty;
                _scannedPositions = null;
                ScannedIconCount = 0;

                ShowStatusMessage($"Preset '{preset.Name}' saved!", true);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error saving: {ex.Message}", false);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void CancelNewPreset()
        {
            ShowNewPresetDialog = false;
            NewPresetName = string.Empty;
            _scannedPositions = null;
            ScannedIconCount = 0;
        }

        public async Task RestorePresetAsync(PresetViewModel presetVm)
        {
            try
            {
                IsLoading = true;
                var success = _desktopIconService.RestoreIconPositions(presetVm.Preset.Icons);

                if (success)
                {
                    presetVm.Preset.LastUsed = DateTime.Now;
                    await _presetService.UpdatePresetAsync(presetVm.Preset, _appData);
                    ShowStatusMessage($"Restored '{presetVm.Name}'!", true);
                }
                else
                {
                    ShowStatusMessage("Failed to restore positions", false);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error restoring: {ex.Message}", false);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DeletePresetAsync(PresetViewModel presetVm)
        {
            try
            {
                await _presetService.DeletePresetAsync(presetVm.Preset.Id, _appData);
                Presets.Remove(presetVm);
                ShowStatusMessage($"Deleted '{presetVm.Name}'", true);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error deleting: {ex.Message}", false);
            }
        }

        public async Task RenamePresetAsync(PresetViewModel presetVm, string newName)
        {
            try
            {
                presetVm.Preset.Name = newName;
                await _presetService.UpdatePresetAsync(presetVm.Preset, _appData);
                ShowStatusMessage($"Renamed to '{newName}'", true);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error renaming: {ex.Message}", false);
            }
        }

        [RelayCommand]
        private async Task ClearDnsCacheAsync()
        {
            try
            {
                IsLoading = true;
                var (success, message) = await _systemUtilitiesService.ClearDnsCacheAsync();
                ShowStatusMessage(message, success);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task EmptyRecycleBinAsync()
        {
            try
            {
                IsLoading = true;
                var (success, message) = await _systemUtilitiesService.EmptyRecycleBinAsync();
                ShowStatusMessage(message, success);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ClearRecentFilesAsync()
        {
            try
            {
                IsLoading = true;
                var (success, message) = await _systemUtilitiesService.ClearRecentFilesAsync();
                ShowStatusMessage(message, success);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ShowStatusMessage(string message, bool isSuccess)
        {
            StatusMessage = message;
            IsStatusSuccess = isSuccess;
            ShowStatus = true;

            await Task.Delay(3000);
            ShowStatus = false;
        }

        [RelayCommand]
        private void MinimizeWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }
        }

        [RelayCommand]
        private void CloseWindow()
        {
            Application.Current.Shutdown();
        }
    }
}
