using System.ComponentModel.DataAnnotations.Schema;

namespace MAS_Implementation.Models;

[ComplexType]
public class ContactInfo
{
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }

    public ContactInfo(string phone, string email)
    {
        Phone = phone;
        Email = email;
    }
    
    public ContactInfo(string phone, string email, string? address)
        : this(phone, email)
    {
        Address = address;
    }
    protected ContactInfo() { }

    public override string ToString() => $"Phone Number: {Phone}\nEmail: {Email}\nAddress: {Address}";
}