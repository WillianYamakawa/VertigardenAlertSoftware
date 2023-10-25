using Project.Models;
using Project.Lib.Database;

namespace Project.Lib.Web;

public class AuthGetter{
    public static User? Get(HttpRequest request, IDataBaseContext context){
        string? value = request.Headers["Authentication"];
        if(value == null) return null;
        if(!value.StartsWith("Basic ")) return null;
        string token = value.Substring(6, value.Length - 6);
        return User.GetByToken(token, context);
    }
}