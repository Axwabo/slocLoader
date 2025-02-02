global using Axwabo.Helpers;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.IO;
global using UnityEngine;
global using Object = UnityEngine.Object;
#if NWAPI
global using Logger = PluginAPI.Core.Log;
global using Player = PluginAPI.Core.Player;
#else
global using Logger = LabApi.Features.Console.Logger;
global using Player = LabApi.Features.Wrappers.Player;
#endif
