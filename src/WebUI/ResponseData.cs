
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
namespace WebUI;

public class ResponseData<T>
{
    public T? Data { get; set; }

    [JsonIgnore] public int StatusCode { get; set; }

    public List<string> Errors { get; set; }

    public static ResponseData<T> Success(int statusCode, T data)
    {
        return new ResponseData<T> { Data = data, StatusCode = statusCode };
    }

    public static ResponseData<T> Success(int statusCode)
    {
        return new ResponseData<T> { StatusCode = statusCode };
    }

    public static ResponseData<T> Fail(int statusCode, List<string> errors)
    {
        return new ResponseData<T> { StatusCode = statusCode, Errors = errors };
    }
}

