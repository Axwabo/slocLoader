

# slocLoader

A plugin that allows for the loading of objects contained within **sloc** files in SCP: Secret Laboratory.

![Logo](https://github.com/Axwabo/slocLoader/blob/main/logo%20small.png?raw=true)

[Watch the tutorial](https://youtu.be/0ssdcpPzO4U)

# MapEditorReborn

Before this project was started, I had no knowledge of the MapEditorReborn plugin.

This is not meant to be a clone of MER, but a different approach to be able to load Unity Scenes in SCP: Secret
Laboratory.

[Check out MapEditorReborn here](https://discord.gg/JwAfeSd79u)

# What is an sloc?

**sloc** stands for **S**ecret **L**aboratory **O**bject **C**ontainer.

An **sloc** file can contain primitive Unity objects (spheres, cubes etc.) and also lights.

Note: SCP:SL only supports spawning **point lights** remotely, therefore **it's not yet possible to create spot,
directional or area lights.**

To create an **sloc** file, use [slocExporter](https://github.com/Axwabo/slocExporter/)

**_sloc files are not text_**, but raw bytes. This means that opening one in a text editor will produce gibberish, since
it's not meant to be interpreted as text. It is a sequence of **int**egers and **float**s.

# Setup

[Axwabo.Helpers](https://github.com/Axwabo/SCPSL-Helpers/) **is required** as a dependency.

## For EXILED

**slocLoader is not available for EXILED 6.0.0 yet.**

## For Northwood Plugin API

1. Download the **slocLoader-nw.dll** file from the [latest release](https://github.com/Axwabo/slocLoader/releases/latest)
2. Place the DLL into the **NW Plugins** folder (Windows: %appdata%\SCP Secret Laboratory\PluginAPI\plugins\port)
3. Download the latest [Harmony](https://github.com/pardeike/Harmony/releases) release (**Harmony.x.x.x.x.zip**, not the source code)
4. From the zip file, put the **net4.8\0Harmony.dll** into the **NW Dependencies** folder (Windows: %appdata%\SCP Secret Laboratory\PluginAPI\plugins\port\dependencies)
5. Restart your server

# Adding objects

To load objects automatically, put your **sloc** file into **%appdata%\EXILED\Plugins\sloc\Objects**

Make sure the **auto_load** property in the plugin config is set to true.

The objects in the folder above will be loaded automatically when the map is generated.

Use the **sl_spawn** command and the name of the object file (without the extension) in-game to spawn an object.
Example: `sl_spawn test`

To remove previously spawned objects, use the **sl_destroy** command. It takes the netID of the object, which the spawn
command outputs.

# Automatic spawning

Make sure the **enable_auto_spawn** property in the plugin config is set to true.

View the list of room names and types [here](https://github.com/Axwabo/SCPSL-Helpers/blob/main/Axwabo.Helpers.NWAPI/Config/RoomType.cs)

In the plugin config, add an item to the **auto_spawn_by_room_name**, **auto_spawn_by_room_type** or **auto_spawn_by_location** list. There are examples provider for ease of use.

Room name template:

```
asset_name: example
point:
  room_name: HCZ_106
  position_offset:
    x: 0
    y: 0
    z: 0
  rotation_offset:
    x: 0
    y: 0
    z: 0
```

RoomType template:

```
asset_name: example
point:
  type: LczClassDSpawn
  position_offset:
    x: 0
    y: 0
    z: 0
  rotation_offset:
    x: 0
    y: 0
    z: 0
```

StaticPosition template:

```
asset_name: example
point:
  position:
    x: 10
    y: 0
    z: 50
  rotation:
    x: 0
    y: 90
    z: 0
```

All assets that have been loaded by the AutomaticObjectLoader will be spawned in the room specified.

If an asset was not loaded, it will be ignored.

Rotation offsets are specified using **Euler angles, _not Quaternions_.**

# API usage

The plugin offers various methods to make loading objects easy.

Add the DLL as an assembly reference to get started.

You can spawn objects directly from a file, or
from [within your assembly](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly.getmanifestresourcestream)

Use the methods in the **slocLoader.API** class to load objects.

Make sure to do this **after the prefabs are loaded**; register a handler to the **PrefabsLoaded** event contained in
the API class.

# Versioning

Each exported **sloc** file has a version number.

Even if a new version of **sloc** is released, old **sloc** files will still be compatible with the current version of
the plugin due to the versioning system.

All versions have their own object readers to read the objects from the **sloc** file. This way, no **sloc** file will
be broken after a new version is released.