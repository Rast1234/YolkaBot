using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace YolkaBot.Client.Android
{
    [Activity(Label = "AndroidTest"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        ,
        ConfigurationChanges =
            ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Activity1 : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //var defaultHost = "your.host:your_port";
            var defaultHost = "127.0.0.1:42038";
            var parts = defaultHost.Trim().Split(':');
            var host = parts[0];
            var port = int.Parse(parts[1]);
            var g = new ClientGame(host, port);
            SetContentView((View) g.Services.GetService(typeof(View)));
            g.Run();
        }
    }
}