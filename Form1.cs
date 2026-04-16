namespace SpaceInvader;

public partial class Form1 : Form
{
    private readonly Game game;
    private readonly System.Windows.Forms.Timer frameTimer = new();
    private DateTime lastFrameUtc;

    public Form1()
    {
        InitializeComponent();
        DoubleBuffered = true;
        BackColor = Color.Black;

        game = new Game(ClientSize);
        lastFrameUtc = DateTime.UtcNow;

        frameTimer.Interval = 16;
        frameTimer.Tick += FrameTimer_Tick;
        MouseClick += Form1_MouseClick;
        frameTimer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        game.Draw(e.Graphics);
    }

    private void FrameTimer_Tick(object? sender, EventArgs e)
    {
        DateTime now = DateTime.UtcNow;
        double deltaTimeSeconds = (now - lastFrameUtc).TotalSeconds;
        lastFrameUtc = now;

        game.Update(deltaTimeSeconds);
        Invalidate();
    }

    private void Form1_MouseClick(object? sender, MouseEventArgs e)
    {
        game.HandleMouseClick(e.Location);
        Invalidate();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Game.StopBackgroundMusic();
        base.OnFormClosed(e);
    }
}