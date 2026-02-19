using BepInEx.Logging;
using System;
using KingdomMod.SharedLib;
using UnityEngine;
using KingdomMod.Shared.Attributes;

namespace KingdomMod.StaminaBar;

#if IL2CPP
[RegisterTypeInIl2Cpp]
#endif
public class StaminaBarHolder : MonoBehaviour
{
    public static StaminaBarHolder Instance { get; private set; }
    private static ManualLogSource log;
    private bool enableStaminaBar = true;

    public static void Initialize(StaminaBarPlugin plugin)
    {
        log = plugin.LogSource;

        GameObject obj = new(nameof(StaminaBarHolder));
        DontDestroyOnLoad(obj);
        obj.hideFlags = HideFlags.HideAndDontSave;
        Instance = obj.AddComponent<StaminaBarHolder>();
    }

#if IL2CPP
    public StaminaBarHolder(IntPtr ptr) : base(ptr) { }
#endif

    private void Start()
    {
        log.LogMessage($"{this.GetType().Name} Start.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            log.LogMessage("N key pressed.");
            enableStaminaBar = !enableStaminaBar;
        }
    }

    private void OnGUI()
    {
        if (!IsPlaying()) return;
        if (!enableStaminaBar) return;

        DrawStaminaBar(0);
        DrawStaminaBar(1);
    }

    private void DrawStaminaBar(int playerId)
    {
        var player = Managers.Inst.kingdom?.GetPlayer(playerId);
        if (player == null) return;
        if (!player.isActiveAndEnabled) return;
        if (!player.hasLocalAuthority) return;

        var steed = player.steed;
        if (steed == null) return;

        var uiPos = WorldToScreenPoint(playerId, steed._mover.transform.position);
        if (uiPos == null) return;
            
        float widthScale = 1.0f;
        float heightScale = 1.0f;
        float runConsumeScale = 0.08f * 3.6f / (Math.Abs(steed.runStaminaRate) * steed.runSpeed);
        float baseWidth = 60.0f * runConsumeScale * widthScale;
        float baseHeight = 14.0f * heightScale;
        float baseLeft = uiPos.Value.x - baseWidth / 2;
        float middleHeight = baseHeight / 2.0f - 2;
        float durationMax = 60.0f;

        var boxRect = new Rect(baseLeft, uiPos.Value.y, baseWidth + 4, baseHeight);
        GUI.Box(boxRect, "");
        GUI.Box(boxRect, "");

        GUI.BeginGroup(new Rect(baseLeft + 2, uiPos.Value.y + 2, baseWidth, baseHeight));

        if (steed.IsTired)
        {
            var tiredTimer = steed._tiredTimer - Time.time;
            if (tiredTimer > 0)
            {
                var lineStart = new Vector2(0, middleHeight);
                var lineEnd = new Vector2(baseWidth * tiredTimer / durationMax, middleHeight);
                GuiHelper.DrawLine(lineStart, lineEnd, Color.red, baseHeight - 4);
            }
        }
        
        if (steed.WellFedTimer > 0)
        {
            var lineStart = new Vector2(0, middleHeight);
            var lineEnd = new Vector2(baseWidth * steed.WellFedTimer / durationMax, middleHeight);
            GuiHelper.DrawLine(lineStart, lineEnd, new Color(1.0f, 0.84f, 0.0f), baseHeight - 4);
        }

        if (steed.Stamina > 0)
        {
            var lineStart = new Vector2(0, middleHeight);
            var lineEnd = new Vector2(baseWidth * steed.Stamina, middleHeight);
            var staminaColor = steed.IsTired ? Color.gray : new Color(0.46f, 0.84f, 0.92f);
            GuiHelper.DrawLine(lineStart, lineEnd, staminaColor, baseHeight - 8);
        }

        GUI.EndGroup();
    }

    private static Vector2? WorldToScreenPoint(int playerId, Vector3 position)
    {
        var camera = GetCameraForPlayerId(playerId);
        if (camera == null) return null;

        var screenPos = camera.WorldToScreenPoint(position);
        var uiPos = new Vector2(screenPos.x, Screen.height - screenPos.y);
        if (Managers.COOP_ENABLED)
        {
            uiPos.y /= 2;
            if (playerId == 1)
                uiPos.y += Screen.height / 2f;
        }

        uiPos.y += 10;
        return uiPos;
    }

    private static Camera GetCameraForPlayerId(int playerId)
    {
        if (playerId == 1 && Managers.COOP_ENABLED)
        {
            return Managers.Inst.game?._secondCameraComponent;
        }
        return Managers.Inst.game?._mainCameraComponent;
    }

    private static bool IsPlaying()
    {
        var game = Managers.Inst?.game;
        if (game == null) return false;
        return game.state is Game.State.Playing or Game.State.NetworkClientPlaying;
    }
}