using System.Text;
using Project.Lib.Database;
using Project.Models;

namespace Project.Models;

public class WarningData{
    public const string TableName = "water_level_warnings";
    private Device? _device;
    private int _deviceID;
    public int ID { get; private set; }
    public int DeviceID { get { return _deviceID; } set { _deviceID = value; _device = null; } }
    public DateTime? CapturedAt { get; set; }
    
    public Device? GetDevice(IDataBaseContext context){
        if(_device == null) _device = Device.GetByID(DeviceID, context);
        return _device;
    }

    public void SetDevice(Device device){
        _device = device;
        if(_device == null) DeviceID = 0;
        else DeviceID = _device.ID;
    }

    public bool Save(IDataBaseContext context){
        if(DeviceID == 0 || CapturedAt == null) return false;
        string query;
        if(ID == 0){
            query = $"INSERT INTO {TableName} (device, captured_at) VALUES (@DeviceID, @CapturedAt)";
        }else{
            query = $"UPDATE {TableName} SET device = @DeviceID, captured_at = @CapturedAt WHERE warning_id = {ID}";
        }
        return context.ExecuteNonQuery(query, new("@DeviceID", DeviceID), new("@CapturedAt", CapturedAt));
    }

    public static WarningData? GetByID(int id, IDataBaseContext context){
        var result = context.Execute($"SELECT device, captured_at FROM {TableName} WHERE warning_id = {id} LIMIT 1");
        if(result == null) return null;
        result.Read();
        WarningData data = new() {
            ID = id,
            DeviceID = (int)result[0],
            CapturedAt = (DateTime)result[1]
        };
        result.Close();
        return data;
    }

    public class WarningDataLevelResult{
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string DeviceToken { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerDoc { get; set; }
        public DateTime CapturedAt { get; set; }

        public WarningDataLevelResult(int id, int deviceId, string deviceToken, int? customerId, string? customerName, string? customerDoc, DateTime capturedAt)
        {
            Id = id;
            DeviceId = deviceId;
            DeviceToken = deviceToken;
            CustomerId = customerId;
            CustomerName = customerName;
            CustomerDoc = customerDoc;
            CapturedAt = capturedAt;
        }

        public WarningDataLevelResult(int id, int deviceId, string deviceToken, DateTime capturedAt)
        {
            Id = id;
            DeviceId = deviceId;
            DeviceToken = deviceToken;
            CapturedAt = capturedAt;
        }
    }

    public static WarningDataLevelResult[]? GetRecords(int pagingStart, int pagingEnd, int? customerId, string? device, DateTime? dateStart, DateTime? dateEnd, IDataBaseContext context){
        StringBuilder builder = new StringBuilder(512);
        List<KeyValuePair<string, object?>> values = new List<KeyValuePair<string, object?>>(4);

        builder.Append($"SELECT {TableName}.warning_id, {Models.Device.TableName}.device_id, {Models.Device.TableName}.token, {Models.Customer.TableName}.customer_id,  {Models.Customer.TableName}.name, {Models.Customer.TableName}.doc, {TableName}.captured_at FROM {TableName} INNER JOIN {Models.Device.TableName} ON {TableName}.device = {Models.Device.TableName}.device_id INNER JOIN {Models.Customer.TableName} ON {Models.Customer.TableName}.customer_id = {Models.Device.TableName}.owner WHERE 1=1");

        if(customerId != null) { 
            builder.Append($" AND {Models.Customer.TableName}.customer_id = @Customer"); 
            values.Add(new ("@Customer", customerId));
        }
        if(device != null) { 
            builder.Append($" AND {Models.Device.TableName}.token = @Device");
            values.Add(new ("@Device", device));
        }
        if(dateStart != null) { 
            builder.Append($" AND {TableName}.captured_at >= @DateStart");
            values.Add(new ("@DateStart", dateStart));
        }
        if(dateStart != null) { 
            builder.Append($" AND {TableName}.captured_at <= @DateEnd");
            values.Add(new ("@DateEnd", dateEnd));
        }

        builder.Append($" ORDER BY {TableName}.captured_at DESC LIMIT {Math.Max(Math.Max(1, pagingStart), pagingEnd) - Math.Max(1, pagingStart) + 1} OFFSET {Math.Max(0, pagingStart-1)}");

        MySqlConnector.MySqlDataReader? reader = context.Execute(builder.ToString(), values.ToArray());
        if(reader == null) return null;

        List<WarningDataLevelResult> result = new List<WarningDataLevelResult>();

        while(reader.Read()){
            result.Add(new WarningDataLevelResult(reader.Get<int>(0), reader.Get<int>(1), reader.Get<string>(2) ?? string.Empty, reader.Get<int?>(3, null), reader.Get<string>(4, null), reader.Get<string>(5, null), reader.Get<DateTime>(6)));
        }

        reader.Close();

        return result.ToArray();
    }

    public WarningData() {
        ID = 0;
        DeviceID = 0;
    }
}