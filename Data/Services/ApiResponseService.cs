using ExpenseApplication.CustomApiResponse;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApplication.Data.Services
{
	public class ApiResponseService
	{
		// Centralized success response
		public IActionResult ApiResponseSuccess(string message, object? data = null)
		{
			var response = new ApiResponse<object>(200, message, data);
			return new OkObjectResult(response);
		}

		// Centralized error response
		public IActionResult ApiResponseError(string errorMessage)
		{
			var response = new ApiResponse<object>(500, "An error occurred", error: errorMessage);
			return new ObjectResult(response) { StatusCode = 500 };
		}

		// Centralized bad request response
		public IActionResult ApiResponseBadRequest(string errorMessage)
		{
			var response = new ApiResponse<object>(400, "Bad Request", error: errorMessage);
			return new BadRequestObjectResult(response);
		}

		// Centralized not found response
		public IActionResult ApiResponseNotFound(string message, string errorMessage)
		{
			var response = new ApiResponse<object>(404, message, error: errorMessage);
			return new NotFoundObjectResult(response);
		}
	}
}
