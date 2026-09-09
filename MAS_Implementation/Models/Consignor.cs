using System.ComponentModel.DataAnnotations;

namespace MAS_Implementation.Models;

public class Consignor
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;
    
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be null or whitespace.");
            if (value.Length > 150)
                throw new ArgumentException("Name cannot be longer than 150 characters.");
            _name = value;
        }
    }
    
    [Required]
    [MaxLength(50)]
    public string Email
    {
        get => _email;
        set
        {
            if (!EmailValidator.IsValidEmail(value))
                throw new ArgumentException("Email cannot be null/whitespace or does not look like email.");
            if (value.Length > 50)
                throw new ArgumentException("Email cannot be longer than 50 characters.");
            _email = value;
        }
    }
    
    [Required]
    [MaxLength(20)]
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be null or whitespace.");
            if (value.Length > 20) 
                throw new ArgumentException("Phone number cannot be longer than 20 characters.");
            _phoneNumber = value;
        }
    }

    public Cataloguer Cataloguer { get; private set; } = null!;
    public List<ConsignmentAgreement> ConsignmentAgreements { get; private set; } = new();

    public void AssignCataloguer(Cataloguer cataloguer)
    {
        ArgumentNullException.ThrowIfNull(cataloguer);
        if (Cataloguer == cataloguer) return;
        
        Cataloguer = cataloguer;
        cataloguer.AddConsignor(this);
    }
    
    public void AddConsignmentAgreement(ConsignmentAgreement consignmentAgreement)
    {
        ArgumentNullException.ThrowIfNull(consignmentAgreement);
        if (ConsignmentAgreements.Contains(consignmentAgreement)) return;
        
        ConsignmentAgreements.Add(consignmentAgreement);
        consignmentAgreement.AssignConsignor(this);
    }
    
    public Consignor(string name, string email, string phoneNumber)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
    }
    
    protected Consignor() { }

    public override string ToString() => $"";
}