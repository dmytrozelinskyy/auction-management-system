using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAS_Implementation.Models;

[Table("StaffMembers")]
public abstract class StaffMember
{
    private string _name = string.Empty;
    private DateTime _dateOfBirth;
    private ContactInfo _contactInfo; 
    
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be null or whitespace.");
            if (value.Length > 100)
                throw new ArgumentException("Name cannot be longer than 100 characters.");
            _name = value;
        }
    }

    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set
        {
            if (value >= DateTime.Today)
                throw new ArgumentException("Date of birth cannot be in the future.");
            _dateOfBirth = value;
        }
    }

    public ContactInfo ContactInformation
    {
        get => _contactInfo;
        set
        {
            if (value == null)
                throw new ArgumentException("Contact information cannot be null.");
            _contactInfo = value;
        }
    }

    protected StaffMember(string name, DateTime dateOfBirth, ContactInfo contactInformation)
    {
        Name = name;
        DateOfBirth = dateOfBirth;
        ContactInformation = contactInformation;
    }
    
    protected StaffMember() { }

    public abstract string GetRole();
    public override string ToString() => $"{Name} [{GetRole()}]";
}