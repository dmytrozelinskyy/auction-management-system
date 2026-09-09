using System.ComponentModel.DataAnnotations;
using MAS_Implementation.Enums;

namespace MAS_Implementation.Models;


public class AuctionRound
{
    private int _roundNumber;
    private DateTime _startTime;
    private DateTime? _endTime; 
    
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int RoundNumber
    {
        get => _roundNumber;
        set
        {
            if (value <= 0) 
                throw new ArgumentOutOfRangeException(nameof(value), "Round number must be positive and not zero.");
            _roundNumber = value;
        }
    }

    public DateTime StartTime
    {
        get => _startTime;
        set
        {
            if (value == default) 
                throw new ArgumentException("Start time cannot be empty.");
            _startTime = value;
        }
    }

    public DateTime? EndTime
    {
        get => _endTime;
        set
        {
            if (value.HasValue && _startTime is { } startTime && startTime != default && value <= startTime)
                throw new ArgumentException("End time cannot be before start time.");
            _endTime = value;
        }
    }

    public RoundStatus Status { get; private set; } = RoundStatus.Closed;
    
    public int AuctionId { get; private set; }
    public Auction Auction { get; private set; } = null!;
        
    public List<Bid> Bids { get; private set; } = new();
    public List<Lot> Lots { get; private set; } = new();

    public AuctionRound(int roundNumber, DateTime startTime)
    {
        RoundNumber = roundNumber;
        StartTime = startTime;
    }

    protected AuctionRound() { }

    public void AssignToAuction(Auction auction)
    {
        ArgumentNullException.ThrowIfNull(auction);
        if (Auction == auction) return;
        Auction = auction;
        AuctionId = auction.Id;
    }

    public void AddBid(Bid bid)
    {
        ArgumentNullException.ThrowIfNull(bid);
        if (Status != RoundStatus.Opened)
            throw new InvalidOperationException("Cannot add bid to closed round.");
        if (Bids.Contains(bid)) return;
            
        Bids.Add(bid);
        bid.AssignAuctionRound(this);
    }

    public void Open()
    {
        if (Status == RoundStatus.Opened)
            throw new InvalidOperationException("Round is already opened.");
        if (Auction.Lots.Count == 0)
            throw new InvalidOperationException("Cannot open a round with no lots assigned to this auction.");
        
        Status = RoundStatus.Opened;
        StartTime = DateTime.Now;
        EndTime = null;

        foreach (var lot in Auction.Lots.Where(l => l.Status == LotStatus.Eligible))
        {
            lot.AssignToRound(Auction);
            if (!Lots.Contains(lot)) Lots.Add(lot);
        }
    }
    
    public void Close()
    {
        if (Status != RoundStatus.Opened) 
            throw new InvalidOperationException("Only an open round can be closed.");
        
        Status = RoundStatus.Closed;
        EndTime = DateTime.Now;

        foreach (var lot in Auction.Lots)
        {
            if (lot.Status == LotStatus.OnAuction)
                lot.DetermineStatus(LotStatus.Passed);
        }
    }

    public void Cancel()
    {
        if (Status == RoundStatus.Cancelled)
            throw new InvalidOperationException("Round is already cancelled.");
        
        Status = RoundStatus.Cancelled;
        EndTime = DateTime.Now;
        foreach (var lot in Auction.Lots)
        {
            if (lot.Status == LotStatus.OnAuction || lot.Status == LotStatus.Assigned) 
                lot.DetermineStatus(LotStatus.Eligible);
        }
    }

    public override string ToString() => $"AuctionRound(#{_roundNumber}, Start time: {_startTime:g}, End time: {_endTime?.ToString("g") ?? "N/A"}, Status: {Status})\n";
}