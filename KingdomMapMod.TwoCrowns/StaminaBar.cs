using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KingdomMapMod.TwoCrowns
{
    public class StaminaBar : MonoBehaviour
    {
        private static ManualLogSource log;
        private bool enableStaminaBar = true;

        public static void Initialize(Plugin plugin)
        {
            log = plugin.Log;
            var addComponent = plugin.AddComponent<StaminaBar>();
            addComponent.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(addComponent.gameObject);
        }

        public StaminaBar()
        {

        }

        private void Start()
        {
            log.LogMessage("StaminaBar Start.");
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

            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return;

            var worldCam = Managers.Inst.game._mainCameraComponent;
            if (!worldCam) return;

            var steed = kingdom.playerOne.steed;
            if (steed != null)
            {
                var screenPos = worldCam.WorldToScreenPoint(steed._mover.transform.position);
                var uiPos = new Vector2(screenPos.x, Screen.height - screenPos.y + 10);

                float widthScale = 1.5f;
                float heightScale = 0.875f;
                float baseWidth = widthScale * steed.wellFedDuration;
                float baseHeight = heightScale * 16;
                float baseLeft = uiPos.x - baseWidth / 2;

                var boxRect = new Rect(baseLeft, uiPos.y, baseWidth + 4, baseHeight);
                GUI.Box(boxRect, "");

                if (steed.tiredTimer > 0)
                {
                    var lineStart = new Vector2(baseLeft + 2, uiPos.y + 2);
                    var lineEnd = new Vector2(baseLeft + 2 + baseWidth * steed.tiredTimer / steed.tiredDuration, uiPos.y + 2);
                    GuiHelper.DrawLine(lineStart, lineEnd, Color.red, (uint)baseHeight - 4);
                }

                if (steed.wellFedTimer > 0)
                {
                    var lineStart = new Vector2(baseLeft + 2, uiPos.y + 2);
                    var lineEnd = new Vector2(baseLeft + 2 + steed.wellFedTimer * widthScale, uiPos.y + 2);
                    GuiHelper.DrawLine(lineStart, lineEnd, new Color(1.0f, 0.84f, 0.0f), (uint)baseHeight - 4);
                }

                if (steed.stamina > 0)
                {
                    var lineStart = new Vector2(baseLeft + 2, uiPos.y + 4);
                    var lineEnd = new Vector2(baseLeft + 2 + baseWidth * steed.stamina, uiPos.y + 4);
                    GuiHelper.DrawLine(lineStart, lineEnd, new Color(0.46f, 0.84f, 0.92f), (uint)baseHeight - 8);
                }

            }
        }

        private bool IsPlaying()
        {
            if (!Managers._Inst) return false;
            if (!Managers._Inst.game) return false;
            return Managers._Inst.game.playingOrInMenuWithClient;
        }
    }
}
