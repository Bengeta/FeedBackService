using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Models.DBTables;
public class MessageModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public long UserId { get; set; }
    public string Message { get; set; }
    public string Answer { get; set; }
}

