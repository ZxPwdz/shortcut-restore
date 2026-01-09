using System;
using System.Collections.Generic;

namespace ShortcutRestore.Models
{
    public class Preset
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastUsed { get; set; }
        public List<IconPosition> Icons { get; set; } = new();

        public Preset()
        {
        }

        public Preset(string name, List<IconPosition> icons)
        {
            Name = name;
            Icons = icons;
        }
    }
}
