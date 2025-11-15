using Consul;

namespace EquipmentControlCenter.CncService;

public class ConsulConfigurationProvider
{
    private readonly IConsulClient _consulClient;
    private readonly string _serviceName;

    public ConsulConfigurationProvider(IConsulClient consulClient, string serviceName)
    {
        _consulClient = consulClient;
        _serviceName = serviceName;
    }

    public async Task<string> GetConfigValueAsync(string key)
    {
        var configKey = $"{_serviceName}/config/{key}";
        var result = await _consulClient.KV.Get(configKey);
        
        if (result.Response == null)
        {
            return string.Empty;
        }

        return System.Text.Encoding.UTF8.GetString(result.Response.Value);
    }

    public async Task<T?> GetConfigValueAsync<T>(string key)
    {
        var value = await GetConfigValueAsync(key);
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        return System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetConfigValueAsync(string key, string value)
    {
        var configKey = $"{_serviceName}/config/{key}";
        var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
        await _consulClient.KV.Put(new KVPair(configKey) { Value = valueBytes });
    }
}