# GPS-Based Hole Detection for Mobile App

## 🎯 Overview

GolfManager v2 supports GPS-based hole detection for the mobile app, allowing automatic identification of which hole a golfer is playing and suggesting clubs based on distance to the green.

## 📊 Data Model

### Hole Entity (Enhanced with GPS)
```csharp
public class Hole
{
    public int HoleNumber { get; set; }
    
    // GPS Coordinates
    public double? TeeBoxLatitude { get; set; }        // Tee box location
    public double? TeeBoxLongitude { get; set; }
    public double? GreenLatitude { get; set; }         // Green center
    public double? GreenLongitude { get; set; }
    public double? FairwayLatitude { get; set; }       // Landing zone (optional)
    public double? FairwayLongitude { get; set; }
    
    // Geofence for auto-detection
    public double? GeofenceRadius { get; set; }        // Meters (default: 50m)
    
    // Hole characteristics
    public string? Dogleg { get; set; }                // "Left", "Right", "None"
    public string? HazardNotes { get; set; }
}
```

### HoleTee Entity (Tee-Specific GPS)
```csharp
public class HoleTee
{
    public int HoleNumber { get; set; }
    public int Yardage { get; set; }
    
    // Override tee box location for specific tees
    public double? TeeBoxLatitude { get; set; }
    public double? TeeBoxLongitude { get; set; }
}
```

## 🎯 Use Cases

### Use Case 1: Auto-Detect Current Hole

**Mobile App Flow:**
1. User starts a round
2. App requests location permission
3. App continuously monitors GPS location
4. When golfer enters a hole's geofence, app auto-switches to that hole
5. App displays hole info and distance to green

**API Call:**
```http
POST /api/v1/courses/pebble-beach/detect-hole
Content-Type: application/json

{
  "latitude": 36.5674,
  "longitude": -121.9500,
  "teeId": "blue-tees-id"
}
```

**Response:**
```json
{
  "holeNumber": 7,
  "holeName": "Signature Hole",
  "par": 4,
  "yardage": 380,
  "distanceToGreen": 375,
  "distanceToTeeBox": 12,
  "suggestedClub": "Driver",
  "confidence": 0.95,
  "location": "tee-box",
  "hazards": ["Water left", "Bunker right"]
}
```

### Use Case 2: Distance Calculation

**Mobile App Flow:**
1. Golfer is on the fairway
2. App calculates distance to green
3. App suggests club based on golfer's average distances
4. Golfer selects club and records shot

**API Call:**
```http
POST /api/v1/courses/pebble-beach/holes/7/distance
Content-Type: application/json

{
  "latitude": 36.5680,
  "longitude": -121.9505,
  "golferId": "golfer-id"
}
```

**Response:**
```json
{
  "distanceToGreen": 145,
  "distanceToPin": 148,
  "elevation": "+12ft",
  "suggestedClubs": [
    { "club": "8 Iron", "avgDistance": 150, "confidence": 0.9 },
    { "club": "9 Iron", "avgDistance": 140, "confidence": 0.7 }
  ]
}
```

### Use Case 3: Auto-Populate Tee Club

**Mobile App Flow:**
1. Golfer arrives at tee box
2. App detects hole and location (tee box)
3. App auto-populates "Tee Club" field based on hole yardage
4. Golfer can override if needed

**Logic:**
```csharp
public string SuggestTeeClub(int yardage, Golfer golfer)
{
    var clubs = golfer.Clubs.OrderByDescending(c => c.AverageDistance);
    
    // For par 3s, suggest club that matches yardage
    if (par == 3)
    {
        return clubs.FirstOrDefault(c => 
            Math.Abs(c.AverageDistance - yardage) < 10)?.Name ?? "Unknown";
    }
    
    // For par 4/5, suggest driver or 3-wood
    if (yardage > 400)
        return "Driver";
    else if (yardage > 350)
        return clubs.FirstOrDefault(c => c.Key == "driver")?.Name ?? "Driver";
    else
        return clubs.FirstOrDefault(c => c.Key == "3w")?.Name ?? "3 Wood";
}
```

## 🧮 Distance Calculation Algorithm

### Haversine Formula
```csharp
public double CalculateDistance(
    double lat1, double lon1, 
    double lat2, double lon2)
{
    const double R = 6371000; // Earth radius in meters
    
    var dLat = ToRadians(lat2 - lat1);
    var dLon = ToRadians(lon2 - lon1);
    
    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
    
    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    
    var distanceMeters = R * c;
    var distanceYards = distanceMeters * 1.09361; // Convert to yards
    
    return Math.Round(distanceYards);
}
```

### Geofence Detection
```csharp
public int? DetectHole(double latitude, double longitude, Course course)
{
    foreach (var hole in course.Holes)
    {
        if (hole.TeeBoxLatitude == null || hole.TeeBoxLongitude == null)
            continue;
        
        var distance = CalculateDistance(
            latitude, longitude,
            hole.TeeBoxLatitude.Value, hole.TeeBoxLongitude.Value);
        
        var radius = hole.GeofenceRadius ?? 50; // Default 50 meters
        
        if (distance <= radius)
            return hole.HoleNumber;
    }
    
    return null; // Not on any hole
}
```

## 📱 Mobile App Implementation

### Background Location Tracking
```csharp
// MAUI - Request location permission
var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

// Start location tracking
var location = await Geolocation.GetLocationAsync(new GeolocationRequest
{
    DesiredAccuracy = GeolocationAccuracy.Best,
    Timeout = TimeSpan.FromSeconds(10)
});

// Continuous tracking during round
var request = new GeolocationListeningRequest(GeolocationAccuracy.Best);
Geolocation.LocationChanged += OnLocationChanged;
await Geolocation.StartListeningForegroundAsync(request);
```

### Auto-Detection Logic
```csharp
private async void OnLocationChanged(object sender, GeolocationLocationChangedEventArgs e)
{
    var location = e.Location;
    
    // Detect current hole
    var result = await _apiClient.DetectHoleAsync(
        _currentCourse.Key,
        location.Latitude,
        location.Longitude,
        _currentTee.Id);
    
    if (result.HoleNumber != _currentHole)
    {
        // Hole changed - update UI
        _currentHole = result.HoleNumber;
        await UpdateHoleDisplay(result);
        
        // Auto-populate tee club
        if (result.Location == "tee-box")
        {
            _currentRoundHole.TeeClub = result.SuggestedClub;
        }
    }
    
    // Update distance to green
    _distanceToGreen = result.DistanceToGreen;
    UpdateDistanceDisplay();
}
```

## 🎨 UI/UX Considerations

### Hole Detection Notification
- Show toast: "Now on Hole 7 - Par 4, 380 yards"
- Vibrate phone for confirmation
- Allow manual override

### Distance Display
- Large, easy-to-read distance to green
- Update in real-time as golfer walks
- Show elevation change if available

### Club Suggestion
- Auto-populate but allow override
- Learn from golfer's selections
- Show confidence level

## 🔋 Battery Optimization

### Strategies
1. **Geofencing**: Only track location when near course
2. **Adaptive Polling**: Reduce GPS frequency when stationary
3. **Background Limits**: Pause tracking between holes
4. **Course Bounds**: Stop tracking when leaving course

```csharp
// Only track when on course
if (IsWithinCourseBounds(location, course))
{
    // High-frequency tracking (every 5 seconds)
    await TrackLocationAsync(TimeSpan.FromSeconds(5));
}
else
{
    // Low-frequency tracking (every 60 seconds)
    await TrackLocationAsync(TimeSpan.FromSeconds(60));
}
```

## 📊 Data Collection for Course Mapping

### Admin Tool: GPS Data Collection
- Allow course admins to walk the course
- Record GPS coordinates for each hole
- Automatically calculate geofence radius
- Verify accuracy with test rounds

### API Endpoints
```
POST   /api/v1/courses/{courseKey}/holes/{holeNumber}/gps/record
       # Record GPS point during course mapping
       
GET    /api/v1/courses/{courseKey}/gps/coverage
       # Check which holes have GPS data
       
POST   /api/v1/courses/{courseKey}/gps/calculate-geofences
       # Auto-calculate optimal geofence radii
```

## ✅ Future Enhancements

- [ ] Elevation data integration
- [ ] Wind speed/direction from weather API
- [ ] Shot tracking (record each shot's GPS location)
- [ ] Heat maps of golfer's shot patterns
- [ ] AR overlay showing distance to hazards
- [ ] Offline mode with cached course data

