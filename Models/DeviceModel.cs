using Project.Lib.Database;
using System;
using System.Text;
using Project.Models;

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

    public bool Save(IDataBaseContext context){
        if(string.IsNullOrWhiteSpace(this.Token)) return false;
        string query;
        if(this.ID == 0){
            query = $"INSERT INTO {TableName} (token, owner) VALUES (@Token, @OwnerId)";
        }else{
            query = $"UPDATE {TableName} SET token = @Token, owner = @OwnerId WHERE device_id = {this.ID}";
        }
        return context.ExecuteNonQuery(query, new("@Token", this.Token), new("@OwnerId", this.OwnerID > 0 ? this.OwnerID : null));
    }

    public static Device? GetByID(int id, IDataBaseContext context) {
        var result = context.Execute($"SELECT owner, token FROM {TableName} WHERE device_id = {id} LIMIT 1");
        if(result == null || !result.Read()) return null;
        
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
        int b = result.Get<int>(1);

        Device data = new() {
            ID = a,
            OwnerID = b,
            Token = token
        };

        result.Close();
        return data;
    }

    public class DeviceResult{
        public int DeviceID { get; set; }
        public string DeviceToken { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerDoc { get; set; }

        public DeviceResult(int deviceID, string deviceToken, int customerID, string customerName, string customerDoc)
        {
            DeviceID = deviceID;
            DeviceToken = deviceToken;
            CustomerID = customerID;
            CustomerName = customerName;
            CustomerDoc = customerDoc;
        }
    }

    public static DeviceResult[]? GetRecords(int pagingStart, int pagingEnd, int? ownerId, IDataBaseContext context){
        StringBuilder builder = new StringBuilder(512);

        builder.Append($"SELECT {TableName}.device_id, {TableName}.token, {Models.Customer.TableName}.customer_id, {Models.Customer.TableName}.name, {Models.Customer.TableName}.doc FROM {TableName} LEFT JOIN {Models.Customer.TableName} ON {TableName}.owner = {Models.Customer.TableName}.customer_id WHERE 1=1");

        if(ownerId != null) { 
            builder.Append($" AND owner = {ownerId}");
        }

        builder.Append($" ORDER BY name LIMIT {Math.Max(Math.Max(1, pagingStart), pagingEnd) - Math.Max(1, pagingStart) + 1} OFFSET {Math.Max(0, pagingStart-1)}");

        MySqlConnector.MySqlDataReader? reader = context.Execute(builder.ToString());
        if(reader == null) return null;

        List<DeviceResult> result = new List<DeviceResult>();

        while(reader.Read()){
            result.Add(new DeviceResult(reader.Get<int>(0), reader.Get<string>(1) ?? string.Empty, reader.Get<int>(2), reader.Get<string>(3) ?? string.Empty, reader.Get<string>(4) ?? string.Empty));
        }

        return result.ToArray();
    }
}