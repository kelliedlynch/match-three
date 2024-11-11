using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MatchThree;

// public delegate void TouchEvent(TouchEventArgs args);

public class InputManager(Game game) : GameComponent(game)
{
    public bool LeftMousePressed = false;
    public Point? MouseDownAt = null;
    public event TouchEventHandler TouchEventCompleted;

    protected void OnTouchEventCompleted(TouchEventArgs args)
    {
        TouchEventCompleted?.Invoke(args);
    }
    
    public override void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();
        if (LeftMousePressed && mouse.LeftButton == ButtonState.Released)
        {
            LeftMousePressed = false;
            if (MouseDownAt is not null)
            {
               OnTouchEventCompleted(new TouchEventArgs((Point)MouseDownAt, mouse.Position));
               MouseDownAt = null;
            }

        }

        if (!LeftMousePressed && mouse.LeftButton == ButtonState.Pressed)
        {
            LeftMousePressed = true;
            MouseDownAt = mouse.Position;
        }
        
        base.Update(gameTime);
    }
}

public delegate void TouchEventHandler(TouchEventArgs args);

public class TouchEventArgs : EventArgs
{
    public Point TouchDown;
    public Point TouchUp;
    
    public TouchEventArgs(Point touchDown, Point touchUp)
    {
        TouchDown = touchDown;
        TouchUp = touchUp;
    }
    

}