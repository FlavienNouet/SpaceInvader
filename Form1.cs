namespace SpaceInvader;
/// <summary>
///  Fenetre principale du jeu, gère le rendu et les interactions utilisateur.
/// </summary>
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

        // Initialisation du jeu et du timer pour les frames
        game = new Game(ClientSize);
        lastFrameUtc = DateTime.UtcNow;

        frameTimer.Interval = 16;
        frameTimer.Tick += FrameTimer_Tick;
        MouseClick += Form1_MouseClick;
        frameTimer.Start();
    }

    // Rendu du jeu à chaque frame
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        game.Draw(e.Graphics);
    }

    // Boucle qui tourne en boucle => "Game Loop"
    private void FrameTimer_Tick(object? sender, EventArgs e)
    {
        DateTime now = DateTime.UtcNow;
        double deltaTimeSeconds = (now - lastFrameUtc).TotalSeconds;
        lastFrameUtc = now;

        game.Update(deltaTimeSeconds);
        Invalidate();
    }

    // Click de la souris au jeu
    private void Form1_MouseClick(object? sender, MouseEventArgs e)
    {
        game.HandleMouseClick(e.Location);
        Invalidate();
    }

    // Nettoyage des ressources lorsque la fenêtre est fermée
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Game.StopBackgroundMusic();
        base.OnFormClosed(e);
    }
}