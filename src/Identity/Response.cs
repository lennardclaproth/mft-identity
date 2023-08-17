namespace LClaproth.MyFinancialTracker.Identity;

public class Response<T>
{
    public T? Data { get; set; }
    public MetaData? MetaData { get; set; }
    public string Message { get; set; }
    public Error? Error { get; set; }

    public Response(T? responseData, Exception? exception)
    {
        if (typeof(T).Equals(typeof(Exception)))
        {
            this.Error = new Error
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace
            };
        }
        else
        {
            Data = responseData;
        }
    }
}

public class MetaData
{

}

public class Error
{
    public string Message { get; set; }
    public string StackTrace { get; set; }
}