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

        // One of server messages, called when an event occurs
        public async Task OnLoad(IServer server)
        {
            Logger.Log($"Loaded {Info.Name}.");
            if (!FileWriter.FileExists("motds.txt"))
            {
                FileWriter.CreateFile("motds.txt");
                FileWriter.WriteAllText("motds.txt", "ShuffleMOTD 1\nShuffleMOTD 2");
            }
            Logger.Log($"Created new motds.txt");
            await Task.CompletedTask;
        }

        public async Task OnServerStatusRequest(ServerStatusRequestEventArgs args)
        {
            var motds = FileReader.ReadAllLines("motds.txt");

            args.Status.Description.Text = motds[new Random().Next(0, motds.Length)].Replace("\\n", "\n");
            await Task.CompletedTask;
        }
    }
}
