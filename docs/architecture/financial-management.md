# Financial Management System

## 🎯 Overview

GolfManager v2 will support comprehensive financial management for leagues, including:
- **Player Payments**: Golfers pay league dues, event fees, skins, etc.
- **League Subscriptions**: Leagues pay platform fees to GolfManager
- **Payment Processing**: Integration with Stripe or similar
- **Financial Reporting**: Track income, expenses, balances

## 💰 Revenue Model

### Two-Sided Marketplace

**1. Golfers → League** (Player Payments)
- League dues (annual, seasonal, monthly)
- Event entry fees
- Skins game buy-ins
- Merchandise purchases
- Guest fees

**2. League → GolfManager** (Platform Fees)
- Monthly/annual subscription
- Per-golfer pricing
- Transaction fees (small % of player payments)
- Premium features (custom domain, advanced analytics)

## 📊 Data Model

### League Financial Entities

#### LeagueSubscription
```csharp
public class LeagueSubscription
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public SubscriptionTier Tier { get; set; }          // Free, Basic, Pro, Enterprise
    public SubscriptionBillingPeriod BillingPeriod { get; set; }  // Monthly, Annual
    public decimal MonthlyPrice { get; set; }
    public int MaxGolfers { get; set; }                 // Tier limit
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }      // Active, Cancelled, PastDue
    public string? StripeSubscriptionId { get; set; }
    public DateTime? NextBillingDate { get; set; }
    
    // Navigation
    public League League { get; set; }
    public ICollection<LeagueInvoice> Invoices { get; set; }
}

public enum SubscriptionTier
{
    Free,           // Up to 20 golfers, basic features
    Basic,          // Up to 50 golfers, $29/month
    Pro,            // Up to 200 golfers, $99/month, custom domain
    Enterprise      // Unlimited, $299/month, white-label
}
```

#### LeagueInvoice (League pays GolfManager)
```csharp
public class LeagueInvoice
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public string SubscriptionId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public InvoiceStatus Status { get; set; }           // Pending, Paid, Overdue, Cancelled
    public DateTime? PaidDate { get; set; }
    public string? StripeInvoiceId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    
    // Line items
    public ICollection<LeagueInvoiceItem> Items { get; set; }
}
```

### Player Financial Entities

#### LeagueFinancialAccount
```csharp
public class LeagueFinancialAccount
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public decimal Balance { get; set; }                // Current balance
    public decimal TotalReceived { get; set; }          // All-time received
    public decimal TotalPaidOut { get; set; }           // All-time paid out
    public string? StripeAccountId { get; set; }        // Stripe Connect account
    public bool IsStripeConnected { get; set; }
    public DateTime? StripeConnectedAt { get; set; }
    
    // Navigation
    public League League { get; set; }
}
```

#### GolferPayment (Golfer pays League)
```csharp
public class GolferPayment
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public string GolferId { get; set; }
    public string? SeasonId { get; set; }
    public string? EventId { get; set; }
    public PaymentType Type { get; set; }               // Dues, EventFee, Skins, Guest, Other
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }            // GolfManager's cut (e.g., 2.9% + $0.30)
    public decimal LeagueAmount { get; set; }           // Amount league receives
    public PaymentStatus Status { get; set; }           // Pending, Completed, Failed, Refunded
    public PaymentMethod Method { get; set; }           // Card, ACH, Cash, Check
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public League League { get; set; }
    public Golfer Golfer { get; set; }
    public Season? Season { get; set; }
    public SeasonEvent? Event { get; set; }
}

public enum PaymentType
{
    LeagueDues,         // Annual/seasonal dues
    EventFee,           // Per-event entry fee
    SkinsGame,          // Skins game buy-in
    GuestFee,           // Guest player fee
    Merchandise,        // League merchandise
    Other
}

public enum PaymentStatus
{
    Pending,            // Payment initiated
    Processing,         // Payment processing
    Completed,          // Payment successful
    Failed,             // Payment failed
    Refunded,           // Payment refunded
    Disputed            // Chargeback/dispute
}
```

#### GolferBalance (Golfer's balance with league)
```csharp
public class GolferBalance
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public string GolferId { get; set; }
    public string? SeasonId { get; set; }
    public decimal Balance { get; set; }                // Current balance (can be negative)
    public decimal TotalPaid { get; set; }              // Total paid to league
    public decimal TotalOwed { get; set; }              // Total owed to league
    public DateTime LastPaymentDate { get; set; }
    public DateTime? LastReminderSent { get; set; }
    
    // Navigation
    public League League { get; set; }
    public Golfer Golfer { get; set; }
    public Season? Season { get; set; }
}
```

#### PaymentSchedule (Recurring payments)
```csharp
public class PaymentSchedule
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public string GolferId { get; set; }
    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public RecurrencePattern Recurrence { get; set; }   // Weekly, Monthly, Seasonal
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public bool IsActive { get; set; }
    public string? StripeSubscriptionId { get; set; }
    
    // Navigation
    public League League { get; set; }
    public Golfer Golfer { get; set; }
}
```

#### Payout (League withdraws funds)
```csharp
public class Payout
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public decimal Amount { get; set; }
    public PayoutStatus Status { get; set; }            // Pending, InTransit, Paid, Failed
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? StripePayoutId { get; set; }
    public string RequestedBy { get; set; }             // User ID
    public string? Notes { get; set; }
    
    // Navigation
    public League League { get; set; }
}
```

## 💳 Payment Integration (Stripe)

### Stripe Connect for Leagues

**Model**: Platform + Connected Accounts
- GolfManager = Platform account
- Each League = Connected account (Stripe Express or Standard)

**Flow**:
1. League admin connects Stripe account
2. Golfer pays league dues
3. Payment goes to league's Stripe account
4. GolfManager takes platform fee (application fee)

### Payment Processing

#### Golfer Pays League Dues
```
Golfer → Stripe → League Stripe Account (minus platform fee)
                → GolfManager Platform Fee
```

#### League Pays GolfManager Subscription
```
League → Stripe → GolfManager Account
```

## 🔧 API Endpoints

### League Subscription Management
```
GET    /api/v1/leagues/{key}/subscription
POST   /api/v1/leagues/{key}/subscription/upgrade
POST   /api/v1/leagues/{key}/subscription/cancel
GET    /api/v1/leagues/{key}/invoices
GET    /api/v1/leagues/{key}/invoices/{id}
```

### Stripe Connect
```
POST   /api/v1/leagues/{key}/stripe/connect
GET    /api/v1/leagues/{key}/stripe/status
POST   /api/v1/leagues/{key}/stripe/disconnect
```

### Player Payments
```
GET    /api/v1/leagues/{key}/payments
POST   /api/v1/leagues/{key}/payments
GET    /api/v1/leagues/{key}/payments/{id}
POST   /api/v1/leagues/{key}/payments/{id}/refund

# Golfer-specific
GET    /api/v1/leagues/{key}/golfers/me/balance
GET    /api/v1/leagues/{key}/golfers/me/payments
POST   /api/v1/leagues/{key}/golfers/me/payments/pay-dues
POST   /api/v1/leagues/{key}/golfers/me/payments/pay-event/{eventId}
```

### Financial Reporting
```
GET    /api/v1/leagues/{key}/financials/summary
GET    /api/v1/leagues/{key}/financials/transactions
GET    /api/v1/leagues/{key}/financials/balances
GET    /api/v1/leagues/{key}/financials/export
```

### Payouts
```
GET    /api/v1/leagues/{key}/payouts
POST   /api/v1/leagues/{key}/payouts/request
GET    /api/v1/leagues/{key}/payouts/{id}
```

## 📋 Features

### For Golfers
- [ ] Pay league dues online
- [ ] Pay event fees
- [ ] View payment history
- [ ] View current balance
- [ ] Set up recurring payments
- [ ] Receive payment receipts
- [ ] Payment reminders

### For League Admins
- [ ] Connect Stripe account
- [ ] Set league dues amounts
- [ ] Set event fees
- [ ] View all payments
- [ ] Track golfer balances
- [ ] Send payment reminders
- [ ] Request payouts
- [ ] Financial reports
- [ ] Export transactions

### For GolfManager Platform
- [ ] Subscription management
- [ ] Billing and invoicing
- [ ] Platform fee collection
- [ ] Revenue analytics
- [ ] Chargeback handling

## 🎨 UI Considerations

### Golfer Payment Flow
1. View balance: "You owe $150 for 2024 season"
2. Click "Pay Now"
3. Enter payment method (Stripe Elements)
4. Confirm payment
5. Receive receipt via email

### League Admin Dashboard
- Current balance
- Pending payments
- Recent transactions
- Golfer balances (who owes what)
- Payout history

## 🔐 Security & Compliance

- PCI DSS compliance (handled by Stripe)
- Secure payment tokenization
- No storing of card numbers
- Audit trail for all transactions
- Refund policies
- Dispute handling

## 💡 Pricing Strategy

### Subscription Tiers
- **Free**: Up to 20 golfers, basic features, 3% transaction fee
- **Basic** ($29/mo): Up to 50 golfers, 2.5% transaction fee
- **Pro** ($99/mo): Up to 200 golfers, custom domain, 2% transaction fee
- **Enterprise** ($299/mo): Unlimited, white-label, 1.5% transaction fee

### Transaction Fees
- Platform fee: 1.5% - 3% (based on tier)
- Stripe fee: 2.9% + $0.30 (standard)
- Total to golfer: ~5% + $0.30

## ✅ Future Enhancements

- [ ] ACH payments (lower fees)
- [ ] International payments
- [ ] Multi-currency support
- [ ] Installment plans
- [ ] Automatic late fees
- [ ] Tax reporting (1099 forms)
- [ ] Accounting software integration (QuickBooks)

