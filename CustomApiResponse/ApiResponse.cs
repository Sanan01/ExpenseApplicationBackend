namespace ExpenseApplication.CustomApiResponse
{
	public class ApiResponse<T>(int statusCode, string message, T? data = default, string? error = null)
	{
		public int StatusCode { get; set; } = statusCode;
		public string Message { get; set; } = message;
		public T? Data { get; set; } = data;
		public string? Error { get; set; } = error;
	}
}
