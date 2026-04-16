using System.Media;

namespace SpaceInvader;

public class PlayerSpaceship : SpaceShip
{
    private readonly SoundPlayer? movementSound;
    private bool isMovementSoundPlaying;
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

        graphics.DrawSprite(Image, Position)
            .DrawLives(Lives, Position, Image);
    }

    public override void Update(double deltaTimeSeconds)
    {
        if (IsKeyDown(Keys.Space))
        {
            Shoot();
        }

        double moveDistance = PlayerSpeedPixelPerSecond * deltaTimeSeconds;
        bool moveLeft = IsKeyDown(Keys.Left) || IsKeyDown(Keys.A);
        bool moveRight = IsKeyDown(Keys.Right) || IsKeyDown(Keys.D);
        bool isMovingHorizontally = moveLeft || moveRight;

        if (moveLeft)
        {
            Position = new Vecteur2d(Math.Max(0, Position.X - moveDistance), Position.Y);
        }

        if (moveRight)
        {
            double maxX = Math.Max(0, GameSize.Width - Image.Width);
            Position = new Vecteur2d(Math.Min(maxX, Position.X + moveDistance), Position.Y);
        }
          if (isMovingHorizontally)
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

internal static class PlayerSpaceshipGraphicsExtensions
{
    public static Graphics DrawSprite(this Graphics graphics, Bitmap image, Vecteur2d position)
    {
        graphics.DrawImage(image, (float)position.X, (float)position.Y, image.Width, image.Height);
        return graphics;
    }

    public static Graphics DrawLives(this Graphics graphics, int lives, Vecteur2d position, Bitmap image)
    {
        string text = $"Vies: {lives}";
        float textX = (float)position.X + image.Width + 8f;
        float textY = (float)position.Y + 8f;

        graphics.DrawString(text, SystemFonts.DefaultFont, Brushes.White, textX, textY);
        return graphics;
    }
}