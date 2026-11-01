using System.Text.Json;
using DaycareAPI2026.Models;

namespace DaycareAPI2026.Data;

// Local demonstration storage. Not suitable for production or real child records.
public sealed class DataStore
{
    private readonly object _gate = new();
    private readonly string _path;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private DaycareDatabase _db;

    public DataStore(IWebHostEnvironment environment)
    {
        string folder = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, "daycare.json");
        _db = File.Exists(_path)
            ? JsonSerializer.Deserialize<DaycareDatabase>(File.ReadAllText(_path), _json) ?? new()
            : new();
    }

    public T Read<T>(Func<DaycareDatabase, T> action)
    {
        lock (_gate) return action(_db);
    }

    public T Write<T>(Func<DaycareDatabase, T> action)
    {
        lock (_gate)
        {
            T result = action(_db);
            string temp = _path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(_db, _json));
            File.Move(temp, _path, true);
            return result;
        }
    }
}
