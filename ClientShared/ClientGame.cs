using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace YolkaBot.Client
{
    public class ClientGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D pepe;
        private List<IInput> inputs;
        private readonly BotControl botConrol;

        public ClientGame(string host, int port)//
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            botConrol = new BotControl(host, port, TimeSpan.FromSeconds(1));

#if ANDROID || IOS || WINRT || WINDOWS_PHONE
// not sure if this is needed at all
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
#endif

        }

        protected override void Initialize()
        {
            font = Content.Load<SpriteFont>("font1");
            pepe = Content.Load<Texture2D>("pepe");
            Drawing.font = font;  // i am sorry for this
            inputs = GetPlatformInputs();
            botConrol.Run();
            base.Initialize();
        }

        private static List<IInput> GetPlatformInputs()
        {
            var result = new List<IInput>();
#if WINDOWS || LINUX
            result.Add(new KeyboardInput());
            result.Add(new GamepadInput());
#endif
#if ANDROID || IOS || WINRT || WINDOWS_PHONE
            result.Add(new VirtualInput(TouchPanel.DisplayWidth, TouchPanel.DisplayHeight));
#endif
            return result;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font1");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var actions = inputs.Select(i => i.Update(gameTime).Read()).ToList();
            var merged = Merge(actions);
            if (merged.Exit)
                Exit();
            botConrol.Update(merged);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.BlanchedAlmond);

            spriteBatch.Begin();
            var screenHeight = GraphicsDevice.Viewport.Height;
            var screenWidth = GraphicsDevice.Viewport.Width;

            var halfScreen = screenHeight * 0.5f;
            var scale = halfScreen / pepe.Height;
            spriteBatch.Draw(pepe, new Vector2(0, screenHeight/2), scale:new Vector2(scale, scale));

            var captionOrigin = new Vector2(screenWidth / 2.0f, screenHeight / 10.0f);
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Drawing.DrawStringCentered($"YolkaBot control app {version}", captionOrigin, Color.Black, spriteBatch);
            spriteBatch.DrawString(font, $"Status: {botConrol.Status}", new Vector2(40, screenHeight / 10.0f * 2), Color.LightGray);
            spriteBatch.DrawString(font, $"Datetime: {DateTime.Now.ToString("HH:mm:ss")}", new Vector2(40, screenHeight / 10.0f * 3), Color.LightGray);
            inputs.Select(i => i.Draw(spriteBatch)).ToList();
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private ActionRequest Merge(IEnumerable<ActionRequest> actions)
        {
            var merged = new ActionRequest
            {
                Left = 0,
                Right = 0,
                Stop = false,
                Activate = false,
                Exit = false
            };
            if (actions.All(a => a.Left == 0))
                merged.Left = 0;
            else
                merged.Left = actions.Select(a => a.Left).FirstOrDefault(x => x != 0);
            if (actions.All(a => a.Right == 0))
                merged.Right = 0;
            else
                merged.Right = actions.Select(a => a.Right).FirstOrDefault(x => x != 0);
            merged.Stop = actions.Any(a => a.Stop);
            merged.Activate = actions.Any(a => a.Activate);
            merged.Exit = actions.Any(a => a.Exit);

            return merged;
        }
    }
}