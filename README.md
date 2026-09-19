# Auction Management System

A desktop application for managing the end-to-end workflow of an auction house - consignment agreements, lot cataloguing - built with **.NET 8** and **WPF**.

## Features
- **Consignment management** - record consignors, draft consignment agreements (commission rate, validity period), and link lots to agreements
- **Lot cataloguing** - track lot details (description, condition, estimates, origin, category) and move lots status lifecycle (`Catalogued` -> `Eligible` -> `Assigned` -> `OnAuction` -> `Sold`/`Passed`/`Cancelled`), including withdrawal.
- **Bidder & paddle registration** - register bidders with membership tiers (Standard/Plus/VIP), issue paddles per auction, and ban bidders when needed
- **Bidding** - record bids against paddles and lots
- **Email Validation** - custom validator for contact info
- **Seeded demo data** - the app seeds a realistic dataset (staff, consignor, agreements, lots, auctions, bidders, paddles) on first run so the UI is populated out of the box


## Tech Stack

- **C# / .NET 8** (Windows, WPF)
- **Entity Framework Core 8** with **SQLite**
- Code-first EF Core migration

## Architecture
- **Models** - domain entities (`Auction`, `Lot`, `Bid`, `Bidder`, `ConsigmentAgreement`, staff types, etc) encapsulating their own behavior (e.g. `Lot.Withdraw()`, `Bidder.Ban()`, `Lot.DetermineStatus()`).
- **Data** - `AppDbContext` (EF Core) for persistence
- **Services** - `AmsService` as the application's data-access and orchestration layer between the UI and the database
- **Validators** - custom validation logic (e.g. email format)
- **UI** - WPF windows/popups (`MainWindow`, `SubmissionPopup`, `NotificationPopup`)

## Getting Started

### Prerequisites
- Windows
- .NET 8 SDK

### Run
```bash
dotnet restore
dotnet run --project MAS_Implementation
```
The SQLite database is created automatically on first run and seeded with sample data. 

## Documentation 

This project was developed with accompanying UML Documentation (class, activity, and state diagrams) and dynamic analysis as part of a university system-analysis course, and was defended as a full project.
