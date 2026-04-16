using System.Media;

namespace SpaceInvader;
/// <summary>
/// Représente le vaisseau que le joueur controle dans le jeu. Le vaisseau peut se déplacer horizontalement et tirer des missiles pour attaquer les ennemis. Il dispose d'une animation de tir et d'un son de déplacement pour améliorer l'expérience de jeu.
/// </summary>
public class PlayerSpaceship : SpaceShip
{
    private readonly SoundPlayer? movementSound; // Son de déplacement du vaisseau
    private bool isMovementSoundPlaying; // Indique si le son de déplacement est actuellement en train de jouer
    public PlayerSpaceship(Game game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : base(GameObject.Side.Ally, game, position, lives, image, gameSize, playerSpeedPixelPerSecond)
    {
        string soundPath = Path.Combine(AppContext.BaseDirectory, "assets", "Audio", "ufo_lowpitch.wav");

        if (File.Exists(soundPath))
        {
            movementSound = new SoundPlayer(soundPath);
        }

        isMovementSoundPlaying = false;
    }

     public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        // Dessiner le vaisseau et les vies restantes à côté
        graphics.DrawSprite(Image, Position)
            .DrawLives(Lives, Position, Image);
    }

    public override void Update(double deltaTimeSeconds)
    {
        if (IsKeyDown(Keys.Space))
        {
            Shoot();
        }

        // Gestion des mouvements horizontaux du vaisseau en fonction des touches fléchées
        double moveDistance = PlayerSpeedPixelPerSecond * deltaTimeSeconds;
        bool moveLeft = IsKeyDown(Keys.Left) || IsKeyDown(Keys.A);
        bool moveRight = IsKeyDown(Keys.Right) || IsKeyDown(Keys.D);
        bool isMovingHorizontally = moveLeft || moveRight;

        if (moveLeft)
        {
            // Bloque le mouvement à gauche pour éviter de sortir de l'écran
            Position = new Vecteur2d(Math.Max(0, Position.X - moveDistance), Position.Y);
        }

        if (moveRight)
        {
            // Bloque le mouvement à droite pour éviter de sortir de l'écran
            double maxX = Math.Max(0, GameSize.Width - Image.Width);
            Position = new Vecteur2d(Math.Min(maxX, Position.X + moveDistance), Position.Y);
        }
                if (isMovingHorizontally && !Game.IsMuted)
        {
            StartMovementSound();
        }
        else
        {
            StopMovementSound();
        }
    }

    private void StartMovementSound()
    {
        if (movementSound is null || isMovementSoundPlaying)
        {
            return;
        }

        movementSound.PlayLooping();
        isMovementSoundPlaying = true;
    }

    private void StopMovementSound()
    {
        if (movementSound is null || !isMovementSoundPlaying)
        {
            return;
        }

        movementSound.Stop();
        isMovementSoundPlaying = false;
    }
    }

// Extensions graphiques pour dessiner le vaisseau du joueur et les vies restantes
internal static class PlayerSpaceshipGraphicsExtensions
{
    // Dessine le sprite du vaisseau à la position spécifiée
    public static Graphics DrawSprite(this Graphics graphics, Bitmap image, Vecteur2d position)
    {
        graphics.DrawImage(image, (float)position.X, (float)position.Y, image.Width, image.Height);
        return graphics;
    }

    // Dessine les vies restantes à côté du vaisseau en utilisant des cœurs rouges
    public static Graphics DrawLives(this Graphics graphics, int lives, Vecteur2d position, Bitmap image)
    {
        int displayedLives = Math.Max(0, lives);
        float startX = (float)position.X + image.Width + 8f;
        float startY = (float)position.Y + 8f;
        const float heartSize = 12f;
        const float spacing = 4f;

        for (int i = 0; i < displayedLives; i++)
        {
            float heartX = startX + i * (heartSize + spacing);
            DrawHeart(graphics, heartX, startY, heartSize);
        }
        return graphics;
    }
    private static void DrawHeart(Graphics graphics, float x, float y, float size)
    {
        using SolidBrush fillBrush = new(Color.Red);
        using Pen borderPen = new(Color.DarkRed);

        float half = size / 2f;
        float quarter = size / 4f;

        graphics.FillEllipse(fillBrush, x, y, half, half);
        graphics.FillEllipse(fillBrush, x + half, y, half, half);

        PointF[] triangle =
        [
            new PointF(x, y + quarter),
            new PointF(x + size, y + quarter),
            new PointF(x + half, y + size)
        ];

        graphics.FillPolygon(fillBrush, triangle);
        graphics.DrawEllipse(borderPen, x, y, half, half);
        graphics.DrawEllipse(borderPen, x + half, y, half, half);
        graphics.DrawPolygon(borderPen, triangle);
    }
}