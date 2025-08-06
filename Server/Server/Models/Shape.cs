using System.Drawing;

namespace Server.Models
{
    public class Shape
    {
        public string type { get; set; }

        // Circle
        public int? x { get; set; }
        public int? y { get; set; }
        public int? radius { get; set; }

        // Rectangle
        public int? width { get; set; }
        public int? height { get; set; }

        // Line
        public int? x1 { get; set; }
        public int? y1 { get; set; }
        public int? x2 { get; set; }
        public int? y2 { get; set; }

        // Polygon
        public List<Point> points { get; set; }

        public string color { get; set; }
    }
}
