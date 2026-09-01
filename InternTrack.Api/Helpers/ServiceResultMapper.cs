using InternTrack.Business.Common;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Helpers;

public static class ServiceResultMapper
{
    public static IActionResult ToActionResult(
        ControllerBase controller,
        ServiceResult result,
        int successStatusCode = StatusCodes.Status200OK,
        bool noContentOnSuccess = false)
    {
        if (result.Type == ResultType.Success)
        {
            if (noContentOnSuccess)
            {
                return controller.NoContent();
            }

            return controller.StatusCode(
                successStatusCode,
                new
                {
                    message = result.Message
                }
            );
        }

        return result.Type switch
        {
            ResultType.NotFound => controller.NotFound(
                new { message = result.Message }
            ),

            ResultType.ValidationError => controller.BadRequest(
                new { message = result.Message }
            ),

            ResultType.Conflict => controller.Conflict(
                new { message = result.Message }
            ),

            ResultType.Forbidden => controller.StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = result.Message }
            ),

            _ => controller.BadRequest(
                new { message = result.Message }
            )
        };
    }

    public static IActionResult ToActionResult<T>(
        ControllerBase controller,
        ServiceResult<T> result)
    {
        if (result.Type == ResultType.Success)
        {
            return controller.Ok(result.Data);
        }

        return result.Type switch
        {
            ResultType.NotFound => controller.NotFound(
                new { message = result.Message }
            ),

            ResultType.ValidationError => controller.BadRequest(
                new { message = result.Message }
            ),

            ResultType.Conflict => controller.Conflict(
                new { message = result.Message }
            ),

            ResultType.Forbidden => controller.StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = result.Message }
            ),

            _ => controller.BadRequest(
                new { message = result.Message }
            )
        };
    }
}