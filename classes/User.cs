
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

[Table("Person", Schema ="dbo")]
public class Person
{

    [Column("username")]
    public string Username {get; set;}
    [JsonIgnore]
    [NotMapped]
    public string Password {get; set;}
    [JsonIgnore]
    [Column("password_hash")]
    public string Password_Hash {get; private set;}
    [Column("id")]
    public int Id {get; private set;}
    [Column("role")]
    public string Role {get; set;}
    
    private Person() {}
    public Person(string username, string password, string role)
    {   
        Username = username;
        Password = password;
        Password_Hash = BCrypt.Net.BCrypt.HashPassword(Password, workFactor: 12);
        Role = role;
    }
}