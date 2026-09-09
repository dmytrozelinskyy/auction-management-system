using System.ComponentModel.DataAnnotations.Schema;

namespace MAS_Implementation.Models;

[Table("Managers")]
public class Manager : StaffMember
{
    public List<Auction> Auctions { get; private set; } = new();
    
    public Manager(string name, DateTime dateOfBirth, ContactInfo contactInformation) 
        : base(name, dateOfBirth, contactInformation) { }
    
    protected Manager() { }
    public void AddAuction(Auction auction)
    {
        ArgumentNullException.ThrowIfNull(auction);
        if (Auctions.Contains(auction)) return;
        
        Auctions.Add(auction);
        auction.AssignManager(this);
    }

    public void RemoveAuction(Auction auction)
    {
        ArgumentNullException.ThrowIfNull(auction);
        if (!Auctions.Contains(auction)) return;
        Auctions.Remove(auction);
    }
    
    public void ScheduleAuction(Auction auction)
    {
        ArgumentNullException.ThrowIfNull(auction);
        if (!Auctions.Contains(auction))
            throw new InvalidOperationException("Manager is not assigned to this auction.");
        auction.Schedule();
    }

    public void CancelAuction(Auction auction)
    {
        ArgumentNullException.ThrowIfNull(auction);
        if (!Auctions.Contains(auction))
            throw new InvalidOperationException("Manager is not assigned to this auction.");
        auction.Cancel();
    }

    public void AssignAuctioneer(Auction auction, Auctioneer auctioneer)
    {
        ArgumentNullException.ThrowIfNull(auction);
        ArgumentNullException.ThrowIfNull(auctioneer);
        if (!Auctions.Contains(auction)) return;
        
        auction.AssignAuctioneer(auctioneer);
    }

    public override string GetRole() => "Manager";
}