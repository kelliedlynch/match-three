using Microsoft.Xna.Framework;

namespace MatchThree;

public class Monster(Game game) : DrawableGameComponent(game)
{
    public int MaxHitPoints;
    public int CurrentHitPoints;
    public int Atk;
    public int Def;
    public string Name;
    public string FileName;
}