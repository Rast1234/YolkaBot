using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YolkaBot.Client
{
    public interface IInput
    {
        ActionRequest Read();
        IInput Update(GameTime gameTime);
        IInput Draw(SpriteBatch spriteBatch);
    }
}