using System.Windows.Forms;
using System.Drawing;

namespace supermario
{
    class GameObjectS
    {
        public PictureBox PictureBox { get; private set; }
        public Point Position { get; private set; }
        public string Type { get; private set; }

        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, PictureBox.Width, PictureBox.Height);

        public GameObjectS(PictureBox pictureBox, Point position, string type)
        {
            PictureBox = pictureBox;
            Position = position;
            Type = type;

            PictureBox.Location = position;
        }

        public void UpdatePosition(Point newPosition)
        {
            Position = newPosition;
            PictureBox.Location = newPosition;
        }

        public void Move(int deltaX, int deltaY)
        {
            Point newPosition = new Point(Position.X + deltaX, Position.Y + deltaY);
            UpdatePosition(newPosition);
        }
    }
}