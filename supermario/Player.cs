using System.Drawing;

namespace supermario
{
    public enum MarioState { Small, Super }

    public class Player
    {
        // ?? State ??????????????????????????????????????????????????????????????
        public int Lives { get; set; }
        public MarioState State { get; private set; }
        public bool IsGrounded { get; set; }
        public Point Position { get; set; }
        public bool IsInvincible { get; private set; }

        private float invincibilityTimer = 0f;
        private const float INVINCIBILITY_DURATION = 2000f; // 2 s of flicker after shrink

        // ?? Movement physics ??????????????????????????????????????????????????
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

        // ?? Events ????????????????????????????????????????????????????????????
        /// <summary>Fired when Super Mario is hit ? he shrinks. No life lost.</summary>
        public event System.Action OnBecameSmall;

        /// <summary>Fired when Small Mario is hit, OR Mario falls into a pit.
        /// A life has already been deducted when this fires.</summary>
        public event System.Action OnDied;

        // ?????????????????????????????????????????????????????????????????????
        public Player(Point startPosition, System.Drawing.Image playerImage)
        {
            Lives = 3;
            State = MarioState.Small;
            IsGrounded = false;
            Position = startPosition;
            VerticalVelocity = 0;
            HorizontalVelocity = 0;
            IsInvincible = false;
        }

        // ?? Damage / death ????????????????????????????????????????????????????

        /// <summary>Call when an enemy or hazard touches Mario (not a pit).</summary>
        public void TakeDamage()
        {
            if (IsInvincible) return;

            if (State == MarioState.Super)
            {
                // Hit while Super ? shrink, start invincibility, no life lost
                State = MarioState.Small;
                StartInvincibility();
                OnBecameSmall?.Invoke();
            }
            else
            {
                // Hit while Small ? lose a life
                Lives--;
                if (Lives < 0) Lives = 0;
                OnDied?.Invoke();
            }
        }

        /// <summary>Call when Mario falls off the bottom of the screen (pit).
        /// Always costs a life regardless of Super/Small state.</summary>
        public void FallIntoPit()
        {
            if (IsInvincible) return; // already dying
            Lives--;
            if (Lives < 0) Lives = 0;
            OnDied?.Invoke();
        }

        // ?? Power-ups ?????????????????????????????????????????????????????????

        /// <summary>Hit a ? block ? collect mushroom.</summary>
        public void CollectMushroom()
        {
            if (State == MarioState.Small)
            {
                State = MarioState.Super;
                // Visual resize is handled by mainWin via OnBecameSmall/state check
            }
            // Already Super: original SMB gives points; we just deactivate the block.
        }

        // ?? Invincibility ?????????????????????????????????????????????????????
        public void StartInvincibility()
        {
            IsInvincible = true;
            invincibilityTimer = 0f;
        }

        /// <summary>Advance invincibility countdown. Call once per fixed physics step.</summary>
        public void UpdateInvincibility(long stepMs)
        {
            if (!IsInvincible) return;
            invincibilityTimer += stepMs;
            if (invincibilityTimer >= INVINCIBILITY_DURATION)
            {
                IsInvincible = false;
                invincibilityTimer = 0f;
            }
        }

        /// <summary>Returns false during the "flicker" frames of invincibility.</summary>
        public bool IsVisible()
        {
            if (!IsInvincible) return true;
            // Toggle visibility every 100 ms for a classic flicker effect
            return ((int)(invincibilityTimer / 100)) % 2 == 0;
        }

        // ?? Movement ??????????????????????????????????????????????????????????
        public void Move(int direction, bool shouldJump)
        {
            // Horizontal movement with acceleration / deceleration
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

        /// <summary>
        /// Called when Mario successfully stomps a Goomba.
        /// Gives a small upward kick, just like the original SMB.
        /// </summary>
        public void StompBounce()
        {
            IsGrounded = false;
            VerticalVelocity = -9f;
            isJumpHeld = false;
        }

        public void Update(float deltaTime) { }

        public void CollectCoin() { /* hook for score later */ }

        public void Respawn(Point startPosition)
        {
            Position = startPosition;
            State = MarioState.Small;
            VerticalVelocity = 0;
            HorizontalVelocity = 0;
            IsGrounded = false;
            isJumpHeld = false;
            IsInvincible = false;
            invincibilityTimer = 0f;
        }
    }
}