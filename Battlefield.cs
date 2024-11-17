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
    public List<Monster> Monsters = new();
    private List<Rectangle> _monsterSpritePositions = new();
    private List<Texture2D> _monsterSpriteTextures = new();
    private List<UIBar> _monsterHealthBars = new();



    private IntVector2 MaxMonsterSize => new IntVector2(
        (Bounds.Width - Spacing * (Monsters.Count - 1)) / Monsters.Count,
        (Bounds.Height - Spacing * (Monsters.Count - 1)) / Monsters.Count);
    private int Spacing => (int)((Bounds.Width * 0.1f)/ Monsters.Count);
    private int BarHeight => (int)(Bounds.Height * 0.04f);

    public void AddMonster(Monster monster)
    {
        Monsters.Add(monster);
        var hpBar = new UIBar(game, Rectangle.Empty, monster.CurrentHitPoints, monster.MaxHitPoints);
        Game.Components.Add(hpBar);
        _monsterHealthBars.Add(hpBar);
        PlaceMonsters();

    }

    public void UpdateMonster(Monster monster)
    {
        var index = Monsters.IndexOf(monster);
        _monsterHealthBars[index].CurrentValue = monster.CurrentHitPoints;
    }

    public void PlaceMonsters()
    {
        int totalWidth = 0;
        _monsterSpriteTextures.Clear();
        _monsterSpritePositions.Clear();
        foreach (var monster in Monsters)
        {
            var sprite = Game.Content.Load<Texture2D>(monster.FileName);
            var ratioX = (double)MaxMonsterSize.X / sprite.Width;
            var ratioY = (double)MaxMonsterSize.Y / sprite.Height;
            var ratio = Math.Min(Math.Min(ratioX, ratioY), 2);

            var scaledWidth = (int)(sprite.Width * ratio);
            var scaledHeight = (int)(sprite.Height * ratio);
            var spriteRect = new Rectangle(0, 0, scaledWidth, scaledHeight);
            _monsterSpritePositions.Add(spriteRect);
            _monsterSpriteTextures.Add(sprite);
            totalWidth += scaledWidth;
            
        }
        
        int currentX = (Bounds.Width - totalWidth - Spacing * (_monsterSpriteTextures.Count - 1)) / 2;
        for (int i = 0; i < _monsterSpriteTextures.Count; i++)
        {
            int y = Bounds.Height - _monsterSpritePositions[i].Height - Spacing;
            var location = new Point(currentX, y) + Bounds.Location;
            _monsterSpritePositions[i] = new Rectangle(location.X, location.Y, _monsterSpritePositions[i].Width, _monsterSpritePositions[i].Height);
            currentX += Spacing + _monsterSpritePositions[i].Width;
            _monsterHealthBars[i].Bounds = new Rectangle(
                _monsterSpritePositions[i].Location, new Point(_monsterSpritePositions[i].Width, BarHeight));
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = Game.Services.GetService<SpriteBatch>();
        for (int i = 0; i < _monsterSpriteTextures.Count; i++)
        {
            spriteBatch.Draw(_monsterSpriteTextures[i], _monsterSpritePositions[i], Color.White);
        }
    }
}