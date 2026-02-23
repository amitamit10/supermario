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

        private const float MoveSpeed = 3.5f;
        private const float MaxMoveSpeed = 4.5f;
        private const float Acceleration = 0.8f;
        private const float Deceleration = 0.6f;
        private const float AirControl = 0.6f;

        private const float Gravity = 0.6f;
        private const float JumpPower = -13f;
        private const float MaxFallSpeed = 15f;

        private const float JumpReleaseGravityMultiplier = 2f;
        private bool isJumpHeld = false;

        // Event fired when damage is taken
        public event System.Action OnDamageTaken;

        public Player(Point startPosition, System.Drawing.Image playerImage)
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
            float accelRate = IsGrounded ? Acceleration : Acceleration * AirControl;

            if (direction != 0)
            {
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

            if (HorizontalVelocity > MaxMoveSpeed) HorizontalVelocity = MaxMoveSpeed;
            if (HorizontalVelocity < -MaxMoveSpeed) HorizontalVelocity = -MaxMoveSpeed;

            int newX = Position.X + (int)HorizontalVelocity;
            if (newX >= 0 && newX <= 2950)
                Position = new Point(newX, Position.Y);

            // Jump
            if (shouldJump && IsGrounded)
            {
                Jump();
                isJumpHeld = true;
            }
            else if (!shouldJump && VerticalVelocity < 0)
            {
                isJumpHeld = false;
            }

            // Gravity
            if (!IsGrounded)
            {
                float gravityToApply = Gravity;
                if (!isJumpHeld && VerticalVelocity < 0)
                    gravityToApply *= JumpReleaseGravityMultiplier;

                VerticalVelocity += gravityToApply;
                if (VerticalVelocity > MaxFallSpeed) VerticalVelocity = MaxFallSpeed;

                int newY = Position.Y + (int)VerticalVelocity;
                Position = new Point(Position.X, newY);
            }
            else
            {
                VerticalVelocity = 0;
            }
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

        public void Update(float deltaTime) { }

        public void CollectCoin() { Score += 10; }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
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