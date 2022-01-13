namespace Distribt.Shared.Serialization;

public interface ISerializer
{
    T DeserializeObject<T>(string input);
    string SerializeObject<T>(T obj);
    T DeserializeObject<T>(byte[] input) where T : class;
    byte[] SerializeObjectToByteArray<T>(T obj);
    object? DeserializeObject(byte[] input, Type myType);
}