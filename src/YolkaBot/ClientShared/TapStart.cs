using Microsoft.Xna.Framework;

namespace YolkaBot.Client
{
    /// <summary>
    /// Tap gestures can not be detected whilst another touch is active. TapStart struct is
    /// used to store the time, id and position of touch down events so we can decide later if they are taps
    /// </summary>
    struct TapStart
    {
        public int Id;
        public double Time;
        public Vector2 Pos;

        public TapStart(int id, double time, Vector2 pos)
        {
            Id = id;
            Time = time;
            Pos = pos;
        }
    }
}