using System.ComponentModel.DataAnnotations;
using MAS_Implementation.Enums;
using Microsoft.EntityFrameworkCore;

namespace MAS_Implementation.Models;

[Index(nameof(PermanentId), IsUnique = true)]
public class Bidder
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _permanentId = string.Empty;
    private MembershipTier _tier;
    
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
                throw new ArgumentOutOfRangeException(nameof(value), "Name cannot be longer than 150 characters.");
            _name = value;
        }
    }

    [Required]
    [MaxLength(150)]
    public string Email
    {
        get => _email;
        set
        {
            if (!EmailValidator.IsValidEmail(value))
                throw new ArgumentException("Email is either null/whitespace or is not input correctly (example: testmail@test.com).");
            if (value.Length > 150)
                throw new ArgumentOutOfRangeException(nameof(value), "Email cannot be longer than 150 characters.");
            
            _email = value;
        }
    }
    
    public string PermanentId
    {
        get => _permanentId;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Permanent ID cannot be empty.");
            _permanentId = value;
        }
    }

    public MembershipTier Tier { get; set; }

    public bool IsBanned { get; set; } = false;
    
    public int? RegistrarId { get; private set; }

    public Registrar Registrar { get; private set; } = null!;
    public List<Paddle> Paddles { get; private set; } = new();
    public List<Bid> Bids { get; private set; } = new();
    
    public Bidder(string name, string email, string permanentId, MembershipTier tier, bool isBanned = false)
    {
        Name = name;
        Email = email;
        PermanentId = permanentId;
        Tier = tier;
        IsBanned = isBanned;
    }
    
    protected Bidder() { }

    public void AssignRegistrar(Registrar registrar)
    {
        ArgumentNullException.ThrowIfNull(registrar);
        if (Registrar == registrar) return;
        
        Registrar = registrar;
        RegistrarId = registrar.Id;
        registrar.AddBidder(this);
    }

    public void AddPaddle(Paddle paddle)
    {
        ArgumentNullException.ThrowIfNull(paddle);
        if (Paddles.Contains(paddle)) return;
        
        Paddles.Add(paddle);
        paddle.AssignBidder(this);
    }

    public void AddBid(Bid bid)
    {
        ArgumentNullException.ThrowIfNull(bid);
        if (Bids.Contains(bid)) return;
        
        Bids.Add(bid);
        bid.AssignBidder(this);
    }

    public void Ban() => IsBanned = true;
    
    public override string ToString() => $"";
}