using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YolkaBot.Client
{
    public static class Drawing
    {
        public static SpriteFont font; // initialize me!
        public static void DrawStringCentered(string text, Vector2 position, Color color, SpriteBatch spriteBatch)
        {
            var size = font.MeasureString(text);
            var origin = size * 0.5f;

            spriteBatch.DrawString(font, text, position, Color.Black, 0, origin, 1, SpriteEffects.None, 0);
        }
    }
}