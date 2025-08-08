# slocLoader

A plugin that allows for the loading of objects contained within **sloc** files in SCP: Secret Laboratory.

![Logo](https://github.com/Axwabo/slocLoader/blob/main/logo%20small.png?raw=true)

[Watch the tutorial](https://youtu.be/wPOQStkr4Zk)

[Set up the plugin](#setup)

# MapEditorReborn

Before this project was started, I had no knowledge of the MapEditorReborn plugin.

This is not meant to be a clone of MER, but a different approach to be able to load Unity Scenes in SCP: Secret Laboratory.

[Check out MapEditorReborn here](https://discord.gg/JwAfeSd79u)

# What is an sloc?

**sloc** stands for **S**ecret **L**aboratory **O**bject **C**ontainer.

An **sloc** file can contain primitive Unity objects (spheres, cubes etc.) and also lights.

To create an **sloc** file, use [slocExporter](https://github.com/Axwabo/slocExporter/)

**_sloc files are not text_**, but raw bytes. This means that opening one in a text editor will produce gibberish, since
it's not meant to be interpreted as text. It is a sequence of **int**egers and **float**s.

**sloc** aims to retain the object hierarchy exactly as is in the Unity editor, meaning
you can use GetComponent(s)InChildren on the server to find child objects.
# Setup

> [!IMPORTANT]
> [Axwabo.Helpers](https://github.com/Axwabo/SCPSL-Helpers/) **is required** as a dependency.

1. Install [Axwabo.Helpers](https://github.com/Axwabo/SCPSL-Helpers/)
2. Install [Harmony 2.2.2](https://github.com/pardeike/Harmony/releases/tag/v2.2.2.0) as a dependency, you need the **net4.8** version
    1. Download the `Harmony.2.2.2.0.zip` asset
    2. Extract the `net48/0Harmony.dll` file from the archive to the `dependencies` folder:
        - Windows: `%appdata%\SCP Secret Laboratory\LabAPI-Beta\dependencies\<port>`
        - Linux: `.config/SCP Secret Laboratory/LabAPI-Beta/dependencies/<port>`
3. Download the `slocLoader.dll` file from the [latest release](https://github.com/Axwabo/slocLoader/releases/)
4. Place the DLL into the `plugins` folder:
    - Windows: `%appdata%\SCP Secret Laboratory\LabAPI\plugins\<port>`
    - Linux: `.config/SCP Secret Laboratory/LabAPI/plugins/<port>`
5. Restart your server

# Adding objects

To load objects automatically, put your **sloc** file into:

- Windows: `%appdata%\SCP Secret Laboratory\LabAPI\configs\<port>\slocLoader\Objects`
- Linux: `.config/SCP Secret Laboratory/LabAPI/configs/<port>/slocLoader/Objects`

Make sure the **auto_load** property in the plugin config is set to true.

The objects in the folder above will be loaded automatically when the map is generated.

Use the **sl_spawn** command and the name of the object file (without the extension) in-game to spawn an object.
Example: `sl_spawn test`

To remove previously spawned objects, use the **sl_destroy** command. It takes the netID of the object, which the spawn
command outputs.

# Automatic spawning

Make sure the **enable_auto_spawn** property in the plugin config is set to true.

View the list of room names and types [here](https://github.com/Axwabo/SCPSL-Helpers/blob/main/Axwabo.Helpers/Config/RoomType.cs)

In the plugin config, add an item to the `auto_spawn_by_room_name`, `auto_spawn_by_room_type` or `auto_spawn_by_location` list. There are examples provider for ease of use.

Room name template:

```yaml
asset_name: example
point:
  room_name: HCZ_079
  position_offset:
    x: 0
    y: 0
    z: 0
  rotation_offset:
    x: 0
    y: 0
    z: 0
make_objects_static: false
```

RoomType template:

```yaml
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
make_objects_static: true
```

StaticPosition template:

```yaml
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
make_objects_static: false
```

All assets that have been loaded by the AutomaticObjectLoader will be spawned in the room specified.

If an asset was not loaded, it will be ignored.

Rotation offsets are specified using **Euler angles, _not Quaternions_.**

The `make_objects_static` property defaults to false. Setting it to `true` will boost server performance as transform updates will not be sent to the client (can cause desync if plugins move the objects).

# Spawning objects in-game

The `sl_spawn` command can be used to spawn assets through the Remote Admin. Required permission is `FacilityManagement`

Example: `sl_s MyObject`

The asset has to be defined as in [automatic spawning](#automatic-spawning)

Your position and horizontal rotation will be applied to the objects by default. To modify this behavior, use the `sl_position` and `sl_rotation` commands.

For example, `sl_pos 0 1000 0` will spawn the specified object at the coordinates **0, 1000, 0** (x, y, z)

To clear the current settings, simply type `sl_pos` or `sl_rot` to remove the defined values.

# API usage

The plugin offers various methods to make loading objects easy.

Add the DLL as an assembly reference to get started.

You can spawn objects directly from a file, or
from [within your assembly](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly.getmanifestresourcestream)

Use the methods in the **slocLoader.API** class to load objects.

Make sure to do this **after the prefabs are loaded**; register a handler to the **PrefabsLoaded** event contained in
the API class.

```csharp
using AdminToys;
using LabApi.Loader;
using slocLoader;
using slocLoader.AutoObjectLoader;
using slocLoader.ObjectCreation;

var config = MyPlugin.Instance.GetConfigDirectory().FullName;

var test = API.SpawnObjects(
    Path.Combine(config, "test.sloc"),
    Vector3.up * 300,
    Quaternion.Euler(0, 180, 0)
);
// make primitives slightly transparent
foreach (var primitive in test.GetComponentsInChildren<PrimitiveObjectToy>())
    primitive.NetworkMaterialColor = primitive.NetworkMaterialColor with {a = 0.8f};

if (AutomaticObjectLoader.TryGetObjects("test2", out var objects))
    API.SpawnObjects(objects, new CreateOptions
    {
        Position = new Vector3(-5, 300, 0),
        StaticRoot = true, // root object does not move
        IsStatic = true, // children don't move
        Scale = Vector3.one * 0.5f
    });

// ConstantMove component that moves the root object every frame
API.SpawnObjects(Path.Combine(config, "moving.sloc"), new CreateOptions
{
    IsStatic = true, // children don't move, only the root does
    RootSmoothing = 20
}).AddComponent<ConstantMove>();
```

Example of spawning a singular object:

```csharp
using UnityEngine;
using slocLoader;
using slocLoader.Objects;

new PrimitiveObject(ObjecType.Capsule)  // non-primitive type will throw an exception
{
    Transform = 
    {
        Position = new Vector3(0, 50, 0),
        Rotation = Quaternion.Euler(0, 50, 0),
        Scale = new Vector3(2, 1, 2)
    },
    Flags = PrimitiveObjectFlags.ServerCollider | PrimitiveObjectFlags.Trigger,
    MaterialColor = Color.red
}.SpawnObject();
```

# Versioning

Each exported **sloc** file has a version number.

Even if a new version of **sloc** is released, old **sloc** files will still be compatible with the current version of the plugin due to the versioning system.

All versions have their own object readers to read the objects from the **sloc** file. This way, no **sloc** file will be broken after a new version is released.
