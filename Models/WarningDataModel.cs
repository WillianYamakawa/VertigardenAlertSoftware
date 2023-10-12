using Project.Lib.Database;

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

    public WarningData() {
        ID = 0;
        DeviceID = 0;
    }
}