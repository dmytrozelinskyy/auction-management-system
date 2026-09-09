using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MAS_Implementation.Enums;

namespace MAS_Implementation.Models;

public class Auction
{
    private string _title = string.Empty;
    private DateTime _startDate;
    private DateTime _endDate;
    private decimal? _reservePrice;
    private string _facility = string.Empty;
    private readonly HashSet<AuctionRound> _rounds = new();
    
    [Key]
    public int Id { get; set; }
    public static decimal MinBiddingPrice { get; set; } = 1.00m;
    
    [Required]
    [MaxLength(150)]
    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Auction title cannot be empty.");
            if (value.Length > 150) throw new ArgumentException("Auction title cannot be longer than 150 characters.");
            _title = value;
        }
    }
    
    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (value == default)
                throw new ArgumentException("Start date must be specified.");
            if (_endDate is { } endDate && endDate != default && value >= endDate)
                throw new ArgumentException("Start date must be before end date.");
            _startDate = value;
        }
    }
    
    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            if (value == default)
                throw new ArgumentException("End date must be specified.");
            if (_startDate is { } startDate && startDate != default && value <= startDate)
                throw new ArgumentException("End date must be after start date.");
            _endDate = value;
        }
    }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ReservePrice
    {
        get => _reservePrice;
        set
        {
            if (value is <= 0) 
                throw new ArgumentOutOfRangeException(nameof(value), "Reserve price must be positive.");
            _reservePrice = value;
        }
    }
    
    [MaxLength(200)]
    public string Facility
    {
        get => _facility;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) 
                throw new ArgumentException("Facility cannot be empty.");
            if (value.Length > 200) 
                throw new ArgumentException("Facility cannot be longer than 200 characters.");
            _facility = value;
        }
    }
    
    [NotMapped]
    public int Duration => (int)(EndDate - StartDate).TotalMinutes;
    
    public IReadOnlyCollection<AuctionRound> Rounds => _rounds;
    
    public Auctioneer? Auctioneer { get; private set; }
    public Manager? Manager { get; private set; }

    public List<Lot> Lots { get; private set; } = new();
    public List<Paddle> Paddles { get; private set; } = new();
    
    public Auction(string title, DateTime startDate, DateTime endDate, string facility)
    {
        Title = title;
        StartDate = startDate;
        EndDate = endDate;
        Facility = facility;
    }

    public Auction(string title, DateTime startDate, DateTime endDate, string facility, decimal? reservePrice)
                : this(title, startDate, endDate, facility)
    { ReservePrice = reservePrice; }
    
    protected Auction() { }
    
    public void AssignAuctioneer(Auctioneer auctioneer)
    {
        ArgumentNullException.ThrowIfNull(auctioneer);
        if (Auctioneer == auctioneer) return;

        Auctioneer?.RemoveAuction(this);
        
        Auctioneer = auctioneer;
        auctioneer.AddAuction(this);
    }

    public void AssignManager(Manager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        if (Manager == manager) return;

        Manager?.RemoveAuction(this);
        
        Manager = manager;
        manager.AddAuction(this);
    }

    public void AddRound(AuctionRound round)
    {
        ArgumentNullException.ThrowIfNull(round);
        if (_rounds.Contains(round)) return;

        _rounds.Add(round);
        round.AssignToAuction(this);
    }
 
    public void AddLot(Lot lot)
    {
        ArgumentNullException.ThrowIfNull(lot);
        if (lot.AuctionId > 0 && lot.AuctionId != Id) 
            throw new ArgumentException("This lot is already assigned to another auction.");
        if (Lots.Contains(lot)) return;
        
        Lots.Add(lot);
        lot.AssignAuction(this);
    }

    public void AddPaddle(Paddle paddle) 
    {
        ArgumentNullException.ThrowIfNull(paddle);
        if (Paddles.Contains(paddle)) return;
        
        Paddles.Add(paddle);
        paddle.AssignAuction(this);
    }

    public void Schedule()
    {
        if (Rounds.Count == 0)
            throw new InvalidOperationException("Cannot schedule an auction with no rounds.");
        if (Auctioneer == null)
            throw new InvalidOperationException("Cannot schedule an auction without an assigned auctioneer.");
        if (StartDate <= DateTime.Now)
            throw new InvalidOperationException("Cannot schedule an auction with a start date in the past.");
        Console.WriteLine("Auction scheduled successfully.");
    }

    public void Cancel()
    {
        foreach (var round in Rounds)
        {
            if (round.Status == RoundStatus.Opened) 
                round.Cancel();
        }
    }
    
    public override string ToString()
    {
        string reservePrice = ReservePrice.HasValue ? ReservePrice.Value.ToString("C") : "Not set";

        return $"Auction\n" +
               $"Title: {Title}\n" +
               $"StartDate: {StartDate:dd/MM/yyyy HH:mm}\n" +
               $"EndDate: {EndDate:dd/MM/yyyy HH:mm}\n" +
               $"Duration: {Duration} minutes\n" +
               $"Facility: {Facility}\n" + 
               $"Reserve Price: {reservePrice}\n";
    }
}