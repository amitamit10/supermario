using System;
using System.Collections.Generic;
using System.Drawing;

namespace supermario.ML
{
    // One Luigi AI agent — owns its own neural network and physics state.
    public class MarioAgent
    {
        private const float Gravity      = 0.58f;
        private const float JumpPower    = -13.8f;
        private const float MaxFallSpeed = 15.5f;
        private const float MoveSpeed    = 4.4f;
        private const float MaxMoveSpeed = 5.6f;
        private const float GroundAccel  = 0.75f;
        private const float AirAccel     = 0.42f;
        private const float GroundDecel  = 0.55f;
        private const float AirDecel     = 0.16f;

        public NeuralNetwork  Brain          { get; }
        public bool           IsAlive        { get; set; } = true;
        public int            Fitness        { get; set; }   // best X reached (world space)
        public int            Score          { get; set; }   // coin bonus (each coin = 50)
        public int            TotalFitness   => Fitness + Score;
        public HashSet<int>   CollectedCoins { get; } = new HashSet<int>();
        public Point          Position       { get; set; }
        public bool           IsGrounded     { get; set; }

        public float VerticalVelocity   { get; private set; }
        public float HorizontalVelocity { get; private set; }

        private float preciseX, preciseY;
        private bool  jumpHeld;
        private int   stuckTimer;           // counts frames with almost no X progress
        private int   lastX;

        public MarioAgent(NeuralNetwork brain, Point startPos)
        {
            Brain    = brain;
            Position = startPos;
            preciseX = startPos.X;
            preciseY = startPos.Y;
            lastX    = startPos.X;
        }

        // ── Physics step (mirrors Player.Move) ──────────────────────────────────
        public void Step(int directionInput, bool jumpInput)
        {
            float targetSpeed = directionInput * MoveSpeed;
            float accel = IsGrounded ? GroundAccel : AirAccel;
            float decel = IsGrounded ? GroundDecel : AirDecel;

            if (directionInput != 0)
                HorizontalVelocity = Approach(HorizontalVelocity, targetSpeed, accel);
            else
                HorizontalVelocity = Approach(HorizontalVelocity, 0, decel);

            HorizontalVelocity = Clamp(HorizontalVelocity, -MaxMoveSpeed, MaxMoveSpeed);
            preciseX = Clamp(preciseX + HorizontalVelocity, 0, 2950);

            if (jumpInput && IsGrounded)
            {
                IsGrounded       = false;
                VerticalVelocity = JumpPower;
            }
            jumpHeld = jumpInput;

            if (!IsGrounded)
            {
                float g = Gravity;
                if (!jumpHeld && VerticalVelocity < 0) g *= 2.4f;
                VerticalVelocity = Math.Min(VerticalVelocity + g, MaxFallSpeed);
                preciseY += VerticalVelocity;
            }
            else
            {
                VerticalVelocity = 0;
            }

            Position = new Point((int)Math.Round(preciseX), (int)Math.Round(preciseY));

            // Track fitness = rightmost point ever reached
            if (Position.X > Fitness) Fitness = Position.X;

            // Stuck detection: if forward progress over the last window is below this
            // threshold, kill the agent. The previous 8 px / 120 frames allowed agents
            // crawling at <2% of full speed to survive forever, slowing training.
            stuckTimer++;
            if (stuckTimer >= 120)
            {
                if (Position.X - lastX < 60) IsAlive = false;
                lastX      = Position.X;
                stuckTimer = 0;
            }
        }

        // ── Collision helpers (called by TrainingForm after Step) ────────────────
        public void LandOn(int topY, int agentHeight)
        {
            preciseY         = topY - agentHeight;
            Position         = new Point((int)Math.Round(preciseX), (int)Math.Round(preciseY));
            VerticalVelocity = 0;
            IsGrounded       = true;
            jumpHeld         = false;
        }

        public void HitCeiling(int bottomY)
        {
            preciseY = bottomY;
            Position = new Point((int)Math.Round(preciseX), (int)Math.Round(preciseY));
            if (VerticalVelocity < 0) VerticalVelocity = 0;
        }

        public void BlockHorizontal(int edgeX)
        {
            preciseX           = Clamp(edgeX, 0, 2950);
            Position           = new Point((int)Math.Round(preciseX), (int)Math.Round(preciseY));
            HorizontalVelocity = 0;
        }

        public void LeaveGround() { IsGrounded = false; }

        // ── Neural inference ─────────────────────────────────────────────────────
        // Returns (direction: -1/0/+1, jump: bool)
        public (int dir, bool jump) Think(double[] inputs)
        {
            double[] outputs = Brain.Forward(inputs);
            // outputs[0] in tanh range [-1,1]: negative=left, positive=right
            int  dir  = outputs[0] > 0.33 ? 1 : (outputs[0] < -0.33 ? -1 : 0);
            bool jump = outputs[1] > 0.5;
            return (dir, jump);
        }

        // ── Inputs: 4 normalised values ──────────────────────────────────────────
        // Caller fills this in with level knowledge. Kept as a static helper so the
        // TrainingForm can call it without duplicating the calculation.
        public static double[] ComputeInputs(
            Point agentPos, int agentW, int agentH,
            IList<Rectangle> platformRects,
            IList<Rectangle> enemyRects,
            bool isGrounded)
        {
            // 1. Gap distance: how far ahead until there is no floor platform below feet.
            // Start probing just past the agent's right edge; cap the start so very wide
            // agents still run the loop.
            double gapDist = 1.0;
            int footY = agentPos.Y + agentH;
            int probeStart = Math.Min(agentW, 290);
            for (int dx = probeStart; dx <= 300; dx += 10)
            {
                int checkX = agentPos.X + dx;
                bool hasFloor = false;
                foreach (var r in platformRects)
                {
                    if (checkX >= r.Left && checkX <= r.Right && footY <= r.Top + 12 && footY >= r.Top - 40)
                    { hasFloor = true; break; }
                }
                if (!hasFloor) { gapDist = dx / 300.0; break; }
            }

            // 2. Enemy distance: nearest enemy ahead, measured from the agent's right edge.
            double enemyDist = 1.0;
            int agentRight = agentPos.X + agentW;
            foreach (var e in enemyRects)
            {
                int dx = e.Left - agentRight;
                if (dx >= 0 && dx < 300)
                    enemyDist = Math.Min(enemyDist, dx / 300.0);
            }

            // 3. Platform height diff ahead (normalized to ±1) – search a window in
            // front of the agent's right edge.
            double heightDiff = 0.0;
            int lookAheadX = agentRight + 40;
            double bestDist = double.MaxValue;
            foreach (var r in platformRects)
            {
                if (r.Left > agentRight && r.Left < lookAheadX + 200)
                {
                    double d = r.Left - agentRight;
                    if (d < bestDist) { bestDist = d; heightDiff = (agentPos.Y - r.Top) / 200.0; }
                }
            }
            heightDiff = Math.Max(-1.0, Math.Min(1.0, heightDiff));

            // 4. Is grounded
            double groundedVal = isGrounded ? 1.0 : 0.0;

            return new[] { gapDist, enemyDist, heightDiff, groundedVal };
        }

        public MarioAgent Clone() => new MarioAgent(Brain.Clone(), Position);

        private static float Approach(float v, float t, float a)
        {
            if (v < t) return Math.Min(v + a, t);
            if (v > t) return Math.Max(v - a, t);
            return v;
        }
        private static float Clamp(float v, float min, float max)
            => v < min ? min : (v > max ? max : v);
    }
}
