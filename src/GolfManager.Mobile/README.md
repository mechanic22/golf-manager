# GolfManager Mobile

.NET MAUI app for iOS and Android. Lives inside `GolfManager.sln` and references `GolfManager.Shared` directly.

---

## Prerequisites

- .NET 10 SDK
- Xcode 26+ (for iOS)
- Android SDK API 36 (installed automatically below if missing)

---

## Running on iOS Simulator

**1. Find your simulator UDID:**
```bash
xcrun simctl list devices available | grep iPhone
```

**2. Run:**
```bash
cd /path/to/golf-manager
dotnet build src/GolfManager.Mobile -f net10.0-ios -t:Run -p:_DeviceName=:v2:udid=YOUR_UDID_HERE
```

Example with iPhone 16:
```bash
dotnet build src/GolfManager.Mobile -f net10.0-ios -t:Run -p:_DeviceName=:v2:udid=99897C9E-7265-4C2E-BD16-510215631597
```

---

## Running on a Physical iPhone

1. Open `GolfManager.sln` in Visual Studio or Rider
2. Select `GolfManager.Mobile` as the startup project
3. Set target to your device
4. Build and run — Xcode signing must be configured first

---

## API Configuration

Edit `Resources/Raw/appsettings.json` before running:

```json
{ "ApiBaseUrl": "https://localhost:7012" }
```

> **Note:** `localhost` works in the iOS Simulator. On a real iPhone, use your Mac's LAN IP (e.g. `https://192.168.1.x:7012`). The API must be running before launching the app.

**Start the API:**
```bash
dotnet run --project src/GolfManager.Api
```

---

## Google OAuth Setup

The "Sign in with Google" button uses the server-side OAuth flow:

1. Go to [Google Cloud Console](https://console.cloud.google.com) → APIs & Services → Credentials
2. Create an **OAuth 2.0 Client ID** (type: **Web application**)
3. Under **Authorized redirect URIs**, add:
   - `https://localhost:7012/signin-google` (development)
   - `https://api.dkgolf.com/signin-google` (production)
4. Copy the credentials into the API's `appsettings.Development.json`:

```json
{
  "Google": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET"
  }
}
```

> The deep link scheme (`dkgolf://`) is already registered in `Info.plist` (iOS) and `AndroidManifest.xml` (Android). The API redirect URI is `/signin-google` — this is the Google SDK's default callback path and must match exactly what you register in Google Console.

---

## Install Android SDK (first time only)

```bash
dotnet build src/GolfManager.Mobile -t:InstallAndroidDependencies -f net10.0-android \
  -p:AndroidSdkDirectory=/Users/YOUR_USER/Library/Android/sdk \
  -p:AcceptAndroidSDKLicenses=True
```
