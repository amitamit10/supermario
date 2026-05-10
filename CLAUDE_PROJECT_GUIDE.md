# Super Mario - Pixel Art Integration Guide

This guide details the complete 2D Pixel Art Texture Pack generated for the Super Mario WinForms project, and provides explicit instructions for **Claude Code** on how to integrate these graphics efficiently into the existing C# codebase.

## 1. The New Pixel Art Texture Pack

The `feature/texture-expansion` branch contains authentic, procedurally generated 16x16 pixel-art assets scaled up 4x for modern displays.

### Asset Map
All assets are located relative to the `assets/textures/` root directory:

**Player Sprites (`player/`)**
- `player_idle.png` - Default standing pose.
- `player_walk_1.png` - Walk animation frame.

**Enemy Sprites (`enemies/`)**
- `goomba_walk_1.png` - Goomba basic sprite.
- `koopa_walk_1.png` - Koopa basic sprite.

**Terrain & Blocks (`terrain_blocks/`)**
- `ground.png` - Standard grass/dirt ground tile.
- `brick.png` - Breakable brick block.
- `question_1.png` - Question mark block frame.

**Pipes (`pipes/`)**
- `pipe_top.png` - Top entrance of the green pipe.
- `pipe_body.png` - Vertical body of the green pipe.

**Collectibles & Items (`collectibles/`)**
- `coin_1.png` - Coin frame.
- `mushroom.png` - Power-up mushroom.

**Backgrounds & UI (`backgrounds/`, `ui_menu/`)**
- `sky_bg.png` - Repeating sky background.

---

## 2. Integration Instructions for Claude Code

### Step 1: Create a TextureLoader
**Do NOT** call `Image.FromFile()` inside the `OnPaint` or `Paint` event loops. This will cause catastrophic memory leaks and severe lag.

Instead, create a static `TextureLoader` class in the `Core/` directory to load and cache the `Image` objects at startup:

```csharp
using System.Drawing;
using System.Collections.Generic;
using System.IO;

public static class TextureLoader
{
    public static Dictionary<string, Image> Sprites = new Dictionary<string, Image>();

    public static void LoadAll()
    {
        string basePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "assets", "textures");
        
        // Example loading:
        Sprites["player_idle"] = Image.FromFile(Path.Combine(basePath, "player", "player_idle.png"));
        Sprites["goomba"] = Image.FromFile(Path.Combine(basePath, "enemies", "goomba_walk_1.png"));
        Sprites["ground"] = Image.FromFile(Path.Combine(basePath, "terrain_blocks", "ground.png"));
        Sprites["brick"] = Image.FromFile(Path.Combine(basePath, "terrain_blocks", "brick.png"));
        // ... Load all mapped assets here
    }
}
```

*Important:* Call `TextureLoader.LoadAll()` exactly once in `mainWin.cs` constructor or `Form_Load` event.

### Step 2: Ensure Assets are Copied to Output Directory
Since the assets are file-based, you must update `supermario.csproj` to include the `assets/` folder with `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`.

```xml
<ItemGroup>
  <Content Include="assets\textures\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### Step 3: Implement Sprite Rendering
In `mainWin.cs` or wherever the drawing happens (`e.Graphics.DrawImage`), replace the current colored rectangles or legacy `.resx` resources with the cached textures.

```csharp
// Example for rendering the player:
if (player.Velocity.X == 0) {
    e.Graphics.DrawImage(TextureLoader.Sprites["player_idle"], player.Bounds);
} else {
    // Basic walk cycle logic using a timer counter
    e.Graphics.DrawImage(TextureLoader.Sprites["player_walk_1"], player.Bounds);
}
```

### Step 4: Sprite Replacement for Level Objects
In `mainWin.LevelBuilder.cs` and `GameObjectS.cs`, modify the object drawing logic to use the loaded textures.
- Map the ground `GameObjectS` to `TextureLoader.Sprites["ground"]`.
- Map the brick blocks to `TextureLoader.Sprites["brick"]`.
- Use a `TextureBrush` if you need to tile the `sky_bg.png` across the form.

---

## 3. Handling Animations

For objects that require animation (e.g., Mario walking, Question Blocks shimmering, Coins spinning), implement a global `FrameCounter` integer in the main game timer loop:

```csharp
private int globalFrame = 0;

private void GameTimer_Tick(object sender, EventArgs e) {
    globalFrame++;
    // Physics & updates here...
    Invalidate();
}
```

When rendering, use `globalFrame` to toggle between sprites:
```csharp
Image currentCoin = (globalFrame % 20 < 10) 
    ? TextureLoader.Sprites["coin_1"] 
    : TextureLoader.Sprites["coin_2"];
e.Graphics.DrawImage(currentCoin, coinBounds);
```

By adhering strictly to this caching and frame-logic pattern, the Super Mario project will maintain high 60fps performance while utilizing the new HD pixel art pack!
