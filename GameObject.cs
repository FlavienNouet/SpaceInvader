namespace SpaceInvader;

public abstract class GameObject
{
    public enum Side
    {
        Ally,
        Enemy,
        Neutral
    }

    public Side Camp { get; }

    protected GameObject(Side camp)
    {
        Camp = camp;
    }
    public abstract void Update(double deltaTimeSeconds);

    public abstract void Draw(Graphics graphics);

    public abstract bool IsAlive();

    public abstract void Collision(Missile missile);
}