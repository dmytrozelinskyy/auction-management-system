using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAS_Implementation.Models;

public class Bid
{
    private double _amount;
    private DateTime _timeStamp;
    private double? _maxAmount;
    
    [Key]
    public int Id { get; set; }

    public double Amount
    {
        get => _amount;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Amount must be greater than 0.");
            _amount = value;
        }
    }

    public DateTime TimeStamp
    {
        get => _timeStamp;
        set
        {
            if (value == default)
                throw new ArgumentException("Time Stamp cannot be default value.");
            _timeStamp = value;
        }
    }
    
    public bool IsAbsentee { get; set; }

    public double? MaxAmount
    {
        get => _maxAmount;
        set
        {
            if (value is <= 0) 
                throw new ArgumentOutOfRangeException(nameof(value), "MaxAmount must be greater than 0.");
            _maxAmount = value;
        }
    }

    public int AuctionRoundId { get; private set; }
    public int BidderId { get; private set; }
    public AuctionRound AuctionRound { get; private set; }
    public Bidder Bidder { get; private set; }

    public void AssignAuctionRound(AuctionRound auctionRound)
    {
        ArgumentNullException.ThrowIfNull(auctionRound);
        if (AuctionRound == auctionRound) return;
        
        AuctionRound = auctionRound;
        AuctionRoundId = auctionRound.Id;
        auctionRound.AddBid(this);
    }

    public void AssignBidder(Bidder bidder)
    {
        ArgumentNullException.ThrowIfNull(bidder);
        if (Bidder == bidder) return;
        
        Bidder = bidder;
        BidderId = bidder.Id;
        bidder.AddBid(this);
    }

    public Bid(double amount, DateTime timeStamp, bool isAbsentee, double? maxAmount)
    {
        Amount = amount;
        TimeStamp = timeStamp;
        IsAbsentee = isAbsentee;
        MaxAmount = maxAmount;
    }
    
    protected Bid() { }

    public override string ToString()
    {
        var isAbsenteeStr = IsAbsentee ? "Bid is placed on behalf of absentee bidder" : "Bid is not placed on behalf of absentee bidder";
        return $"[Bid]\n Amount {Amount} | Time stamp: {TimeStamp} | IsAbsentee: {isAbsenteeStr} | Max amount: {MaxAmount?.ToString() ?? "N/A"}";
    }
}