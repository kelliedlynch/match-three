using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using Roguelike.Utility;

namespace MatchThree;

public class GameBoard(Game game, Rectangle bounds) : DrawableGameComponent(game)
{
    private int _rows = 6;
    private int _columns = 6;

    private int _spacing = 10;
    private GamePiece _selectedPiece;
    private readonly Bag<GamePiece> _movingPieces = [];
    public GamePiece[,] GamePieces;
    private Rectangle[,] _gridRects;
    private Rectangle[] _insertPositions;
    private Rectangle _bounds = bounds;
    private IntVector2 _tileSize;
    private readonly Random _random = new();

    private event Action CascadeMatches;
    public event Action<Bag<GamePiece>> MatchActivated;
    
    
    public override void Initialize()
    {
        CascadeMatches += OnCascadeMatches;
        GamePieces = new GamePiece[_columns, _rows];
        var man = Game.Services.GetService<InputManager>();
        man.TouchEventCompleted += HandleTouchEvent;
        GenerateGrid();
        FillBoard();
        // CascadeMatches?.Invoke();
    }

    private void GenerateGrid()
    {
        var tileX = (_bounds.Width - _spacing * (_columns - 1))/ _columns ;
        var tileY = (_bounds.Height - _spacing * (_rows - 1))/ _rows;
        _tileSize = new (tileX, tileY);
        _gridRects = new Rectangle[_columns, _rows];
        _insertPositions = new Rectangle[_columns];
        var xPos = _bounds.X;
        var yPos = _bounds.Y;
        for (int i = 0; i < _columns; i++)
        {
            _insertPositions[i] = new Rectangle(xPos, yPos - _spacing - _tileSize.Y, _tileSize.X, _tileSize.Y);
            for (int j = 0; j < _rows; j++)
            {
                _gridRects[i, j] = new Rectangle(xPos, yPos, _tileSize.X, _tileSize.Y);
                if (j < _rows - 1)
                {
                    yPos += _tileSize.Y + _spacing;
                }
                else
                {
                    yPos = _bounds.Y;
                }
            }

            xPos += _tileSize.X + _spacing;
        }
    }

    private void FillBoard()
    {
        for (int i = 0; i < _columns; i++)
        {
            for (int j = 0; j < _rows; j++)
            {
                var newPiece = GamePieceFactory.GenerateRandom(Game, new IntVector2(i, j));
                newPiece.Bounds = _gridRects[i, j];
                Game.Components.Add(newPiece);
                GamePieces[i, j] = newPiece;
            }
        }

        
        while (FindAllMatches().Count > 0)
        {
            var matches = FindAllMatches();
            foreach (var set in matches)
            {
                var index = _random.Next(matches.Count - 1);
                var piece = set.ToList()[index];
                var newPiece = GamePieceFactory.GenerateRandom(Game, piece.GridPosition);
                newPiece.Bounds = _gridRects[piece.GridPosition.X, piece.GridPosition.Y];
                Game.Components.Add(newPiece);
                GamePieces[piece.GridPosition.X, piece.GridPosition.Y] = newPiece;
                Game.Components.Remove(piece);
            }
        }
    }

    private void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        SwapPositions(piece1, piece2);
        MovePieceTo(piece1, _gridRects[piece1.GridPosition.X, piece1.GridPosition.Y].Location);
        MovePieceTo(piece2, _gridRects[piece2.GridPosition.X, piece2.GridPosition.Y].Location);
    }

    private void SwapPositions(GamePiece piece1, GamePiece piece2)
    {
        (piece1.GridPosition, piece2.GridPosition) = (piece2.GridPosition, piece1.GridPosition);
        GamePieces[piece1.GridPosition.X, piece1.GridPosition.Y] = piece1;
        GamePieces[piece2.GridPosition.X, piece2.GridPosition.Y] = piece2;
    }

    private void OnCascadeMatches()
    {
        var allMatches = FindAllMatches();
        if (allMatches.IsEmpty)
        {
            return;
        }
        RemoveMatches(allMatches);
        ApplyGravity();
        
    }

    private void OnMoveCompleted(GamePiece piece)
    {
        _movingPieces.Remove(piece);
        if (!_movingPieces.IsEmpty)
        {
            return;
        }

        if(EmptySpacesExist())
        {
            // _gameBoardState = GameBoardState.PiecesMoving;
            ApplyGravity();
            return;
        }
        
        CascadeMatches?.Invoke();
    }

    private void ApplyGravity()
    {
        var shiftCount = 0;
        while (EmptySpacesExist())
        {
            ShiftDown(shiftCount);
            shiftCount++;
        }        

        foreach (var piece in GamePieces)
        {
            if(piece == null) continue;
            var loc = _gridRects[piece.GridPosition.X, piece.GridPosition.Y].Location;
            if (piece.ScreenPosition == loc.ToVector2())
            {
                continue;
            }
            DropPieceTo(piece, loc);
        }
    }

    private bool EmptySpacesExist()
    {
        foreach (var piece in GamePieces)
        {
            if (piece == null) return true;
        }

        return false;
    }

    private void ShiftDown(int prevShifts = 0)
    {
        for (int i = _columns - 1; i >=0; i--)
        {
            
            for (int j = _rows - 1; j >= 0; j--)
            {
                if (GamePieces[i, j] == null)
                {
                    if (j == 0)
                    {
                        var yOffset = _tileSize.Y * prevShifts + _spacing * prevShifts;
                        var newPiece = SpawnNewPiece(i);
                        newPiece.ScreenPosition += new IntVector2(0, -yOffset);
                        
                        continue;
                    }

                    var upOne = GamePieces[i, j - 1];
                    if ( upOne == null)
                    {
                        continue;
                    }
                    upOne.GridPosition.Y = j;
                    GamePieces[i, j] = upOne;
                    GamePieces[i, j - 1] = null;
                }
            }
        }
    }

    private GamePiece SpawnNewPiece(int column)
    {
        var newPiece = GamePieceFactory.GenerateRandom(Game, new IntVector2(column, 0));
        newPiece.Bounds = _insertPositions[column];
        GamePieces[column, 0] = newPiece;
        Game.Components.Add(newPiece);
        return newPiece;
    }
    
    private void MovePieceTo(GamePiece piece, Point screenPosition)
    {
        piece.MoveTo(screenPosition);
        _movingPieces.Add(piece);
        piece.MoveCompleted += OnMoveCompleted;
    }
    
    private void DropPieceTo(GamePiece piece, Point screenPosition)
    {
        piece.FallTo(screenPosition);
        _movingPieces.Add(piece);
        piece.MoveCompleted += OnMoveCompleted;
    }

    private void RemoveMatches(Bag<Bag<GamePiece>> matches)
    {
        // var man = Game.Services.GetService<BattleManager>();
        foreach (var matchSet in matches)
        {
            MatchActivated?.Invoke(matchSet);
        }
    }

    public bool AnyMatchesPossible()
    {
        for (var i = 0; i < _columns - 2; i++)
        {
            for (var j = 0; j < _rows - 2; j++)
            {
                if (CanBeSwapped(GamePieces[i, j], GamePieces[i + 1, j]) ||
                    CanBeSwapped(GamePieces[i, j], GamePieces[i, j + 1]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CanBeSwapped(GamePiece piece1, GamePiece piece2)
    {
        SwapPositions(piece1, piece2);
        foreach (var piece in new List<GamePiece> { piece1, piece2 })
        {
            var matches = new Bag<GamePiece>();
            foreach (var dir in Direction.CardinalDirections)
            {
                matches.AddRange(MatchingPiecesInDirection(piece, dir));
                if (matches.Count <= 1) continue;
                SwapPositions(piece1, piece2);
                return true;
            }
        }
        SwapPositions(piece1, piece2);
        return false;
    }

    private Bag<GamePiece> MatchingPiecesInDirection(GamePiece piece, IntVector2 direction)
    {
        var matches = new Bag<GamePiece>();
        var testLoc = piece.GridPosition + direction;
        
        var testPiece = PieceAtGridPosition(testLoc);
        if (testPiece == null) return matches;
        while (testPiece.PieceType == piece.PieceType)
        {
            matches.Add(testPiece);
            testLoc += direction;
            testPiece = PieceAtGridPosition(testLoc);
            if (testPiece == null) break;
        }

        return matches;
    }

    private GamePiece PieceAtGridPosition(IntVector2 location)
    {
        try
        {
            return GamePieces[location.X, location.Y];
        }
        catch 
        {
            return null; 
        }
    }

    private Bag<Bag<GamePiece>> FindAllMatches()
    {
        // var pendingDeletion = new Bag<GamePiece>();
        var allMatchSets = new Bag<Bag<GamePiece>>();
        var verticalMatchSets = new Bag<Bag<GamePiece>>();
        var horizontalMatchSets = new Bag<Bag<GamePiece>>();
        for (int i = 0; i < _columns ; i++)
        {
            for (int j = 0; j < _rows; j++)
            {
                var currentPiece = GamePieces[i, j];
                
                if (i < _columns - 2 && !horizontalMatchSets.Any(x => x.Contains(currentPiece)))
                {
                    var hSet = new Bag<GamePiece>{currentPiece};
                    hSet.AddRange(MatchingPiecesInDirection(currentPiece, Direction.Right));
                    if (hSet.Count >= 3)
                    {
                        horizontalMatchSets.Add(hSet);
                    }
                }

                if (j < _rows - 2 && !verticalMatchSets.Any(x => x.Contains(currentPiece))) 
                {
                    var vSet = new Bag<GamePiece>{currentPiece};
                    vSet.AddRange(MatchingPiecesInDirection(currentPiece, Direction.Down));
                    if (vSet.Count >= 3)
                    {
                        verticalMatchSets.Add(vSet);
                    }
                }

            }
        }
        
        foreach (var hvSet in horizontalMatchSets.Concat(verticalMatchSets))
        {
            var fullSet = new Bag<GamePiece>();
            fullSet.AddRange(hvSet);
            foreach (var alreadyMatched in allMatchSets)
            {
                if (alreadyMatched.Any(x => hvSet.Contains(x)))
                {
                    fullSet.AddRange(alreadyMatched);
                    allMatchSets.Remove(alreadyMatched);
                }
            }
            foreach (var piece in hvSet)
            {
                foreach (var hSet in horizontalMatchSets.Where(hSet => hSet.Contains(piece)))
                {
                    fullSet.AddRange(hSet);
                }
                foreach (var vSet in verticalMatchSets.Where(vSet => vSet.Contains(piece)))
                {
                    fullSet.AddRange(vSet);
                }
            }

            
            var bag = new Bag<GamePiece>();
            fullSet.Distinct().ToList().ForEach(x => bag.Add(x));
            
            allMatchSets.Add(bag);
        }

        return allMatchSets;
    }

    private GamePiece PieceAtScreenPosition(Point position)
    {
        for (int i = 0; i < _columns; i++)
        {
            for (int j = 0; j < _rows; j++)
            {
                if (_gridRects[i, j].Contains(position))
                {
                    return GamePieces[i, j];
                }
            }
        }

        return null;
    }

    private void HandleTouchEvent(TouchEventArgs args)
    {
        var startPiece = PieceAtScreenPosition(args.TouchDown);
        var endPiece = PieceAtScreenPosition(args.TouchUp);
        if (startPiece is null || endPiece is null || startPiece != endPiece) return;
        if (endPiece.Selected)
        {
            endPiece.Selected = false;
            _selectedPiece = null;
            return;
        }
            
        if (_selectedPiece is not null && _selectedPiece != endPiece)
        {
            SwapPieces(_selectedPiece, endPiece);
            _selectedPiece.Selected = false;
            endPiece.Selected = false;
            _selectedPiece = null;
                
            return;
        }

        if (_selectedPiece is not null) return;
        _selectedPiece = endPiece;
        endPiece.Selected = true;
    }

}

public static class Direction
{
    public static IntVector2 Up { get; } = new (0, -1);
    public static IntVector2 Down { get; } = new (0, 1);
    public static IntVector2 Left { get; } = new (-1, 0);

    public static IntVector2 Right { get; } = new (1, 0);

    // public static IntVector2 UpLeft = Up + Left;
    // public static IntVector2 UpRight = Up + Right;
    // public static IntVector2 DownLeft = Down + Left;
    // public static IntVector2 DownRight = Down + Right;
    public static readonly List<IntVector2> CardinalDirections = [Up, Down, Left, Right];
}