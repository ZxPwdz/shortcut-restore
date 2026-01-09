using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShortcutRestore.Models;

namespace ShortcutRestore.ViewModels
{
    public partial class PresetViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        public Preset Preset { get; }

        [ObservableProperty]
        private bool _isEditing = false;

        [ObservableProperty]
        private string _editName = string.Empty;

        public string Name => Preset.Name;
        public int IconCount => Preset.Icons.Count;
        public DateTime CreatedAt => Preset.CreatedAt;
        public string CreatedAtFormatted => Preset.CreatedAt.ToString("MMM d, yyyy h:mm tt");
        public string LastUsedFormatted => Preset.LastUsed?.ToString("MMM d, yyyy h:mm tt") ?? "Never";

        public PresetViewModel(Preset preset, MainViewModel mainViewModel)
        {
            Preset = preset;
            _mainViewModel = mainViewModel;
            _editName = preset.Name;
        }

        [RelayCommand]
        private Task RestoreAsync()
        {
            return _mainViewModel.RestorePresetAsync(this);
        }

        [RelayCommand]
        private Task DeleteAsync()
        {
            return _mainViewModel.DeletePresetAsync(this);
        }

        [RelayCommand]
        private void StartEdit()
        {
            EditName = Preset.Name;
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveEditAsync()
        {
            if (!string.IsNullOrWhiteSpace(EditName) && EditName != Preset.Name)
            {
                await _mainViewModel.RenamePresetAsync(this, EditName.Trim());
                OnPropertyChanged(nameof(Name));
            }
            IsEditing = false;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            EditName = Preset.Name;
            IsEditing = false;
        }
    }
}
