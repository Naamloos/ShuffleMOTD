# ShuffleMOTD
A simple Obsidian plugin that shuffles your MOTD

## Download
ShuffleMOTD gets built using GitHub actions by default. At this moment, the plugin is unsigned. This will change as the ability to set a signing key through environment variables gets introduced.

[Download the latest build here](https://github.com/ObsidianMC/ShuffleMOTD/releases)

## Config format
On first run, ShuffleMOTD will generate a new config file named `motd.json`. It looks like this:
```json
{
	"motds":
	[
		"Hi!",
		"Nice meme!",
	],
	
	"format" : "Sample format! {0} is the motd"
}
```
Use `Format` to set your MOTD format. Set it to `{0}` to shuffle the whole MOTD.
Use `MOTDs` to set your random MOTDs.
