namespace BankOs.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Code { get; set; } = null!;
    public string Message { get; set; } = null!;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa") => new()
    {
        Success = true,
        Code = "SUCCESS",
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Fail(string code, string message) => new()
    {
        Success = false,
        Code = code,
        Message = message,
        Data = default
    };
}