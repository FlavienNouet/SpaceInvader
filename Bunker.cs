namespace SpaceInvader;

public class Bunker : SimpleObject
{
    private const int BunkerWidth = 60;
    private const int BunkerHeight = 40;

    public Bunker(Vecteur2d position)
        : base(position, 3, CreateBunkerImage())
    {
    }

    public override void Update(double deltaTimeSeconds)
    {
    }

    private static Bitmap CreateBunkerImage()
    {
        Bitmap bitmap = new(BunkerWidth, BunkerHeight);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);

        using Brush baseBrush = new SolidBrush(Color.ForestGreen);
        using Brush shadowBrush = new SolidBrush(Color.DarkGreen);

        graphics.FillRectangle(baseBrush, 0, 8, BunkerWidth, BunkerHeight - 8);
        graphics.FillRectangle(baseBrush, 8, 0, BunkerWidth - 16, 18);
        graphics.FillRectangle(baseBrush, 16, 0, BunkerWidth - 32, 10);
        graphics.FillRectangle(shadowBrush, 0, BunkerHeight - 8, BunkerWidth, 8);

        graphics.FillRectangle(Brushes.Transparent, 18, 8, 8, 8);
        graphics.FillRectangle(Brushes.Transparent, 34, 8, 8, 8);
        graphics.FillRectangle(Brushes.Transparent, 26, 16, 8, 8);

        return bitmap;
    }
}