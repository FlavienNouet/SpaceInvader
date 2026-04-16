namespace SpaceInvader;

public class Bunker : SimpleObject
{
    private const int BunkerWidth = 60;
    private const int BunkerHeight = 40;

    private static readonly Color BunkerColor = Color.FromArgb(255, 0, 0, 0);

    public Bunker(Vecteur2d position)
        : base(GameObject.Side.Neutral, position, 3, CreateBunkerImage())
    {
    }

    protected override void OnCollision(Missile missile, int numberOfPixelsInCollision)
    {
        ArgumentNullException.ThrowIfNull(missile);

        missile.Lives = Math.Max(0, missile.Lives - 1);
    }

    public override void Update(double deltaTimeSeconds)
    {
    }

    private static Bitmap CreateBunkerImage()
    {
        Bitmap bitmap = new(BunkerWidth, BunkerHeight);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);

        using Brush baseBrush = new SolidBrush(BunkerColor);
        using Brush shadowBrush = new SolidBrush(BunkerColor);

        graphics.FillRectangle(baseBrush, 0, 8, BunkerWidth, BunkerHeight - 8);
        graphics.FillRectangle(baseBrush, 8, 0, BunkerWidth - 16, 18);
        graphics.FillRectangle(baseBrush, 16, 0, BunkerWidth - 32, 10);
        graphics.FillRectangle(shadowBrush, 0, BunkerHeight - 8, BunkerWidth, 8);

        

        return bitmap;
    }
}