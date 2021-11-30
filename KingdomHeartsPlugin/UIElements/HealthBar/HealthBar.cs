using System;
using System.IO;
using System.Numerics;

using ImGuiNET;
using ImGuiScene;

using Dalamud.Game.ClientState.Objects.SubKinds;

using KingdomHeartsPlugin.Enums;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.UIElements.Health
{
    public class HealthBar
    {

        private readonly Vector3 _bgColor;

        public HealthBar(Vector3 bgColor)
        {

            _bgColor = bgColor;
            
            LowHealthAlpha = 0;
            LowHealthAlphaDirection = 0;

            // textures
            BarOutlineTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\bar_outline.png"));
            BarColorlessTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\bar_colorless.png"));
            BarForegroundTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\bar_foreground.png"));
            BarRecoveryTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\bar_recovery.png"));
            BarEdgeTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\bar_edge.png"));
            HealthRingSegmentTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\ring_health_segment.png"));
            RingValueSegmentTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"));
            RingOutlineTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\ring_outline_segment.png"));
            RingTrackTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\ring_track.png"));
            RingBaseTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\ring_base_edge.png"));
            RingEndTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\HealthBar\ring_end_edge.png"));

            // UI objects
            HealthRingBg = new Ring(RingValueSegmentTexture, _bgColor.X, _bgColor.Y, _bgColor.Z);
            HealthLostRing = new Ring(RingValueSegmentTexture, 1, 0, 0);
            RingOutline = new Ring(RingOutlineTexture);
            HealthRing = new Ring(HealthRingSegmentTexture);
            HealthRestoredRing = new Ring(HealthRingSegmentTexture) { Flip = true };
            _bgColor = bgColor;
        }

        public unsafe void Draw(PlayerCharacter player, float yBounce)
        {

            var drawList = ImGui.GetWindowDrawList();

            if (ImGui.GetDrawListSharedData() == IntPtr.Zero) return;

            ImGui.Dummy(new Vector2(220, 256));

            if (KingdomHeartsPluginDev.Ui.Configuration.HpBarEnabled)
            {
                UpdateValues(player.CurrentHp, player.MaxHp);
                Draw(drawList, player.CurrentHp, HpBeforeDamaged, player.CurrentHp, player.MaxHp, yBounce);
            }

            if (KingdomHeartsPluginDev.Ui.Configuration.ShowHpVal && KingdomHeartsPluginDev.Ui.Configuration.HpBarEnabled)
            {
                // Draw HP Value
                var basePosition = ImGui.GetItemRectMin() + new Vector2(KingdomHeartsPluginDev.Ui.Configuration.HpValueTextPositionX, KingdomHeartsPluginDev.Ui.Configuration.HpValueTextPositionY) * KingdomHeartsPluginDev.Ui.Configuration.Scale;
                /*float hp = KingdomHeartsPlugin.Ui.Configuration.TruncateHp && player.CurrentHp >= 10000
                    ? player.CurrentHp / 1000f
                    : player.CurrentHp;

                string hpVal = KingdomHeartsPlugin.Ui.Configuration.TruncateHp && player.CurrentHp >= 10000
                    ? player.CurrentHp >= 100000 ? $"{hp:0}K" : $"{hp:0.#}K" : $"{hp}";*/

                ImGuiAdditions.TextShadowedDrawList(drawList,
                    KingdomHeartsPluginDev.Ui.Configuration.HpValueTextSize,
                    $"{StringFormatting.FormatDigits(player.CurrentHp, KingdomHeartsPluginDev.Ui.Configuration.HpValueTextStyle)}",
                    basePosition,
                    new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f),
                    new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3, (TextAlignment)KingdomHeartsPluginDev.Ui.Configuration.HpValueTextAlignment);
            }
        }

        private void UpdateValues(uint currentHp, uint maxHp)
        {
            if (LastHp > currentHp && LastHp <= maxHp)
            {
                DamagedHealthAlpha = 1f;
                HpBeforeDamaged = LastHp;
            }

            if (LastHp < currentHp)
            {
                if (HealthRestoreTime <= 0)
                {
                    FilledHp = LastHp;
                    HpBeforeRestored = LastHp;
                }

                HealthRestoreTime = 1f;
            }

            UpdateLowHealthValues(currentHp, maxHp);

            UpdateDamagedHealthValues();

            UpdateRestoredHealthValues(currentHp);

            if (HpBeforeDamaged > maxHp)
                HpBeforeDamaged = maxHp;

            LastHp = currentHp;
        }

        private void UpdateRestoredHealthValues(uint currentHp)
        {
            if (HealthRestoreTime > 0)
            {
                HealthRestoreTime -= 1 * KingdomHeartsPluginDev.UiSpeed;
            }
            else if (FilledHp < currentHp)
            {
                FilledHp += (currentHp - HpBeforeRestored) * KingdomHeartsPluginDev.UiSpeed;
                if (HpBeforeRestored > currentHp)
                    HpBeforeRestored = currentHp;
            }

            if (FilledHp > currentHp)
                FilledHp = currentHp;
        }

        private void UpdateDamagedHealthValues()
        {
            switch (DamagedHealthAlpha)
            {
                case > 0.97f:
                    DamagedHealthAlpha -= 0.09f * KingdomHeartsPluginDev.UiSpeed;
                    break;
                case > 0.6f:
                    DamagedHealthAlpha -= 0.8f * KingdomHeartsPluginDev.UiSpeed;
                    break;
                case > 0.59f:
                    DamagedHealthAlpha -= 0.005f * KingdomHeartsPluginDev.UiSpeed;
                    break;
                case > 0.0f:
                    DamagedHealthAlpha -= 1f * KingdomHeartsPluginDev.UiSpeed;
                    break;
            }
        }

        private void UpdateLowHealthValues(uint health, uint maxHealth)
        {
            if (LowHealthSoundTime > 0)
            {
                LowHealthSoundTime -= KingdomHeartsPluginDev.UiSpeed;
            }

            // Sound player
            if (LowHealthSoundTime is <= 0 and > -100 && health <= maxHealth * (KingdomHeartsPluginDev.Ui.Configuration.LowHpPercent / 100f) && health > 0)
            {
                if (KingdomHeartsPluginDev.Ui.Configuration.LowHealthSoundEnabled)
                    SoundEngine.PlaySound(KingdomHeartsPluginDev.Ui.Configuration.LowHealthSoundPath, KingdomHeartsPluginDev.Ui.Configuration.LowHealthSoundVolume);

                LowHealthSoundTime = KingdomHeartsPluginDev.Ui.Configuration.LowHealthSoundDelay;
            }
            else if (health > maxHealth * (KingdomHeartsPluginDev.Ui.Configuration.LowHpPercent / 100f) || health == 0 || LowHealthSoundTime == -100 && KingdomHeartsPluginDev.Ui.Configuration.LowHealthSoundDelay != -100)
            {
                LowHealthSoundTime = 0;
            }

            if ((health > maxHealth * (KingdomHeartsPluginDev.Ui.Configuration.LowHpPercent / 100f) || health <= 0) && LowHealthAlpha <= 0) return;

            if (LowHealthAlphaDirection == 0)
            {
                LowHealthAlpha += 1.6f * KingdomHeartsPluginDev.UiSpeed;

                if (LowHealthAlpha >= .4)
                    LowHealthAlphaDirection = 1;
            }
            else
            {
                LowHealthAlpha -= 1.6f * KingdomHeartsPluginDev.UiSpeed;

                if (LowHealthAlpha <= 0)
                    LowHealthAlphaDirection = 0;
            }

            HealthRingBg.Color = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);

        }

        private void Draw(ImDrawListPtr drawList, uint hp, uint damagedHp, uint restoredHp, uint maxHp, float yBounce)
        {
            var ringLength = HealthRingBg.getCircumference() * 0.75d;
            var barLength = KingdomHeartsPluginDev.Ui.Configuration.HpBarLength;

            var ringHpCapacity = (float)(maxHp * (ringLength / (ringLength + barLength)));
            var barHpCapacity = Math.Max(0, maxHp - ringHpCapacity);

            var ringHp = hp > ringHpCapacity ? ringHpCapacity : hp;
            var barHp = Math.Max(0, hp - ringHp);

            var ringFilledHp = FilledHp <= ringHpCapacity ? FilledHp : ringHpCapacity;
            var barFilledHp = Math.Max(0, FilledHp - ringFilledHp);

            var ringDamagedHp = Math.Max(0, Math.Min(ringHpCapacity, damagedHp));
            var barDamagedHp = Math.Max(0, damagedHp - ringDamagedHp);

            var ringRestoredHp = Math.Max(0, Math.Min(ringHpCapacity, restoredHp));
            var barRestoredHp = Math.Max(0, restoredHp - ringRestoredHp);

            Dalamud.Logging.PluginLog.Debug($"ringLength: {ringLength}, barLength: {barLength}\n"
                + $"ringHpCapacity: {ringHpCapacity}, barHpCapacity: {barHpCapacity}\n"
                + $"hp: {hp}, damagedHp: {damagedHp}, restoredHp: {restoredHp}, maxHp: {maxHp}\n"
                + $"ringHp: {ringHp}, barHp: {barHp}\n"
                + $"ringFilledHp: {ringFilledHp}, barFilledHp: {barFilledHp}\n"
                + $"ringDamagedHp: {ringDamagedHp}, barDamagedHp: {barDamagedHp}\n"
                + $"ringRestoredHp: {ringRestoredHp}, barRestoredHp: {barRestoredHp}");

            var drawPosition = ImGui.GetItemRectMin();

            // for now, ensure the ring is always fully drawn
            // var ringVisiblePercent = maxHp / (float)ringHpCapacity * HpLengthMultiplier;

            // calculate the adjusted y position for things based on damage bounce
            int bounceYPos = (int)(yBounce);
            int scaledBounceYPos = (int)(bounceYPos * KingdomHeartsPluginDev.Ui.Configuration.Scale);

            try
            {
                DrawRingEdgesAndTrack(drawList, drawPosition + new Vector2(0, scaledBounceYPos));
            }
            catch
            {
                // Will sometimes error when hot reloading and I have no idea what is causing it. So exit.
                return;
            }

            DrawRingInterior(
                drawList, 
                ringHp / ringHpCapacity,
                ringFilledHp / ringHpCapacity,
                ringDamagedHp / ringHpCapacity, 
                ringRestoredHp / ringHpCapacity, 
                drawPosition + new Vector2(0, scaledBounceYPos)
            );

            DrawRingOutline(drawList, drawPosition + new Vector2(0, scaledBounceYPos));

            DrawLongHealthBar(
                drawList,
                barHp / barHpCapacity,
                barFilledHp / barHpCapacity,
                barDamagedHp / barHpCapacity,
                barRestoredHp / barHpCapacity,
                scaledBounceYPos
            );
        }

        private void DrawRingInterior(ImDrawListPtr drawList, float hpPercent, float filledHpPercent, float damagedHpPercent, float restoredHpPercent, Vector2 drawPosition)
        {
            HealthRingBg.Draw(
                drawList,
                100f, // always draw the full background
                drawPosition,
                3,
                KingdomHeartsPluginDev.Ui.Configuration.Scale
            );

            if (DamagedHealthAlpha > 0)
            {
                HealthLostRing.Alpha = DamagedHealthAlpha;
                HealthLostRing.Draw(
                    drawList,
                    damagedHpPercent,
                    drawPosition,
                    3,
                    KingdomHeartsPluginDev.Ui.Configuration.Scale
                );
            }

            if (KingdomHeartsPluginDev.Ui.Configuration.ShowHpRecovery)
            {
                if (filledHpPercent < hpPercent)
                    HealthRestoredRing.Draw(
                        drawList,
                        restoredHpPercent,
                        drawPosition,
                        3,
                        KingdomHeartsPluginDev.Ui.Configuration.Scale
                    );

                HealthRing.Draw(
                    drawList,
                    filledHpPercent,
                    drawPosition,
                    3,
                    KingdomHeartsPluginDev.Ui.Configuration.Scale
                );
            }
            else
            {
                HealthRing.Draw(
                    drawList,
                    hpPercent,
                    drawPosition,
                    3,
                    KingdomHeartsPluginDev.Ui.Configuration.Scale
                );
            }
        }

        private void DrawRingOutline(ImDrawListPtr drawList, Vector2 drawPosition)
        {
            RingOutline.Draw(
                drawList, 
                100f, 
                drawPosition, 
                3, 
                KingdomHeartsPluginDev.Ui.Configuration.Scale
            );
        }

        private void DrawLongHealthBar(ImDrawListPtr drawList, float percent, float filledHpPercent, float damagedPercent, float restoredPercent, int bounceYPos)
        {
            var hpBarLength = KingdomHeartsPluginDev.Ui.Configuration.HpBarLength;
            // var basePosition = drawPosition + new Vector2(129, 212);
            var basePosition = new Vector2(129, 212 + bounceYPos);

            var healthLength = hpBarLength * (KingdomHeartsPluginDev.Ui.Configuration.ShowHpRecovery ? filledHpPercent : percent);
            var damagedHealthLength = KingdomHeartsPluginDev.Ui.Configuration.ShowHpDamage ? hpBarLength * damagedPercent : 0.0f;
            var restoredHealthLength = KingdomHeartsPluginDev.Ui.Configuration.ShowHpRecovery ? hpBarLength * restoredPercent : 0.0f;

            // var healthLength = KingdomHeartsPluginDev.Ui.Configuration.ShowHpRecovery ? HpTemp : hp) * HpLengthMultiplier - fullRing) / HpPerWidth;
            // var damagedHealthLength = (HpBeforeDamaged * HpLengthMultiplier - fullRing) / HpPerWidth;
            // var restoredHealthLength = ((KingdomHeartsPluginDev.Ui.Configuration.ShowHpRecovery ? hp : 0) * HpLengthMultiplier - fullRing) / HpPerWidth;

            Vector3 lowHealthColor = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);
            ImageDrawing.DrawImage(
                drawList, 
                BarEdgeTexture, 
                new Vector2(basePosition.X - 6 - hpBarLength, basePosition.Y)
            );
            ImageDrawing.DrawImage(
                drawList, 
                BarColorlessTexture, 
                new Vector4(
                    basePosition.X - hpBarLength, 
                    basePosition.Y + 4,
                    hpBarLength, 
                    BarColorlessTexture.Height
                ), 
                ImGui.GetColorU32(new Vector4(lowHealthColor.X, lowHealthColor.Y, lowHealthColor.Z, 1))
            );

            if (damagedHealthLength > 0)
            {
                ImageDrawing.DrawImage(
                    drawList, 
                    BarColorlessTexture, 
                    new Vector4(
                        basePosition.X - damagedHealthLength, 
                        basePosition.Y + 4, 
                        damagedHealthLength, 
                        BarColorlessTexture.Height
                    ), 
                    ImGui.GetColorU32(new Vector4(1f, 0f, 0f, DamagedHealthAlpha))
                );
            }

            if (restoredHealthLength > 0)
            {
                ImageDrawing.DrawImage(
                    drawList, 
                    BarRecoveryTexture, 
                    new Vector4(
                        basePosition.X - restoredHealthLength, 
                        basePosition.Y + 4, 
                        restoredHealthLength, 
                        BarRecoveryTexture.Height
                    )
                );
            }

            if (healthLength > 0)
            {
                ImageDrawing.DrawImage(
                    drawList, 
                    BarForegroundTexture, 
                    new Vector4(
                        basePosition.X - healthLength, 
                        basePosition.Y + 4, 
                        healthLength, 
                        BarForegroundTexture.Height
                    )
                );
            }

            ImageDrawing.DrawImage(
                drawList, 
                BarOutlineTexture, 
                new Vector4(
                    basePosition.X - hpBarLength, 
                    basePosition.Y,
                    hpBarLength, 
                    BarOutlineTexture.Height
                )
            );
        }

        private void DrawRingEdgesAndTrack(ImDrawListPtr drawList, Vector2 position)
        {
            var size = 256 * KingdomHeartsPluginDev.Ui.Configuration.Scale;

            drawList.PushClipRect(position, position + new Vector2(size, size));
            drawList.AddImage(RingTrackTexture.ImGuiHandle, position, position + new Vector2(size, size));
            drawList.AddImage(RingBaseTexture.ImGuiHandle, position, position + new Vector2(size, size));

            // position the end of the hp ring
            ImageDrawing.ImageRotated(
                drawList,
                RingEndTexture.ImGuiHandle,
                new Vector2(position.X + size / 2f, position.Y + size / 2f),
                new Vector2(RingEndTexture.Width * KingdomHeartsPluginDev.Ui.Configuration.Scale, RingEndTexture.Height * KingdomHeartsPluginDev.Ui.Configuration.Scale),
                // draw a three-quarter circle (0.75f)
                0.75f * (float)Math.PI * 2
            );
            drawList.PopClipRect();
        }

        public void Dispose()
        {
            RingOutlineTexture?.Dispose();
            HealthRingSegmentTexture?.Dispose();
            RingValueSegmentTexture?.Dispose();
            RingTrackTexture?.Dispose();
            RingBaseTexture?.Dispose();
            RingEndTexture?.Dispose();
            BarColorlessTexture?.Dispose();
            BarEdgeTexture?.Dispose();
            BarForegroundTexture?.Dispose();
            BarOutlineTexture?.Dispose();
            BarRecoveryTexture?.Dispose();

            HealthRing = null;
            HealthRingBg = null;
            RingOutline = null;
            HealthRestoredRing = null;
            HealthLostRing = null;
            RingOutlineTexture = null;
            HealthRingSegmentTexture = null;
            RingValueSegmentTexture = null;
            RingTrackTexture = null;
            RingBaseTexture = null;
            RingEndTexture = null;
            BarColorlessTexture = null;
            BarEdgeTexture = null;
            BarForegroundTexture = null;
            BarOutlineTexture = null;
            BarRecoveryTexture = null;
        }

        // Temp Health Values
        public uint LastHp { get; set; }
        private uint HpBeforeDamaged { get; set; }
        private uint HpBeforeRestored { get; set; }
        private float FilledHp { get; set; }
        private float HpLengthMultiplier { get; set; }

        // Alpha Channels
        public float DamagedHealthAlpha { get; private set; }
        public float LowHealthAlpha { get; private set; }
        private int LowHealthAlphaDirection { get; set; }

        // Timers
        private float HealthRestoreTime { get; set; }
        private float LowHealthSoundTime { get; set; }

        // Textures
        private TextureWrap HealthRingSegmentTexture { get; set; }
        private TextureWrap BarOutlineTexture { get; set; }
        private TextureWrap BarColorlessTexture { get; set; }
        private TextureWrap BarForegroundTexture { get; set; }
        private TextureWrap BarRecoveryTexture { get; set; }
        private TextureWrap BarEdgeTexture { get; set; }
        private TextureWrap RingValueSegmentTexture { get; set; }
        private TextureWrap RingOutlineTexture { get; set; }
        private TextureWrap RingTrackTexture { get; set; }
        private TextureWrap RingBaseTexture { get; set; }
        private TextureWrap RingEndTexture { get; set; }

        // Rings
        private Ring HealthRing { get; set; }
        private Ring RingOutline { get; set; }
        private Ring HealthRingBg { get; set; }
        private Ring HealthRestoredRing { get; set; }
        private Ring HealthLostRing { get; set; }

    }
}
