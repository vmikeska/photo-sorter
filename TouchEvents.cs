using System;
using System.Windows.Input;
using photo_sorter.ints;

public class TouchEvents
{
    private Boolean AlreadySwiped = false;
    private TouchPoint TouchStart;
    private photo_sorter.MainWindow Win;
    private ConfigInt Config;

    public event EventHandler<string> DirectionSwipedEvent;

    public TouchEvents(photo_sorter.MainWindow win, ConfigInt config)
    {
        Win = win;
        Config = config;
        win.TouchDown += new EventHandler<TouchEventArgs>(BasePage_TouchDown);
        win.TouchUp += new EventHandler<TouchEventArgs>(BasePage_TouchUp);
        win.TouchMove += new EventHandler<TouchEventArgs>(BasePage_TouchMove);
        
    }

    void BasePage_TouchDown(object sender, TouchEventArgs e)
    {
        TouchStart = e.GetTouchPoint(Win);
    }

    void BasePage_TouchUp(object sender, TouchEventArgs e)
    {
        AlreadySwiped = false;
    }

    void BasePage_TouchMove(object sender, TouchEventArgs e)
    {

        if (TouchStart == null)
        {
            return;
        }

        if (!AlreadySwiped)
        {
            var Touch = e.GetTouchPoint(Win);

            string direction = null;

            var xStart = TouchStart.Position.X;
            var yStart = TouchStart.Position.Y;

            var xCurrent = Touch.Position.X;
            var yCurrent = Touch.Position.Y;

            var xDiff = xCurrent - xStart;
            var yDiff = yCurrent - yStart;

            var xLeftReached = xDiff < -Config.sensitivity;
            if (xLeftReached)
            {
                direction = "LEFT";
                AlreadySwiped = true;
            }

            var xRightReached = xDiff > Config.sensitivity;
            if (xRightReached)
            {
                direction = "RIGHT";
                AlreadySwiped = true;
            }

            var yTopReached = yDiff < -Config.sensitivity;
            if (yTopReached)
            {
                direction = "TOP";
                AlreadySwiped = true;
            }

            var yBottomReached = yDiff > Config.sensitivity;
            if (yBottomReached)
            {
                direction = "BOTTOM";
                AlreadySwiped = true;
            }

            if (direction != null)
            {
                DirectionSwipedEvent.Invoke(Win, direction);
            }
        }

        e.Handled = true;
    }
}
