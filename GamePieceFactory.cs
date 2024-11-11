using System;
using Microsoft.Xna.Framework;
using Roguelike.Utility;

namespace MatchThree;

public static class GamePieceFactory
{
    private static readonly Random Random = new Random();

    public static GamePiece GenerateRandom(Game game)
    {
        var allTypes = Enum.GetValues(typeof(PieceType));
        var thisType = (PieceType)allTypes.GetValue(Random.Next(allTypes.Length))!;
        var newPiece = new GamePiece(game);
        newPiece.PieceType = thisType;
        return newPiece;
    }

    public static GamePiece GenerateRandom(Game game, IntVector2 gridPosition)
    {
        var newPiece = GenerateRandom(game);
        newPiece.GridPosition = gridPosition;
        return newPiece;
    }
}