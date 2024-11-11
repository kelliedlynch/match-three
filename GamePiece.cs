using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using Roguelike.Utility;

namespace MatchThree;

public class GamePiece(Game game) : DrawableGameComponent(game)
{
    public PieceType PieceType;
    public int Level = 1;
    public bool Highlighted = false;
    public bool Selected = false;

    public IntVector2 GridPosition;
    private Vector2 _screenPosition;
    private Vector2 _size;
    public Point TargetPosition;

    public Rectangle Bounds
    {
        get => new Rectangle(_screenPosition.ToPoint(), _size.ToPoint());
        set
        {
            _screenPosition = value.Location.ToVector2();
            _size = value.Size.ToVector2();
        }
    }

    public Vector2 Velocity = Vector2.Zero;
    public float Speed = 0.9f;
    public MoveState MoveState = MoveState.NotMoving;
    

    public string FileName
    {
        get
        {
            return PieceType switch
            {
                PieceType.Diamond => "tileBlue_04",
                PieceType.Circle => "tilePink_11",
                PieceType.Square => "tileRed_01",
                PieceType.Pentagon => "tileGreen_05",
                PieceType.Star => "tileOrange_08",
                PieceType.Jewel => "tileYellow_22",
                _ => ""
            };
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (MoveState == MoveState.NotMoving)
        {
            return;
        }

        if (Velocity == Vector2.Zero)
        {
            Velocity = (TargetPosition.ToVector2() - _screenPosition) * Speed;
        }
        var distanceToTarget = TargetPosition.ToVector2() - _screenPosition;
        if (Math.Abs(distanceToTarget.X) <= Math.Abs(Velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds) && Math.Abs(distanceToTarget.Y) <= Math.Abs(Velocity.Y * (float)gameTime.ElapsedGameTime.TotalSeconds))
        {
            _screenPosition = TargetPosition.ToVector2();
            Velocity = Vector2.Zero;
            MoveState = MoveState.NotMoving;
            return;
        }

        // distanceToTarget.Normalize();
        _screenPosition += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void MoveTo(Point position)
    {
        TargetPosition = position;
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
        var pos = _screenPosition.ToIntVector2();
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

