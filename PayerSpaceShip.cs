namespace SpaceInvader;

public class PlayerSpaceship : SpaceShip
{
    public PlayerSpaceship(Game game, Vecteur2d position, int lives, Bitmap image, Size gameSize, double playerSpeedPixelPerSecond = 200)
        : base(game, position, lives, image, gameSize, playerSpeedPixelPerSecond)
    {
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