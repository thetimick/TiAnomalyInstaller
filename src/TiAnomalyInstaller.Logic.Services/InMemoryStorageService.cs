// ⠀
// InMemoryStorageService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 10.01.2026.
// ⠀

using System.Collections.Concurrent;

namespace TiAnomalyInstaller.Logic.Services;

public enum InMemoryStorageKey
{
    ConfigError
}

public interface IInMemoryStorageService
{
    void SetValue<T>(InMemoryStorageKey key, T? value);
    T? GetValue<T>(InMemoryStorageKey key);
}

public class InMemoryStorageService: IInMemoryStorageService
{
    private readonly ConcurrentDictionary<string, object?> _storage = new();
    
    public void SetValue<T>(InMemoryStorageKey key, T? value)
    {
        _storage.AddOrUpdate(key.ToString(), value, (_, _) => value);
    }
    
    public T? GetValue<T>(InMemoryStorageKey key)
    {
        if (!_storage.TryGetValue(key.ToString(), out var value)) 
            return default;
        
        if (value is T result)
            return result;
        
        throw new InvalidCastException($"Invalid type for key '{key}'. Expected '{typeof(T).Name}', but got '{value?.GetType().Name}'.");
    }
}