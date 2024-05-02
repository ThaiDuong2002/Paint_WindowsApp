using System.Windows;
using System.Windows.Media;

namespace Contact;

public class Util
{
    public static bool IsBetween(double x, double x1, double x2)
    {
        if (x1 > x2)
            return x < x1 && x > x2;
        else
            return x < x2 && x > x1;
    }
    public static float GetAlphaAngleRadian(double RotateAngle)
    {
        float a;
        if (RotateAngle >= 0 && RotateAngle < 180) { a = (float)(RotateAngle / (180 / Math.PI)); }
        else
        {
            a = (float)((RotateAngle - 360) / (180 / Math.PI));
        }

        a %= (float)(2 * Math.PI);

        return a;
    }

    public static Tuple<double, double> GetCenterPointTranslation(CustomPoint oldCenterPoint, CustomPoint newCenterPoint, ShapePoint shape)
    {
        var ir = new RotateTransform(shape.RotateAngle, oldCenterPoint.X, oldCenterPoint.Y);
        var fr = new RotateTransform(shape.RotateAngle, newCenterPoint.X, newCenterPoint.Y);
        var ip = ir.Transform(new Point(shape.TopLeft.X, shape.TopLeft.Y));
        var fp = fr.Transform(new Point(shape.TopLeft.X, shape.TopLeft.Y));

        var txx = ip.X - fp.X;
        var tyy = ip.Y - fp.Y;

        return new Tuple<double, double>(txx, tyy);
    }
}

//to select reference value of X or Y of CustomPoint cord 
public class Cord
{
    public CustomPoint Point = new();

    public Cord()
    {
    }

    public Cord(CustomPoint point)
    {
        Point = point;
    }

    virtual public double GetCord()
    {
        return Point.X;
    }
    virtual public void SetCord(double x)
    {
        Point.X += x;
    }
}

public class CordY : Cord
{
    public CordY() { }

    public CordY(CustomPoint point) : base(point) { }

    public override double GetCord()
    {
        return Point.Y;
    }

    public override void SetCord(double x)
    {
        Point.Y += x;
    }
}
