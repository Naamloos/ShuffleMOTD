# ShuffleMOTD
A simple Obsidian plugin that shuffles your MOTD

## Config format
On first run, ShuffleMOTD will generate a new config file named `motd.json`. It looks like this:
```json
{
	"MOTDs":
	[
		"Hi!",
		"Nice meme!",
	],
	
	"Format" : "Sample format! {0} is the motd"
}
```
Use `Format` to set your MOTD format. Set it to `{0}` to shuffle the whole MOTD.

Use `MOTDs` to set your random MOTDs.