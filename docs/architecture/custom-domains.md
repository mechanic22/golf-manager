# Custom Domain Support for Multi-Tenancy

## 🎯 Overview

GolfManager v2 supports custom domains for leagues, allowing each league to have their own branded domain (e.g., `digikeygolf.com`) instead of using the default subdomain pattern.

## 🌐 Domain Patterns

### Default Pattern (Subdomain)
```
https://digikey.golfmanager.app
https://sunday-league.golfmanager.app
```

### Custom Domain Pattern
```
https://digikeygolf.com
https://sundayleague.golf
```

## 📊 Data Model

### League Entity
```csharp
public class League
{
    public string Key { get; set; }                   // e.g., "digikey"
    
    // Custom Domain Support
    public string? CustomDomain { get; set; }         // e.g., "digikeygolf.com"
    public bool UseCustomDomain { get; set; }
    public string? CustomDomainVerificationToken { get; set; }
    public DateTime? CustomDomainVerifiedAt { get; set; }
}
```

## 🔧 Implementation Flow

### 1. League Admin Sets Custom Domain

**API Call:**
```http
PUT /api/v1/leagues/digikey/domain
Content-Type: application/json

{
  "customDomain": "digikeygolf.com"
}
```

**Response:**
```json
{
  "customDomain": "digikeygolf.com",
  "verificationToken": "gm_verify_abc123xyz",
  "verificationMethod": "DNS_TXT",
  "instructions": "Add TXT record: gm_verify_abc123xyz",
  "verified": false
}
```

### 2. DNS Configuration

League admin adds DNS records:

**Option A: CNAME (Recommended)**
```
CNAME digikeygolf.com -> golfmanager.app
TXT   _golfmanager-verify.digikeygolf.com -> gm_verify_abc123xyz
```

**Option B: A Record**
```
A     digikeygolf.com -> [API IP Address]
TXT   _golfmanager-verify.digikeygolf.com -> gm_verify_abc123xyz
```

### 3. Domain Verification

**API Call:**
```http
POST /api/v1/leagues/digikey/domain/verify
```

**Backend Process:**
1. Query DNS for TXT record `_golfmanager-verify.digikeygolf.com`
2. Verify token matches
3. Update `CustomDomainVerifiedAt` timestamp
4. Enable custom domain routing

### 4. SSL Certificate Provisioning

**Automatic (Let's Encrypt):**
- Use ACME protocol to provision SSL certificate
- Auto-renew every 60 days
- Store certificate in Azure Key Vault or similar

## 🔀 Request Routing

### Middleware: Domain Resolution

```csharp
public class LeagueDomainMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        
        // Check if custom domain
        var league = await _leagueService.GetByCustomDomainAsync(host);
        
        if (league != null)
        {
            // Set league context for this request
            context.Items["LeagueId"] = league.Id;
            context.Items["LeagueKey"] = league.Key;
        }
        else
        {
            // Check subdomain pattern: {leagueKey}.golfmanager.app
            var subdomain = host.Split('.')[0];
            league = await _leagueService.GetByKeyAsync(subdomain);
            
            if (league != null)
            {
                context.Items["LeagueId"] = league.Id;
                context.Items["LeagueKey"] = league.Key;
            }
        }
        
        await _next(context);
    }
}
```

### URL Generation

All API responses should use the league's preferred domain:

```csharp
public string GetLeagueUrl(League league)
{
    if (league.UseCustomDomain && league.CustomDomainVerifiedAt != null)
    {
        return $"https://{league.CustomDomain}";
    }
    
    return $"https://{league.Key}.golfmanager.app";
}
```

## 🔐 Security Considerations

### 1. Domain Verification
- Require DNS TXT record verification
- Prevent domain hijacking
- Re-verify periodically (every 30 days)

### 2. SSL/TLS
- Enforce HTTPS only
- Auto-provision certificates
- HSTS headers

### 3. CORS
- Configure CORS based on league domain
- Allow league's custom domain in CORS policy

## 📋 API Endpoints

```
GET    /api/v1/leagues/{leagueKey}/domain
PUT    /api/v1/leagues/{leagueKey}/domain
POST   /api/v1/leagues/{leagueKey}/domain/verify
DELETE /api/v1/leagues/{leagueKey}/domain
GET    /api/v1/leagues/{leagueKey}/domain/status
```

## 🎨 Frontend Considerations

### Blazor WebAssembly
- Detect current domain on app load
- Set API base URL accordingly
- Handle cross-domain authentication

### MAUI Mobile App
- Allow user to enter custom domain
- Store preferred domain in app settings
- Support both subdomain and custom domain patterns

## 📊 Database Indexes

```sql
CREATE UNIQUE INDEX IX_League_CustomDomain 
ON Leagues(CustomDomain) 
WHERE CustomDomain IS NOT NULL;

CREATE INDEX IX_League_Key 
ON Leagues(Key);
```

## 🚀 Deployment Considerations

### Infrastructure
- **Azure**: Use Azure Front Door or Application Gateway
- **AWS**: Use CloudFront or ALB
- **Self-hosted**: Use Nginx or Traefik with SNI support

### DNS Management
- Support for wildcard certificates (*.golfmanager.app)
- Individual certificates for custom domains
- DNS challenge for Let's Encrypt

## ✅ Future Enhancements

- [ ] Support for subdomains of custom domains (e.g., `app.digikeygolf.com`)
- [ ] White-label branding (custom logo, colors per domain)
- [ ] Email from custom domain (e.g., `noreply@digikeygolf.com`)
- [ ] Custom domain analytics

