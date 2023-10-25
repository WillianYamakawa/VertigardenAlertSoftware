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
        return null;
    }

    public bool Save(IDataBaseContext context){
        return false;
    }

    public class Result{

        public int Id { get; set; }
        public string Name { get; set; }
        public DocumentType DocType { get; set; }
        public string Doc { get; set; }
        public int DeviceCount { get; set; }

        public Result(int id, string name, DocumentType docType, string doc, int deviceCount)
        {
            Id = id;
            Name = name;
            DocType = docType;
            Doc = doc;
            DeviceCount = deviceCount;
        }
    }

    public static Result[]? GetRecords(int pagingStart, int pagingEnd, string? name, string? doc, IDataBaseContext context){
        return null;
    }

    public Customer() => this.ID = 0;

    public Customer(string? name, DocumentType docType, string? document) : this()
    {
        DocType = docType;
        Document = document;
        Name = name;
    }

}