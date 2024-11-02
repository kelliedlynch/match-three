using System;
using Microsoft.Xna.Framework;
using Roguelike.Utility;

namespace MatchThree;

public class GamePiece : DrawableGameComponent
{
    public PieceType PieceType;
    public IntVector2 Location;
    public int Level = 1;
    public bool Highlighted = false;

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
    
    public GamePiece(Game game) : base(game)
    {
        
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

public static class GamePieceFactory
{
    public static Random Random = new Random();

    public static GamePiece GenerateRandom(Game game)
    {
        var allTypes = Enum.GetValues(typeof(PieceType));
        var thisType = (PieceType)allTypes.GetValue(Random.Next(allTypes.Length))!;
        var newPiece = new GamePiece(game);
        newPiece.PieceType = thisType;
        return newPiece;
    }

    public static GamePiece GenerateRandom(Game game, IntVector2 location)
    {
        var newPiece = GenerateRandom(game);
        newPiece.Location = location;
        return newPiece;
    }
}