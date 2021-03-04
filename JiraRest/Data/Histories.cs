using System;

#nullable disable

namespace JiraRest.Data
{
    public class Histories
    {
        public int id { get; set; }
        public User author { get; set; }
        public DateTime created { get; set; }
        public History[] items { get; set; }
    }
}