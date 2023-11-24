using System.Text;
using Project.Lib.Database;

namespace Project.Models;

public class Customer{
    public const string TableName = "customers";

    public enum DocumentType : byte{
        CPF = 0x0, 
        CNPJ = 0x1
    }
    public int ID { get; private set; }
    public DocumentType DocType { get; set; }
    public string? Document { get; set; }
    public string? Name { get; set; }

    public static string SanitizeDoc(string raw){
        StringBuilder builder = new StringBuilder(raw.Length);
        for (int i = 0; i < raw.Length; i++)
        {
            if (Char.IsDigit(raw[i]))
                builder.Append(raw[i]);
        }
        return builder.ToString();
    }

    public static Customer? GetByID(int id, IDataBaseContext context){
        var result = context.Execute($"SELECT customer_id, name, doc_type, doc FROM {TableName} WHERE customer_id = {id}");
        if(result == null || !result.Read()) return null;
        Customer customer = new Customer(){
            ID = result.Get<int>(0),
            Name = result.Get<string>(1),
            DocType = result.Get<string>(2) == "CPF" ? DocumentType.CPF : DocumentType.CNPJ,
            Document = result.Get<string>(3)
        };
        result.Close();
        return customer;
    }

    public bool Save(IDataBaseContext context){
        if(string.IsNullOrWhiteSpace(this.Name) || string.IsNullOrWhiteSpace(this.Document)) return false;
        string query;
        if(this.ID == 0){
            query = $"INSERT INTO {TableName} (name, doc_type, doc) VALUES (@Name, @DocType, @Doc)";
        }else{
            query = $"UPDATE {TableName} SET name = @Name, doc_type = @DocType, doc =  @Doc WHERE customer_id = {this.ID}";
        }
        return context.ExecuteNonQuery(query, new("@Name", this.Name), new("@DocType", this.DocType == DocumentType.CPF ? "CPF" : "CNPJ"), new ("@Doc", this.Document));
    }

    public static Customer[]? GetRecords(int pagingStart, int pagingEnd, string? name, string? doc, IDataBaseContext context){
        StringBuilder builder = new StringBuilder(512);
        List<KeyValuePair<string, object?>> values = new List<KeyValuePair<string, object?>>(4);

        builder.Append($"SELECT customer_id, name, doc_type, doc FROM {TableName} WHERE 1=1");

        if(name != null) { 
            builder.Append($" AND name LIKE @Name");
            values.Add(new ("@Name", '%' + name + '%'));
        }
        if(doc != null) { 
            builder.Append($" AND doc LIKE @Doc");
            values.Add(new ("@Doc", '%' + doc + '%'));
        }

        builder.Append($" ORDER BY name LIMIT {Math.Max(Math.Max(1, pagingStart), pagingEnd) - Math.Max(1, pagingStart) + 1} OFFSET {Math.Max(0, pagingStart-1)}");

        MySqlConnector.MySqlDataReader? reader = context.Execute(builder.ToString(), values.ToArray());
        if(reader == null) return null;

        List<Customer> result = new List<Customer>();

        while(reader.Read()){
            result.Add(new Customer() { ID = reader.Get<int>(0), Name = reader.Get<string>(1), DocType = reader.Get<string>(2) == "CPF" ? DocumentType.CPF : DocumentType.CNPJ, Document = reader.Get<string>(3) });
        }

        reader.Close();

        return result.ToArray();
    }

    public Customer() => this.ID = 0;

    public Customer(string? name, DocumentType docType, string? document) : this()
    {
        DocType = docType;
        Document = document;
        Name = name;
    }

}