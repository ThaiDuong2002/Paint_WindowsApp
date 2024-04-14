using Contact;

namespace PaintProject
{
    public class ShapeSingleton
    {
        private readonly ShapeFactory _shapeFactory;
        private static ShapeSingleton? _instance = null;
        private ShapeSingleton()
        {
            _shapeFactory = new ShapeFactory();
        }
        public static ShapeSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ShapeSingleton();
                }
                return _instance;
            }
        }
        public IEnumerable<IShape> Shapes => _shapeFactory.GetShapes();
    }
}
