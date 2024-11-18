using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Collections;
using MonoGame.Extended.ECS;
using Roguelike.Utility;

namespace MatchThree;

public class MainGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public IntVector2 ScreenSize = new(600, 1080);
    public int GameBoardPadding = 10;
    public IntVector2 GameBoardSize = new(560, 560);

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = ScreenSize.X;
        _graphics.PreferredBackBufferHeight = ScreenSize.Y;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        var inputManager = new InputManager(this);
        Components.Add(inputManager);
        Services.AddService(inputManager);


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Services.AddService(_spriteBatch);


    }

    protected override void BeginRun()
    {
        var xOffset = (ScreenSize.X - GameBoardSize.X) / 2;
        var yOffset = ScreenSize.Y - GameBoardSize.Y - GameBoardPadding;
        var boardRect = new Rectangle(xOffset, yOffset, GameBoardSize.X, GameBoardSize.Y);
        var gameBoard = new GameBoard(this, boardRect);
        Components.Add(gameBoard);

        var rect = new Rectangle(xOffset, GameBoardPadding, GameBoardSize.X, ScreenSize.Y - GameBoardSize.Y - GameBoardPadding * 3);
        var battlefield = new Battlefield(this, rect);
        Components.Add(battlefield);
        
        var battleManager = new BattleManager(this, gameBoard, battlefield);
        Services.AddService(battleManager);

        var monsters = new List<Monster>();

        for (int i = 0; i < 1; i++)
        {
            var mon = new Monster();
            mon.FileName = "Graphics/Slime RPG Basic";
            mon.MaxHealth = 100;
            mon.CurrentHealth = 100;
            monsters.Add(mon);
        }
        
        battleManager.InitializeBattle(monsters);
        battleManager.BeginBattle();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override bool BeginDraw()
    {
        GraphicsDevice.Clear(Color.Gray);
        Services.GetService<SpriteBatch>().Begin();
        return base.BeginDraw();
    }

    protected override void Draw(GameTime gameTime)
    {

        base.Draw(gameTime);
    }

    protected override void EndDraw()
    {
        Services.GetService<SpriteBatch>().End();
        base.EndDraw();
    }
}