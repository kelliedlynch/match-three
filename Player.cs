using Microsoft.Xna.Framework;

namespace MatchThree;

public class Player(Game game) : GameComponent(game)
{
    public int HitPoints = 10;
}