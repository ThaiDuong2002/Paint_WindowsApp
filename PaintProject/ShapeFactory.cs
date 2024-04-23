using Contact;
using System.IO;
using System.Reflection;

namespace PaintProject
{
    public class ShapeFactory
    {
        readonly Dictionary<string, IShape> _shapes = new Dictionary<string, IShape>();
        public ShapeFactory()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string? folder = Path.GetDirectoryName(exePath);

            if (folder == null)
            {
                return;
            }

            FileInfo[] files = new DirectoryInfo(folder).GetFiles("*.dll");
            foreach (FileInfo file in files)
            {
                Assembly assembly = Assembly.LoadFrom(file.FullName);
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsClass && typeof(IShape).IsAssignableFrom(type) && type != typeof(CustomPoint))
                    {
                        IShape shape = (IShape)Activator.CreateInstance(type);
                        if (shape != null)
                        {
                            _shapes.Add(shape.Name, shape);
                        }
                    }
                }
            }
        }
        public IEnumerable<IShape> GetShapes()
        {
            List<IShape> shapes = [.. _shapes.Values];
            return shapes;
        }
        public IEnumerable<string> GetShapeNames()
        {
            List<string> names = [.. _shapes.Keys];
            return names;
        }
    }
}
