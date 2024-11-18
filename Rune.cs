using MonoGame.Extended.Collections;

namespace MatchThree;

public class Rune
{
    public string Name;
    public string Description;

    public void OnMatch(Bag<GamePiece> set)
    {
        
    }
    
}

public class TestRune : Rune
{
    public TestRune()
    {
        Name = "Test";
        Description = "Yellow gems do +1 damage";
    }
}