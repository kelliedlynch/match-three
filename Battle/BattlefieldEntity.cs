using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MatchThree;

public class BattlefieldEntity : DrawableGameComponent
{
    // private Battlefield _field;
    public Monster Monster;
    public UIBar HealthBar;
    private int _barHeight = 10;
    public Rectangle SpritePosition;
    public Rectangle HealthBarPosition;
    public Texture2D SpriteTexture;
    // public bool Targeted = false;
    public TargetIndicator TargetIndicator;
    
    public BattlefieldEntity(Game game, Battlefield field, Monster monster) : base(game)
    {
        Monster = monster;
        monster.HealthChanged += OnHealthChanged;
        // _field = field;
        LoadTexture();
    }
    
    public void LoadTexture()
    {
        SpriteTexture = Game.Content.Load<Texture2D>(Monster.FileName);
    }

    public void SetPosition(Rectangle rect)
    {
        SpritePosition = rect;
        HealthBarPosition = rect.GetRelativeRectangle(0, 0, rect.Width, _barHeight);
        HealthBar = new UIBar(HealthBarPosition, Monster.CurrentHealth, Monster.MaxHealth);
        TargetIndicator = new TargetIndicator(rect);
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = Game.Services.GetService<SpriteBatch>();
        spriteBatch.Draw(SpriteTexture, SpritePosition, Color.White);
        HealthBar.Draw(Game, spriteBatch);
        if (Monster.Targeted)
        {
            TargetIndicator.Draw(Game, spriteBatch);
        }
    }

    public void OnHealthChanged()
    {
        HealthBar.CurrentValue = Monster.CurrentHealth;
        HealthBar.MaxValue = Monster.MaxHealth;
    }
}
