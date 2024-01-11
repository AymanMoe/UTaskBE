namespace UTask.Data.Dtos
{
    public class RegisterationDto
    {
        //string email, string password, string firstName, string lastName, UserType type
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Type { get; set; }
        public AddressDto Address { get; set; }
    }
}
