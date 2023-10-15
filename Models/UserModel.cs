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
    public string? Email { get; set; }
    public bool Notify { get; set; }

    public static User? GetByToken(string token, IDataBaseContext context) {
        var result = context.Execute($"SELECT user_id, login, password, is_admin, email, notify FROM {TableName} WHERE token = {token} LIMIT 1");
        if(result == null) return null;
        result.Read();
        User user = new User(){
            ID = (int)result[0],
            Login = (string)result[1],
            Password = (string)result[2],
            Token = token,
            IsAdmin = (int)result[3] == 0 ? false : true,
            Email = result.Get<string>(4, null),
            Notify = result.Get<int>(5) == 1 ? true : false
        };
        result.Close();
        return user;
    }

    public static User? Validate(string login, string password, IDataBaseContext context) {
        var result = context.Execute($"SELECT user_id, token, is_admin, email, notify FROM {TableName} WHERE login = @Login AND password = @Password LIMIT 1", new KeyValuePair<string, object?>("@Login", login), new KeyValuePair<string, object?>("@Password", password));
        if(result == null || !result.Read()) return null;

        User user = new User(){
            ID = (int)result[0],
            Login = login,
            Password = password,
            Token = result.Get<string>(1, null),
            IsAdmin = Convert.ToBoolean(result.Get<sbyte>(2, 0)),
            Email = result.Get<string>(4, null),
            Notify = result.Get<int>(5) == 1 ? true : false
        };
        result.Close();
        return user;
    }

    public static string[]? GetAllActiveEmails(IDataBaseContext context){
        List<string> emails = new(10);
        var result = context.Execute($"SELECT email FROM {TableName} WHERE notify = 1 AND email IS NOT NULL");
        if(result == null) return null;

        while(result.Read()){
            emails.Add(result.Get<string>(0) ?? string.Empty);
        }

        result.Close();

        return emails.ToArray();
    }

    public bool Save(IDataBaseContext context){
        if(string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password)) return false;
        string query;
        if(ID == 0){
            query = $"INSERT INTO {TableName} (login, password, token, is_admin) VALUES (@Login, @Password, @Token, @IsAdmin, @Email, @Notify)";
        }else{
            query = $"UPDATE {TableName} SET login = @Login, password = @Password, token =  @Token, is_admin = @IsAdmin, email = @Email, notify = @Notify WHERE user_id = {ID}";
        }
        return context.ExecuteNonQuery(query, new("@Login", Login), new("@Password", Password), new ("@Token", Token), new("@IsAdmin", IsAdmin ? 1 : 0 ), new ("@Email", Email), new("@Notify", IsAdmin ? 1 : 0));
    }

    public User(string login, string password, string? token = null, bool isAdmin = false, string? email = null, bool notify = false){
        this.Login = login;
        this.Password = password;
        this.Token = token;
        this.IsAdmin = isAdmin;
        this.Email = email;
        this.Notify = notify;
    }

    public User(string login, string password) : this(login, password, null) {}

    public User() : this(string.Empty, string.Empty) {}
}