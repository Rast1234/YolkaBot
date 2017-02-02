using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace YolkaBot.Client
{
    public class GamepadInput : IInput
    {
        private GamePadState oldGamepadState;

        public IInput Draw(SpriteBatch spriteBatch)
        {
            return this;
        }

        public ActionRequest Read()
        {
            var capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            var gamepadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.None);
            var action = new ActionRequest();

            if (capabilities.IsConnected)
            {
                action.Left = (int)Math.Round(gamepadState.ThumbSticks.Left.Y * 100);
                action.Right = (int)Math.Round(gamepadState.ThumbSticks.Right.Y * 100);
                // 20% is a dead zone
                if (Math.Abs(action.Left) < 20)
                    action.Left = 0;
                if (Math.Abs(action.Right) < 20)
                    action.Right = 0;
                // triangle
                if (gamepadState.IsButtonDown(Buttons.Y))
                    action.Exit = true;
                // select
                if (gamepadState.IsButtonDown(Buttons.Back) && (oldGamepadState == null || oldGamepadState.IsButtonUp(Buttons.Back)))
                    action.Activate = true;
                // cross
                if (gamepadState.IsButtonDown(Buttons.A) && (oldGamepadState == null || oldGamepadState.IsButtonUp(Buttons.A)))
                    action.Stop = true;
                oldGamepadState = gamepadState;
            }
            return action;
        }

        public IInput Update(GameTime gameTime)
        {
            return this;
        }
    }
}