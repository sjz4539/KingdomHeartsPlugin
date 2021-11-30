using System;
using System.Numerics;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.UIElements
{
    internal class Ring : IDisposable
    {

        public const int DEFAULT_DIAMETER = 256; 

        private readonly int diameter;

        public Ring(TextureWrap image, float colorR = 1f, float colorG = 1f, float colorB = 1f, float alpha = 1f, int diameter = DEFAULT_DIAMETER)
        {
            Image = image;
            Color = new Vector3(colorR, colorG, colorB);
            Alpha = alpha;
            Flip = false;
            this.diameter = diameter;
        }

        public int getDiameter()
        {
            return diameter;
        }

        public double getCircumference()
        {
            return Math.PI * diameter;
        }

        public void Draw(ImDrawListPtr drawList, float percent, Vector2 position, int segments, float scale = 1f)
        {
            segments = Math.Max(1, Math.Min(4, segments));

            int size = (int)Math.Ceiling(diameter * scale);
            int sizeHalf = (int) Math.Floor(size / 2f);
            percent = Math.Max(percent > 0 ? 0.002f : 0, percent);
            var color = ImGui.GetColorU32(new Vector4(Color.X, Color.Y, Color.Z, Alpha));

            drawSegment(
                drawList,
                position,
                position, 
                position + new Vector2(sizeHalf, sizeHalf),
                Math.Min(percent * 0.25f * segments, 0.25f),
                scale
            );

            /*drawList.PushClipRect(
                position, 
                position + new Vector2(sizeHalf, sizeHalf)
            );

            ImageDrawing.ImageRotated(
                drawList, 
                Image.ImGuiHandle, 
                new Vector2(position.X + sizeHalf, position.Y + sizeHalf), 
                new Vector2(size, size), 
                (-0.25f + (Flip ? 0.5f : 0) + Math.Min(percent * 0.25f * segments, 0.25f)) * (float)Math.PI * 2, 
                color
            );

            drawList.PopClipRect();*/

            if (segments < 2) return;

            drawSegment(
                drawList, 
                position,
                position + new Vector2(sizeHalf, 0),
                position + new Vector2(sizeHalf * 2, sizeHalf),
                Math.Min(Math.Max(percent * 0.25f * segments, 0.25f), 0.5f), 
                scale
            );

            /*drawList.PushClipRect(
                position + new Vector2(sizeHalf, 0), 
                position + new Vector2(sizeHalf * 2, sizeHalf)
            );

            ImageDrawing.ImageRotated(
                drawList, 
                Image.ImGuiHandle, 
                new Vector2(position.X + sizeHalf, position.Y + sizeHalf), 
                new Vector2(size, size), 
                (-0.25f + (Flip ? 0.5f : 0) + Math.Min(Math.Max(percent * 0.25f * segments, 0.25f), 0.5f)) * (float)Math.PI * 2, 
                color
            );

            drawList.PopClipRect();*/

            if (segments < 3) return;

            drawSegment(
                drawList, 
                position,
                position + new Vector2(sizeHalf, sizeHalf),
                position + new Vector2(sizeHalf * 2, sizeHalf * 2),
                Math.Min(Math.Max(percent * 0.25f * segments, 0.5f), 0.75f), 
                scale
            );

            /*drawList.PushClipRect(
                position + new Vector2(sizeHalf, sizeHalf), 
                position + new Vector2(sizeHalf * 2, sizeHalf * 2)
            );

            ImageDrawing.ImageRotated(
                drawList, 
                Image.ImGuiHandle, 
                new Vector2(position.X + sizeHalf, position.Y + sizeHalf), 
                new Vector2(size, size), 
                (-0.25f + (Flip ? 0.5f : 0) + Math.Min(Math.Max(percent * 0.25f * segments, 0.5f), 0.75f)) * (float)Math.PI * 2, 
                color
            );

            drawList.PopClipRect();*/

            if (segments < 4) return;

            drawSegment(
                drawList, 
                position,
                position + new Vector2(0, sizeHalf),
                position + new Vector2(sizeHalf, sizeHalf * 2),
                Math.Min(Math.Max(percent * 0.25f * segments, 0.75f), 1f), 
                scale
            );

            /*drawList.PushClipRect(
                position + new Vector2(0, sizeHalf), 
                position + new Vector2(sizeHalf, sizeHalf * 2)
            );

            ImageDrawing.ImageRotated(
                drawList, 
                Image.ImGuiHandle, 
                new Vector2(position.X + sizeHalf, position.Y + sizeHalf), 
                new Vector2(size, size), 
                (-0.25f + (Flip ? 0.5f : 0) + Math.Min(Math.Max(percent * 0.25f * segments, 0.75f), 1f)) * (float)Math.PI * 2, 
                color
            );

            drawList.PopClipRect();*/
        }

        private void drawSegment(ImDrawListPtr drawList, Vector2 position, Vector2 clipRectMin, Vector2 clipRectMax, float angle, float scale = 1f)
        {
            int size = (int)Math.Ceiling(diameter * scale);
            int sizeHalf = (int)Math.Floor(size / 2f);
            var color = ImGui.GetColorU32(new Vector4(Color.X, Color.Y, Color.Z, Alpha));

            drawList.PushClipRect(clipRectMin, clipRectMax);

            ImageDrawing.ImageRotated(
                drawList,
                Image.ImGuiHandle,
                new Vector2(position.X + sizeHalf, position.Y + sizeHalf),
                new Vector2(size, size),
                (-0.25f + (Flip ? 0.5f : 0) + angle) * (float)Math.PI * 2,
                color
            );

            drawList.PopClipRect();
        }

        public void Dispose()
        {
        }

        private TextureWrap Image { get; }
        internal Vector3 Color { get; set; }
        internal float Alpha { get; set; }
        internal bool Flip { get; set; }
    }
}
