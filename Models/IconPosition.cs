namespace ShortcutRestore.Models
{
    public class IconPosition
    {
        public string Name { get; set; } = string.Empty;
        public string? Path { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public IconPosition()
        {
        }

        public IconPosition(string name, int x, int y, string? path = null)
        {
            Name = name;
            X = x;
            Y = y;
            Path = path;
        }

        public override string ToString() => $"{Name} ({X}, {Y})";
    }
}
