namespace ScruMster.Models
{
    public class User
    {
        public int UserID { get; set; }
        public int TeamID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Mail { get; set; }
        public string Position { get; set; }
        public bool Boss { get; set; }
    }
}