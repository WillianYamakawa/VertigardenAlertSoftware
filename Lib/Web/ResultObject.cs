namespace Project.Lib.Web;

public class ResultObject{
    public List<object> Errors { get; set; }
    public object? Payload { get; set; }
    public bool Unauthorized { get; set; }
    public bool Unauthenticated { get; set; }
    public string? Redirect { get; set; }

    public static ResultObject BuildErrors(params object[] errors){
        ResultObject result = new ResultObject();
        result.Errors.AddRange(errors);
        return result;
    }

    public static ResultObject Build(object? payload = null, bool unauthorized = false, bool unauthenticated = false, string? redirect = null) => new ResultObject() { Payload = payload, Unauthorized = unauthorized, Unauthenticated = unauthenticated, Redirect = redirect};

    public ResultObject() => this.Errors = new List<object>();
}