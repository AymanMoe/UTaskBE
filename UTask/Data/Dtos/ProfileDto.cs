namespace UTask.Data.Dtos
{
    public class ProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string Phone { get; set; }
        public AddressDto Address { get; set; }

    }
}