using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace YolkaBot.Client
{
    public class VirtualInput : IInput
    {
        private readonly VirtualGamepad virtualGamepad;
        private readonly Button activateButton;
        private readonly Button stopButton;

        public VirtualInput(int width, int height)
        {
            virtualGamepad = new VirtualGamepad(width, height);
            activateButton = new Button("[ACTIVATE]", new Vector2(width / 2, height / 10 * 6), Color.BlueViolet, Color.OrangeRed, Drawing.font);
            stopButton = new Button("[STOP]", new Vector2(width / 2, height / 10 * 8), Color.BlueViolet, Color.OrangeRed, Drawing.font);
        }

        public IInput Draw(SpriteBatch spriteBatch)
        {
            activateButton.Draw(spriteBatch);
            stopButton.Draw(spriteBatch);

            DrawStick(virtualGamepad.left, spriteBatch /*, new Vector2(10, 10)*/);
            DrawStick(virtualGamepad.right, spriteBatch /*, new Vector2(TouchPanel.DisplayWidth/2, 10)*/);

            var leftPercent = (int)Math.Round(virtualGamepad.left.StickDirection.Y * 100);
            var rightPercent = (int)Math.Round(virtualGamepad.right.StickDirection.Y * 100);
            var leftTextOrigin = new Vector2(TouchPanel.DisplayWidth / 4.0f, TouchPanel.DisplayHeight / 4.0f);
            var rightTextOrigin = new Vector2(TouchPanel.DisplayWidth / 4.0f * 3, TouchPanel.DisplayHeight / 4.0f);
            Drawing.DrawStringCentered($"{leftPercent}%", leftTextOrigin, Color.Black, spriteBatch);
            Drawing.DrawStringCentered($"{rightPercent}%", rightTextOrigin, Color.Black, spriteBatch);
            return this;
        }

        private void DrawStick(VirtualStick stick, SpriteBatch spriteBatch, Vector2? debugTextLocation = null)
        {
            if (debugTextLocation.HasValue)
            {
                spriteBatch.DrawString(Drawing.font, $@"======
LOC {stick.StartLocation}
SIZ {stick.AliveZoneSize}
POS {stick.StickPos}
DIR {stick.StickDirection}
MAG {stick.StickMagnitude}
??? {stick.StickPos - stick.StartLocation}
",
    debugTextLocation.Value, Color.Black);

                Drawing.DrawStringCentered($"A", new Vector2(stick.StickStartRegion.Left, virtualGamepad.left.StickStartRegion.Top), Color.Black, spriteBatch);
                Drawing.DrawStringCentered($"B", new Vector2(stick.StickStartRegion.Right, virtualGamepad.left.StickStartRegion.Top), Color.Black, spriteBatch);
                Drawing.DrawStringCentered($"C", new Vector2(stick.StickStartRegion.Left, virtualGamepad.left.StickStartRegion.Bottom), Color.Black, spriteBatch);
                Drawing.DrawStringCentered($"D", new Vector2(stick.StickStartRegion.Right, virtualGamepad.left.StickStartRegion.Bottom), Color.Black, spriteBatch);

                Drawing.DrawStringCentered($"><", stick.StickPos, Color.Black, spriteBatch);
            }

            Drawing.DrawStringCentered($"x", stick.StartLocation, Color.Black, spriteBatch);

            var betterDirection = stick.StickDirection * new Vector2(1, -1);
            var pos = betterDirection * stick.AliveZoneSize + stick.StartLocation;
            Drawing.DrawStringCentered($"@", pos, Color.Black, spriteBatch);


        }

        public ActionRequest Read()
        {
            var action = new ActionRequest();
            var gamepadState = virtualGamepad.GetGamePadState();
            action.Left = (int)Math.Round(gamepadState.ThumbSticks.Left.Y * 100);
            action.Right = (int)Math.Round(gamepadState.ThumbSticks.Right.Y * 100);
            // 20% is a dead zone
            if (Math.Abs(action.Left) < 20)
                action.Left = 0;
            if (Math.Abs(action.Right) < 20)
                action.Right = 0;
            if (gamepadState.Buttons.Back == ButtonState.Pressed)
                action.Exit = true;
            if (activateButton.IsPressed)
                action.Activate = true;
            if (stopButton.IsPressed)
                action.Stop= true;
            return action;
        }

        public IInput Update(GameTime gameTime)
        {
            virtualGamepad.Update(gameTime);
            int tmp;
            Vector2 tmp2;
            activateButton.IsPressed = virtualGamepad.TryGetTouch(activateButton.rectangle, out tmp, out tmp2);
            stopButton.IsPressed = virtualGamepad.TryGetTouch(stopButton.rectangle, out tmp, out tmp2);
            return this;
        }
    }
}
