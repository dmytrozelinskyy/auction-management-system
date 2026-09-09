using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MAS_Implementation.Enums;

namespace MAS_Implementation.Models;

[Table("Auctioneers")]
public class Auctioneer : StaffMember
{
    private string _licenseNumber;
    
    [Required]
    [MaxLength(50)]
    public string LicenseNumber
    {
        get => _licenseNumber;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("License number cannot be empty");
            if (value.Length > 50) 
                throw new ArgumentException("License number cannot be more than 50 characters");
            _licenseNumber = value;
        }
    }

    public List<Auction> Auctions { get; private set; } = new();

    public Auctioneer(string name, DateTime dateOfBirth, ContactInfo contactInformation, string licenseNumber)
        : base(name, dateOfBirth, contactInformation)
    {
        LicenseNumber = licenseNumber;
    }
    
    protected Auctioneer() { }

    public void AddAuction(Auction auction)
    {
        ArgumentNullException.ThrowIfNull(auction);
        if (Auctions.Contains(auction)) return;
        
        Auctions.Add(auction);
        auction.AssignAuctioneer(this);
    }

    public void RemoveAuction(Auction auction) => Auctions.Remove(auction);
    
    public void OpenRound(AuctionRound round)
    {
        ArgumentNullException.ThrowIfNull(round);
        if(!Auctions.Contains(round.Auction))
            throw new InvalidOperationException("This Auctioneer does not lead the round's auction.");

        round.Open();
    }

    public Bid RecordLiveBid(AuctionRound round, Paddle paddle, double amount)
    {
        ArgumentNullException.ThrowIfNull(round);
        ArgumentNullException.ThrowIfNull(paddle);
        if (round.Status != RoundStatus.Opened)
            throw new InvalidOperationException("Round is not open for bidding.");
        if (paddle.Auction != round.Auction)
            throw new InvalidOperationException("The paddle was not issued for this auction.");
        if (paddle.Bidder.IsBanned)
            throw new InvalidOperationException("The paddle holder is banned.");
        if (amount < (double)Auction.MinBiddingPrice)
            throw new ArgumentException($"Bid amount must be at least {Auction.MinBiddingPrice}.");

        var bid = new Bid(amount, DateTime.Now, isAbsentee: false, maxAmount: default);
        round.AddBid(bid);
        return bid;
    }

    public void CloseRound(AuctionRound round)
    {
        ArgumentNullException.ThrowIfNull(round);
        if (!Auctions.Contains(round.Auction))
            throw new InvalidOperationException("This Auctioneer does not lead this auction's round.");
        
        round.Close();
    }

    public override string GetRole() => "Auctioneer";

    public override string ToString() => base.ToString() + $"\nLicense Number: {LicenseNumber}";
}