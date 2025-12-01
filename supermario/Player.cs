using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
public class Player
{
public int Health { get; set; }
public int Score { get; set; }
public Point Position { get; set; }
public bool IsGrounded { get; set; }

private float VerticalVelocity;
private const int MoveSpeed = 5;
private const float Gravity = 1.0f;
private const int JumpStrength = 15;
private const int GroundLevelY = 400;
private Point StartPosition;

public Player(Point startPosition, Image playerImage)
{
    Health = 1;
    Score = 0;
    IsGrounded = true;
    VerticalVelocity = 0;
    Position = startPosition;
    StartPosition = startPosition;
}

public void Move(int direction, bool shouldJump)
{
    int newX = Position.X + (direction * MoveSpeed);
    Position = new Point(newX, Position.Y);

    if (shouldJump && IsGrounded)
    {
        IsGrounded = false;
        VerticalVelocity = -JumpStrength;
    }
}

public void SetCoin(int coinValue)
{
    Score += coinValue;
}

public void SetDamage(int damageAmount)
{
    Health -= damageAmount;
}

public void Respawn()
{
    Position = StartPosition;
    Health = 1;
    IsGrounded = true;
    VerticalVelocity = 0;
}
}
}