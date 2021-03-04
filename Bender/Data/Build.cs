using System;

namespace Bender.Data
{
    public class Build
    {
        public string? Description { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Archived { get; set; }
        public bool Released { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
