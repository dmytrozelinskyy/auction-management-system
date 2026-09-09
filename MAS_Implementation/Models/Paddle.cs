using System.ComponentModel.DataAnnotations;

namespace MAS_Implementation.Models;

public class Paddle
{
    private string _paddleNumber = string.Empty;
    private DateTime _issueDate;
    
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string PaddleNumber
    {
        get => _paddleNumber;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Paddle number cannot be null or whitespace.");
            if (value.Length > 100)
                throw new ArgumentException("Paddle number cannot exceed 100 characters.");
            _paddleNumber = value;
        }
    }

    public DateTime IssueDate
    {
        get => _issueDate;
        set
        {
            if (value.Date > DateTime.Today)
                throw new ArgumentException("Issue date cannot be in the future.");
            _issueDate = value;
        }
    }
    
    public int AuctionId { get; private set; }
    public int BidderId { get; private set;  }

    public Auction Auction { get; private set; } = null!;
    public Bidder Bidder { get; private set; } = null!;
    

    public Paddle(string paddleNumber, DateTime issueDate)
    {
        PaddleNumber = paddleNumber;
        IssueDate = issueDate;
    }
    
    protected Paddle() { }
    public void AssignAuction(Auction auction)
    {
        ArgumentNullException.ThrowIfNull(auction);
        if (Auction == auction) return;
        
        Auction = auction;
        AuctionId = auction.Id;
        auction.AddPaddle(this);
    }

    public void AssignBidder(Bidder bidder)
    {
        ArgumentNullException.ThrowIfNull(bidder);
        if (Bidder == bidder) return;
        
        Bidder = bidder;
        BidderId = bidder.Id;
        bidder.AddPaddle(this);
    }

    public override string ToString() => $"Paddle with number: {PaddleNumber}\nIssue date: {IssueDate:d}";
}