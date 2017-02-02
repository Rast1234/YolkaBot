using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace YolkaBot.Client
{
    class VirtualStick
    {
        // How far from the alive zone we can get before the touch stick starts to follow in FreeFollow mode
        public float AliveZoneFollowFactor { get; set; } = 1.3f;
        // How quickly the touch stick follows in FreeFollow mode
        public float AliveZoneFollowSpeed { get; set; }
        public float DeadZoneSize { get; set; }
        public float AliveZoneSize { get; set; }
        // If we let the touch origin get too close to the screen edge,
        // the direction is less accurate, so push it away from the edge.
        public float DistFromScreenEdge { get; set; }
        public Rectangle StickStartRegion { get; set; }
        public Vector2 StartLocation { get; set; }
        public Vector2 FixedLocation { get; set; }
        public Vector2 StickDirection { get; set; }
        public Vector2 StickPos { get; set; }
        public float StickMagnitude { get; set; }

        public bool IsLeft { get; set; }
        public int LastExcludedRightTouchId = -1;
        public List<Rectangle> RightStickStartExcludeRegions = new List<Rectangle>(5);

        public TouchLocation? Stick = null;
        public bool UsingStick { get { return Stick.HasValue; } }

        private TouchStickStyle stickStyle;
        public TouchStickStyle StickStyle
        {
            get { return stickStyle; }
            set
            {
                stickStyle = value;
                if (stickStyle == TouchStickStyle.Fixed)
                    StartLocation = FixedLocation;
            }
        }

        /// <summary>
        /// Calculate the stick's direction and magnitude
        /// </summary>
        /// <param name="pos">The current left touch position</param>
        public void EvaluatePoint(Vector2 pos, float dt)
        {
            StickDirection = pos - StartLocation;
            float stickLength = StickDirection.Length();
            if (stickLength <= DeadZoneSize)
            {
                StickDirection = Vector2.Zero;
                StickMagnitude = 0.0f;
            }
            else
            {
                var tmp = StickDirection;
                tmp.Normalize();
                StickDirection = new Vector2(tmp.X, tmp.Y * -1f);
                if (stickLength < AliveZoneSize)
                {
                    StickMagnitude = stickLength / AliveZoneSize;
                    StickDirection = new Vector2(StickDirection.X * StickMagnitude, StickDirection.Y * StickMagnitude);
                }
                else
                {
                    StickMagnitude = 1.0f;

                    if (StickStyle == TouchStickStyle.FreeFollow && stickLength > AliveZoneSize * AliveZoneFollowFactor)
                    {
                        Vector2 targetLoc = new Vector2(
                            pos.X - StickDirection.X * AliveZoneSize * AliveZoneFollowFactor,
                            pos.Y + StickDirection.Y * AliveZoneSize * AliveZoneFollowFactor);

                        StartLocation = GetNewStartLocation(StartLocation, targetLoc, stickLength, dt);
                    }
                }
            }
        }

        private Vector2 GetNewStartLocation(Vector2 startLocation, Vector2 targetLoc, float stickLength, float dt)
        {
            Vector2.Lerp(ref startLocation, ref targetLoc,
                (stickLength - AliveZoneSize * AliveZoneFollowFactor) * AliveZoneFollowSpeed * dt,
                out startLocation);


            if (IsLeft)
            {
                if (startLocation.X > StickStartRegion.Width)
                    startLocation.X = (float)StickStartRegion.Width;
            }
            else
            {
                if (startLocation.X < StickStartRegion.Left)
                    startLocation.X = (float)StickStartRegion.Left;
            }
            if (startLocation.Y < StickStartRegion.Top)
                startLocation.Y = (float)StickStartRegion.Top;

            if (IsLeft)
            {
                if (startLocation.X < StickStartRegion.Left + DistFromScreenEdge)
                    startLocation.X = StickStartRegion.Left + DistFromScreenEdge;
            }
            else
            {
                if (startLocation.X > StickStartRegion.Right - DistFromScreenEdge)
                    startLocation.X = StickStartRegion.Right - DistFromScreenEdge;
            }

            if (startLocation.Y > StickStartRegion.Bottom - DistFromScreenEdge)
                startLocation.Y = StickStartRegion.Bottom - DistFromScreenEdge;

            return startLocation;
        }

    }
}