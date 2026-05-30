using System;
using System.Drawing;

namespace supermario
{
    /// ════════════════════════════════════════════════════════════════════
    ///  Player — מצב ופיזיקה של השחקן / player state & physics
    /// --------------------------------------------------------------------
    ///  פיזיקה פשוטה ובסיסית: תנועה אופקית במהירות קבועה, כבידה קבועה,
    ///  וקפיצה שמציבה מהירות אנכית. (אין האצה/האטה מדורגת.)
    ///  Simple, basic physics: constant horizontal speed, constant gravity,
    ///  and a jump that sets the vertical velocity. (No acceleration ramps.)
    /// ════════════════════════════════════════════════════════════════════
    public class Player
    {
        // ── מיקום מדויק (float) כדי שהתנועה תהיה חלקה / sub-pixel position ──
        private Point position;
        private float preciseX;
        private float preciseY;
        private float horizontalVelocity;

        // ── קבועי פיזיקה / physics constants ─────────────────────────────
        private const float MoveSpeed = 5f;          // מהירות הליכה / walk speed
        private const float Gravity = 0.6f;          // כבידה לפריים / gravity per frame
        private const float JumpPower = -13f;        // עוצמת קפיצה (שלילי = מעלה) / jump strength
        private const float MaxFallSpeed = 15f;      // מהירות נפילה מרבית / terminal fall speed

        // ── מצב גלוי / public state ──────────────────────────────────────
        public int Health { get; set; }
        public int Score { get; set; }
        public bool IsGrounded { get; set; }
        public float MaxX { get; set; } = 2950;

        public Point Position
        {
            get => position;
            set { position = value; preciseX = value.X; preciseY = value.Y; }
        }

        public Point PreviousPosition { get; private set; }
        public float VerticalVelocity { get; private set; }
        public float HorizontalVelocity => horizontalVelocity;

        // Action רגיל (לא event) כדי שניתן להציב עם = ולא לערום מאזינים
        // a plain Action (not an event) so it can be assigned with = without stacking
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

        // ════════════════════════════════════════════════════════════════
        //  תנועה / Movement  (נקרא פעם בכל פריים / called once per frame)
        // ════════════════════════════════════════════════════════════════
        public void Move(int direction, bool jumpPressed, bool jumpHeld)
        {
            PreviousPosition = Position;

            // תנועה אופקית במהירות קבועה / constant-speed horizontal movement
            horizontalVelocity = direction * MoveSpeed;
            preciseX = Clamp(preciseX + horizontalVelocity, 0, MaxX);

            // קפיצה רק כשעל הקרקע / jump only when grounded
            if (jumpPressed && IsGrounded) Jump();

            // כבידה / gravity
            if (!IsGrounded)
            {
                VerticalVelocity = Math.Min(VerticalVelocity + Gravity, MaxFallSpeed);
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
        }

        // ════════════════════════════════════════════════════════════════
        //  תגובות התנגשות / Collision responses
        // ════════════════════════════════════════════════════════════════
        public void LandOn(int topY, int playerHeight)
        {
            preciseY = topY - playerHeight;
            position = new Point((int)Math.Round(preciseX), (int)Math.Round(preciseY));
            VerticalVelocity = 0;
            IsGrounded = true;
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
            if (IsGrounded) IsGrounded = false;
        }

        public void CollectCoin() { Score += 10; }

        // קפיצונת קטנה אחרי דריכה על אויב / small bounce after stomping an enemy
        public void Bounce()
        {
            VerticalVelocity = -7f;
            IsGrounded = false;
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
        }

        // הגבלת ערך לטווח / clamp a value to a range
        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
