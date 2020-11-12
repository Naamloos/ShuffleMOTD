using Newtonsoft.Json;
using Obsidian.API;
using Obsidian.API.Events;
using Obsidian.API.Plugins;
using Obsidian.API.Plugins.Services;
using Obsidian.CommandFramework.Attributes;
using Obsidian.CommandFramework.Entities;
using System;
using System.Threading.Tasks;

namespace ShuffleMOTD
{
    [Plugin(Name = "ShuffleMOTD", Version = "1.0",
            Authors = "Naamloos", Description = "Shuffles the MOTD",
            ProjectUrl = "https://github.com/Naamloos/ShuffleMOTD")]
    public class Plugin : PluginBase
    {
        // Any interface from Obsidian.Plugins.Services can be injected into properties
        [Inject] public ILogger Logger { get; set; }
        [Inject] public IFileReader FileReader { get; set; }
        [Inject] public IFileWriter FileWriter { get; set; }

        MotdCollection motds;

        // One of server messages, called when an event occurs
        public async Task OnLoad(IServer server)
        {
            Logger.Log($"Loaded {Info.Name}.");

            if (!FileWriter.FileExists("motd.json"))
            {
                FileWriter.CreateFile("motd.json");

                await FileWriter.WriteAllTextAsync("motd.json", JsonConvert.SerializeObject(new MotdCollection()));
                Logger.Log($"Created new motds.json. Please edit this file and restart the server.");
            }

            motds = JsonConvert.DeserializeObject<MotdCollection>(FileReader.ReadAllText("motd.json"));

            await Task.CompletedTask;
        }

        public async Task OnServerStatusRequest(ServerStatusRequestEventArgs args)
        {
            if (motds.Motds.Length < 1)
                return;

            var motd = string.Format(motds.Format, motds.Motds[new Random().Next(0, motds.Motds.Length)]);

            args.Status.Description.Text = motd;
            await Task.CompletedTask;
        }
    }
}
