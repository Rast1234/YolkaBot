using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace YolkaBot.Client
{
    public class KeyboardInput : IInput
    {
        public IInput Draw(SpriteBatch spriteBatch)
        {
            return this;
        }

        public ActionRequest Read()
        {
            var state = Keyboard.GetState();
            var action = new ActionRequest();

            if (state.IsKeyDown(Keys.Escape))
                action.Exit = true;
            if (state.IsKeyDown(Keys.Enter))
                action.Activate = true;
            if (state.IsKeyDown(Keys.Space))
                action.Stop = true;

            if (state.IsKeyDown(Keys.D1))
                action.Left = 100;
            if (state.IsKeyDown(Keys.Q))
                action.Left = 50;
            if (state.IsKeyDown(Keys.A))
                action.Left = -50;
            if (state.IsKeyDown(Keys.Z))
                action.Left = -100;

            if (state.IsKeyDown(Keys.D2))
                action.Right = 100;
            if (state.IsKeyDown(Keys.W))
                action.Right = 50;
            if (state.IsKeyDown(Keys.S))
                action.Right = -50;
            if (state.IsKeyDown(Keys.X))
                action.Right = -100;

            return action;
        }

        public IInput Update(GameTime gameTime)
        {
            return this;
        }
    }
}
