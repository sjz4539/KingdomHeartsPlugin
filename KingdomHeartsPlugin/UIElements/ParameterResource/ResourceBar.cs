using System;
using System.IO;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Enums;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.UIElements.ParameterResource
{
    public class ResourceBar
    {
        private TextureWrap _barBackgroundTexture, _barForegroundTexture, _mpBaseTexture, _barEdgeTexture;

        private enum Resource
        {
            Mp,
            Cp,
            Gp
        }

        public ResourceBar()
        {
            _barBackgroundTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\ResourceBar\background.png"));
            _barForegroundTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\ResourceBar\foreground.png"));
            _mpBaseTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\ResourceBar\MP_base.png"));
            _barEdgeTexture = KingdomHeartsPluginDev.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPluginDev.TemplateLocation, @"Textures\ResourceBar\edge.png"));
        }

        public void Update(PlayerCharacter player)
        {
            var minLength = 1;
            var maxLength = 1;
            var lengthRate = 1f;

            if (player.MaxMp > 0)
            {
                ResourceValue = player.CurrentMp;
                ResourceMax = player.MaxMp;
                ResourceType = Resource.Mp;

                minLength = KingdomHeartsPluginDev.Ui.Configuration.MinimumMpLength;
                maxLength = KingdomHeartsPluginDev.Ui.Configuration.MaximumMpLength;
                lengthRate = KingdomHeartsPluginDev.Ui.Configuration.MpPerPixelLength;
            }
            else if (player.MaxCp > 0)
            {
                ResourceValue = player.CurrentCp;
                ResourceMax = player.MaxCp;
                ResourceType = Resource.Cp;

                minLength = KingdomHeartsPluginDev.Ui.Configuration.MinimumCpLength;
                maxLength = KingdomHeartsPluginDev.Ui.Configuration.MaximumCpLength;
                lengthRate = KingdomHeartsPluginDev.Ui.Configuration.CpPerPixelLength;
            }
            else if (player.MaxGp > 0)
            {
                ResourceValue = player.CurrentGp;
                ResourceMax = player.MaxGp;
                ResourceType = Resource.Gp;

                minLength = KingdomHeartsPluginDev.Ui.Configuration.MinimumGpLength;
                maxLength = KingdomHeartsPluginDev.Ui.Configuration.MaximumGpLength;
                lengthRate = KingdomHeartsPluginDev.Ui.Configuration.GpPerPixelLength;
            }

            var lengthMultiplier = ResourceMax < minLength ? minLength / (float)ResourceMax : ResourceMax > maxLength ? (float)maxLength / ResourceMax : 1f;
            MaxResourceLength = (int)Math.Ceiling(ResourceMax / lengthRate * lengthMultiplier);
            ResourceLength = (int)Math.Ceiling(ResourceValue / lengthRate * lengthMultiplier);
        }

        public void Draw(PlayerCharacter player, float yBounce)
        {
            Update(player);
            var drawList = ImGui.GetWindowDrawList();
            var basePosition =  new Vector2(KingdomHeartsPluginDev.Ui.Configuration.ResourceBarPositionX, KingdomHeartsPluginDev.Ui.Configuration.ResourceBarPositionY);
            var textPosition = new Vector2(KingdomHeartsPluginDev.Ui.Configuration.ResourceTextPositionX, KingdomHeartsPluginDev.Ui.Configuration.ResourceTextPositionY) * KingdomHeartsPluginDev.Ui.Configuration.Scale;

            // Base
            ImageDrawing.DrawImage(drawList, _mpBaseTexture, new Vector2(basePosition.X - 1, basePosition.Y - 1), new Vector4(0, 0, 74 / 80f, 1));

            // BG
            ImageDrawing.DrawImage(drawList, _barBackgroundTexture, new Vector4(basePosition.X - MaxResourceLength, basePosition.Y, MaxResourceLength, _barBackgroundTexture.Height));

            // FG
            ImageDrawing.DrawImage(drawList, _barForegroundTexture, new Vector4(basePosition.X - ResourceLength, basePosition.Y + 5, ResourceLength, _barForegroundTexture.Height));

            // Edge
            ImageDrawing.DrawImage(drawList, _barEdgeTexture, new Vector2(basePosition.X - MaxResourceLength - 6, basePosition.Y - 1));

            // Base Edge
            ImageDrawing.DrawImageRotated(drawList, _barEdgeTexture, new Vector2(basePosition.X + 74, basePosition.Y + 15), new Vector2(_barEdgeTexture.Width, _barEdgeTexture.Height), (float)Math.PI);

            if (KingdomHeartsPluginDev.Ui.Configuration.ShowResourceVal)
                ImGuiAdditions.TextShadowedDrawList(drawList, KingdomHeartsPluginDev.Ui.Configuration.ResourceTextSize, $"{StringFormatting.FormatDigits(KingdomHeartsPluginDev.Ui.Configuration.TruncateMp && ResourceType == Resource.Mp ? ResourceValue / 100 : ResourceValue, KingdomHeartsPluginDev.Ui.Configuration.ResourceTextStyle)}", ImGui.GetItemRectMin() + basePosition * KingdomHeartsPluginDev.Ui.Configuration.Scale + textPosition, new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f), new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3, (TextAlignment)KingdomHeartsPluginDev.Ui.Configuration.ResourceTextAlignment);
        }

        public void Dispose()
        {
            _barBackgroundTexture.Dispose();
            _barEdgeTexture.Dispose();
            _barForegroundTexture.Dispose();
            _mpBaseTexture.Dispose();

            _barBackgroundTexture = null;
            _barForegroundTexture = null;
            _mpBaseTexture = null;
            _barEdgeTexture = null;
        }

        private uint ResourceValue { get; set; }
        private Resource ResourceType { get; set; }
        private uint ResourceMax { get; set; }
        private float ResourceLength { get; set; }
        private float MaxResourceLength { get; set; }
    }
}
