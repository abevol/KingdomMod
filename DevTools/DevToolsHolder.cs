using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using File = System.IO.File;
using HarmonyLib;
using Coatsink.Common;
using System.Reflection;
using KingdomMod.SharedLib;
using KingdomMod.Shared.Attributes;

#if IL2CPP
using Il2CppInterop.Runtime.Injection;
#endif

namespace KingdomMod.DevTools;

[RegisterTypeInIl2Cpp]
public class DevToolsHolder : MonoBehaviour
{
    public static DevToolsHolder Instance { get; private set; }
    private static ManualLogSource log;
    private bool enabledDebugInfo = false;
    private bool enabledObjectsInfo = false;
    private bool enableDevTools = false;
    private readonly GUIStyle guiStyle = new();
    private int tick = 0;
    private readonly List<ObjectsInfo> objectsInfoList = new();
    private static Texture2D ColoredTexture2D = null;
    private static readonly GUIStyle boxGuiStyle = new();

#if IL2CPP
    public DevToolsHolder(IntPtr ptr) : base(ptr) { }
#endif

    public static void Initialize(DevToolsPlugin plugin)
    {
        log = plugin.LogSource;
        GameObject obj = new(nameof(DevToolsHolder));
        DontDestroyOnLoad(obj);
        obj.hideFlags = HideFlags.HideAndDontSave;
        Instance = obj.AddComponent<DevToolsHolder>();
    }

    public DevToolsHolder()
    {
        Patcher.PatchAll();
        log.LogMessage($"isDebugBuild: {Debug.isDebugBuild}");
    }

    private void Start()
    {
        log.LogMessage($"{this.GetType().Name} Start.");

    }

    public class Patcher
    {
        public static void PatchAll()
        {
            try
            {
                var harmony = new Harmony("KingdomMod.DevTools.Patcher");
                harmony.PatchAll();
                foreach (var patchedMethod in harmony.GetPatchedMethods())
                {
                    log.LogMessage($"patchedMethod: {patchedMethod.Name}");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"[Patcher] => {ex}");
            }
        }

#if DEBUG
            [HarmonyPatch(typeof(UnityEngine.Debug), "get_isDebugBuild")]
            public class DebugIsDebugBuildPatcher
            {
                public static void Postfix(ref bool __result)
                {
                    __result = true;
                }
            }
#endif

        // [HarmonyPatch(typeof(UnityEngine.DebugLogHandler), nameof(DebugLogHandler.Internal_Log))]
        // public class DebugLogHandlerPatcher
        // {
        //     public static void Postfix(LogType level, LogOption options, string msg, UnityEngine.Object obj)
        //     {
        //         log.LogMessage(msg);
        //
        //     }
        // }
        //
        // [HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.Log), new Type[] { typeof(Il2CppSystem.Object) })]
        // public class DebugLogPatcher
        // {
        //     public static void Postfix(Il2CppSystem.Object message)
        //     {
        //         log.LogMessage(message.ToString());
        //
        //     }
        // }
        //
        // [HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogFormat), new Type[]{ typeof(string), typeof(Il2CppSystem.Object[]) })]
        // public class DebugLogFormatPatcher
        // {
        //     public static void Postfix(string format, params Il2CppSystem.Object[] args)
        //     {
        //         log.LogMessage(format);
        //
        //     }
        // }
        //
        // [HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogWarningFormat), new Type[] { typeof(string), typeof(Il2CppSystem.Object[]) })]
        // public class DebugLogWarningFormatPatcher
        // {
        //     public static void Postfix(string format, params Il2CppSystem.Object[] args)
        //     {
        //         log.LogMessage(format);
        //
        //     }
        // }
        //
        // [HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogWarningFormat),
        //     new Type[] { typeof(UnityEngine.Object), typeof(string), typeof(Il2CppSystem.Object[]) })]
        // public class DebugLogWarningFormatPatcher01
        // {
        //     public static void Postfix(UnityEngine.Object context, string format, params Il2CppSystem.Object[] args)
        //     {
        //         log.LogMessage(format);
        //
        //     }
        // }
        //
        // [HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogWarningFormat),
        //     new Type[] { typeof(UnityEngine.Object), typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
        // public class DebugLogWarningFormatPatcher02
        // {
        //     public static void Postfix(string format, [Optional] Il2CppReferenceArray<Il2CppSystem.Object> args)
        //     {
        //         log.LogMessage(format);
        //
        //     }
        // }
        //
        // [HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogWarningFormat),
        //     new Type[] { typeof(UnityEngine.Object), typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
        // public class DebugLogWarningFormatPatcher03
        // {
        //     public static void Postfix(UnityEngine.Object context, string format, [Optional] Il2CppReferenceArray<Il2CppSystem.Object> args)
        //     {
        //         log.LogMessage(format);
        //
        //     }
        // }

        [HarmonyPatch(typeof(UnityEngine.TextGenerator), "ValidatedSettings")]
        public class TextGeneratorValidatedSettingsPatcher
        {
            public static bool Prefix(ref TextGenerationSettings __result, TextGenerationSettings settings)
            {
                if (settings.font == null) return true;

                // log.LogMessage($"fontNames: {string.Join(", ", settings.font.fontNames)}, {settings.font.fontSize}, {settings.font.dynamic}, {settings.fontSize}, {settings.fontStyle}, {settings.resizeTextForBestFit}");

                if (settings.font.dynamic == false)
                {
                    if (settings.fontSize != 0 || settings.fontStyle > FontStyle.Normal)
                    {
                        settings.fontSize = 0;
                        settings.fontStyle = FontStyle.Normal;
                    }

                    if (settings.resizeTextForBestFit)
                    {
                        settings.resizeTextForBestFit = false;
                    }

                    __result = settings;
                    return false;
                }
                return true;
            }
        }
    }

    private static Player GetLocalPlayer()
    {
        var kingdom = Managers.Inst.kingdom;
        if (kingdom == null)
            return null;
        return kingdom.GetPlayer(NetworkBigBoss.HasWorldAuth ? 0 : 1);
    }

    private void Update()
    {
        // 检测Ctrl+D组合键
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            enableDevTools = !enableDevTools;
            log.LogMessage($"DevTools {(enableDevTools ? "enabled" : "disabled")}.");
        }

        // 只有在DevTools启用时才处理其他热键
        if (!enableDevTools) return;

        if (Input.GetKeyDown(KeyCode.Home))
        {
            log.LogMessage("Home key pressed.");
            enabledDebugInfo = !enabledDebugInfo;
        }

        if (Input.GetKeyDown(KeyCode.End))
        {
            log.LogMessage("End key pressed.");
            enabledObjectsInfo = !enabledObjectsInfo;
        }

        if (Input.GetKeyDown(KeyCode.Insert))
        {
            log.LogMessage("Insert key pressed.");

            var cursorSystem = GameObject.FindFirstObjectByType<CursorSystem>();
            if (cursorSystem)
                cursorSystem.SetForceVisibleCursor(true);

            var biomeHelper = GameObject.Find("BiomeHelper");
            if (biomeHelper)
            {
                var biomeHolder = biomeHelper.GetComponent<BiomeHolder>();
                if (biomeHolder)
                {
                    biomeHolder.debugToolsEnabled = true;
                }
            }

            InterfaceOverlay.DebugDisable = false;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            List<GameObjectDetails> objectTree = new List<GameObjectDetails>();

            log.LogMessage("Dumping All Objects to JSON using GetAllScenesGameObject()...");
            foreach (var obj in GameObjectDetails.GetAllScenesGameObjects())
            {
                objectTree.Add(new GameObjectDetails(obj));
            }

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\AllObjectsDump.json",
                GameObjectDetails.JsonSerialize(objectTree));

            log.LogMessage("Complete!");
            log.LogMessage("JSON written to " + (AppDomain.CurrentDomain.BaseDirectory + "\\AllObjectsDump.json").Replace("\\\\", "\\"));

            string path = (AppDomain.CurrentDomain.BaseDirectory + "\\AllObjectsDump.json").Replace("\\\\", "\\");
            Application.OpenURL("file:///" + path.Replace("\\", "/"));
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            log.LogMessage($"Dump Prefabs:");

            var prefabList = GameObject.FindObjectsByType<PrefabID>(FindObjectsSortMode.None);
            foreach (var prefab in prefabList)
            {
                log.LogMessage($"PrefabID:{prefab.prefabID}, name: {prefab.name}");
            }

            var prefabs = Managers.Inst.prefabs;
            if (prefabs != null)
            {
                var dumpStr = "\npublic enum GamePrefabID\n{\n";
                foreach (var prefab in prefabs.prefabMasterCopies)
                {
                    var prefabName = prefab.name.Replace(' ', '_');
                    dumpStr = dumpStr + "    " + prefabName + " = " + prefab.prefabID + ",\n";
                }

                dumpStr = dumpStr + "}\n";
                log.LogMessage(dumpStr);
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            log.LogMessage($"Dump LevelBlocks:");

            var levelBlocks = Managers.Inst.level.GetLevelBlocks();
            log.LogMessage($"LevelBlocks: {levelBlocks.Count}");
            foreach (var levelBlock in levelBlocks)
            {
                log.LogMessage($"groups: {levelBlock.groupOne}, {levelBlock.groupTwo}, {levelBlock.groupThree}");
            }
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            var player = GetLocalPlayer();
            if (player != null)
            {
                var payable = player.selectedPayable;
                if (payable != null)
                {
#if IL2CPP
                        var payableTree = payable.TryCast<PayableTree>();
#else
                    var payableTree = payable as PayableTree;
#endif
                    if (payableTree != null)
                    {
                        log.LogMessage($"Try to cutdown the tree: {payable.GetGO.name}");
                        WorkableTree.AutoChopTrees = true;
                        payableTree.Pay();
                        WorkableTree.AutoChopTrees = false;
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            var player = GetLocalPlayer();
            if (player != null)
            {
                log.LogMessage($"Try to add Peasant.");

                var prefab = Resources.Load<Peasant>("Prefabs/Characters/Peasant").gameObject;
                Vector3 vector = player.transform.TransformPoint(new Vector3(1f, 1f, 0.01f));
                var lastSpawned = Pool.SpawnOrInstantiateGO(prefab, vector, Quaternion.identity, null);
                lastSpawned.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
            }
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            var player = GetLocalPlayer();
            if (player != null)
            {
                log.LogMessage($"Try to add Griffin.");

                var prefab = Resources.Load<Steed>("Prefabs/Steeds/Griffin P1");
                prefab.Price = 1;
                Vector3 vector = player.transform.TransformPoint(new Vector3(1f, 1f, 0.01f));
                var lastSpawned = Pool.SpawnOrInstantiateGO(prefab.gameObject, vector, Quaternion.identity, null);
                lastSpawned.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
            }
        }

        // if (Input.GetKeyDown(KeyCode.F3))
        // {
        //     var player = GetLocalPlayer();
        //     if (player != null)
        //     {
        //         log.LogMessage($"Try to add Wall0.");
        //
        //         // var prefab = Resources.Load<Wall>("prefabs/buildings and interactive/wall0");
        //         Vector3 vector = new Vector3(player.transform.position.x, 0.0f, 0.1f);
        //
        //         Wall wall = Instantiate<Wall>(Managers.Inst.holder.wallPrefabs[0]);
        //         wall.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
        //         wall.transform.position = vector;
        //
        //         // var lastSpawned = Pool.SpawnOrInstantiateGO(prefab.gameObject, vector, Quaternion.identity, GameObject.FindGameObjectWithTag("GameLayer").transform);
        //         // lastSpawned.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
        //     }
        // }
        //
        // if (Input.GetKeyDown(KeyCode.F4))
        // {
        //     var player = GetLocalPlayer();
        //     if (player != null)
        //     {
        //         log.LogMessage($"Try to add Tower0.");
        //
        //         // var prefab = Resources.Load<Tower>("prefabs/buildings and interactive/tower0");
        //
        //         var prefab = Managers.Inst.prefabs.GetPrefabById((int)GamePrefabID.Tower0);
        //         prefab.GetComponent<PayableUpgrade>().nextPrefab = Managers.Inst.prefabs.GetPrefabById((int)GamePrefabID.Tower2);
        //
        //         Vector3 vector = new Vector3(player.transform.position.x, 0.0f, 1.6f);
        //         var lastSpawned = Pool.SpawnOrInstantiateGO(prefab.gameObject, vector, Quaternion.identity,
        //             GameObject.FindGameObjectWithTag("GameLayer").transform);
        //         // lastSpawned.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            log.LogMessage($"F9 key pressed.");

            var array = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].name == "DebugTools")
                {
                    array[i].gameObject.SetActive(true);
                    var componentsInChildren = array[i].gameObject.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                    for (int j = 0; j < componentsInChildren.Length; j++)
                    {
                        componentsInChildren[j].font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                        componentsInChildren[j].fontSize = 4;
                        componentsInChildren[j].fontStyle = FontStyle.Bold;
                        componentsInChildren[j].text = componentsInChildren[j].text.ToLower();
                    }
#if IL2CPP
                        var iDebugTools = new IDebugTools(array[i].GetComponent("DebugTools").Pointer);
#else
                    var iDebugTools = (IDebugTools)(array[i].GetComponent("DebugTools"));
#endif
                    var cursorSystem = GameObject.FindFirstObjectByType<CursorSystem>();
                    if (cursorSystem)
                        cursorSystem.SetForceVisibleCursor(!iDebugTools.IsOpen());

                    iDebugTools.OpenButtonPressed();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            log.LogMessage($"F10 key pressed.");
            log.LogMessage($"Try to change max coins to 1000, you may need to reload game by F5 key.");

            Type type = typeof(CurrencyBag);
            FieldInfo field = type.GetField("OVERFLOW_LIMIT", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null && field.IsInitOnly)
            {
                field.SetValue(null, 1000);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var player = GetLocalPlayer();
            if (player != null)
            {
                player.TeleportFlash();
                player.dumpPassengerSound.Play(player.transform.position, 1);

                GameObject boulder = Pool.SpawnGO(Resources.Load<Boulder>("Prefabs/Objects/Boulder").gameObject, Vector3.zero, Quaternion.identity);
                var compCacher = Managers.Inst.compCacher;
                compCacher.GetCachedComponent<Boulder>(boulder)._launchedByEnemies = false;
                compCacher.GetCachedComponent<Boulder>(boulder).maxHitCitizens = 1000;
                compCacher.GetCachedComponent<Boulder>(boulder).StuckProbability = 0f;
                compCacher.GetCachedComponent<Boulder>(boulder).hitDamage = 300;
                compCacher.GetCachedComponent<SpriteRendererFX>(boulder).FadeIn(0.5f);
                boulder.transform.parent = player.transform.parent;
                boulder.transform.localPosition = Vector3.zero;
                boulder.transform.localRotation = Quaternion.identity;
                compCacher.GetCachedComponent<Boulder>(boulder).SetFake();
                boulder.transform.SetParent(player.transform.parent, true);
                boulder.transform.localScale = new Vector3(3f, 3f, 3f);
                boulder.transform.position = player.transform.position;
                boulder.transform.rotation = Quaternion.LookRotation(player.transform.forward);

                Vector2 vector6 = new Vector2((int)player.mover.GetDirection(), 0.2f);
                Vector2 vector7 = vector6.normalized * 14f;
                compCacher.GetCachedComponent<Boulder>(boulder).Launch(vector7, player.gameObject);
                Pool.Despawn(boulder, 5f);
            }
        }

        tick = tick + 1;
        if (tick > 60)
            tick = 0;

        if (tick == 0)
        {
            if (!IsPlaying()) return;

            if (enabledObjectsInfo)
                UpdateObjectsInfo();
        }
    }

    private void OnGUI()
    {
        if (!IsPlaying()) return;

        // 只有在DevTools启用时才显示其他信息
        if (!enableDevTools) return;

        // 显示DevTools状态
        guiStyle.normal.textColor = Color.green;
        GUI.Label(new Rect(10, Screen.height - 20, 200, 20), "DevTools Enabled", guiStyle);

        if (enabledObjectsInfo)
            DrawObjectsInfo();
        if (enabledDebugInfo)
            DrawDebugInfo();
    }

    private bool IsPlaying()
    {
        var game = Managers.Inst?.game;
        if (game == null) return false;
        return game.state is Game.State.Playing or Game.State.NetworkClientPlaying or Game.State.Menu;
    }

    private void UpdateObjectsInfo()
    {
        var worldCam = Managers.Inst.game._mainCameraComponent;
        if (!worldCam) return;

        objectsInfoList.Clear();
        var objects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in objects)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (!renderer || renderer.isVisible == false) continue;
            if (obj.GetComponent<Bird>()) continue;
            if (obj.GetComponent<Cloud>()) continue;
            if (obj.GetComponent<Grass>()) continue;
            if (obj.GetComponent<Flame>()) continue;
            if (obj.GetComponent<Tile>()) continue;
            if (obj.GetComponent<Foliage>()) continue;
            if (obj.GetComponent<ParticleSystem>()) continue;
            if (obj.GetComponent<BackgroundWall>()) continue;
            if (obj.GetComponent<ReflectedLight>()) continue;
            if (obj.GetComponent<Monument>()) continue;
            if (obj.GetComponent<Fish>()) continue;
            if (obj.GetComponent<Parallax2DChild>()) continue;
            // if (obj.GetComponent<Rigidbody2D>()) continue;
            // if (obj.GetComponent<PolygonCollider2D>()) continue;
            if (obj.GetComponent<WorkableTree>()) continue;

            if (obj.name.StartsWith("Haze")) continue;
            if (obj.name.StartsWith("castle_back")) continue;

            if (obj.name == "Glow") continue;
            if (obj.name == "Puff") continue;
            if (obj.name == "trunk") continue;
            if (obj.name == "sparkles") continue;
            if (obj.name == "highlight") continue;
            if (obj.name == "Roots(Clone)") continue;

            Vector3 screenPos = worldCam.WorldToScreenPoint(obj.transform.position);
            Vector3 uiPos = new Vector3(screenPos.x, Screen.height - screenPos.y, 0);
            if (uiPos.x > 0 && uiPos.x < Screen.width && uiPos.y > 0 && uiPos.y < Screen.height)
            {
                objectsInfoList.Add(new ObjectsInfo(new Rect(uiPos.x, uiPos.y, 100, 100), obj.transform.position, obj.name));
                //  + " (" + obj.transform.position.ToString() + ")(" + uiPos.ToString() + ")"
                log.LogInfo(obj.name);
            }
        }
    }

    private void DrawObjectsInfo()
    {
        guiStyle.normal.textColor = Color.white;
        foreach (var obj in objectsInfoList)
        {
            GUI.Label(obj.Pos, obj.Info, guiStyle);
            // var vecPos = obj.pos;
            // vecPos.y += 20;
            // GUI.Label(vecPos, obj.vec.x.ToString(), SpotMarkGUIStyle);
        }

        // var dog = GameObject.FindFirstObjectByType<Dog>();
        // if (!dog)
        //     return;
        //
        // Vector3 dogScreenPos = worldCam.WorldToScreenPoint(dog.transform.position);
        // Vector3 dogUiPos = new Vector3(dogScreenPos.x, Screen.height - dogScreenPos.y, 0);
        // GUI.Label(new Rect(dogUiPos.x, dogUiPos.y, 100, 100), dog.name, SpotMarkGUIStyle);

    }

    private void DrawDebugInfo()
    {
        if (ColoredTexture2D == null)
        {
            ColoredTexture2D = MakeColoredTexture2D(1, 1, Color.black);
            boxGuiStyle.normal.background = ColoredTexture2D;
            log.LogMessage("MakeColoredTexture2D.");
        }
        DrawGlobalDebugInfo();
        DrawPlayerDebugInfo(0);
        DrawPlayerDebugInfo(1);
    }

    private void DrawGlobalDebugInfo()
    {
        GUI.BeginGroup(new Rect(Screen.width - 210, 0, 210, Screen.height));
        {
            GUI.Box(new Rect(5, 5, 200, Screen.height - 10), "", boxGuiStyle);

            var infoLines = new List<string>();
            infoLines.Add($"Global");
            infoLines.Add($"HasWorldAuth: {NetworkBigBoss.HasWorldAuth}");
            infoLines.Add($"IsHost: {NetworkBigBoss.Instance.activeNetRouter.IsHost()}");
            infoLines.Add($"IsClient: {NetworkBigBoss.Instance.activeNetRouter.IsClient()}");
            infoLines.Add($"ProgramDirector.state: {ProgramDirector.state}");
            infoLines.Add($"ProgramDirector.IsClient: {ProgramDirector.IsClient}");
            infoLines.Add($"COOP_ENABLED: {Managers.COOP_ENABLED}");

            var game = Managers.Inst.game;
            infoLines.Add($"minLevelWidth: {game.currentLevelConfig.minLevelWidth}");

            var kingdom = Managers.Inst.kingdom;
            var castlePayable = kingdom.castle?._payableUpgrade;
            if (castlePayable != null)
            {
                infoLines.Add($"castle: {kingdom.castle.name}");
                infoLines.Add($"blockPaymentUpgrade: {castlePayable.blockPaymentUpgrade}");
                infoLines.Add($"cooldown: {castlePayable.cooldown}");
                infoLines.Add($"timeAvailableFrom: {castlePayable.timeAvailableFrom}");
                infoLines.Add($"timeAvailableFrom: {castlePayable.timeAvailableFrom - Time.time}");
                castlePayable.IsLocked(GetLocalPlayer(), out var reason);
                infoLines.Add($"IsLocked: {reason}");
                infoLines.Add($"price: {castlePayable.Price}");
            }

            infoLines.Add($"regularFont: {string.Join(", ", Language.current.regularFont.font.fontNames)}, {Language.current.menuFont.font.dynamic}");
            infoLines.Add($"bigTextFont: {string.Join(", ", Language.current.bigTextFont.font.fontNames)}, {Language.current.menuFont.font.dynamic}");
            infoLines.Add($"menuFont: {string.Join(", ", Language.current.menuFont.font.fontNames)}, {Language.current.menuFont.font.dynamic}");
            infoLines.Add($"menuAltFont: {string.Join(", ", Language.current.menuAltFont.font.fontNames)}, {Language.current.menuFont.font.dynamic}");

            guiStyle.normal.textColor = Color.white;
            guiStyle.alignment = TextAnchor.UpperLeft;
            for (int i = 0; i < infoLines.Count; i++)
            {
                GUI.Label(new Rect(14, 16 + 20 * i, 200, 20), infoLines[i], guiStyle);
            }
        }
        GUI.EndGroup();
    }

    private void DrawPlayerDebugInfo(int playerId)
    {
        var kingdom = Managers.Inst.kingdom;
        var player = kingdom.GetPlayer(playerId);
        if (player == null) return;
        if (player.isActiveAndEnabled == false) return;

        var groupX = 0.0f;
        var groupHeight = Screen.height * 1.0f;

        if (playerId == 1)
            groupX = 210;

        GUI.BeginGroup(new Rect(groupX, 0, 210, groupHeight));
        {
            GUI.Box(new Rect(5, 5, 200, groupHeight - 10), "", boxGuiStyle);

            var infoLines = new List<string>();
            infoLines.Add($"player: {player.model}");
            infoLines.Add($"isActiveAndEnabled: {player.isActiveAndEnabled}");
            infoLines.Add($"hasLocalAuthority: {player.hasLocalAuthority}");
            var steed = player.steed;
            if (steed != null)
            {
                infoLines.Add("steedType" + ": " + steed.steedType);
                infoLines.Add($"stamina: {steed.Stamina}");
                infoLines.Add($"reserveStamina: {steed.reserveStamina}, {steed.reserveProbability}");
                infoLines.Add($"eatDelay: {steed.eatDelay}, {steed.eatFullStaminaDelay}");
                infoLines.Add($"runStaminaRate: {steed.runStaminaRate}");
                infoLines.Add($"standStaminaRate: {steed.standStaminaRate}");
                infoLines.Add($"walkStaminaRate: {steed.walkStaminaRate}");
                infoLines.Add("time" + ": " + Time.time);
                infoLines.Add($"IsTired: {steed.IsTired}");
                infoLines.Add($"TiredTime: {steed.TiredTime}");
                infoLines.Add($"tiredTimer: {steed._tiredTimer}");
                infoLines.Add($"tiredDuration: {steed.tiredDuration}");
                infoLines.Add($"wellFedTimer: {steed.WellFedTimer}");
                infoLines.Add($"wellFedDuration: {steed.wellFedDuration}");
                infoLines.Add($"walkSpeed: {steed.walkSpeed}");
                infoLines.Add($"runSpeed: {steed.runSpeed}");
                infoLines.Add($"forestSpeedMultiplier: {steed.forestSpeedMultiplier}");
            }

            guiStyle.normal.textColor = Color.white;
            guiStyle.alignment = TextAnchor.UpperLeft;
            for (int i = 0; i < infoLines.Count; i++)
            {
                GUI.Label(new Rect(14, 16 + 20 * i, 200, 20), infoLines[i], guiStyle);
            }
        }
        GUI.EndGroup();
    }

    private static Texture2D MakeColoredTexture2D(int width, int height, Color color)
    {
        var coloredTexture = new Texture2D(width, height);
        coloredTexture.SetPixel(0, 0, color);
        coloredTexture.wrapMode = TextureWrapMode.Repeat;
        coloredTexture.Apply();
        return coloredTexture;
    }

    public class ObjectsInfo
    {
        public Rect Pos;
        public Vector3 Vec;
        public string Info;

        public ObjectsInfo(Rect pos, Vector3 vec, string info)
        {
            this.Pos = pos;
            this.Vec = vec;
            this.Info = info;
        }
    }

}