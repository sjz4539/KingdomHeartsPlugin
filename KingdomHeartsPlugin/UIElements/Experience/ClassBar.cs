﻿using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Utilities;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using KingdomHeartsPlugin.Enums;

namespace KingdomHeartsPlugin.Configuration
{
    public partial class Defaults
    {
        public const float LevelTextX = 134;
        public const float LevelTextY = 76;
        public const float LevelTextSize = 32;
        public const TextAlignment LevelTextAlignment = TextAlignment.Center;

        public const float ClassIconX = 130;
        public const float ClassIconY = 140;
    }

    public partial class Settings
    {
        public float LevelTextX { get; set; } = Defaults.LevelTextX;
        public float LevelTextY { get; set; } = Defaults.LevelTextY;
        public float LevelTextSize { get; set; } = Defaults.LevelTextSize;
        public TextAlignment LevelTextAlignment { get; set; } = Defaults.LevelTextAlignment;
        public float ClassIconX { get; set; } = Defaults.ClassIconX;
        public float ClassIconY { get; set; } = Defaults.ClassIconY;
    }
}

namespace KingdomHeartsPlugin.UIElements.Experience
{
    public class ClassBar
    {
        private TextureWrap _expBarSegmentTexture, _expBarBaseTexture;
        private IntPtr _expAddonPtr;

        public ClassBar()
        {
            _expBarBaseTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\Experience\ring_experience_outline.png"));
            _expBarSegmentTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\Experience\ring_experience_segment.png"));

            ExperienceRing = new Ring(_expBarSegmentTexture);
            ExperienceRingRest = new Ring(_expBarSegmentTexture, alpha: 0.25f);
            ExperienceRingGain = new Ring(_expBarSegmentTexture, 0.65f, 0.92f, 1.00f) { Flip = true };
            ExperienceRingBg = new Ring(_expBarSegmentTexture, 0.07843f, 0.07843f, 0.0745f) { Flip = true };
        }

        private void Update(PlayerCharacter player)
        {
            _expAddonPtr = KingdomHeartsPluginDev.Gui.GetAddonByName("_Exp", 1);

            try
            {
                var current = Marshal.ReadInt32(_expAddonPtr + 0x278);
                var max = Marshal.ReadInt32(_expAddonPtr + 0x27C);
                int rest = Marshal.ReadInt32(_expAddonPtr + 0x280);
                UpdateExperience(current, max, rest, player.ClassJob.Id, player.Level);
            }
            catch
            {
                try
                {
                    _expAddonPtr = KingdomHeartsPluginDev.Gui.GetAddonByName("_Exp", 1);
                }
                catch
                {
                    // ignored
                }
            }
        }
        private void UpdateExperience(int exp, int maxExp, int rest, uint job, byte level)
        {
            if (LastLevel < level)
            {
                ExpTemp = 0;
                LastExp = 0;
            }

            if (LastJob != job)
            {
                ExpTemp = exp;
                LastExp = exp;
                ExpGainTime = 0;
            }

            if (LastExp > exp) ExpTemp = exp;

            if (LastExp < exp) GainExperience(LastExp);

            UpdateGainedExperience(exp);

            LastExp = exp;
            LastJob = job;
            LastLevel = level;
            Experience = exp;
            MaxExperience = maxExp;
            RestedBonusExperience = rest;
        }

        private void GainExperience(int exp)
        {
            if (ExpGainTime <= 0)
            {
                ExpBeforeGain = exp;
                ExpTemp = exp;
            }

            ExpGainTime = 3f;
        }

        private void UpdateGainedExperience(int exp)
        {
            if (ExpGainTime > 0)
            {
                ExpGainTime -= 1 * KingdomHeartsPluginDev.UiSpeed;
            }
            else if (ExpTemp < exp)
            {
                ExpTemp += (exp - ExpBeforeGain) * KingdomHeartsPluginDev.UiSpeed;
            }

            if (ExpTemp > exp)
                ExpTemp = exp;
        }

        public void Draw(PlayerCharacter player, float yBounce)
        {
            Update(player);
            var drawList = ImGui.GetWindowDrawList();

            int size = (int)Math.Ceiling(256 * KingdomHeartsPluginDev.Ui.Configuration.Scale);
            var drawPosition = ImGui.GetItemRectMin() + new Vector2(0, (int)(yBounce * KingdomHeartsPluginDev.Ui.Configuration.Scale));

            if (KingdomHeartsPluginDev.Ui.Configuration.ExpBarEnabled)
            {

                ExperienceRingBg.Draw(drawList, 1, drawPosition, 4, KingdomHeartsPluginDev.Ui.Configuration.Scale);

                ExperienceRingRest.Draw(drawList, (Experience + RestedBonusExperience) / (float)MaxExperience, drawPosition, 4, KingdomHeartsPluginDev.Ui.Configuration.Scale);

                ExperienceRingGain.Draw(drawList, Experience / (float)MaxExperience, drawPosition, 4, KingdomHeartsPluginDev.Ui.Configuration.Scale);

                ExperienceRing.Draw(drawList, ExpTemp / MaxExperience, drawPosition, 4, KingdomHeartsPluginDev.Ui.Configuration.Scale);

                drawList.PushClipRect(drawPosition, drawPosition + new Vector2(size, size));
                drawList.AddImage(_expBarBaseTexture.ImGuiHandle, drawPosition, drawPosition + new Vector2(size, size));
                drawList.PopClipRect();
            }

            Portrait.Draw(yBounce);

            if (KingdomHeartsPluginDev.Ui.Configuration.ClassIconEnabled)
            {
                float iconSize = 3f * KingdomHeartsPluginDev.Ui.Configuration.Scale;

                if (KingdomHeartsPluginDev.Cs.LocalPlayer is null) return;

                ImageDrawing.DrawIcon(drawList, (ushort)(62000 + KingdomHeartsPluginDev.Cs.LocalPlayer.ClassJob.Id),
                    new Vector2(iconSize, iconSize),
                    //new Vector2((int)(size / 2f), (int)(size / 2f + 18 * KingdomHeartsPlugin.Ui.Configuration.Scale)) +
                    new Vector2(KingdomHeartsPluginDev.Ui.Configuration.ClassIconX, KingdomHeartsPluginDev.Ui.Configuration.ClassIconY) +
                    new Vector2(0, (int)(yBounce * KingdomHeartsPluginDev.Ui.Configuration.Scale)));
            }

            if (KingdomHeartsPluginDev.Ui.Configuration.LevelEnabled)
                ImGuiAdditions.TextShadowedDrawList(drawList, KingdomHeartsPluginDev.Ui.Configuration.LevelTextSize,
                    $"Lv{KingdomHeartsPluginDev.Cs.LocalPlayer.Level}",
                    drawPosition + new Vector2(KingdomHeartsPluginDev.Ui.Configuration.LevelTextX, KingdomHeartsPluginDev.Ui.Configuration.LevelTextY),
                    new Vector4(249 / 255f, 247 / 255f, 232 / 255f, 0.9f),
                    new Vector4(96 / 255f, 78 / 255f, 23 / 255f, 0.25f), 3,
                    KingdomHeartsPluginDev.Ui.Configuration.LevelTextAlignment);

            if (KingdomHeartsPluginDev.Ui.Configuration.ExpValueTextEnabled)
                ImGuiAdditions.TextShadowedDrawList(drawList, KingdomHeartsPluginDev.Ui.Configuration.ExpValueTextSize,
                    $"{StringFormatting.FormatDigits(Experience, (NumberFormatStyle)KingdomHeartsPluginDev.Ui.Configuration.ExpValueTextFormatStyle)} / {StringFormatting.FormatDigits(MaxExperience, (NumberFormatStyle)KingdomHeartsPluginDev.Ui.Configuration.ExpValueTextFormatStyle)}",
                    drawPosition + new Vector2(KingdomHeartsPluginDev.Ui.Configuration.ExpValueTextPositionX, KingdomHeartsPluginDev.Ui.Configuration.ExpValueTextPositionY),
                    new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f),
                    new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f),
                    3,
                    (TextAlignment)KingdomHeartsPluginDev.Ui.Configuration.ExpValueTextAlignment);
        }

        public void Dispose()
        {
            _expBarBaseTexture?.Dispose();
            _expBarSegmentTexture?.Dispose();
            ExperienceRing.Dispose();
            ExperienceRingRest.Dispose();
            ExperienceRingGain.Dispose();
            ExperienceRingBg.Dispose();

            _expBarBaseTexture = null;
            _expBarSegmentTexture = null;
            ExperienceRing = null;
            ExperienceRingRest = null;
            ExperienceRingGain = null;
            ExperienceRingBg = null;
            _expAddonPtr = IntPtr.Zero;
        }

        private int Experience { get; set; }
        private int RestedBonusExperience { get; set; }
        private int MaxExperience { get; set; }
        private int LastExp { get; set; }
        private uint LastJob { get; set; }
        private byte LastLevel { get; set; }
        private int ExpBeforeGain { get; set; }
        private float ExpTemp { get; set; }
        private float ExpGainTime { get; set; }
        private Ring ExperienceRing { get; set; }
        private Ring ExperienceRingRest { get; set; }
        private Ring ExperienceRingGain { get; set; }
        private Ring ExperienceRingBg { get; set; }
    }
}
