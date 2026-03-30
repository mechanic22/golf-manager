# Automatic Token Refresh Implementation

## Overview

The application now automatically refreshes JWT access tokens before they expire, eliminating the need for users to log in every hour.

## How It Works

### 1. Token Storage
When a user logs in, the following data is stored in browser localStorage:
- Access token (JWT)
- Refresh token
- Token expiration time
- User information (email, name, ID, admin status)

### 2. Automatic Refresh Timer
- A timer is scheduled to refresh the token **5 minutes before expiration**
- If the token expires in less than 5 minutes, it refreshes immediately
- The timer is automatically rescheduled after each successful refresh

### 3. Token Refresh Flow
1. Timer triggers 5 minutes before token expiration
2. Client sends refresh token to API endpoint: `POST /api/v1/auth/refresh`
3. API validates the refresh token and issues a new access token
4. Client stores the new tokens and reschedules the next refresh
5. If refresh fails, user is automatically logged out

### 4. Authenticated HTTP Client Handler
- All HTTP requests automatically include the `Authorization: Bearer <token>` header
- No need to manually add auth headers in service methods
- Cleaner, more maintainable code

## Components

### AuthService (`GolfManager.Web/Services/AuthService.cs`)
- Manages authentication state
- Stores and retrieves tokens from localStorage
- Schedules automatic token refresh
- Handles token refresh logic

### AuthenticatedHttpClientHandler (`GolfManager.Web/Services/AuthenticatedHttpClientHandler.cs`)
- DelegatingHandler that intercepts all HTTP requests
- Automatically adds Authorization header
- Logs 401 responses for debugging

### Program.cs
- Registers the AuthenticatedHttpClientHandler
- Initializes AuthService on app startup to restore session

## Configuration

Token expiration times are configured in the API's `appsettings.json`:

```json
{
  "Jwt": {
    "AccessTokenExpirationMinutes": "60",
    "RefreshTokenExpirationDays": "30"
  }
}
```

- **Access Token**: Valid for 60 minutes
- **Refresh Token**: Valid for 30 days
- **Auto-refresh**: Triggers 5 minutes before access token expires

## User Experience

### Before
- User logs in
- After 60 minutes, token expires
- Next API call fails with 401
- User sees "Failed to create league" error
- User must log out and log back in

### After
- User logs in
- Token automatically refreshes every 55 minutes
- User stays logged in for up to 30 days (refresh token lifetime)
- Seamless experience with no interruptions

## Error Handling

### Token Refresh Fails
If the refresh token is invalid or expired:
1. User is automatically logged out
2. All tokens are cleared from localStorage
3. User is redirected to login page

### 401 Unauthorized Response
If an API call returns 401:
1. Client logs the error
2. Returns user-friendly message: "Your session has expired. Please log in again."
3. No JSON parsing errors on empty 401 responses

## Security Considerations

1. **Refresh Token Rotation**: Each refresh generates a new refresh token, invalidating the old one
2. **Secure Storage**: Tokens stored in browser localStorage (consider HttpOnly cookies for production)
3. **Token Revocation**: API supports revoking individual or all user tokens
4. **Expiration Validation**: API validates token expiration with zero clock skew

## Testing

To test the automatic refresh:
1. Log in to the application
2. Check browser console for: `[AuthService] Token expires at <time>, will refresh in <minutes> minutes`
3. Wait for the refresh to trigger (or set a shorter expiration time for testing)
4. Verify the token is refreshed without user intervention
5. Confirm API calls continue to work after refresh

## Future Enhancements

- [ ] Move tokens to HttpOnly cookies for better security
- [ ] Add refresh token sliding expiration
- [ ] Implement "Remember Me" functionality
- [ ] Add token refresh retry logic with exponential backoff
- [ ] Show user notification when session is about to expire

