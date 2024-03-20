namespace UTask.Data.Dtos
{
    public class AddressDto
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string? Apartment { get; set; }
        public string? Notes { get; set; }

    }
}
