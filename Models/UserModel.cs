using Project.Models;
using Project.Lib.Database;

namespace Project.Models;

public class User{
    public const string TableName = "users";
    public int ID { get; private set; }
    public string Login { get; set; }
    public string Password { get; set; }

    public string? Token { get; set; }

    public bool IsAdmin { get; set; }

    public static User? GetByToken(string token, IDataBaseContext context) {
        var result = context.Execute($"SELECT user_id, login, password, is_admin FROM {TableName} WHERE token = {token} LIMIT 1");
        if(result == null) return null;
        result.Read();
        User user = new User(){
            ID = (int)result[0],
            Login = (string)result[1],
            Password = (string)result[2],
            Token = token,
            IsAdmin = (int)result[3] == 1 ? true : false
        };
        result.Close();
        return user;
    }

    public static User? Validate(string login, string password, IDataBaseContext context) {
        var result = context.Execute($"SELECT user_id, token, is_admin FROM {TableName} WHERE login = @Login AND password = @Password LIMIT 1", new KeyValuePair<string, object?>("@Login", login), new KeyValuePair<string, object?>("@Password", password));
        if(result == null || !result.Read()) return null;

        User user = new User(){
            ID = (int)result[0],
            Login = login,
            Password = password,
            Token = result.Get<string>(1, null),
            IsAdmin = Convert.ToBoolean(result.Get<sbyte>(2, 0))
        };
        result.Close();
        return user;
    }

    public bool Save(IDataBaseContext context){
        if(string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password)) return false;
        string query;
        if(ID == 0){
            query = $"INSERT INTO {TableName} (login, password, token, is_admin) VALUES (@Login, @Password, @Token, @IsAdmin)";
        }else{
            query = $"UPDATE {TableName} SET login = @Login, password = @Password, token =  @Token, is_admin = @IsAdmin WHERE user_id = {ID}";
        }
        return context.ExecuteNonQuery(query, new("@Login", Login), new("@Password", Password), new ("@Token", Token), new("@IsAdmin", IsAdmin ? 1 : 0 ));
    }

    public User(string login, string password, string? token, bool isAdmin = false){
        this.Login = login;
        this.Password = password;
        this.Token = token;
        this.IsAdmin = isAdmin;
    }

    public User(string login, string password) : this(login, password, null) {}

    public User() : this(string.Empty, string.Empty) {}
}