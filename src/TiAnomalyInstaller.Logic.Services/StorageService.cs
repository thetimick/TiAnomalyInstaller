// ⠀
// StorageService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 17.01.2026.
// ⠀

using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Ardalis.SmartEnum;

namespace TiAnomalyInstaller.Logic.Services;

/// <summary>
/// Доступные ключи для Storage
/// </summary>
public class StorageServiceKey: SmartEnum<StorageServiceKey>
{
    public static readonly StorageServiceKey ProfileUrl = new(nameof(ProfileUrl), 0);
    public static readonly StorageServiceKey Version = new(nameof(Version), 1);

    // ─────────────── Lifecycle ───────────────

    private StorageServiceKey(string name, int value) : base(name, value) { }
}

/// <summary>
/// Интерфейс для работы с хранилищем данных.
/// Предоставляет методы для установки, получения, удаления и управления данными в хранилище.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Устанавливает значение для указанного ключа.
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <param name="key">Ключ, по которому будет сохранено значение.</param>
    /// <param name="value">Значение для сохранения. Если значение null, ключ будет удалён из хранилища.</param>
    void Set<T>(StorageServiceKey key, T? value);

    /// <summary>
    /// Получает значение из хранилища по указанному ключу.
    /// </summary>
    /// <typeparam name="T">Ожидаемый тип значения.</typeparam>
    /// <param name="key">Ключ, по которому будет получено значение.</param>
    /// <param name="defaultValue">Значение по умолчанию, возвращаемое, если ключ отсутствует.</param>
    /// <returns>Значение, связанное с ключом, или значение по умолчанию, если ключ отсутствует.</returns>
    T? Get<T>(StorageServiceKey key, T? defaultValue = default);

    /// <summary>
    /// Получает строковое значение из хранилища по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому будет получено значение.</param>
    /// <param name="defaultValue">Значение по умолчанию, возвращаемое, если ключ отсутствует.</param>
    /// <returns>Строковое значение, связанное с ключом, или значение по умолчанию, если ключ отсутствует.</returns>
    string? GetString(StorageServiceKey key, string? defaultValue = null);

    /// <summary>
    /// Получает целочисленное значение из хранилища по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому будет получено значение.</param>
    /// <param name="defaultValue">Значение по умолчанию, возвращаемое, если ключ отсутствует.</param>
    /// <returns>Целочисленное значение, связанное с ключом, или значение по умолчанию, если ключ отсутствует.</returns>
    int GetInt(StorageServiceKey key, int defaultValue = 0);

    /// <summary>
    /// Получает значение типа long из хранилища по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому будет получено значение.</param>
    /// <param name="defaultValue">Значение по умолчанию, возвращаемое, если ключ отсутствует.</param>
    /// <returns>Значение типа long, связанное с ключом, или значение по умолчанию, если ключ отсутствует.</returns>
    long GetLong(StorageServiceKey key, long defaultValue = 0);

    /// <summary>
    /// Получает значение типа double из хранилища по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому будет получено значение.</param>
    /// <param name="defaultValue">Значение по умолчанию, возвращаемое, если ключ отсутствует.</param>
    /// <returns>Значение типа double, связанное с ключом, или значение по умолчанию, если ключ отсутствует.</returns>
    double GetDouble(StorageServiceKey key, double defaultValue = 0);

    /// <summary>
    /// Получает булево значение из хранилища по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому будет получено значение.</param>
    /// <param name="defaultValue">Значение по умолчанию, возвращаемое, если ключ отсутствует.</param>
    /// <returns>Булево значение, связанное с ключом, или значение по умолчанию, если ключ отсутствует.</returns>
    bool GetBool(StorageServiceKey key, bool defaultValue = false);

    /// <summary>
    /// Получает значение типа DateTime из хранилища по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, по которому будет получено значение.</param>
    /// <param name="defaultValue">Значение по умолчанию, возвращаемое, если ключ отсутствует.</param>
    /// <returns>Значение типа DateTime, связанное с ключом, или значение по умолчанию, если ключ отсутствует.</returns>
    DateTime GetDateTime(StorageServiceKey key, DateTime defaultValue = default);

    /// <summary>
    /// Проверяет, существует ли значение для указанного ключа в хранилище.
    /// </summary>
    /// <param name="key">Ключ для проверки.</param>
    /// <returns>True, если значение существует, иначе False.</returns>
    bool Contains(StorageServiceKey key);

    /// <summary>
    /// Удаляет значение из хранилища по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ, значение которого нужно удалить.</param>
    void Remove(StorageServiceKey key);

    /// <summary>
    /// Очищает все данные из хранилища.
    /// </summary>
    void Clear();

    /// <summary>
    /// Сохраняет текущее состояние хранилища в файл.
    /// </summary>
    void Save();

    /// <summary>
    /// Перезагружает данные из файла в хранилище.
    /// </summary>
    void Reload();
}

public partial class StorageService: IDisposable
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Lock _lock = new();
    private readonly ConcurrentDictionary<string, object?> _cache = new();
    private bool _isDirty;
    private readonly ILogger _logger;

    public StorageService(string folderName, ILogger<StorageService> logger)
    {
        _filePath = Path.Combine(folderName, "storage.json");
        _jsonOptions = new JsonSerializerOptions {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        _logger = logger;
        
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        Load();
    }
    
    public void Dispose()
    {
        SaveIfDirty();
        GC.SuppressFinalize(this);
    }
}

// ────────────────────────────────────────────────
// IStorageService
// ────────────────────────────────────────────────

public partial class StorageService: IStorageService
{
    public void Set<T>(StorageServiceKey key, T? value)
    {
        lock (_lock)
        {
            if (value == null)
                _cache.TryRemove(key.Name, out _);
            else
                _cache[key.Name] = value;
            _isDirty = true;
        }
    }

    public T? Get<T>(StorageServiceKey key, T? defaultValue = default)
    {
        lock (_lock)
        {
            if (!_cache.TryGetValue(key.Name, out var rawValue))
                return defaultValue;
            try
            {
                return rawValue switch {
                    JsonElement je => je.Deserialize<T>(_jsonOptions),
                    T val => val,
                    _ => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(rawValue, _jsonOptions), _jsonOptions)
                };
            }
            catch
            {
                return defaultValue;
            }
        }
    }
    
    public string? GetString(StorageServiceKey key, string? defaultValue = null)
        => Get(key, defaultValue);
    public int GetInt(StorageServiceKey key, int defaultValue = 0)
        => Get(key, defaultValue);
    public long GetLong(StorageServiceKey key, long defaultValue = 0)
        => Get(key, defaultValue);
    public double GetDouble(StorageServiceKey key, double defaultValue = 0)
        => Get(key, defaultValue);
    public bool GetBool(StorageServiceKey key, bool defaultValue = false)
        => Get(key, defaultValue);
    public DateTime GetDateTime(StorageServiceKey key, DateTime defaultValue = default)
        => Get(key, defaultValue);

    public bool Contains(StorageServiceKey key) => _cache.ContainsKey(key.Name);

    public void Remove(StorageServiceKey key)
    {
        lock (_lock)
        {
            if (_cache.TryRemove(key.Name, out _))
                _isDirty = true;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
            _isDirty = true;
        }
    }
    
    public void Reload() => Load();
    public void Save() => SaveIfDirty();
}

// ────────────────────────────────────────────────
// Private Methods
// ────────────────────────────────────────────────

public partial class StorageService
{
    private void Load()
    {
        lock (_lock)
        {
            _cache.Clear();
            if (!File.Exists(_filePath)) 
                return;
            
            try
            {
                var json = File.ReadAllText(_filePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _jsonOptions);
                if (dict == null) 
                    return;
                foreach (var kv in dict)
                    _cache[kv.Key] = kv.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError("Storage load error: {ex}", ex);
            }
        }
    }
    
    private void SaveIfDirty()
    {
        if (!_isDirty) 
            return;

        lock (_lock)
        {
            if (!_isDirty) 
                return;

            try
            {
                var serializable = new Dictionary<string, object?>();

                foreach (var kv in _cache) 
                {
                    serializable[kv.Key] = kv.Value switch {
                        JsonElement je => je,
                        _ => kv.Value
                    };
                }

                var json = JsonSerializer.Serialize(serializable, _jsonOptions);
                File.WriteAllText(_filePath, json);
                _isDirty = false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Storage save error: {ex}", ex);
            }
        }
    }
}