namespace SpaceInvader;

public class EnemyBlock : GameObject
{
    private readonly HashSet<SpaceShip> enemyShips = new();
    private readonly int baseWidth;
    private int lineCount;

    public Size Size { get; private set; }

    public Vecteur2d Position { get; private set; }

    public EnemyBlock(Vecteur2d position, int baseWidth)
    {
        ArgumentNullException.ThrowIfNull(position);

        Position = position;
        this.baseWidth = Math.Max(1, baseWidth);
        Size = Size.Empty;
    }

    public void AddLine(int nbShips, int nbLives, Bitmap shipImage)
    {
        ArgumentNullException.ThrowIfNull(shipImage);

        if (nbShips <= 0)
        {
            return;
        }

        double lineY = Position.Y + lineCount * (shipImage.Height + 10);
        double freeWidth = Math.Max(0, baseWidth - shipImage.Width);
        double horizontalStep = nbShips > 1 ? freeWidth / (nbShips - 1) : 0;

        for (int i = 0; i < nbShips; i++)
        {
            double shipX = Position.X + i * horizontalStep;
            SpaceShip enemy = new SpaceShip(new Vecteur2d(shipX, lineY), nbLives, (Bitmap)shipImage.Clone());
            enemyShips.Add(enemy);
        }

        lineCount++;
        UpdateSize();
    }

    public void UpdateSize()
    {
        List<SpaceShip> aliveShips = enemyShips.Where(ship => ship.IsAlive()).ToList();

        if (aliveShips.Count == 0)
        {
            Size = Size.Empty;
            return;
        }

        double minX = aliveShips.Min(ship => ship.Position.X);
        double minY = aliveShips.Min(ship => ship.Position.Y);
        double maxX = aliveShips.Max(ship => ship.Position.X + ship.Image.Width);
        double maxY = aliveShips.Max(ship => ship.Position.Y + ship.Image.Height);

        Position = new Vecteur2d(minX, minY);
        Size = new Size(
            (int)Math.Ceiling(maxX - minX),
            (int)Math.Ceiling(maxY - minY));
    }

    public override void Update(double deltaTimeSeconds)
    {
    }

    public override void Draw(Graphics graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        foreach (SpaceShip ship in enemyShips)
        {
            if (ship.IsAlive())
            {
                ship.Draw(graphics);
            }
        }
    }

    public override bool IsAlive()
    {
        return enemyShips.Any(ship => ship.IsAlive());
    }

    public override void Collision(Missile missile)
    {
        ArgumentNullException.ThrowIfNull(missile);

        foreach (SpaceShip ship in enemyShips)
        {
            ship.Collision(missile);
        }

        UpdateSize();
    }
}