//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using System.Text;
//using System.Net.Sockets;
//using System.IO;
//using System.Threading;
//using YolkaBot;

//namespace DesktopClient
//{
//    public class Game1 : Game
//    {
//        GraphicsDeviceManager graphics;
//        SpriteBatch spriteBatch;
//        SpriteFont font;
        
//        BotControl botConrol;

//        public Game1(string host, int port)
//        {
//            graphics = new GraphicsDeviceManager(this);
//            Content.RootDirectory = "Content";
//            botConrol = new BotControl(host, port, 100);
//        }

//        protected override void Initialize()
//        {
//            botConrol.Run();
//            base.Initialize();
//        }

//        protected override void LoadContent()
//        {
//            spriteBatch = new SpriteBatch(GraphicsDevice);
//            font = Content.Load<SpriteFont>("font1");
//        }

//        protected override void UnloadContent()
//        {
//        }

//        protected override void Update(GameTime gameTime)
//        {

//            base.Update(gameTime);
//        }

        

        

//        protected override void Draw(GameTime gameTime)
//        {
//            GraphicsDevice.Clear(Color.CornflowerBlue);

//            spriteBatch.Begin();
//            spriteBatch.DrawString(font,$"Status: [{botConrol.status}]", new Vector2(50, 10), Color.Black);
//            spriteBatch.End();
//            base.Draw(gameTime);
//        }

//    }
//}
