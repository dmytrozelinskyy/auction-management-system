using Microsoft.EntityFrameworkCore;
using MAS_Implementation.Models;

namespace MAS_Implementation.Data;

public class AppDbContext : DbContext
{
    public DbSet<StaffMember> StaffMembers { get; set; }
    public DbSet<Manager> Managers { get; set; }
    public DbSet<Cataloguer> Cataloguers { get; set; }
    public DbSet<Registrar> Registrars { get; set; }
    public DbSet<Auctioneer> Auctioneers { get; set; }
    
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<AuctionRound> AuctionRounds { get; set; }
    public DbSet<Lot> Lots { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<Paddle> Paddles { get; set; }
    public DbSet<Bidder> Bidders { get; set; }
    public DbSet<Consignor> Consignors { get; set; }
    public DbSet<ConsignmentAgreement> ConsignmentAgreements { get; set; }
    public DbSet<VerificationTask> VerificationTasks { get; set; }
    public DbSet<Document> Documents { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite("Data Source=ams.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StaffMember>().UseTptMappingStrategy();
        
        modelBuilder.Entity<Auction>()
            .Navigation(a => a.Rounds)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        modelBuilder.Entity<Auction>()
            .HasMany(a => a.Rounds)
            .WithOne(r => r.Auction)
            .HasForeignKey(r => r.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Auction>()
            .HasMany(a => a.Lots)
            .WithOne(l => l.Auction)
            .HasForeignKey(l => l.AuctionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<ConsignmentAgreement>()
            .HasMany(ca => ca.Lots)
            .WithOne(l => l.ConsignmentAgreement)
            .HasForeignKey(l => l.ConsignmentAgreementId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cataloguer>()
            .HasMany(c => c.Lots)
            .WithOne(l => l.Cataloguer)
            .HasForeignKey(l => l.CataloguerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Lot>()
            .HasMany(l => l.VerificationTasks)
            .WithOne(v => v.Lot)
            .HasForeignKey(v => v.LotId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<VerificationTask>()
            .HasMany(v => v.Documents)
            .WithOne()
            .HasForeignKey("VerificationTaskId")
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<AuctionRound>()
            .HasMany(r => r.Lots)
            .WithOne()
            .HasForeignKey("AuctionRoundId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<AuctionRound>()
            .HasMany(r => r.Bids)
            .WithOne(b => b.AuctionRound)
            .HasForeignKey(b => b.AuctionRoundId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Auction>()
            .HasMany(a => a.Paddles)
            .WithOne(p => p.Auction)
            .HasForeignKey(p => p.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Bidder>()
            .HasMany(b => b.Paddles)
            .WithOne(p => p.Bidder)
            .HasForeignKey(p => p.BidderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Bidder>()
            .HasMany(b => b.Bids)
            .WithOne(b => b.Bidder)
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Consignor>()
            .HasMany(c => c.ConsignmentAgreements)
            .WithOne(a => a.Consignor)
            .HasForeignKey(a => a.ConsignorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Auction>()
            .HasOne(a => a.Auctioneer)
            .WithMany(ae => ae.Auctions)
            .HasForeignKey("AuctioneerId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Auction>()
            .HasOne(a => a.Manager)
            .WithMany(m => m.Auctions)
            .HasForeignKey("ManagerId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Cataloguer>()
            .HasMany(c => c.Consignors)
            .WithOne(cn => cn.Cataloguer)
            .HasForeignKey("CataloguerId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Registrar>()
            .HasMany(r => r.Bidders)
            .WithOne(b => b.Registrar)
            .HasForeignKey(b => b.RegistrarId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Bidder>()
            .HasIndex(b => b.PermanentId)
            .IsUnique();
        
        modelBuilder.Entity<Paddle>()
            .HasIndex(p => new {p.AuctionId, p.PaddleNumber})
            .IsUnique();
    }
}