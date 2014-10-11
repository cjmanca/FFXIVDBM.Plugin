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
There is some rudimentary support for T9 as well, however since my static hasn't passed golem phase yet, it probably won't work very well after that until someone else steps up to try finishing that encounter (or we're able to get further).
If you have any programming knowledge at all (even basic), I'd encourage you to look at the existing scripts, and use it as a basis for creating new encounter scripts for other encounters. You can submit new encounter scripts on github, here:
https://github.com/cjmanca/FFXIVDBM.Plugin

Currently it's English only, but preliminary work has been done to support other languages. I can't test them however, so I'll need users of other languages to convert the encounter scripts to those languages.

Install info:
1. Open FFXIV-APP
2. Click "Update" along the left
3. Click "Plugin Source" at the top
4. Paste the following URL into the "Source" field:
https://github.com/cjmanca/FFXIVDBM.Plugin/raw/master/distribution/VERSION.json
5. Click "Add or update source" at the bottom
6. Click "Available Plugins" at the top
7. Click "Refresh Plugins" at the bottom
8. Select "FFXIVDBM.Plugin" in the list
9. Click "Install" at the bottom
10. After the install is finished, restart FFXIV-APP
