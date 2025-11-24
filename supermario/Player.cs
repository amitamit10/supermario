using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
    public class Player 
    {
        public int Health { get; private set; }
        public int Score { get; private set; }
        public bool IsGrounded { get; private set; }
        public Point Position { get; private set; }

        private float VerticalVelocity;
        private const int MoveSpeed = 5;
        private const float Gravity = 1.0f;
        private const int GroundLevelY = 400;

        public Player(Point startPosition, Image playerImage)
           
        {
            Health = 1;
            Score = 0;
            IsGrounded = true;
            VerticalVelocity = 0;
        }

        public void Move(int direction)
        {
            int newX = Position.X + (direction * MoveSpeed);

            Position = new Point(newX, Position.Y);
        }

        public void Jump()
        {
            if (IsGrounded)
            {
                IsGrounded = false;
                VerticalVelocity = -1;
            }
        }

        public  void Update(float deltaTime)
        {
            
        }

        public void CollectCoin()
        {
            Score += 10;
        }

        public  void TakeDamage(int amount)
        {
            Health -= amount;
        }

        public void Respawn(Point startPosition)
        {
           
        }
    }
}