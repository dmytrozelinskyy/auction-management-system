using System.ComponentModel.DataAnnotations.Schema;
using MAS_Implementation.Enums;

namespace MAS_Implementation.Models;

[Table("Registrars")]
public class Registrar : StaffMember
{
    public Registrar(string name, DateTime dateOfBirth, ContactInfo contactInformation)
        : base(name, dateOfBirth, contactInformation) { }
    
    protected Registrar() { }
    public List<Bidder> Bidders { get; private set; } = new();

    public void AddBidder(Bidder bidder)
    {
        ArgumentNullException.ThrowIfNull(bidder);
        if (Bidders.Contains(bidder)) return;
        Bidders.Add(bidder);
        bidder.AssignRegistrar(this);
    }
    
    public Bidder RegisterBidder(string name, string email, string permanentId, MembershipTier tier)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bidder name cannot be empty.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Bidder email cannot be empty.");
        if (string.IsNullOrWhiteSpace(permanentId))
            throw new ArgumentException("Bidder permanent id cannot be empty.");
        return new Bidder(name, email, permanentId, tier);
    }

    public void BanBidder(Bidder bidder)
    {
        ArgumentNullException.ThrowIfNull(bidder);
        if (bidder.IsBanned)
            throw new InvalidOperationException("Bidder is already banned.");
        bidder.Ban();
    }

    public Paddle IssuePaddleNumber(Bidder bidder, Auction auction)
    {
        ArgumentNullException.ThrowIfNull(bidder);
        ArgumentNullException.ThrowIfNull(auction);

        if (bidder.IsBanned)
            throw new InvalidOperationException("Paddle number cannot be issued to a banned bidder.");
        if (auction.Paddles.Any(p => p.Bidder == bidder))
            throw new InvalidOperationException("Bidder already has a paddle number for this auction.");

        var paddleNumber = (auction.Paddles.Count + 1).ToString("D3");

        var paddle = new Paddle(paddleNumber, DateTime.Now);
        auction.AddPaddle(paddle);

        return paddle;
    }

    public Bid RecordAbsenteeBid(Bidder bidder, AuctionRound round, double maxAmount)
    {
        ArgumentNullException.ThrowIfNull(bidder);
        ArgumentNullException.ThrowIfNull(round);
        
        if (bidder.IsBanned)
            throw new InvalidOperationException("Absentee bid cannot be recorded from banned bidder.");
        if (!round.Auction.Paddles.Any(p => p.Bidder == bidder))
            throw new InvalidOperationException(
                "Bidder must have a paddle for this auction before placing an absentee bid.");
        if (maxAmount <= 0)
            throw new ArgumentException("Max amount must be positive");
        if (maxAmount < (double)Auction.MinBiddingPrice)
            throw new ArgumentException($"Max amount must be at least {Auction.MinBiddingPrice}");
        
        var bid = new Bid(0, DateTime.Now, isAbsentee: true, maxAmount);
        round.AddBid(bid);

        return bid;
    }

    public override string GetRole() => "Registrar";
}