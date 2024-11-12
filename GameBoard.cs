using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Collections;
using Roguelike.Utility;

namespace MatchThree;

public class GameBoard : DrawableGameComponent
{
    public int Rows = 6;
    public int Columns = 6;
    public GamePiece[,] GamePieces;
    public Rectangle[,] GridRects;
    public IntVector2 TileSize = new IntVector2(64, 64);
    public int Spacing = 10;
    private GamePiece _selectedPiece;
    private GameBoardState _gameBoardState = GameBoardState.AwaitingInput;
    private Vector2 _offscreenOffset = new (0, -100);
    private Bag<GamePiece> _movingPieces = new();

    private event Action CascadeMatches;
    
    
    public GameBoard(Game game) : base(game)
    {
        // var batch = new SpriteBatch(GraphicsDevice);
        // Game.Services.AddService(typeof(SpriteBatch), batch);
        CascadeMatches += OnCascadeMatches;
        GamePieces = new GamePiece[Columns, Rows];
        GridRects = new Rectangle[Columns, Rows];
        var man = game.Services.GetService<InputManager>();
        man.TouchEventCompleted += HandleTouchEvent;
        GenerateGrid();
        FillBoard();
        CascadeMatches?.Invoke();
    }

    public void GenerateGrid()
    {
        var xPos = 0;
        var yPos = 0;
        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                GridRects[i, j] = new Rectangle(xPos, yPos, TileSize.X, TileSize.Y);
                if (j < Rows - 1)
                {
                    yPos += TileSize.Y + Spacing;
                }
                else
                {
                    yPos = 0;
                }
            }

            xPos += TileSize.X + Spacing;
        }
    }

    public void FillBoard()
    {
        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                var newPiece = GamePieceFactory.GenerateRandom(Game, new IntVector2(i, j));
                newPiece.Bounds = GridRects[i, j];
                Game.Components.Add(newPiece);
                GamePieces[i, j] = newPiece;
            }
        }
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        (piece1.GridPosition, piece2.GridPosition) = (piece2.GridPosition, piece1.GridPosition);
        GamePieces[piece1.GridPosition.X, piece1.GridPosition.Y] = piece1;
        GamePieces[piece2.GridPosition.X, piece2.GridPosition.Y] = piece2;
        MovePieceTo(piece1, GridRects[piece1.GridPosition.X, piece1.GridPosition.Y].Location);
        MovePieceTo(piece2, GridRects[piece2.GridPosition.X, piece2.GridPosition.Y].Location);
        // piece1.MoveTo(piece2.Bounds.Location);
        // piece2.MoveTo(piece1.Bounds.Location);

        // CascadeMatches?.Invoke();
    }

    public void OnCascadeMatches()
    {
        var allMatches = CheckForMatches();
        if (allMatches.IsEmpty)
        {
            _gameBoardState = GameBoardState.AwaitingInput;
            return;
        }
        _gameBoardState = GameBoardState.PiecesMoving;
        RemoveMatches(allMatches);
        FillSpaces();
        
    }

    public void OnMoveCompleted(GamePiece piece)
    {
        _movingPieces.Remove(piece);
        if (_movingPieces.IsEmpty)
        {
            CascadeMatches?.Invoke();
        }
        
    }

    public void FillSpaces()
    {
        for (int i = Columns - 1; i >=0; i--)
        {
            for (int j = Rows - 1; j >= 0; j--)
            {
                if (GamePieces[i, j] == null)
                {
                    var currentRow = j;
                    GamePiece replacementPiece = null;
                    while (GamePieces[i, j] == null && currentRow >= 0)
                    {
                        if (currentRow == 0)
                        {
                            replacementPiece = GamePieceFactory.GenerateRandom(Game, new IntVector2(i, currentRow));
                            replacementPiece.Bounds = GridRects[i, j];
                            replacementPiece.ScreenPosition += _offscreenOffset;
                            
                            // replacementPiece.Position = GridRects[i, j].Location.ToVector2();
                            // replacementPiece.TargetPosition = replacementPiece.Position;
                            
                            Game.Components.Add(replacementPiece);
                        }
                        else
                        {
                            replacementPiece = GamePieces[i, currentRow - 1];
                            GamePieces[i, currentRow - 1] = null;
                        }
                        GamePieces[i, j] = replacementPiece;
                        if (GamePieces[i, j] is not null)
                        {
                            GamePieces[i, j].GridPosition = new IntVector2(i, j);
                        }
                        currentRow--;
                    }
                }
            }
        }

        foreach (var piece in GamePieces)
        {
            var loc = GridRects[piece.GridPosition.X, piece.GridPosition.Y].Location;
            if (piece.ScreenPosition == loc.ToVector2())
            {
                continue;
            }
            MovePieceTo(piece, loc);
        }
    }

    private void MovePieceTo(GamePiece piece, Point screenPosition)
    {
        piece.MoveTo(screenPosition);
        _movingPieces.Add(piece);
        piece.MoveCompleted += OnMoveCompleted;
        _gameBoardState = GameBoardState.PiecesMoving;
    }

    public void RemoveMatches(Bag<Bag<GamePiece>> matches)
    {
        foreach (var matchSet in matches)
        {
            foreach (var piece in matchSet)
            {
                Game.Components.Remove(piece);
                GamePieces[piece.GridPosition.X, piece.GridPosition.Y] = null;
            }
        }
    }

    public Bag<Bag<GamePiece>> CheckForMatches()
    {
        var pendingDeletion = new Bag<GamePiece>();
        var allMatchSets = new Bag<Bag<GamePiece>>();
        var verticalMatchSets = new Bag<Bag<GamePiece>>();
        var horizontalMatchSets = new Bag<Bag<GamePiece>>();
        for (int i = 0; i < Columns ; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                var currentPiece = GamePieces[i, j];
                if (pendingDeletion.Contains(currentPiece))
                {
                    continue;
                }

                if (i < Columns - 2)
                {
                    var currentColumn = i;
                    var hMatchSet = new Bag<GamePiece> { currentPiece };
                    while (currentColumn < Columns - 1)
                    {
                        var rightPiece = GamePieces[currentColumn + 1, j];
                        if (rightPiece.PieceType == currentPiece.PieceType)
                        {
                            hMatchSet.Add(rightPiece);
                            currentColumn++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (hMatchSet.Count >= 3)
                    {
                        horizontalMatchSets.Add(hMatchSet);
                    }
                }


                if (j < Rows - 2)
                {
                    var currentRow = j;
                    var vMatchSet = new Bag<GamePiece> { currentPiece };
                    while (currentRow < Rows - 1)
                    {
                        var downPiece = GamePieces[i, currentRow + 1];
                        if (downPiece.PieceType == currentPiece.PieceType)
                        {
                            vMatchSet.Add(downPiece);
                            currentRow++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (vMatchSet.Count >= 3)
                    {
                        verticalMatchSets.Add(vMatchSet);
                    }
                }

            }
            
        }
        foreach (var hMatchSet in horizontalMatchSets)
        {
            var overlappingSetFound = false;
            foreach (var piece in hMatchSet)
            {
                    
                    
                foreach (var vMatchSet in verticalMatchSets)
                {
                        
                    if (vMatchSet.Contains(piece))
                    {
                        var fullMatchSet = new Bag<GamePiece>();
                        fullMatchSet.AddRange(vMatchSet);
                        fullMatchSet.AddRange(hMatchSet);
                        allMatchSets.Add(fullMatchSet);
                        pendingDeletion.AddRange(fullMatchSet);
                        overlappingSetFound = true;
                        break;
                    }
                }
                if (overlappingSetFound)
                {
                    break;
                }
            }

            if (!overlappingSetFound)
            {
                allMatchSets.Add(hMatchSet);
                pendingDeletion.AddRange(hMatchSet);
            }
        }

        foreach (var vSet in verticalMatchSets)
        {
            var setAlreadyTracked = false;
            foreach (var piece in vSet)
            {
                if (pendingDeletion.Contains(piece))
                {
                    setAlreadyTracked = true;
                    break;
                }
                    
            }

            if (setAlreadyTracked)
            {
                continue;
            }
                
            allMatchSets.Add(vSet);
            pendingDeletion.AddRange(vSet);
        }

        // foreach (var piece in GamePieces)
        // {
        //     piece.Highlighted = false;
        // }        
        // foreach (var set in allMatchSets)
        // {
        //     foreach (var piece in set)
        //     {
        //         piece.Highlighted = true;
        //     }
        // }
        return allMatchSets;
    }

    public GamePiece GetPieceAtPosition(Point position)
    {
        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                if (GridRects[i, j].Contains(position))
                {
                    return GamePieces[i, j];
                }
            }
        }

        return null;
    }

    public void HandleTouchEvent(TouchEventArgs args)
    {
        if (_gameBoardState != GameBoardState.AwaitingInput)
        {
            return;
        }
        var startPiece = GetPieceAtPosition(args.TouchDown);
        var endPiece = GetPieceAtPosition(args.TouchUp);
        if (startPiece is not null && endPiece is not null && startPiece == endPiece)
        {
            if (endPiece.Selected)
            {
                endPiece.Selected = false;
                _selectedPiece = null;
                return;
            }
            
            if (_selectedPiece is not null && _selectedPiece != endPiece)
            {

                // var swap = new SwapEventArgs(_selectedPiece, endPiece);
                
                SwapPieces(_selectedPiece, endPiece);
                _selectedPiece.Selected = false;
                endPiece.Selected = false;
                _selectedPiece = null;
                
                _gameBoardState = GameBoardState.PiecesMoving;
                return;
            }

            if (_selectedPiece is null)
            {
                _selectedPiece = endPiece;
                endPiece.Selected = true;
                return;
            }

            return;
        }
    }

    public override void Update(GameTime gameTime)
    {
        var moving = false;
        foreach (var piece in GamePieces)
        {
            if (piece.MoveState == MoveState.Moving)
            {
                moving = true;
                break;
            }
        }

        if (moving)
        {
            _gameBoardState = GameBoardState.PiecesMoving;
            return;
        }
        _gameBoardState = GameBoardState.AwaitingInput;
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = Game.Services.GetService<SpriteBatch>();

        var xPos = 0;
        var yPos = 0;



        
        // for (int i = 0; i < Columns; i++)
        // {
        //     for (int j = 0; j < Rows; j++)
        //     {
        //         var destinationRect = new Rectangle(xPos, yPos, TileSize.X, TileSize.Y);
        //         var tex = Game.Content.Load<Texture2D>("Graphics/" + GamePieces[i, j].FileName);
        //         var tempColor = Color.White;
        //         if (_selectedPiece == GamePieces[i, j])
        //         {
        //             // tempColor = Color.Gray;
        //             GamePieces[i, j].Highlighted = true;
        //             destinationRect = new Rectangle(xPos - Spacing, yPos - Spacing, TileSize.X + Spacing * 2,
        //                 TileSize.Y + Spacing * 2);
        //         }
        //         spriteBatch.Draw(tex, destinationRect, tempColor);
        //
        //         if (j < Rows - 1)
        //         {
        //             yPos += TileSize.Y + Spacing;
        //         }
        //         else
        //         {
        //             yPos = 0;
        //         }
        //     }
        //
        //     xPos += TileSize.X + Spacing;
        // }
        
        base.Draw(gameTime);
    }

}

public enum GameBoardState
{
    AwaitingInput,
    PiecesMoving
}

public delegate void SwapEventHandler(SwapEventArgs args);

public class SwapEventArgs(GamePiece selectedPiece, GamePiece endPiece) : EventArgs
{
    public GamePiece FirstPiece;
    public GamePiece SecondPiece;
  

}