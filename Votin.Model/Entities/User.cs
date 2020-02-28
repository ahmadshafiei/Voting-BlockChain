namespace Voting.Model.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string PublicKey { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public Role Role { get; set; }
    }

    public enum Role
    {
        Voter = 1,
        Admin = 2
    }
}