using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KingdomHeartsPlugin.UIElements.Experience;
using KingdomHeartsPlugin.UIElements.LimitBreak;
using KingdomHeartsPlugin.UIElements.Health;
using KingdomHeartsPlugin.UIElements.ParameterResource;
using System;
using System.Numerics;

namespace KingdomHeartsPlugin.UIElements
{
    public class WidgetFrame : IDisposable
    {
        public static Vector3 DEFAULT_HP_BAR_BG_COLOR = new Vector3(0.07843f, 0.07843f, 0.0745f);
        
        private float _yBounce;
        private float _yBounceSpeed;
        private float _yBounceAnimationTicks;

        public WidgetFrame()
        {
            _yBounce = 0;
            _yBounceSpeed = 0;
            _yBounceAnimationTicks = 0;

            HealthBar = new HealthBar(DEFAULT_HP_BAR_BG_COLOR);
            LimitGauge = new LimitGauge();
            ResourceBar = new ResourceBar();
            ExpBar = new ClassBar();
        }

        public unsafe void Draw()
        {
            var player = KingdomHeartsPluginDev.Cs.LocalPlayer;
            var parameterWidget = (AtkUnitBase*)KingdomHeartsPluginDev.Gui.GetAddonByName("_ParameterWidget", 1);
            var drawList = ImGui.GetWindowDrawList();

            if (parameterWidget != null) // TODO: Can this be null? If so, we may miss the check below...
            {
                // Do not do or draw anything if the parameter widget is not visible
                if (!parameterWidget->IsVisible)
                {
                    return;
                }
            }

            // Do not do or draw anything if player is null or game ui is hidden
            if (player is null || KingdomHeartsPluginDev.Gui.GameUiHidden)
            {
                return;
            }

            // Do not do or draw anything if the draw list is bad.
            if (ImGui.GetDrawListSharedData() == IntPtr.Zero)
            {
                return;
            }

            ImGui.Dummy(new Vector2(220, 256));

            updateValues(player);

            if (KingdomHeartsPluginDev.Ui.Configuration.HpBarEnabled) HealthBar.Draw(player, _yBounce);
            if (KingdomHeartsPluginDev.Ui.Configuration.ResourceBarEnabled) ResourceBar.Draw(player, _yBounce);
            if (KingdomHeartsPluginDev.Ui.Configuration.LimitBarEnabled) LimitGauge.Draw(_yBounce);
            if (KingdomHeartsPluginDev.Ui.Configuration.ExpBarEnabled) ExpBar.Draw(player, _yBounce);
        }

        private void updateValues(PlayerCharacter player)
        {

            if (HealthBar.LastHp > player.CurrentHp && HealthBar.LastHp <= player.MaxHp)
            {
                _yBounce = 0;
                _yBounceSpeed = -3;
            }

            // Vertical bounce effect
            _yBounceAnimationTicks += 240 * KingdomHeartsPluginDev.UiSpeed;

            while (_yBounceAnimationTicks > 1)
            {
                _yBounceAnimationTicks--;
                _yBounce += _yBounceSpeed;

                if (_yBounce > 3)
                {
                    _yBounceSpeed -= 0.2f;
                }

                else if (_yBounce < -3)
                {
                    _yBounceSpeed += 0.2f;
                }
                else if (_yBounce is > -3 and < 3 && _yBounceSpeed is > -0.33f and < 0.33f)
                {
                    _yBounceSpeed = 0;
                    _yBounce = 0;
                }
                else if (_yBounceSpeed != 0)
                {
                    _yBounceSpeed *= 0.94f;
                }
            }

            _yBounce = _yBounce * KingdomHeartsPluginDev.Ui.Configuration.DamageBounceIntensity / 100f;

        }

        public void Dispose()
        {
            LimitGauge?.Dispose();
            ResourceBar?.Dispose();
            ExpBar?.Dispose();
            HealthBar?.Dispose();

            LimitGauge = null;
            ResourceBar = null;
            ExpBar = null;
            HealthBar = null;
        }

        public LimitGauge LimitGauge { get; set; }
        public ResourceBar ResourceBar { get; set; }
        public ClassBar ExpBar { get; set; }
        public HealthBar HealthBar { get; set; }

    }
}
