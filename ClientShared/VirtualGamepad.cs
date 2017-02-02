//#if ANDROID || IOS || WINRT || WINDOWS_PHONE

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


namespace YolkaBot.Client
{
    class VirtualGamepad
    {
        public VirtualStick left;
        public VirtualStick right;
        
        // Total game time
        private double totalTime;
        
        private TapStart[] tapStarts = new TapStart[4];
        private int tapStartCount = 0;
        private bool JustTouched { get; set; }
        private Vector2 TouchStartPos { get; set; }
        private int? TouchStartId { get; set; }
        private bool JustTapped { get; set; }
        private Vector2 TapPosition { get; set; }

        // Current state of the TouchPanel
        private TouchCollection state;
        private int TouchCount
        {
            get
            {
                return state.Count;
            }
        }
        private Vector2 FirstTouch
        {
            get
            {
                if (state.Count > 0)
                    return state[0].Position;
                else
                    return Vector2.Zero;
            }
        }
        private int? FirstTouchId
        {
            get
            {
                if (state.Count > 0)
                    return state[0].Id;
                else
                    return null;
            }
        }

        // CONSTRUCTOR
        public VirtualGamepad(int width, int height)
        {
            var aliveZoneSize = System.Math.Min(width, height) * 0.3f;
            var aliveZoneFollowFactor = 1.3f;
            var aliveZoneFollowSpeed = 0.05f;
            
            var leftFixedLocation = new Vector2(
                aliveZoneSize * aliveZoneFollowFactor,
                height - aliveZoneSize * aliveZoneFollowFactor);

            var leftStartRegion = new Rectangle(
                0, 100,
                (int)(width * 0.3f), (int)height - 100);

            var rightFixedLocation = new Vector2(
                width - aliveZoneSize * aliveZoneFollowFactor,
                height - aliveZoneSize * aliveZoneFollowFactor);

            var rightStartRegion = new Rectangle(
                (int)(width * 0.5f), 100,
                (int)(width * 0.5f), (int)height - 100);

            left = new VirtualStick
            {
                AliveZoneSize = aliveZoneSize,
                AliveZoneFollowFactor = aliveZoneFollowFactor,
                AliveZoneFollowSpeed = aliveZoneFollowSpeed,
                DeadZoneSize = 5.0f,
                DistFromScreenEdge = 25.0f,
                StickStartRegion = leftStartRegion,
                FixedLocation = leftFixedLocation,
                StickStyle = TouchStickStyle.Fixed,
                IsLeft = true,
            };

            right = new VirtualStick
            {
                AliveZoneSize = aliveZoneSize,
                AliveZoneFollowFactor = aliveZoneFollowFactor,
                AliveZoneFollowSpeed = aliveZoneFollowSpeed,
                DeadZoneSize = 5.0f,
                DistFromScreenEdge = 25.0f,
                StickStartRegion = rightStartRegion,
                FixedLocation = rightFixedLocation,
                StickStyle = TouchStickStyle.Fixed,
                IsLeft = false,
            };
        }

        public void ClearTaps()
        {
            tapStartCount = 0;
        }

        public bool TryGetTouchPos(int touchId, out Vector2 pos)
        {
            foreach (TouchLocation loc in state)
            {
                if (loc.Id == touchId)
                {
                    pos = loc.Position;
                    return true;
                }
            }
            pos = Vector2.Zero;
            return false;
        }

        public bool TryGetTouch(Rectangle rect, out int touchId, out Vector2 pos)
        {
            foreach (TouchLocation loc in state)
            {
                if (rect.Contains((int)loc.Position.X, (int)loc.Position.Y))
                {
                    touchId = loc.Id;
                    pos = loc.Position;
                    return true;
                }
            }
            pos = Vector2.Zero;
            touchId = -1;
            return false;
        }

        /// <summary>
        /// Returns a GamePadState from the current stick direction. Aids in porting Xbox games.
        /// </summary>
        /// <returns></returns>
        public GamePadState GetGamePadState()
        {
            // Get gamepad 0 state, simply to detect the WP7 hardware Back button
            GamePadState gs0 = GamePad.GetState(PlayerIndex.One);

            GamePadState gs = new GamePadState(
                new GamePadThumbSticks(left.StickDirection, right.StickDirection),
                new GamePadTriggers(0.0f, 0.0f),
                gs0.Buttons,
                new GamePadDPad());

            return gs;
        }

        public void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            totalTime += dt;

            JustTouched = false;
            JustTapped = false;
            TouchStartId = null;

            state = TouchPanel.GetState();
            TouchLocation? leftTouch = null, rightTouch = null;

            if (tapStartCount > state.Count)
            {
                tapStartCount = state.Count;

                // Workaround. Sometimes very fast taps won't be registered as TouchLocations with state of Released
                // meaning the algorithm in the for loop below falls down :(
                // Here we assume that only one tap was missed
                if (state.Count == 0)
                {
                    JustTapped = true;
                    TapPosition = tapStarts[0].Pos;
                }
            }

            foreach (TouchLocation loc in state)
            {
                if (loc.State == TouchLocationState.Released)
                {
                    int tapStartId = -1;
                    for (int i = 0; i < tapStartCount; ++i)
                    {
                        if (tapStarts[i].Id == loc.Id)
                        {
                            // This touch was released. Check if it was a tap
                            tapStartId = i;

                            // COMMENTED CODE WAS TO ENSURE TAPS ARE NOT REGISTERED FOR HOLDS (LONG TAPS)
                            //if ((Engine.Instance.TimeTotal - tapStarts[i].Time) < 1.0f)
                            //{
                            JustTapped = true;
                            TapPosition = loc.Position;
                            //}
                            //else
                            //{
                            //  System.Diagnostics.Debug.WriteLine("Rejected touch: Held too long");
                            //}

                            break;
                        }
                    }
                    if (tapStartId >= 0)
                    {
                        // Remove the tap start as it has been released
                        for (int i = tapStartId; i < tapStartCount - 1; ++i)
                            tapStarts[i] = tapStarts[i + 1];

                        tapStartCount--;
                    }
                    continue;
                }
                else if (loc.State == TouchLocationState.Pressed && tapStartCount < tapStarts.Length)
                {
                    // Started new touch
                    tapStarts[tapStartCount] = new TapStart(loc.Id, totalTime, loc.Position);
                    tapStartCount++;
                    JustTouched = true;
                    TouchStartId = loc.Id;
                    TouchStartPos = loc.Position;
                }
                // COMMENTED CODE WAS TO REMOVE TAPS THAT DEVIATE TOO FAR FROM THEIR ORIGINAL POSITION
                //else
                //{
                //    int removeTapId = -1;
                //    for (int i = 0; i < tapStartCount; ++i)
                //    {
                //        if (tapStarts[i].Id == loc.Id)
                //        {
                //            // Remove any tap that deviates too far from it's original position
                //            float distSqr = Vector2.DistanceSquared(tapStarts[i].Pos, loc.Position);
                //            if (distSqr > 3600.0f)
                //            {
                //                //System.Diagnostics.Debug.WriteLine("Rejected touch: Deviated too far");
                //                removeTapId = i;
                //            }
                //            break;
                //        }
                //    }
                //    if (removeTapId >= 0)
                //    {
                //        // Remove the tap start as it has moved further than is valid
                //        for (int i = removeTapId; i < tapStartCount - 1; ++i)
                //            tapStarts[i] = tapStarts[i + 1];

                //        tapStartCount--;
                //    }
                //}

                if (left.Stick.HasValue && loc.Id == left.Stick.Value.Id)
                {
                    // Continue left touch
                    leftTouch = loc;
                    continue;
                }
                if (right.Stick.HasValue && loc.Id == right.Stick.Value.Id)
                {
                    // Continue right touch
                    rightTouch = loc;
                    continue;
                }

                TouchLocation locPrev;
                if (!loc.TryGetPreviousLocation(out locPrev))
                    locPrev = loc;

                if (!left.Stick.HasValue)
                {
                    // if we are not currently tracking a left thumbstick and this touch is on the left
                    // half of the screen, start tracking this touch as our left stick
                    if (left.StickStartRegion.Contains((int)locPrev.Position.X, (int)locPrev.Position.Y))
                    {
                        if (left.StickStyle == TouchStickStyle.Fixed)
                        {
                            if (Vector2.Distance(locPrev.Position, left.StartLocation) < left.AliveZoneSize)
                            {
                                leftTouch = locPrev;
                            }
                        }
                        else
                        {
                            leftTouch = locPrev;
                            left.StartLocation = leftTouch.Value.Position;

                            if (left.StartLocation.X < left.StickStartRegion.Left + left.DistFromScreenEdge)
                                left.StartLocation = new Vector2(left.StickStartRegion.Left + left.DistFromScreenEdge, left.StartLocation.Y);
                            if (left.StartLocation.Y > left.StickStartRegion.Bottom - left.DistFromScreenEdge)
                                left.StartLocation = new Vector2(left.StartLocation.X, left.StickStartRegion.Bottom - left.DistFromScreenEdge);
                        }
                        continue;
                    }
                }

                if (!right.Stick.HasValue && locPrev.Id != right.LastExcludedRightTouchId)
                {
                    // if we are not currently tracking a right thumbstick and this touch is on the right
                    // half of the screen, start tracking this touch as our right stick
                    if (right.StickStartRegion.Contains((int)locPrev.Position.X, (int)locPrev.Position.Y))
                    {
                        // Check if any of the excluded regions contain the point
                        bool excluded = false;
                        foreach (Rectangle r in right.RightStickStartExcludeRegions)
                        {
                            if (r.Contains((int)locPrev.Position.X, (int)locPrev.Position.Y))
                            {
                                excluded = true;
                                right.LastExcludedRightTouchId = locPrev.Id;
                                continue;
                            }
                        }

                        if (excluded)
                            continue;

                        right.LastExcludedRightTouchId = -1;

                        if (right.StickStyle == TouchStickStyle.Fixed)
                        {
                            if (Vector2.Distance(locPrev.Position, right.StartLocation) < right.AliveZoneSize)
                            {
                                rightTouch = locPrev;
                            }
                        }
                        else
                        {
                            rightTouch = locPrev;
                            right.StartLocation = rightTouch.Value.Position;

                            // Ensure touch is not too close to screen edge
                            if (right.StartLocation.X > right.StickStartRegion.Right - right.DistFromScreenEdge)
                                right.StartLocation = new Vector2(right.StickStartRegion.Right - right.DistFromScreenEdge, right.StartLocation.Y);
                            if (right.StartLocation.Y > right.StickStartRegion.Bottom - right.DistFromScreenEdge)
                                right.StartLocation = new Vector2(right.StartLocation.X, right.StickStartRegion.Bottom - right.DistFromScreenEdge);
                        }
                        continue;
                    }
                }
            }

            if (leftTouch.HasValue)
            {
                left.Stick = leftTouch;
                left.StickPos = leftTouch.Value.Position;
                left.EvaluatePoint(left.StickPos, dt);
            }
            else
            {
                bool foundNew = false;
                if (left.Stick.HasValue)
                {
                    // No left touch now but previously there was. Check to see if the TouchPanel decided
                    // to reset our touch id. Search for any touch within 10 pixel radius.
                    foreach (TouchLocation loc in state)
                    {
                        Vector2 pos = loc.Position;
                        var tmp = left.StickPos;
                        float distSqr; Vector2.DistanceSquared(ref pos, ref tmp, out distSqr);
                        if (distSqr < 100f)
                        {
                            foundNew = true;
                            left.Stick = loc;
                            left.StickPos = loc.Position;
                            left.EvaluatePoint(left.StickPos, dt);
                        }
                    }
                }

                if (!foundNew)
                {
                    left.Stick = null;
                    left.StickDirection = Vector2.Zero;
                    left.StickMagnitude = 0.0f;
                }
            }

            if (rightTouch.HasValue)
            {
                right.Stick = rightTouch;
                right.StickPos = rightTouch.Value.Position;
                right.EvaluatePoint(right.StickPos, dt);
            }
            else
            {
                bool foundNew = false;
                if (right.Stick.HasValue)
                {
                    // No right touch now but previously there was. Check to see if the TouchPanel decided
                    // to reset our touch id. Search for any touch within 10 pixel radius.
                    foreach (TouchLocation loc in state)
                    {
                        Vector2 pos = loc.Position;
                        var tmp = right.StickPos;
                        float distSqr; Vector2.DistanceSquared(ref pos, ref tmp, out distSqr);
                        if (distSqr < 100f)
                        {
                            foundNew = true;
                            right.Stick = loc;
                            right.StickPos = loc.Position;
                            right.EvaluatePoint(right.StickPos, dt);
                        }
                    }
                }

                // 
                if (!foundNew)
                {
                    right.Stick = null;
                    right.StickDirection = Vector2.Zero;
                    right.StickMagnitude = 0.0f;
                }
            }

        }
    }
}

//#endif