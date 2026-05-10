# Super Mario - Pixel Art Sprite Sheet Integration Guide

This guide details the expanded 2D Pixel Art Texture Pack and Sprite Sheets generated for the Super Mario WinForms project. It provides explicit instructions for **Claude Code** on how to integrate these multi-frame sprite sheets efficiently into the C# codebase using `Graphics.DrawImage` slicing.

## 1. The New Sprite Sheet Texture Pack

All newly generated multi-frame assets are organized inside `assets/textures/sprite_sheets/`. These assets are scaled 4x (where a standard 16x16 tile is physically 64x64 pixels).

### Sprite Sheet Definitions

**1. `player_sheet.png`**
- **Dimensions**: 256 x 64 pixels (4 frames side-by-side).
- **Frames**: 
  - `0`: Idle
  - `1`: Walk Frame 1
  - `2`: Walk Frame 2
  - `3`: Jump

**2. `enemies_sheet.png`**
- **Dimensions**: 256 x 96 pixels (4 frames side-by-side, 64x96 per frame).
- **Frames**:
  - `0`: Goomba Walk 1
  - `1`: Goomba Walk 2
  - `2`: Koopa Walk 1
  - `3`: Koopa Walk 2

**3. `items_sheet.png`**
- **Dimensions**: 320 x 64 pixels (5 frames side-by-side).
- **Frames**:
  - `0 - 3`: Coin Spin Cycle (Full Face, Angle, Edge, Angle)
  - `4`: Mushroom Power-Up

**4. `blocks_sheet.png`**
- **Dimensions**: 320 x 64 pixels (5 frames side-by-side).
- **Frames**:
  - `0 - 1`: Question Block Shimmer Cycle
  - `2`: Empty/Hit Block
  - `3`: Breakable Brick

**5. `world_bg.png`**
- **Dimensions**: 256 x 128 pixels.
- **Usage**: A wide panoramic background tile featuring classic sky, clouds, and green hills. Designed to be drawn repeatedly across the screen width.

---

## 2. Integration Instructions for Claude Code

### Step 1: Centralized Texture Loading
Instead of loading thousands of individual images, load the sprite sheets **once** into memory.

```csharp
using System.Drawing;
using System.Collections.Generic;
using System.IO;

public static class TextureLoader
{
    public static Dictionary<string, Image> Sheets = new Dictionary<string, Image>();

    public static void LoadAll()
    {
        string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "assets", "textures", "sprite_sheets");
        Sheets["player"] = Image.FromFile(Path.Combine(path, "player_sheet.png"));
        Sheets["enemies"] = Image.FromFile(Path.Combine(path, "enemies_sheet.png"));
        Sheets["items"] = Image.FromFile(Path.Combine(path, "items_sheet.png"));
        Sheets["blocks"] = Image.FromFile(Path.Combine(path, "blocks_sheet.png"));
        Sheets["bg"] = Image.FromFile(Path.Combine(path, "world_bg.png"));
    }
}
```

### Step 2: Sprite Slicing Extension Method
To extract a specific frame from a sheet at render-time without memory overhead, use `Graphics.DrawImage` with source and destination rectangles. Claude should implement a helper method:

```csharp
public static void DrawFrame(this Graphics g, Image sheet, int frameIndex, int frameWidth, int frameHeight, Rectangle destRect)
{
    Rectangle srcRect = new Rectangle(frameIndex * frameWidth, 0, frameWidth, frameHeight);
    g.DrawImage(sheet, destRect, srcRect, GraphicsUnit.Pixel);
}
```

### Step 3: Animation State Machines
In `mainWin.cs`, Claude needs to establish a global animation tick counter that increments inside the `GameTimer_Tick` event.

```csharp
private int globalTick = 0; // Increments every timer tick

private void GameTimer_Tick(object sender, EventArgs e) {
    globalTick++;
    // Physics...
    Invalidate();
}
```

**Player Animation Logic Example:**
```csharp
int frameWidth = 64; // Since assets are scaled 4x (16 * 4)
int frameHeight = 64;
int frameIndex = 0; // Default Idle

if (player.IsJumping) {
    frameIndex = 3;
} else if (player.Velocity.X != 0) {
    // Alternate between Walk 1 (index 1) and Walk 2 (index 2) every 10 ticks
    frameIndex = (globalTick / 10 % 2 == 0) ? 1 : 2;
}

e.Graphics.DrawFrame(TextureLoader.Sheets["player"], frameIndex, frameWidth, frameHeight, player.Bounds);
```

**Enemy Animation Logic Example:**
```csharp
// Goomba alternates between frame 0 and 1
int goombaFrame = (globalTick / 15 % 2 == 0) ? 0 : 1;
e.Graphics.DrawFrame(TextureLoader.Sheets["enemies"], goombaFrame, 64, 96, enemy.Bounds);
```

### Step 4: Background Parallax or Tiling
To render the `world_bg.png`, Claude can simply tile it across the screen in the `OnPaint` event *before* drawing game objects:

```csharp
Image bg = TextureLoader.Sheets["bg"];
for (int x = 0; x < this.ClientSize.Width; x += bg.Width) {
    e.Graphics.DrawImage(bg, new Rectangle(x, 0, bg.Width, bg.Height));
}
```

By strictly implementing this sprite sheet architecture, the codebase remains remarkably clean, memory is highly optimized, and the frame-by-frame classic retro identity is perfectly preserved!
