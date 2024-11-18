using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MatchThree;

public class TargetIndicator(Rectangle bounds)
{
    public Rectangle Bounds = bounds;
    public int BorderWidth = 2;
    public Color BorderColor = Color.White;

    public void Draw(Game game, SpriteBatch spriteBatch)
    {
        var pixel = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        pixel.SetData(new[] { Color.White });
        spriteBatch.Draw(pixel, Bounds.GetRelativeRectangle(0, 0, BorderWidth, Bounds.Height), BorderColor);
        spriteBatch.Draw(pixel, Bounds.GetRelativeRectangle(0, 0, Bounds.Width, BorderWidth), BorderColor);
        spriteBatch.Draw(pixel, Bounds.GetRelativeRectangle(Bounds.Width - BorderWidth, 0, BorderWidth, Bounds.Height), BorderColor);
        spriteBatch.Draw(pixel, Bounds.GetRelativeRectangle(0, Bounds.Height - BorderWidth, Bounds.Width, BorderWidth), BorderColor);
    }
}