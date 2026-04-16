namespace SpaceInvader;

public class EnemyBlock : GameObject
{
    private readonly HashSet<SpaceShip> enemyShips = new();
    private readonly int baseWidth;
    private readonly Size gameSize;
    private readonly Game? game;
    private readonly Random random = new();
    private double randomShootProbability = 0.05;
    private int lineCount;
    private int horizontalDirection = 1;
    private double horizontalSpeedPixelPerSecond = 30;

    private const double DownStepPixels = 20;
    private const double HorizontalSpeedIncrease = 8;

    public Size Size { get; private set; }

    public Vecteur2d Position { get; private set; }

    public EnemyBlock(Vecteur2d position, int baseWidth)
    : this(position, baseWidth, SystemInformation.VirtualScreen.Size, null)
    {
    }

    public EnemyBlock(Vecteur2d position, int baseWidth, Size gameSize, Game? game)
    : base(GameObject.Side.Enemy)
    {
    
        ArgumentNullException.ThrowIfNull(position);

        Position = position;
        this.baseWidth = Math.Max(1, baseWidth);
        this.gameSize = gameSize;
        this.game = game;
        Size = Size.Empty;
    }

    public void AddLine(int nbShips, int nbLives, Bitmap shipImage, Bitmap? alternateImage = null)
    {
        ArgumentNullException.ThrowIfNull(shipImage);

        if (nbShips <= 0)
        {
            return;
        }

        double lineY = Position.Y + lineCount * (shipImage.Height + 10);
        const double preferredGap = 26;
        double preferredStep = shipImage.Width + preferredGap;
        double maxStepToFitBlock = nbShips > 1
            ? Math.Max(shipImage.Width, (baseWidth - shipImage.Width) / (nbShips - 1))
            : 0;
        double horizontalStep = nbShips > 1 ? Math.Min(preferredStep, maxStepToFitBlock) : 0;

        double lineUsedWidth = nbShips == 1
            ? shipImage.Width
            : shipImage.Width + (nbShips - 1) * horizontalStep;
        double lineStartX = Position.X + (baseWidth - lineUsedWidth) / 2.0;

        for (int i = 0; i < nbShips; i++)
        {
            double shipX = lineStartX + i * horizontalStep;
            SpaceShip enemy = new SpaceShip(game, new Vecteur2d(shipX, lineY), nbLives, (Bitmap)shipImage.Clone(), gameSize, 200, alternateImage);
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
         if (!IsAlive())
        {
            return;
        }

        foreach (SpaceShip ship in enemyShips)
        {
            if (ship.IsAlive())
            {
                ship.Update(deltaTimeSeconds);
            }
        }

        TryShoot(deltaTimeSeconds);

        double horizontalDelta = horizontalDirection * horizontalSpeedPixelPerSecond * deltaTimeSeconds;
        bool hitLeftBorder = Position.X + horizontalDelta < 0;
        bool hitRightBorder = Position.X + Size.Width + horizontalDelta > gameSize.Width;

        if (hitLeftBorder || hitRightBorder)
        {
            horizontalDirection *= -1;
            horizontalSpeedPixelPerSecond += HorizontalSpeedIncrease;
            MoveShips(0, DownStepPixels);
            randomShootProbability += 0.02;
            UpdateSize();
            return;
        }

        MoveShips(horizontalDelta, 0);
        UpdateSize();
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


    public bool TryGetNearestEnemyCenter(Vecteur2d fromPosition, out Vecteur2d targetPosition)
    {
        ArgumentNullException.ThrowIfNull(fromPosition);

        List<SpaceShip> aliveShips = enemyShips.Where(ship => ship.IsAlive()).ToList();
        if (aliveShips.Count == 0)
        {
            targetPosition = new Vecteur2d();
            return false;
        }

        SpaceShip nearestShip = aliveShips
            .OrderBy(ship =>
            {
                double centerX = ship.Position.X + ship.Image.Width / 2.0;
                double centerY = ship.Position.Y + ship.Image.Height / 2.0;
                double dx = centerX - fromPosition.X;
                double dy = centerY - fromPosition.Y;
                return dx * dx + dy * dy;
            })
            .First();

        targetPosition = new Vecteur2d(
            nearestShip.Position.X + nearestShip.Image.Width / 2.0,
            nearestShip.Position.Y + nearestShip.Image.Height / 2.0);
        return true;
    }


    private void MoveShips(double deltaX, double deltaY)
    {
        foreach (SpaceShip ship in enemyShips)
        {
            ship.Position = new Vecteur2d(ship.Position.X + deltaX, ship.Position.Y + deltaY);
        }
    }
     private void TryShoot(double deltaTimeSeconds)
    {
        if (game is null)
        {
            return;
        }

        foreach (SpaceShip ship in enemyShips)
        {
            if (!ship.IsAlive())
            {
                continue;
            }

            double randomValue = random.NextDouble();
            if (randomValue <= randomShootProbability * deltaTimeSeconds)
            {
                if (game.SelectedDifficulty == Game.Difficulty.Hard)
                {
                    Vecteur2d playerCenter = new(
                        game.PlayerShip.Position.X + game.PlayerShip.Image.Width / 2.0,
                        game.PlayerShip.Position.Y + game.PlayerShip.Image.Height / 2.0);
                    ship.ShootAt(playerCenter);
                }
                else
                {
                    ship.Shoot(true);
                }
            }
        }
    }
}