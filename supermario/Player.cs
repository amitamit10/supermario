using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
    public class Player
    {
        public int Health { get; set; }
        public int Score { get; set; }
        public bool IsGrounded { get; set; }
        public Point Position { get; set; }

        private float VerticalVelocity;
        private float HorizontalVelocity;

        // Improved physics constants
        private const float MoveSpeed = 6f;
        private const float MaxMoveSpeed = 8f;
        private const float Acceleration = 1.2f;
        private const float Deceleration = 0.8f;
        private const float AirControl = 0.6f; // Control while in air

        private const float Gravity = 0.6f;
        private const float JumpPower = -13f;
        private const float MaxFallSpeed = 15f;

        // Variable jump height (hold jump longer = jump higher)
        private const float JumpReleaseGravityMultiplier = 2f;
        private bool isJumpHeld = false;

        // Event to notify when player takes damage
        public event System.Action OnDamageTaken;

        public Player(Point startPosition, Image playerImage)
        {
            Health = 3;
            Score = 0;
            IsGrounded = false;
            Position = startPosition;
            VerticalVelocity = 0;
            HorizontalVelocity = 0;
        }

        public void Move(int direction, bool shouldJump)
        {
            // Horizontal movement with acceleration/deceleration
            float targetSpeed = direction * MoveSpeed;
            float accelRate = (IsGrounded ? Acceleration : Acceleration * AirControl);

            if (direction != 0)
            {
                // Accelerate towards target speed
                if (HorizontalVelocity < targetSpeed)
                {
                    HorizontalVelocity += accelRate;
                    if (HorizontalVelocity > targetSpeed) HorizontalVelocity = targetSpeed;
                }
                else if (HorizontalVelocity > targetSpeed)
                {
                    HorizontalVelocity -= accelRate;
                    if (HorizontalVelocity < targetSpeed) HorizontalVelocity = targetSpeed;
                }
            }
            else
            {
                // Decelerate when no input
                if (HorizontalVelocity > 0)
                {
                    HorizontalVelocity -= Deceleration;
                    if (HorizontalVelocity < 0) HorizontalVelocity = 0;
                }
                else if (HorizontalVelocity < 0)
                {
                    HorizontalVelocity += Deceleration;
                    if (HorizontalVelocity > 0) HorizontalVelocity = 0;
                }
            }

            // Cap horizontal speed
            if (HorizontalVelocity > MaxMoveSpeed) HorizontalVelocity = MaxMoveSpeed;
            if (HorizontalVelocity < -MaxMoveSpeed) HorizontalVelocity = -MaxMoveSpeed;

            // Apply horizontal movement
            int newX = Position.X + (int)HorizontalVelocity;
            if (newX >= 0 && newX <= 2950)
            {
                Position = new Point(newX, Position.Y);
            }

            // Jump logic with variable height
            if (shouldJump && IsGrounded)
            {
                Jump();
                isJumpHeld = true;
            }
            else if (!shouldJump && VerticalVelocity < 0)
            {
                // Released jump early - apply extra gravity for shorter jump
                isJumpHeld = false;
            }

            // Apply gravity
            if (!IsGrounded)
            {
                float gravityToApply = Gravity;

                // Apply extra gravity if jump released early (for variable jump height)
                if (!isJumpHeld && VerticalVelocity < 0)
                {
                    gravityToApply *= JumpReleaseGravityMultiplier;
                }

                VerticalVelocity += gravityToApply;

                // Cap fall speed
                if (VerticalVelocity > MaxFallSpeed)
                {
                    VerticalVelocity = MaxFallSpeed;
                }

                // Apply vertical velocity
                int newY = Position.Y + (int)VerticalVelocity;
                Position = new Point(Position.X, newY);
            }
            else
            {
                VerticalVelocity = 0;
            }

            // REMOVED: Hardcoded floor collision that was causing the bug
            // The mainWin.cs CheckPlatformCollisions() handles all collisions properly
        }

        public void Jump()
        {
            if (IsGrounded)
            {
                IsGrounded = false;
                VerticalVelocity = JumpPower;
                isJumpHeld = true;
            }
        }

        public void Update(float deltaTime)
        {
            // Reserved for future use
        }

        public void CollectCoin()
        {
            Score += 10;
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;

            // Notify listeners that damage was taken
            OnDamageTaken?.Invoke();
        }

        public void Respawn(Point startPosition)
        {
            Position = startPosition;
            Health = 3;
            VerticalVelocity = 0;
            HorizontalVelocity = 0;
            IsGrounded = false;
            isJumpHeld = false;
        }
    }
}