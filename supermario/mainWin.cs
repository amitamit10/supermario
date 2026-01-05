using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace supermario
{
    public partial class mainWin : Form
    {
        // Game components
        private Player player;
        private GameManager gameManager;
        private Timer gameTimer;
        private List<GameObjectS> platforms;

        // Player control flags
        private bool moveRight = false;
        private bool moveLeft = false;
        private bool jump = false;

        // Camera/scroll offset
        private int cameraX = 0;
        private const int SCROLL_THRESHOLD = 400; // Start scrolling when player reaches this X position

        // NOTE: picboxplayer is defined in mainWin.Designer.cs

        public mainWin()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Set up form properties
            this.KeyPreview = true;
            this.DoubleBuffered = true;
            this.Focus();

            // Initialize game manager
            gameManager = new GameManager();

            // Make sure picboxplayer is in front
            if (picboxplayer != null)
            {
                picboxplayer.BringToFront();

                // Set player sprite - try resource first, fallback to red square
                try
                {
                    picboxplayer.Image = Properties.Resources.הורדה;
                    picboxplayer.BackColor = Color.Transparent;
                }
                catch
                {
                    picboxplayer.BackColor = Color.Red; // Fallback - visible red square
                }

                picboxplayer.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            // Initialize player at starting position ON THE GROUND
            // Ground is at Y=500, player height=50, so spawn at Y=450
            player = new Player(new Point(100, 450), null);
            player.IsGrounded = true;
            picboxplayer.Location = player.Position;

            // Initialize collections
            platforms = new List<GameObjectS>();

            // Create long level
            CreateLongLevel();

            // Set up game timer
            gameTimer = new Timer();
            gameTimer.Interval = 20; // ~50 FPS
            gameTimer.Tick += GameLoop;

            // Hook up keyboard events
            this.KeyDown += MainWin_KeyDown;
            this.KeyUp += MainWin_KeyUp;
        }

        private void CreateLongLevel()
        {
            // Ground platform - extra long with brick texture
            PictureBox ground = new PictureBox();
            ground.Size = new Size(3000, 50);
            ground.Location = new Point(0, 500);
            ground.SizeMode = PictureBoxSizeMode.StretchImage;
            try
            {
                ground.Image = Properties.Resources.oie_cnoULepStRjX; // Brick texture
            }
            catch
            {
                ground.BackColor = Color.Brown; // Fallback
            }
            this.Controls.Add(ground);
            platforms.Add(new GameObjectS(ground, ground.Location, "platform"));

            // Create a long series of platforms with textures
            // Section 1: Starting stairs
            AddPlatform(200, 450, 120, 20);
            AddPlatform(350, 400, 120, 20);
            AddPlatform(500, 350, 120, 20);

            // Section 2: Gap jumps
            AddPlatform(700, 400, 100, 20);
            AddPlatform(870, 400, 100, 20);
            AddPlatform(1040, 400, 100, 20);

            // Section 3: Up and down
            AddPlatform(1200, 380, 100, 20);
            AddPlatform(1350, 320, 100, 20);
            AddPlatform(1500, 380, 100, 20);

            // Section 4: High platforms
            AddPlatform(1650, 300, 100, 20);
            AddPlatform(1800, 250, 100, 20);
            AddPlatform(1950, 300, 100, 20);

            // Section 5: Final stretch
            AddPlatform(2100, 400, 120, 20);
            AddPlatform(2270, 400, 120, 20);
            AddPlatform(2440, 400, 120, 20);

            // Finish platform
            AddPlatform(2650, 350, 200, 20);

            // Victory marker at the end
            PictureBox finishLine = new PictureBox();
            finishLine.Size = new Size(50, 150);
            finishLine.Location = new Point(2750, 350);
            finishLine.BackColor = Color.Gold;
            this.Controls.Add(finishLine);
            platforms.Add(new GameObjectS(finishLine, finishLine.Location, "finish"));
        }

        private void AddPlatform(int x, int y, int width, int height)
        {
            PictureBox platform = new PictureBox();
            platform.Size = new Size(width, height);
            platform.Location = new Point(x, y);
            platform.SizeMode = PictureBoxSizeMode.StretchImage;

            // Try to use brick texture, fallback to green
            try
            {
                platform.Image = Properties.Resources.oie_cnoULepStRjX; // Brick texture
            }
            catch
            {
                platform.BackColor = Color.Green; // Fallback
            }

            this.Controls.Add(platform);
            platforms.Add(new GameObjectS(platform, platform.Location, "platform"));
        }

        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D:
                    moveRight = true;
                    break;
                case Keys.A:
                    moveLeft = true;
                    break;
                case Keys.W:
                case Keys.Space:
                    jump = true;
                    break;
                case Keys.Enter:
                    if (!gameManager.IsGameRunning)
                    {
                        gameManager.StartGame();
                        gameTimer.Start();
                        this.Text = "Super Mario - GAME RUNNING";
                    }
                    break;
                case Keys.Escape:
                    gameTimer.Stop();
                    gameManager.EndGame();
                    this.Text = "Super Mario - PAUSED";
                    break;
            }
        }

        private void MainWin_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D:
                    moveRight = false;
                    break;
                case Keys.A:
                    moveLeft = false;
                    break;
                case Keys.W:
                case Keys.Space:
                    jump = false;
                    break;
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameManager.IsGameRunning)
                return;

            // Process input
            int direction = 0;
            if (moveRight) direction = 1;
            if (moveLeft) direction = -1;

            // Update player
            player.Move(direction, jump);

            // Check collisions with platforms
            CheckPlatformCollisions();

            // Update camera scrolling
            UpdateCamera();

            // Check if player reached finish line
            if (player.Position.X >= 2750)
            {
                gameTimer.Stop();
                MessageBox.Show("You Win! Level Complete!", "Victory!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RestartLevel();
                return;
            }

            // Check if player fell off
            if (player.Position.Y > 600)
            {
                RestartLevel();
                return;
            }

            // Debug info
            this.Text = $"Super Mario - Pos:({player.Position.X},{player.Position.Y}) Camera:{cameraX}";
        }

        private void UpdateCamera()
        {
            // Calculate where player should be on screen
            int playerScreenX = player.Position.X - cameraX;

            // If player goes past the scroll threshold, move camera
            if (playerScreenX > SCROLL_THRESHOLD && player.Position.X > SCROLL_THRESHOLD)
            {
                // Calculate new camera position
                int newCameraX = player.Position.X - SCROLL_THRESHOLD;

                // Don't scroll past the end
                if (newCameraX > 2000) // Max scroll
                    newCameraX = 2000;

                // Calculate scroll amount
                int scrollAmount = newCameraX - cameraX;
                cameraX = newCameraX;

                // Move all platforms left by scroll amount
                foreach (var platform in platforms)
                {
                    platform.PictureBox.Left -= scrollAmount;
                }
            }
            // If player goes back past left threshold, scroll camera back
            else if (playerScreenX < 200 && cameraX > 0)
            {
                int newCameraX = player.Position.X - 200;
                if (newCameraX < 0) newCameraX = 0;

                int scrollAmount = newCameraX - cameraX;
                cameraX = newCameraX;

                foreach (var platform in platforms)
                {
                    platform.PictureBox.Left -= scrollAmount;
                }
            }

            // Update player position on screen
            picboxplayer.Location = new Point(player.Position.X - cameraX, player.Position.Y);

            // Make sure player stays on top
            picboxplayer.BringToFront();
        }

        private void CheckPlatformCollisions()
        {
            // Player's world position and size
            Rectangle playerRect = new Rectangle(player.Position.X, player.Position.Y,
                                                 picboxplayer.Width, picboxplayer.Height);

            bool foundGround = false;

            foreach (var platform in platforms)
            {
                // Get platform's world position (not screen position)
                Rectangle platformWorldRect = new Rectangle(
                    platform.Position.X,
                    platform.Position.Y,
                    platform.PictureBox.Width,
                    platform.PictureBox.Height
                );

                // Check if player intersects with platform
                if (playerRect.IntersectsWith(platformWorldRect))
                {
                    // Calculate overlap on each side
                    int overlapLeft = (playerRect.Right - platformWorldRect.Left);
                    int overlapRight = (platformWorldRect.Right - playerRect.Left);
                    int overlapTop = (playerRect.Bottom - platformWorldRect.Top);
                    int overlapBottom = (platformWorldRect.Bottom - playerRect.Top);

                    // Find minimum overlap (the side we hit)
                    int minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight),
                                             Math.Min(overlapTop, overlapBottom));

                    // Landing on top of platform
                    if (minOverlap == overlapTop && overlapTop < 20)
                    {
                        player.Position = new Point(player.Position.X, platformWorldRect.Top - picboxplayer.Height);
                        player.IsGrounded = true;
                        foundGround = true;
                        break;
                    }
                }
            }

            // If no platform collision found, player is in air
            if (!foundGround)
            {
                player.IsGrounded = false;
            }
        }

        private void RestartLevel()
        {
            gameManager.ResetGame();
            cameraX = 0;

            // Reset player ON THE GROUND (Y=450)
            player.Respawn(new Point(100, 450));
            player.IsGrounded = true;
            picboxplayer.Location = player.Position;

            // Reset all platforms to original positions
            ClearPlatforms();
            CreateLongLevel();

            gameManager.StartGame();
            gameTimer.Start();
        }

        private void ClearPlatforms()
        {
            foreach (var platform in platforms)
            {
                this.Controls.Remove(platform.PictureBox);
                platform.PictureBox.Dispose();
            }
            platforms.Clear();
        }

        private void mainWin_Load(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Controls:\n" +
                "W/Space - Jump\n" +
                "A - Move Left\n" +
                "D - Move Right\n\n" +
                "⚠️ Press ENTER to START!\n\n" +
                "Reach the GOLD block at the end to win!",
                "Super Mario - Long Level",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            this.Text = "Super Mario - Press ENTER to Start";
        }
    }
}