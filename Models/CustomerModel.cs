using Project.Lib.Database;

namespace Project.Models;

public class Customer{
    public enum DocumentType : byte{
        CPF = 0x0, 
        CNPJ = 0x1
    }
    public int ID { get; set; }
    public DocumentType DocType { get; set; }
    public string? Document { get; set; }
    public string? Name { get; set; }

    public static Customer? GetByID(int id, IDataBaseContext context){
        return null;
    }
}