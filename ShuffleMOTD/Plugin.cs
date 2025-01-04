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
    public sealed class Plugin : PluginBase
    {
        [Inject] 
        public ILogger Logger { get; set; }

        public MotdCollection motds;

        public override async ValueTask OnLoadedAsync(IServer server)
        {
            var file = File.Open("motd.json", FileMode.OpenOrCreate);

            if (file.Length < 1)
            {
                await JsonSerializer.SerializeAsync(file, new MotdCollection());
                await file.FlushAsync();
                file.Position = 0;
                Logger.LogInformation($"Created new motd.json. Please edit this file and restart the server.");
            }

            motds = await JsonSerializer.DeserializeAsync<MotdCollection>(file);
        }

        public override void ConfigureRegistry(IPluginRegistry pluginRegistry)
        {
            pluginRegistry.MapEvent(OnServerStatusRequest, Priority.Critical);
        }

        public void OnServerStatusRequest(ServerStatusRequestEventArgs args)
        {
            if (motds.Motds.Length < 1)
                return;

            var motd = string.Format(motds.Format, motds.Motds[Random.Shared.Next(0, motds.Motds.Length)]);
            args.Status.Description.Text = motd;
        }
    }

    public class MotdCollection
    {
        [JsonPropertyName("motds")]
        public string[] Motds = [];

        [JsonPropertyName("format")]
        public string Format = "{0} is the current MOTD.";
    }
}
