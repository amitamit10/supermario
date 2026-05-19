using System;
using System.Drawing;

namespace supermario
{
    public class Player
    {
        private Point position;
        private float preciseX;
        private float preciseY;
        private float horizontalVelocity;
        private bool isJumpHeld = false;

        private const float GroundMoveSpeed = 4.4f;
        private const float MaxMoveSpeed = 5.6f;
        private const float GroundAcceleration = 0.75f;
        private const float AirAcceleration = 0.42f;
        private const float GroundDeceleration = 0.55f;
        private const float AirDeceleration = 0.16f;

        private const float Gravity = 0.58f;
        private const float JumpPower = -13.8f;
        private const float MaxFallSpeed = 15.5f;
        private const float JumpReleaseGravityMultiplier = 2.4f;

        public int Health { get; set; }
        public int Score { get; set; }
        public bool IsGrounded { get; set; }
        public float MaxX { get; set; } = 2950;

        public Point Position
        {
            get => position;
            set
            {
                position = value;
                preciseX = value.X;
                preciseY = value.Y;
            }
        }

        public Point PreviousPosition { get; private set; }
        public float VerticalVelocity { get; private set; }
        public float HorizontalVelocity => horizontalVelocity;

        // Plain Action (not event) so it can be assigned with = to prevent stacking
        public System.Action OnDamageTaken;

        public Player(Point startPosition, System.Drawing.Image playerImage)
        {
            Health = 3;
            Score = 0;
            IsGrounded = false;
            position = startPosition;
            PreviousPosition = startPosition;
            preciseX = startPosition.X;
            preciseY = startPosition.Y;
            VerticalVelocity = 0;
            horizontalVelocity = 0;
        }

        public void Move(int direction, bool jumpPressed, bool jumpHeld)
        {
            PreviousPosition = Position;

            float targetSpeed = direction * GroundMoveSpeed;
            float accelRate = IsGrounded ? GroundAcceleration : AirAcceleration;
            float decelRate = IsGrounded ? GroundDeceleration : AirDeceleration;

            if (direction != 0)
            {
                horizontalVelocity = Approach(horizontalVelocity, targetSpeed, accelRate);
            }
            else
            {
                horizontalVelocity = Approach(horizontalVelocity, 0, decelRate);
            }

            horizontalVelocity = Clamp(horizontalVelocity, -MaxMoveSpeed, MaxMoveSpeed);
            float newX = preciseX + horizontalVelocity;
            if (newX < 0 || newX > MaxX) horizontalVelocity = 0;
            preciseX = Clamp(newX, 0, MaxX);

            if (jumpPressed && IsGrounded)
            {
                Jump();
            }

            isJumpHeld = jumpHeld;

            if (!IsGrounded)
            {
                float gravityToApply = Gravity;
                if (!isJumpHeld && VerticalVelocity < 0)
                    gravityToApply *= JumpReleaseGravityMultiplier;

                VerticalVelocity = Math.Min(VerticalVelocity + gravityToApply, MaxFallSpeed);
                preciseY += VerticalVelocity;
            }
            else
            {
                VerticalVelocity = 0;
            }

            position = new Point((int)Math.Round(preciseX), (int)Math.Round(preciseY));
        }

        public void Jump()
        {
            if (!IsGrounded) return;

            IsGrounded = false;
            VerticalVelocity = JumpPower;
            isJumpHeld = true;
        }

        public void LandOn(int topY, int playerHeight)
        {
            preciseY = topY - playerHeight;
            position = new Point((int)Math.Round(preciseX), (int)Math.Round(preciseY));
            VerticalVelocity = 0;
            IsGrounded = true;
            isJumpHeld = false;
        }

        public void HitCeiling(int bottomY)
        {
            preciseY = bottomY;
            position = new Point((int)Math.Round(preciseX), (int)Math.Round(preciseY));
            if (VerticalVelocity < 0) VerticalVelocity = 0;
        }

        public void BlockHorizontal(int edgeX)
        {
            preciseX = Clamp(edgeX, 0, MaxX);
            position = new Point((int)Math.Round(preciseX), (int)Math.Round(preciseY));
            horizontalVelocity = 0;
        }

        public void LeaveGround()
        {
            if (IsGrounded)
                IsGrounded = false;
        }

        public void CollectCoin() { Score += 10; }

        // Called when the player stomps an enemy – gives a short upward bounce
        public void Bounce()
        {
            VerticalVelocity = -7f;
            IsGrounded = false;
            isJumpHeld = false;
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
            OnDamageTaken?.Invoke();
        }

        public void Respawn(Point startPosition)
        {
            Position = startPosition;
            PreviousPosition = startPosition;
            Health = 3;
            VerticalVelocity = 0;
            horizontalVelocity = 0;
            IsGrounded = false;
            isJumpHeld = false;
        }

        private static float Approach(float value, float target, float amount)
        {
            if (value < target) return Math.Min(value + amount, target);
            if (value > target) return Math.Max(value - amount, target);
            return value;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
