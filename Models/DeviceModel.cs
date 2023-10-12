using Project.Lib.Database;

namespace Project.Models;

public class Device{
    public const string TableName = "devices";
    private Customer? _owner;
    private int _ownerID;
    public int ID { get; set; }
    public int OwnerID { get { return _ownerID; } set { _ownerID = value; _owner = null; } }
    public string? Token { get; set; }

    public Customer? GetOwner(IDataBaseContext context){
        if(_owner == null) _owner = Customer.GetByID(OwnerID, context);
        return _owner;
    }

    public void SetOwner(Customer owner){
        _owner = owner;
        if(_owner == null) OwnerID = 0;
        else OwnerID = _owner.ID;
    }

    public static Device? GetByID(int id, IDataBaseContext context) {
        var result = context.Execute($"SELECT owner, token FROM {TableName} WHERE device_id = {id} LIMIT 1");
        if(result == null) return null;
        result.Read();
        Device data = new() {
            ID = id,
            OwnerID = (int)result[0],
            Token = (string)result[1]
        };
        result.Close();
        return data;
    }

    public static Device? GetByToken(string token,  IDataBaseContext context){
        var result = context.Execute($"SELECT device_id, owner FROM {TableName} WHERE token = '{token}' LIMIT 1");
        if(result == null || !result.Read()) return null;
        int a = (int)result[0];
        int b = result.GetNullableStruct<int>(1);

        Device data = new() {
            ID = a,
            OwnerID = b,
            Token = token
        };

        result.Close();
        return data;
    }
}