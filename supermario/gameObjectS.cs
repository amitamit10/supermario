using System.Windows.Forms;
using System.Drawing;

namespace supermario
{
    class GameObjectS
    {
        public PictureBox PictureBox;
        public Point Position;
        public string Type;

        public Rectangle Bounds => PictureBox.Bounds;

        public GameObjectS(PictureBox pictureBox, Point position, string type)
        {
            PictureBox = pictureBox;
            Position = position;
            Type = type;

            PictureBox.Location = position;
        }
    }
}
