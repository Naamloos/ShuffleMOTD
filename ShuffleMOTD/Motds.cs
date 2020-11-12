using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShuffleMOTD
{
    public class MotdCollection
    {
        [JsonProperty("MOTDs")]
        public string[] Motds = { "Hi!", "Nice meme!" };

        [JsonProperty("Format")]
        public string Format = "Sample format! {0} is the motd";
    }
}
