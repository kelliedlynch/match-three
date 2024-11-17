using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Tweening;
using Roguelike.Utility;

namespace MatchThree;

public class GamePiece(Game game) : DrawableGameComponent(game)
{
    public PieceType PieceType;
    public int Value = 1;
    public bool Highlighted = false;
    public bool Selected = false;

    public IntVector2 GridPosition;
    public Vector2 ScreenPosition;
    private Vector2 _size;
    public Point TargetPosition;
    
    private readonly Tweener Tweener = new Tweener();
    public event Action<GamePiece> MoveCompleted;

    public Rectangle Bounds
    {
        get => new Rectangle(ScreenPosition.ToPoint(), _size.ToPoint());
        set
        {
            ScreenPosition = value.Location.ToVector2();
            _size = value.Size.ToVector2();
        }
    }

    public Vector2 Velocity = Vector2.Zero;
    public float Speed = 0.4f;
    public MoveState MoveState = MoveState.NotMoving;
    

    public string FileName
    {
        get
        {
            return PieceType switch
            {
                // PieceType.Diamond => "tileBlue_04",
                // PieceType.Circle => "tilePink_11",
                // PieceType.Square => "tileRed_01",
                // PieceType.Pentagon => "tileGreen_05",
                // PieceType.Star => "tileOrange_08",
                // PieceType.Jewel => "tileYellow_22",
                PieceType.Diamond => "rune_blue_x",
                PieceType.Circle => "rune_turq_x",
                PieceType.Square => "rune_purple_x",
                PieceType.Pentagon => "rune_orange_x",
                PieceType.Star => "rune_yellow_x",
                PieceType.Jewel => "rune_green_x",
                _ => ""
            };
        }
    }
    
    

    public Color Color
    {
        get
        {
            return PieceType switch
            {
                PieceType.Diamond => Color.Red,
                PieceType.Circle => Color.DeepSkyBlue,
                PieceType.Square => Color.Purple,
                PieceType.Pentagon => Color.Yellow,
                PieceType.Star => Color.Orange,
                PieceType.Jewel => Color.LawnGreen,
                _ => Color.White
            };
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (MoveState == MoveState.NotMoving)
        {
            return;
        }
        Tweener.Update(gameTime.GetElapsedSeconds());
        if (ScreenPosition == TargetPosition.ToVector2())
        {
            MoveState = MoveState.NotMoving;
            MoveCompleted?.Invoke(this);
            
            // Tweener.CancelAndCompleteAll();
            // TargetPosition = Point.Zero;
        }
        
    }

    public void MoveTo(Point position)
    {
        TargetPosition = position;
        Tweener.TweenTo(this, x => x.ScreenPosition, position.ToVector2(), Speed)
            .Easing(EasingFunctions.CubicInOut);
        MoveState = MoveState.Moving;
    }

    public void FallTo(Point position)
    {
        TargetPosition = position;
        var delay = new Random().NextDouble() * 0.1;
        Tweener.TweenTo(this, x => x.ScreenPosition, position.ToVector2(), Speed, (float)delay)
            .Easing(EasingFunctions.BounceOut);
        MoveState = MoveState.Moving;
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = Game.Services.GetService<SpriteBatch>();
        if(Selected)
        {
            DrawBorder(spriteBatch);
        }
        var tex = Game.Content.Load<Texture2D>("Graphics/" + FileName);
        // var destinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)_size.X, (int)_size.Y);
        spriteBatch.Draw(tex, Bounds, Color.White);
        
        // base.Draw(gameTime);
    }

    private void DrawBorder(SpriteBatch spriteBatch)
    {
        var pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        pixel.SetData(new[] { Color.White });
        var borderWidth = 3;
        var borderColor = Color.White;
        var pos = ScreenPosition.ToIntVector2();
        var size = _size.ToIntVector2();
        spriteBatch.Draw(pixel, new Rectangle(pos.X, pos.Y, size.X, borderWidth), borderColor);

        spriteBatch.Draw(pixel, new Rectangle(pos.X, pos.Y, borderWidth, size.Y), borderColor);

        spriteBatch.Draw(pixel, new Rectangle(pos.X + size.X - borderWidth,
            pos.Y,
            borderWidth,
            size.Y), borderColor);

        spriteBatch.Draw(pixel, new Rectangle(pos.X,
            pos.Y + size.Y - borderWidth,
            size.X,
            borderWidth), borderColor);
    }
}

public enum PieceType
{
    Diamond,
    Square,
    Circle,
    Pentagon,
    Jewel,
    Star
}

public enum MoveState
{
    Moving,
    NotMoving
}

