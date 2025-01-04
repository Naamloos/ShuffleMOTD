using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Obsidian.API;
using Obsidian.API.Events;
using Obsidian.API.Plugins;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ShuffleMOTD
{
    public sealed class ShuffleMOTDPlugin : PluginBase
    {
        private const string MOTD_FILE = "motd.json";
        private const string PLEASE_EDIT = "Please edit motd.json.";
        private const string CREATED_NEW_MOTD = "Created new motd.json. Edit this file to set your MOTD. These changes are adapted in real-time.";
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };
        private FileSystemWatcher _fileWatcher;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        [Inject]
        public ILogger<ShuffleMOTDPlugin> Logger { get; set; }

        public MotdCollection LoadedMotdCollection { get; private set; }

        public override async ValueTask OnLoadedAsync(IServer server)
        {
            await using var file = File.Open(MOTD_FILE, FileMode.OpenOrCreate);

            if (file.Length == 0)
            {
                await SerializeMotdAsync(new MotdCollection(), file);
                Logger.LogInformation(CREATED_NEW_MOTD);
            }

            file.Position = 0;
            LoadedMotdCollection = await JsonSerializer.DeserializeAsync<MotdCollection>(file) ?? new MotdCollection();
            file.Position = 0;
            await SerializeMotdAsync(LoadedMotdCollection, file);

            InitializeFileWatcher();
        }

        private void InitializeFileWatcher()
        {
            _fileWatcher = new FileSystemWatcher
            {
                Path = AppDomain.CurrentDomain.BaseDirectory,
                Filter = MOTD_FILE,
                NotifyFilter = NotifyFilters.LastWrite
            };

            _fileWatcher.Changed += async (sender, e) => await OnMotdFileChangedAsync();
            _fileWatcher.EnableRaisingEvents = true;
        }

        private async Task OnMotdFileChangedAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                await using var file = File.Open(MOTD_FILE, FileMode.Open);
                LoadedMotdCollection = await JsonSerializer.DeserializeAsync<MotdCollection>(file) ?? LoadedMotdCollection;
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "Failed to deserialize motd.json. Ignoring changes.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static async ValueTask SerializeMotdAsync(MotdCollection motdCollection, FileStream file)
        {
            file.Position = 0;
            await JsonSerializer.SerializeAsync(file, motdCollection, _jsonOptions);
            await file.FlushAsync();
        }

        public override void ConfigureRegistry(IPluginRegistry pluginRegistry)
        {
            pluginRegistry.MapEvent(OnServerStatusRequest, Priority.Critical);
        }

        private void OnServerStatusRequest(ServerStatusRequestEventArgs args)
        {
            if (LoadedMotdCollection.Motds.Length == 0)
            {
                args.Status.Description.Text = PLEASE_EDIT;
                return;
            }

            var selection = LoadedMotdCollection.Motds[Random.Shared.Next(LoadedMotdCollection.Motds.Length)];
            var motd = string.IsNullOrWhiteSpace(LoadedMotdCollection.Format) ? selection : string.Format(LoadedMotdCollection.Format, selection);
            args.Status.Description.Text = motd;
        }
    }

    public class MotdCollection
    {
        [JsonPropertyName("motds")]
        public string[] Motds { get; set; } = Array.Empty<string>();

        [JsonPropertyName("format")]
        public string Format { get; set; } = "{0}";
    }
}
