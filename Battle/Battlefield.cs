using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Roguelike.Utility;

namespace MatchThree;

public class Battlefield(Game game, Rectangle bounds) : DrawableGameComponent(game)
{
    public Rectangle Bounds = bounds;
    // public List<Monster> Monsters = new();

    private readonly List<BattlefieldEntity> _monsters = new();
    // private List<Rectangle> _monsterSpritePositions = new();
    // private List<Texture2D> _monsterSpriteTextures = new();
    // private List<UIBar> _monsterHealthBars = new();
    
    private IntVector2 MaxMonsterSize => new IntVector2(
        (Bounds.Width - Spacing * (_monsters.Count - 1)) / _monsters.Count,
        (Bounds.Height - Spacing * (_monsters.Count - 1)) / _monsters.Count);
    private int Spacing => (int)((Bounds.Width * 0.1f)/ _monsters.Count);
    private int BarHeight => (int)(Bounds.Height * 0.04f);

    public event Action<BattlefieldEntity> MonsterTouched;
    public event Action BattlefieldTouched;


    public override void Initialize()
    {
        var man = Game.Services.GetService<InputManager>();
        man.TouchEventCompleted += OnTouchEventCompleted;
        // base.Initialize();
    }
    
    

    public void AddMonster(Monster monster)
    {
        var entity = new BattlefieldEntity(Game, this, monster);
        Game.Components.Add(entity);
        _monsters.Add(entity);
        
        // Monsters.Add(monster);
        // var hpBar = new UIBar(game, Rectangle.Empty, monster.CurrentHitPoints, monster.MaxHitPoints);
        // Game.Components.Add(hpBar);
        // _monsterHealthBars.Add(hpBar);
        PlaceMonsters();

    }

    // public void UpdateMonster(Monster monster)
    // {
    //     var entity = Monsters.FirstOrDefault(e => e.Monster == monster);
    //     
    // }

    public void PlaceMonsters()
    {
        int totalWidth = 0;
        // _monsterSpriteTextures.Clear();
        // _monsterSpritePositions.Clear();
        foreach (var monster in _monsters)
        {
            var sprite = monster.SpriteTexture;
            var ratioX = (double)MaxMonsterSize.X / sprite.Width;
            var ratioY = (double)MaxMonsterSize.Y / sprite.Height;
            var ratio = Math.Min(Math.Min(ratioX, ratioY), 2);

            var scaledWidth = (int)(sprite.Width * ratio);
            var scaledHeight = (int)(sprite.Height * ratio);
            var spriteRect = new Rectangle(0, 0, scaledWidth, scaledHeight);
            monster.SpritePosition = spriteRect;
            // _monsterSpritePositions.Add(spriteRect);
            // _monsterSpriteTextures.Add(sprite);
            totalWidth += scaledWidth;
            
        }
        
        int currentX = (Bounds.Width - totalWidth - Spacing * (_monsters.Count - 1)) / 2;
        foreach (var monster in _monsters)
        {
            int y = Bounds.Height - monster.SpritePosition.Height - Spacing;
            var location = new Point(currentX, y) + Bounds.Location;
            var newPosition = new Rectangle(location.X, location.Y, monster.SpritePosition.Width, monster.SpritePosition.Height);
            monster.SetPosition(newPosition);
            currentX += Spacing + monster.SpritePosition.Width;
            // _monsterHealthBars[i].Bounds = new Rectangle(
            //     _monsterSpritePositions[i].Location, new Point(_monsterSpritePositions[i].Width, BarHeight));
        }
    }

    public void OnTouchEventCompleted(TouchEventArgs args)
    {
        if (!Bounds.Contains(args.TouchUp)) return;
        
        foreach (var monster in _monsters)
        {
            if (!monster.SpritePosition.Contains(args.TouchUp)) continue;
            MonsterTouched?.Invoke(monster);
            return;
        }
        BattlefieldTouched?.Invoke();
    }

    public override void Draw(GameTime gameTime)
    {
        // var spriteBatch = Game.Services.GetService<SpriteBatch>();
        // foreach (var monster in Monsters)
        // {
        //     spriteBatch.Draw(_monsterSpriteTextures[i], _monsterSpritePositions[i], Color.White);
        // }
    }
}