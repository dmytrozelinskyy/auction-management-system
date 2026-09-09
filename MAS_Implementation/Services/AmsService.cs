using MAS_Implementation.Data;
using MAS_Implementation.Enums;
using MAS_Implementation.Models;
using Microsoft.EntityFrameworkCore;

namespace MAS_Implementation.Services;

public class AmsService : IDisposable
{
    private readonly AppDbContext _context;

    public AmsService()
    {
        _context = new AppDbContext();
        _context.Database.EnsureCreated();
    }

    public Cataloguer? GetCataloguer() => _context.Cataloguers.FirstOrDefault();
    public Manager? GetManager() => _context.Managers.FirstOrDefault();
    public Registrar? GetRegistrar() => _context.Registrars.FirstOrDefault();
    public Auctioneer? GetAuctioneer() => _context.Auctioneers.FirstOrDefault();
    public List<StaffMember> GetStaffMembers() => _context.StaffMembers.ToList();
    
    public List<ConsignmentAgreement> GetActiveAgreements() =>
        _context.ConsignmentAgreements
            .Include(ca => ca.Consignor)
            .Include(ca => ca.Lots)
            .Where(ca => ca.ValidFrom <= DateTime.Now && ca.ValidTo >= DateTime.Today)
            .ToList();

    public List<Auction> GetAuctionsWithPaddlesAndBidders() =>
        _context.Auctions.AsNoTracking().Include(a => a.Paddles).ThenInclude(p => p.Bidder).ToList();
    
    public List<Lot> GetLots() =>
        _context.Lots.AsNoTracking().ToList();
    
    public void SaveConsignor(Consignor consignor)
    {
        ArgumentNullException.ThrowIfNull(consignor);
        _context.Consignors.Add(consignor);
        _context.SaveChanges();
    }

    public void SaveAgreement(ConsignmentAgreement agreement)
    {
        ArgumentNullException.ThrowIfNull(agreement);
        _context.ConsignmentAgreements.Add(agreement);
        _context.SaveChanges();
    }

    public void SaveLot(Lot lot)
    {
        ArgumentNullException.ThrowIfNull(lot);
        _context.Lots.Add(lot);
        _context.SaveChanges();
    }

    public void UpdateLot()
    {
        _context.SaveChanges();
    }

    public void WithdrawLot(Lot lot)
    {
        ArgumentNullException.ThrowIfNull(lot);
        lot.Withdraw();
        _context.SaveChanges();
    }

    public void SaveAuction(Auction auction)
    {
        ArgumentNullException.ThrowIfNull(auction);
        _context.Auctions.Add(auction);
        _context.SaveChanges();
    }

    public void UpdateAuction()
    {
        _context.SaveChanges();
    }

    public void SaveBidder(Bidder bidder)
    {
        ArgumentNullException.ThrowIfNull(bidder);
        _context.Bidders.Add(bidder);
        _context.SaveChanges();
    }

    public void BanBidder(Bidder bidder)
    {
        bidder.Ban();
        _context.SaveChanges();
    }

    public void SavePaddle(Paddle paddle)
    {
        ArgumentNullException.ThrowIfNull(paddle);
        _context.Paddles.Add(paddle);
        _context.SaveChanges();
    }

    public void SaveBid(Bid bid)
    {
        ArgumentNullException.ThrowIfNull(bid);
        _context.Bids.Add(bid);
        _context.SaveChanges();
    }

    public void SeedDatabase()
    {
        if (_context.Auctions.Any()) return;

        var cataloguer = new Cataloguer("Adam Adam",
            new DateTime(1990, 7, 11),
            new ContactInfo("+48 111 222 333", "adam@test.com"),
            "Art & Watches"
        );
        
        var manager = new Manager(
            "Jane Jane",
            new DateTime(1972, 1, 1),
            new ContactInfo("+48 222 333 444", "jane@test.com"));

        var registrar = new Registrar(
            "Bob Jones",
            new DateTime(1982, 1, 1),
            new ContactInfo("+48 333 444 555", "bob@test.com"));

        var auctioneer = new Auctioneer(
            "Carol White",
            new DateTime(1980, 1, 1),
            new ContactInfo("+48 444 555 666", "carol@test.com"),
            "AUC-2024-001");

        _context.StaffMembers.AddRange(cataloguer, manager, registrar, auctioneer);
        _context.SaveChanges();
        
        var consignor1 = new Consignor("John Johnson",    "test@johnson.com",    "+48 333 222 111");
        var consignor2 = new Consignor("Anna Torowska", "anna@torowska.com", "+48 666 555 444");
        _context.Consignors.AddRange(consignor1, consignor2);
        _context.SaveChanges();
        
        var agreement1 = new ConsignmentAgreement(15.0, DateTime.Today.AddDays(-20), DateTime.Today.AddYears(1));
        agreement1.AssignConsignor(consignor1);
        agreement1.Title = "Spring Collection 2026";

        var agreement2 = new ConsignmentAgreement(12.5, DateTime.Today.AddDays(-30), DateTime.Today.AddMonths(8));
        agreement2.AssignConsignor(consignor1);
        agreement2.Title = "Private Estate Sale New Era";

        var agreement3 = new ConsignmentAgreement(18.0, DateTime.Today.AddDays(-10), DateTime.Today.AddYears(2));
        agreement3.AssignConsignor(consignor2);
        agreement3.Title = "Modern Art Consignment";

        var agreement4 = new ConsignmentAgreement(10.0, DateTime.Today.AddDays(-5), DateTime.Today.AddMonths(6));
        agreement4.AssignConsignor(consignor2);
        agreement4.Title = "Jewelry & Watches 2026";

        var agreement5 = new ConsignmentAgreement(20.0, DateTime.Today.AddDays(-20), DateTime.Today.AddMonths(4));
        agreement5.AssignConsignor(consignor1);
        agreement5.Title = "Antiques Summer Sale";

        _context.ConsignmentAgreements.AddRange(agreement1, agreement2, agreement3
        ,agreement4, agreement5);
        _context.SaveChanges();
        
        var lot1 = new Lot("Victorian Silver Pocket Watch", "Made in 1887", "Brilliant",
            5000, 8000, 4500, "United Kingdom", "Watches");
        lot1.AssignCataloguer(cataloguer);     
        lot1.AssignConsignmentAgreement(agreement1); 
        lot1.DetermineStatus(LotStatus.Eligible);

        var lot2 = new Lot("Abstract Oil on Canvas", "Signed lower right", "Good",
            2000, 4000, 1800, "France", "Fine Art");
        lot2.AssignCataloguer(cataloguer);
        lot2.AssignConsignmentAgreement(agreement2);
        lot2.DetermineStatus(LotStatus.Eligible);

        var lot3 = new Lot("Art Deco Diamond Necklace", null, "Excellent",
            8000, 12000, 7500, "Poland", "Jewelry");
        lot3.AssignCataloguer(cataloguer);
        lot3.AssignConsignmentAgreement(agreement3);
        lot3.DetermineStatus(LotStatus.Eligible);

        _context.Lots.AddRange(lot1, lot2, lot3);
        _context.SaveChanges();
        
        var bidder1 = new Bidder("Maria Nowak",  "maria@nowak.pl",  "BID-0001", MembershipTier.VIP);
        var bidder2 = new Bidder("Peter Wagner", "peter@wagner.de", "BID-0002", MembershipTier.Plus);
        _context.Bidders.AddRange(bidder1, bidder2);
        _context.SaveChanges();
        
        var auction = new Auction("Spring Sale",
            DateTime.Today.AddDays(14),
            DateTime.Today.AddDays(14).AddHours(6),
            "Grand Ballroom");
        auction.AssignManager(manager);
        auction.AssignAuctioneer(auctioneer);
        
        var auction1 = new Auction("Summer Antiques Auction",
            DateTime.Today.AddDays(30),
            DateTime.Today.AddDays(30).AddHours(9),
            "Big Warsaw Garden");
        auction1.AssignManager(manager);
        auction1.AssignAuctioneer(auctioneer);
        
        _context.Auctions.AddRange(auction, auction1);
        _context.SaveChanges();
        
        var round1 = new AuctionRound(1, DateTime.Today.AddDays(14).AddHours(1));
        var round2 = new AuctionRound(2, DateTime.Today.AddDays(14).AddHours(2));
        var round3 = new AuctionRound(3, DateTime.Today.AddDays(30).AddHours(3));
        
        auction.AddRound(round1); 
        auction.AddRound(round2);
        auction.AddLot(lot1);
        auction.AddLot(lot2);
        
        auction1.AddRound(round1);
        auction1.AddLot(lot3);
        _context.SaveChanges();
        
        var paddle1 = new Paddle("001", DateTime.Today);
        paddle1.AssignAuction(auction); 
        paddle1.AssignBidder(bidder1);

        var paddle2 = new Paddle("001", DateTime.Today);
        paddle2.AssignAuction(auction1);
        paddle2.AssignBidder(bidder2);

        _context.Paddles.AddRange(paddle1, paddle2);
        _context.SaveChanges();

        Console.WriteLine("Database was successfully seeded with initial data.");
    }

    public void Dispose() => _context.Dispose();
}