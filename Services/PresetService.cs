using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ShortcutRestore.Models;

namespace ShortcutRestore.Services
{
    public class PresetService
    {
        private readonly string _dataFolder;
        private readonly string _dataFile;
        private readonly JsonSerializerOptions _jsonOptions;

        public PresetService()
        {
            _dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ShortcutRestore");

            _dataFile = Path.Combine(_dataFolder, "presets.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            EnsureDataFolderExists();
        }

        private void EnsureDataFolderExists()
        {
            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }
        }

        public async Task<AppData> LoadAsync()
        {
            try
            {
                if (!File.Exists(_dataFile))
                {
                    return new AppData();
                }

                var json = await File.ReadAllTextAsync(_dataFile);
                return JsonSerializer.Deserialize<AppData>(json, _jsonOptions) ?? new AppData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading presets: {ex.Message}");
                return new AppData();
            }
        }

        public AppData Load()
        {
            try
            {
                if (!File.Exists(_dataFile))
                {
                    return new AppData();
                }

                var json = File.ReadAllText(_dataFile);
                return JsonSerializer.Deserialize<AppData>(json, _jsonOptions) ?? new AppData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading presets: {ex.Message}");
                return new AppData();
            }
        }

        public async Task SaveAsync(AppData data)
        {
            try
            {
                EnsureDataFolderExists();
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                await File.WriteAllTextAsync(_dataFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving presets: {ex.Message}");
                throw;
            }
        }

        public void Save(AppData data)
        {
            try
            {
                EnsureDataFolderExists();
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(_dataFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving presets: {ex.Message}");
                throw;
            }
        }

        public async Task AddPresetAsync(Preset preset, AppData data)
        {
            data.Presets.Add(preset);
            await SaveAsync(data);
        }

        public async Task UpdatePresetAsync(Preset preset, AppData data)
        {
            var index = data.Presets.FindIndex(p => p.Id == preset.Id);
            if (index >= 0)
            {
                data.Presets[index] = preset;
                await SaveAsync(data);
            }
        }

        public async Task DeletePresetAsync(Guid presetId, AppData data)
        {
            data.Presets.RemoveAll(p => p.Id == presetId);
            await SaveAsync(data);
        }
    }
}
