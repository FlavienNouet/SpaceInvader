namespace SpaceInvader;

public class Bunker : SimpleObject
{
    private const int BunkerWidth = 60;
    private const int BunkerHeight = 40;

    private static readonly Color CollisionColor = Color.FromArgb(255, 0, 0, 0);

    public Bunker(Vecteur2d position)
        : base(position, 3, CreateBunkerImage())
    {
    }

public override void Collision(Missile missile)
    {
        ArgumentNullException.ThrowIfNull(missile);

        if (!IsAlive() || !missile.IsAlive())
        {
            return;
        }

        Rectangle bunkerRectangle = GetObjectRectangle(Position, Image.Width, Image.Height);
        Rectangle missileRectangle = GetObjectRectangle(missile.Position, missile.Image.Width, missile.Image.Height);

        if (!bunkerRectangle.IntersectsWith(missileRectangle))
        {
            return;
        }

        bool hasCollision = false;

        for (int missileLocalY = 0; missileLocalY < missile.Image.Height && !hasCollision; missileLocalY++)
        {
            for (int missileLocalX = 0; missileLocalX < missile.Image.Width; missileLocalX++)
            {
                Color missilePixel = missile.Image.GetPixel(missileLocalX, missileLocalY);

                if (missilePixel.A == 0)
                {
                    continue;
                }

                int screenX = missileRectangle.Left + missileLocalX;
                int screenY = missileRectangle.Top + missileLocalY;
                int bunkerLocalX = screenX - bunkerRectangle.Left;
                int bunkerLocalY = screenY - bunkerRectangle.Top;

                if (!IsInsideImage(bunkerLocalX, bunkerLocalY, Image.Width, Image.Height))
                {
                    continue;
                }

                Color bunkerPixel = Image.GetPixel(bunkerLocalX, bunkerLocalY);

                if (bunkerPixel.ToArgb() != CollisionColor.ToArgb())
                {
                    continue;
                }

                Image.SetPixel(bunkerLocalX, bunkerLocalY, Color.Transparent);
                hasCollision = true;
                break;
            }
        }

        if (hasCollision)
        {
            missile.Lives = Math.Max(0, missile.Lives - 1);
        }
    }
    public override void Update(double deltaTimeSeconds)
    {
    }

    private static Bitmap CreateBunkerImage()
    {
        Bitmap bitmap = new(BunkerWidth, BunkerHeight);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);

        using Brush baseBrush = new SolidBrush(CollisionColor);
        using Brush shadowBrush = new SolidBrush(CollisionColor);

        graphics.FillRectangle(baseBrush, 0, 8, BunkerWidth, BunkerHeight - 8);
        graphics.FillRectangle(baseBrush, 8, 0, BunkerWidth - 16, 18);
        graphics.FillRectangle(baseBrush, 16, 0, BunkerWidth - 32, 10);
        graphics.FillRectangle(shadowBrush, 0, BunkerHeight - 8, BunkerWidth, 8);

        

        return bitmap;
    }

     private static Rectangle GetObjectRectangle(Vecteur2d position, int width, int height)
    {
        return new Rectangle((int)Math.Floor(position.X), (int)Math.Floor(position.Y), width, height);
    }

    private static bool IsInsideImage(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
}