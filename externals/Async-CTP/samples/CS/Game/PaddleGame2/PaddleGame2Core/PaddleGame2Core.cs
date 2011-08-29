using System;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;

public class GameData
{
    public float[,] xs; // x speed
    public float[,] ys; // y speed
    public int remaining, active;
    public Canvas canvas;
    public TextBlock tb;
    public Line[,] lines;
    public Rectangle paddle;
    public Ellipse[,] blocks;
    public MediaElement[] beeps;
    public MediaElement[] blockBeeps;
    public int curBeep = 0;
    public DateTime lastTick;
}

public static class GameLogic
{
    readonly static Random RNG = new System.Random();
    const float E = 0.0001f;
    static readonly SolidColorBrush WHITE = new SolidColorBrush(Colors.White);
    static readonly SolidColorBrush GRAY = new SolidColorBrush(Colors.Gray);
    static readonly SolidColorBrush BLACK = new SolidColorBrush(Colors.Black);

    // pixel size of a ball/brick
    const float SIZE = 5.0f;
    const float HALFSIZE = SIZE / 2.0f;
    // initial grid size of blocks
    const int WIDTH = 80;
    const int HEIGHT = 20;
    // pixel location of bottom of bricks
    const float BOTBRICKS = (float)HEIGHT * SIZE;
    // canvas size
    const float CANWIDTH = SIZE * WIDTH;
    const float CANHEIGHT = SIZE * 80.0f;
    // paddle size
    const float PADHEIGHT = 10.0f;
    const float PADWIDTH = 8.0f * SIZE;
    const float HALFPADWIDTH = PADWIDTH / 2.0f;
    const float maxWidth = CANWIDTH - PADWIDTH;
    // pixel location of top of paddle
    const float TOPPAD = CANHEIGHT - 60.0f;
    // pixel speed per second
    const float PADVELOCITY = 250;
    const float LINEVELOCITY = 50;
    // how many beeps will we load for the game
    const int NUMBEEP = 20;


    public static Tuple<FrameworkElement,Button> MakeIntroScreen()
    {
        var content = new StackPanel();
        var title = new TextBlock() { Height = 45.0, Text = "Super BreakAway!", FontSize = 30.0, HorizontalAlignment = HorizontalAlignment.Center };
        var subTitle = new TextBlock() { Height = 30.0, HorizontalAlignment = HorizontalAlignment.Center, Text = "Use CURSOR KEYS to move the paddle" };
        var button = new Button() { Content = "Play" };
        content.Children.Add(title);
        content.Children.Add(subTitle);
        content.Children.Add(button);
        return new Tuple<FrameworkElement,Button>(content,button);
    }


    public static Tuple<FrameworkElement,MediaElement> MakeVictoryScreen(int remaining)
    {
        var content = new StackPanel();
        var title = new TextBox() { Text = (remaining > 0) ? string.Format("THE END\r\nleft {0} bricks", remaining) : "VICTORY!!!\r\nAll bricks cleared!!!" };
        var sound = new MediaElement() { LoadedBehavior = MediaState.Manual, Source = new System.Uri(remaining > 0 ? "boooo.wav" : "happykids.wav", System.UriKind.Relative) };
        content.Children.Add(title);
        content.Children.Add(sound);
        sound.Play();
        return new Tuple<FrameworkElement,MediaElement>(content,sound);
    }


    public static FrameworkElement MakeGameScreen(GameData dat)
    {
        var title = new TextBlock() { Height = 45.0, Width = CANWIDTH, Text = "Super BreakAway!", FontSize = 30.0, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        var subTitle = new TextBlock() { Height = 30.0, Width = CANWIDTH, Text = "written in F#, by Brian McNamara, translated to C# by Avner and Hillel", FontSize = 10.0, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        var game = new Border() { BorderThickness = new Thickness(1.0f), BorderBrush = BLACK, Child = dat.canvas };
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Auto) });
        for (int i = 1; i <= 3; i++) grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.0, GridUnitType.Auto) });
        grid.Children.Add(title); Grid.SetColumn(title,0); Grid.SetRow(title, 0);
        grid.Children.Add(subTitle); Grid.SetColumn(subTitle,0); Grid.SetRow(subTitle,1);
        grid.Children.Add(game); Grid.SetColumn(game,0); Grid.SetRow(game,2);

        return grid;
    }

    
    public static GameData InitializeGameData()
    {
        var dat = new GameData();
        dat.remaining = WIDTH * HEIGHT - 1;
        dat.active = 1;
        dat.canvas = new Canvas() { Width = CANWIDTH, Height = CANHEIGHT, Background = WHITE };

        // Textbox...
        dat.tb = new TextBlock() { Height = 25.0, Width = CANWIDTH, FontSize = 20.0, Text = string.Format("{0} bricks remain, {1} balls active", dat.remaining, dat.active) };
        dat.canvas.AddAt(TOPPAD + PADHEIGHT + 5.0f, 10.0f, dat.tb);

        // Paddle...
        dat.paddle = new Rectangle() { Width = PADWIDTH, Height = PADHEIGHT, Fill = BLACK };
        dat.canvas.AddAt(TOPPAD, CANWIDTH / 2.0f, dat.paddle);
        
        // Lines and Blocks...
        dat.lines = new Line[HEIGHT, WIDTH]; // trailer lines
        dat.blocks = new Ellipse[HEIGHT, WIDTH];
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                var f = (float)x / (float)WIDTH; Color fill;
                if (f < 0.25) fill = Color.FromArgb(0xFF, 0xFF, (byte)(f * 1020), 0);
                else if (f < 0.5) fill = Color.FromArgb(0xFF, (byte)((0.75 - f) * 1020), 0xFF, 0);
                else if (f < 0.75) fill = Color.FromArgb(0xFF, 0, (byte)((1.5 - f) * 1020), (byte)((f - 0.5) * 1020));
                else fill = Color.FromArgb(0xFF, (byte)((f - 0.75) * 1020), 0, 0xFF);
                var e = new Ellipse() { Width = SIZE, Height = SIZE, Fill = new SolidColorBrush(fill) };
                dat.canvas.AddAt((float)(SIZE * y), (float)(SIZE * x), e);
                dat.blocks[y, x] = e;
            }
        }

        // Beeps...
        dat.beeps = new MediaElement[NUMBEEP];
        dat.blockBeeps = new MediaElement[NUMBEEP];
        for (int i = 0; i < NUMBEEP; i++)
        {
            dat.beeps[i] = new MediaElement() { LoadedBehavior = MediaState.Manual, Source = new System.Uri("BEEPPURE.wav", System.UriKind.Relative) };
            dat.canvas.Children.Add(dat.beeps[i]);
            dat.blockBeeps[i] = new MediaElement() { LoadedBehavior = MediaState.Manual, Source = new System.Uri("BEEPDOUB.wav", System.UriKind.Relative) };
            dat.canvas.Children.Add(dat.blockBeeps[i]);
        }

        // Speeds...
        dat.xs = new float[HEIGHT, WIDTH];
        dat.ys = new float[HEIGHT, WIDTH];
        var mid = WIDTH / 2;
        dat.ys[HEIGHT - 1, mid] = 4.0f;
        dat.xs[HEIGHT - 1, mid] = 0.4f;

        // Start the first block dropping...
        var initBlock = dat.blocks[HEIGHT - 1, mid];
        Canvas.SetTop(initBlock, Canvas.GetTop(initBlock) + SIZE);
        dat.lines[HEIGHT - 1, mid] = new Line() { X1 = Canvas.GetLeft(initBlock), X2 = Canvas.GetLeft(initBlock), Y1 = Canvas.GetTop(initBlock), Y2 = Canvas.GetTop(initBlock), StrokeThickness = SIZE / 3.0, Stroke = GRAY };
        dat.canvas.Children.Add(dat.lines[HEIGHT - 1, mid]);
        dat.lastTick = DateTime.Now;

        return dat;
    }


    public static void UpdateBalls(GameData dat)
    {
        var now = DateTime.Now;
        var interval = now.Subtract(dat.lastTick).TotalSeconds;
        dat.lastTick = now;

        var wantPaddleBeep = false;
        var wantBlockBeep = false;

        var leftPad = Canvas.GetLeft(dat.paddle);
        for (int y = 0; y <= HEIGHT - 1; y++)
        {
            for (int x = 0; x <= WIDTH - 1; x++)
            {
                if (dat.ys[y, x] != 0.0)
                {
                    var origCenteredX = Canvas.GetLeft(dat.blocks[y, x]) + HALFSIZE;
                    var origCenteredY = Canvas.GetTop(dat.blocks[y, x]) + HALFSIZE;
                    // compute new X
                    var newX = dat.xs[y, x] * LINEVELOCITY * interval + (float)Canvas.GetLeft(dat.blocks[y, x]);
                    if (newX < 0.0) { newX = 0; dat.xs[y, x] *= -1; }
                    if (newX > CANWIDTH - E) { newX = CANWIDTH - E; dat.xs[y, x] *= -1; }
                    var newY = dat.ys[y, x] * LINEVELOCITY * interval + (float)Canvas.GetTop(dat.blocks[y, x]);
                    if (newY < 0.0) { newY = 0; dat.ys[y, x] *= -1; }

                    // update position
                    Canvas.SetTop(dat.blocks[y, x], newY);
                    Canvas.SetLeft(dat.blocks[y, x], newX);
                    // update trailer line
                    var newCenteredX = Canvas.GetLeft(dat.blocks[y, x]) + HALFSIZE;
                    var newCenteredY = Canvas.GetTop(dat.blocks[y, x]) + HALFSIZE;
                    var dx = origCenteredX - newCenteredX;
                    var dy = origCenteredY - newCenteredY;
                    var dd = Math.Sqrt(dx * dx + dy * dy);
                    dat.lines[y, x].X2 = newCenteredX;
                    dat.lines[y, x].Y2 = newCenteredY;
                    dat.lines[y, x].X1 = 20.0 * dx/dd + newCenteredX;
                    dat.lines[y, x].Y1 = 20.0 * dy/dd + newCenteredY;

                    var top = Canvas.GetTop(dat.blocks[y, x]);
                    if (top >= TOPPAD && top < TOPPAD + PADHEIGHT)
                    {
                        // see if hit paddle
                        var left = Canvas.GetLeft(dat.blocks[y, x]);
                        if (left >= leftPad && left < leftPad + PADWIDTH)
                        {
                            dat.ys[y, x] = -Math.Abs(dat.ys[y, x]);
                            dat.xs[y, x] = dat.xs[y, x] + ((float)left - (float)leftPad - HALFPADWIDTH) / HALFPADWIDTH;
                            if (dat.xs[y, x] == 0) dat.xs[y, x] = E;
                            wantPaddleBeep = true;
                        }
                    }
                    else if (top < BOTBRICKS)
                    {
                        //// see if hit a brick
                        var left = Canvas.GetLeft(dat.blocks[y, x]);
                        var brickX = (int)(left / SIZE);
                        var brickY = (int)(top / SIZE);
                        var thereIsStillABrickHere = dat.ys[brickY, brickX] == 0.0;
                        if (thereIsStillABrickHere)
                        {
                            var brick = dat.blocks[brickY, brickX];
                            var t = Canvas.GetTop(brick);
                            var l = Canvas.GetLeft(brick);
                            var intersect = left >= l && left < l + SIZE && top >= t && top < t + SIZE;
                            if (intersect)
                            {
                                dat.remaining -= 1;
                                dat.active += 1;
                                dat.tb.Text = string.Format("{0} bricks remain, {1} balls active", dat.remaining, dat.active);
                                var side = hitSide((float)(l - left), (float)(t - top), dat.xs[y, x], dat.ys[y, x]);
                                if (side)
                                {
                                    dat.xs[y, x] *= -1;
                                }
                                else
                                {
                                    dat.ys[y, x] *= -1;
                                }

                                dat.ys[brickY, brickX] = (float)(SIZE * (RNG.NextDouble() + 1.0) / 2.1);
                                dat.xs[brickY, brickX] = (float)(SIZE * (RNG.NextDouble() - 0.5));
                                if (dat.xs[brickY, brickX] == 0) dat.xs[brickY, brickX] = E;
                                Canvas.SetTop(brick, t + SIZE * 1.5);
                                var initBlock = dat.blocks[brickY, brickX];
                                dat.lines[brickY, brickX] = new Line() { X1 = Canvas.GetLeft(initBlock), X2 = Canvas.GetLeft(initBlock), Y1 = Canvas.GetTop(initBlock), Y2 = Canvas.GetTop(initBlock), StrokeThickness = SIZE / 3.0, Stroke = GRAY };
                                dat.canvas.Children.Add(dat.lines[brickY, brickX]);
                                wantBlockBeep = true;
                            }
                        }
                    }
                    else if (top > CANHEIGHT)
                    {
                        dat.xs[y, x] = 0.0f;
                        dat.ys[y, x] = 0.0f;
                        dat.canvas.Children.Remove(dat.blocks[y, x]);
                        dat.canvas.Children.Remove(dat.lines[y, x]);
                        dat.active -= 1;
                        dat.tb.Text = string.Format("{0} bricks remain, {1} balls active", dat.remaining, dat.active);
                    }
                }
            }

        }

        dat.curBeep = (dat.curBeep + 1) % NUMBEEP;

        if (wantPaddleBeep)
        {
            dat.beeps[dat.curBeep].Stop();
            dat.beeps[dat.curBeep].Play();
        }
        if (wantBlockBeep)
        {
            dat.blockBeeps[dat.curBeep].Stop();
            dat.blockBeeps[dat.curBeep].Play();
        }


    }

    public static void MovePaddle(GameData dat)
    {
        var interval = DateTime.Now.Subtract(dat.lastTick).TotalSeconds;

        var pos = Canvas.GetLeft(dat.paddle);
        if (Keyboard.IsKeyDown(Key.Left) && pos > 0) Canvas.SetLeft(dat.paddle, pos - PADVELOCITY * interval);
        if (Keyboard.IsKeyDown(Key.Right) && pos < maxWidth) Canvas.SetLeft(dat.paddle, pos + PADVELOCITY * interval);
    }


    // screen coordinates, a ball hit a block(0-SIZE,0-size) at point
    // (x,y) with velocity (dx,dy) - did it hit the side of the brick?
    static bool hitSide(float x, float y, float dx, float dy)
    {
        var ballSlope = -dy / dx;
        if (dy > 0.0)
        {
            if (dx < 0.0)
            {
                // it's going 'down-left'
                var s = y / (SIZE - x);
                return ballSlope < s;
            }
            else
            {
                // it's going 'down-right'
                var s = -y / x;
                return ballSlope > s;
            }
        }
        else
        {
            if (dx > 0.0)
            {
                // it's going 'up-right'
                var s = (SIZE - y) / x;
                return ballSlope < s;
            }
            else
            {
                // it's going 'up-left'
                var s = -(SIZE - y) / (SIZE - x);
                return ballSlope > s;
            }
        }
    }




    static internal void AddAt(this Canvas canvas, float top, float left, UIElement element)
    {
        Canvas.SetTop(element, top);
        Canvas.SetLeft(element, left);
        canvas.Children.Add(element);
    }

}
