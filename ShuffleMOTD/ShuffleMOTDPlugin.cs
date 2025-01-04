using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Obsidian.API;
using Obsidian.API.Events;
using Obsidian.API.Plugins;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShuffleMOTD
{
    public sealed class ShuffleMOTDPlugin : PluginBase
    {
        private const string MOTD_FILE = "motd.json";

        [Inject] 
        public ILogger<ShuffleMOTDPlugin> Logger { get; set; }

        public MotdCollection LoadedMotdCollection;

        public override async ValueTask OnLoadedAsync(IServer server)
        {
            var file = File.Open(MOTD_FILE, FileMode.OpenOrCreate);

            if (file.Length < 1)
            {
                await JsonSerializer.SerializeAsync(file, new MotdCollection(), options: new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    IndentSize = 4
                });
                await file.FlushAsync();
                file.Position = 0;
                Logger.LogInformation($"Created new motd.json. Please edit this file and restart the server.");
            }

            LoadedMotdCollection = await JsonSerializer.DeserializeAsync<MotdCollection>(file);
            file.Position = 0;
            await JsonSerializer.SerializeAsync(file, LoadedMotdCollection, options: new JsonSerializerOptions()
            {
                WriteIndented = true,
                IndentSize = 4
            });

            await file.DisposeAsync();
        }

        public override void ConfigureRegistry(IPluginRegistry pluginRegistry)
        {
            pluginRegistry.MapEvent(OnServerStatusRequest, Priority.Critical);
        }

        public void OnServerStatusRequest(ServerStatusRequestEventArgs args)
        {
            if (LoadedMotdCollection.Motds.Length < 1)
                return;

            if (LoadedMotdCollection.Motds[0] == "EDIT ME")
            {
                args.Status.Description.Text = "Please edit motd.json and restart the server.";
                return;
            }

            var selection = LoadedMotdCollection.Motds[Random.Shared.Next(0, LoadedMotdCollection.Motds.Length)];
            var motd = string.IsNullOrWhiteSpace(LoadedMotdCollection.Format)? selection : string.Format(LoadedMotdCollection.Format, selection);
            args.Status.Description.Text = motd;
        }
    }

    public class MotdCollection
    {
        [JsonPropertyName("motds")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string[] Motds { get; set; } = [ "EDIT ME", "SECOND MOTD", "THIRD MOTD... etc etc" ];

        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Format { get; set; } = "{0} is the current MOTD.";
    }
}
