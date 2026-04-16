namespace SpaceInvader;

public class Bunker : SimpleObject
{

    public Bunker(Vecteur2d position)
        : base(GameObject.Side.Neutral, position, 3, Game.CreateBunkerImage())
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

}