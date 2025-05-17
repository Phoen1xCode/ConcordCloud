using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace ConcordCloud.Core.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public int StatusCode { get; set; }

    public ApiResponse() {}

    public ApiResponse(bool success, string message, T data, int statusCode)
    {
        Success = success;
        Message = message;
        Data = data;
        StatusCode = statusCode;
    }


    public static ApiResponse<T> Ok(T data, string message = "Success")
    {
        return new ApiResponse<T>(true, message, data, (int)HttpStatusCode.OK);
    }

    public static ApiResponse<T> Created(T data, string message = "Created")
    {
        return new ApiResponse<T>(true, message, data, (int)HttpStatusCode.Created);
    }

    public static ApiResponse<T> BadRequest(string message = "Bad Request")
    {
        return new ApiResponse<T>(false, message, default, (int)HttpStatusCode.BadRequest);
    }

    public static ApiResponse<T> NotFound(string message = "Not Found")
    {
        return new ApiResponse<T>(false, message, default, (int)HttpStatusCode.NotFound);
    }

    public static ApiResponse<T> Unauthorized(string message = "Unauthorized")
    {
        return new ApiResponse<T>(false, message, default, (int)HttpStatusCode.Unauthorized);
    }

    public static ApiResponse<T> Forbidden(string message = "Forbidden")
    {
        return new ApiResponse<T>(false, message, default, (int)HttpStatusCode.Forbidden);
    }

    public static ApiResponse<T> Error(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        return new ApiResponse<T>(false, message, default, (int)statusCode);
    }

    public ObjectResult ToActionResult()
    {
        return new ObjectResult(this)
        {
            StatusCode = StatusCode
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public ApiResponse() : base()
    {
    }

    public ApiResponse(bool success, string message, object data, int statusCode) 
        : base(success, message, data, statusCode)
    {
    }

    public static ApiResponse Ok(string message = "Success")
    {
        return new ApiResponse(true, message, null, (int)HttpStatusCode.OK);
    }

    public static ApiResponse Created(string message = "Created")
    {
        return new ApiResponse(true, message, null, (int)HttpStatusCode.Created);
    }

    public static ApiResponse BadRequest(string message = "Bad Request")
    {
        return new ApiResponse(false, message, null, (int)HttpStatusCode.BadRequest);
    }

    public static ApiResponse NotFound(string message = "Not Found")
    {
        return new ApiResponse(false, message, null, (int)HttpStatusCode.NotFound);
    }

    public static ApiResponse Unauthorized(string message = "Unauthorized")
    {
        return new ApiResponse(false, message, null, (int)HttpStatusCode.Unauthorized);
    }

    public static ApiResponse Forbidden(string message = "Forbidden")
    {
        return new ApiResponse(false, message, null, (int)HttpStatusCode.Forbidden);
    }

    public static ApiResponse Error(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        return new ApiResponse(false, message, null, (int)statusCode);
    }
}
