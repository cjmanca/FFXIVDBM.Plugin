FFXIVDBM.Plugin
===============

DBM stands for Deadly Boss Mods, and is similar in nature to it's WOW originator (although currently all warnings are via Text-To-Speech - no visual warnings yet).

I've been working on this for a couple of months. I don't really think it's ready for release yet, but I haven't had the time to do much on it for a month or so now. I'm going to release as-is to the community, so others can help finish it.

This plugin will help provide warnings for different encounters, with customizable encounter scripts. Eventually, the goal is to have every encounter scripted out, allowing the average user to just install the plugin and not have to worry about anything.

Currently, the following encounters are scripted and working 99.9%:
* Titan Hard Mode (was my test encounter when first coding the plugin)
* T6
* T7
* T8
* T9
* T10
* T11
* T12

If you have any programming knowledge at all (even basic), I'd encourage you to look at the existing scripts, and use it as a basis for creating new encounter scripts for other encounters. You can submit new encounter scripts on github, here:

https://github.com/cjmanca/FFXIVDBM.Plugin

Currently it's English only, but preliminary work has been done to support other languages. I can't test them however, so I'll need users of other languages to convert the encounter scripts to those languages, or provide me with a transcript of the clear for the encounter. You can find these encounter transcripts in:
FFXIV-APP Directory/Plugins/FFXIVDBM.Plugin/zones/(Language)/(zone ID)/Auto Helper/(Bossname).cs

Now included in the default plugin list in FFXIVAPP!
You can find it as "FFXIVDBM.Plugin" in the "Update" -> "Available Plugins" tab of FFXIVAPP

Manual Install info (shouldn't need to do it this way anymore):
* Open FFXIV-APP
* Click "Update" along the left
* Click "Plugin Source" at the top
* Paste the following URL into the "Source" field:

https://github.com/cjmanca/FFXIVDBM.Plugin/raw/master/distribution/VERSION.json
* Click "Add or update source" at the bottom
* Click "Available Plugins" at the top
* Click "Refresh Plugins" at the bottom
* Select "FFXIVDBM.Plugin" in the list
* Click "Install" at the bottom
* After the install is finished, restart FFXIV-APP
