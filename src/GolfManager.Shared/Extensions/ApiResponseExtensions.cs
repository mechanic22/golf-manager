namespace GolfManager.Shared.DTOs.Common;

public static class ApiResponseExtensions
{
    /// <summary>
    /// Returns true when the response exists, reports success, and contains non-null data.
    /// Use this as the single standard for all API response checks in the UI.
    /// </summary>
    public static bool IsSuccessWithData<T>(this ApiResponse<T>? response) where T : class =>
        response?.Success == true && response.Data != null;

    /// <summary>
    /// Same check for value-type data (int, double, bool, etc.).
    /// </summary>
    public static bool IsSuccess<T>(this ApiResponse<T>? response) =>
        response?.Success == true;
}
