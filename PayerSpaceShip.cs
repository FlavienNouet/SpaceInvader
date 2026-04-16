namespace SpaceInvader;

public class PlayerSpaceship : SpaceShip
{
    public PlayerSpaceship(Game game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : base(GameObject.Side.Ally, game, position, lives, image, gameSize, playerSpeedPixelPerSecond)
    {
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

        if (IsKeyDown(Keys.Left) || IsKeyDown(Keys.A))
        {
            Position = new Vecteur2d(Math.Max(0, Position.X - moveDistance), Position.Y);
        }

        if (IsKeyDown(Keys.Right) || IsKeyDown(Keys.D))
        {
            double maxX = Math.Max(0, GameSize.Width - Image.Width);
            Position = new Vecteur2d(Math.Min(maxX, Position.X + moveDistance), Position.Y);
        }
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