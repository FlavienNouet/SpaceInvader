namespace SpaceInvader;

public class Vecteur2d
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vecteur2d()
        : this(0, 0)
    {
    }

    public Vecteur2d(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double Norme()
    {
        return Math.Sqrt(X * X + Y * Y);
    }

    public Vecteur2d Additionner(Vecteur2d autre)
    {
        return new Vecteur2d(X + autre.X, Y + autre.Y);
    }

    public Vecteur2d Soustraire(Vecteur2d autre)
    {
        return new Vecteur2d(X - autre.X, Y - autre.Y);
    }

    public Vecteur2d Multiplier(double scalaire)
    {
        return new Vecteur2d(X * scalaire, Y * scalaire);
    }

    public double ProduitScalaire(Vecteur2d autre)
    {
        return X * autre.X + Y * autre.Y;
    }

    public override string ToString()
    {
        return $"({X}; {Y})";
    }

    public static Vecteur2d operator +(Vecteur2d gauche, Vecteur2d droite)
    {
        return gauche.Additionner(droite);
    }

    public static Vecteur2d operator -(Vecteur2d gauche, Vecteur2d droite)
    {
        return gauche.Soustraire(droite);
    }

    public static Vecteur2d operator *(Vecteur2d vecteur, double scalaire)
    {
        return vecteur.Multiplier(scalaire);
    }

    public static Vecteur2d operator *(double scalaire, Vecteur2d vecteur)
    {
        return vecteur.Multiplier(scalaire);
    }
}