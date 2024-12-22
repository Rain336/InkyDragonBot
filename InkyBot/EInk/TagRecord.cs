using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using InkyBot.Converters;

namespace InkyBot.EInk;

public class TagRecord
{
    [JsonPropertyName("mac")]
    [JsonConverter(typeof(PhysicalAddressJsonConverter))]
    public PhysicalAddress? Address { get; set; }

    [JsonPropertyName("name")]
    public string? Hash { get; set; }
    
    [JsonPropertyName("lastseen")]
    public uint LastSeen { get; set; }
    
    [JsonPropertyName("nextupdate")]
    public uint NextUpdate { get; set; }
    
    [JsonPropertyName("nextcheckin")]
    public uint NextCheckIn { get; set; }
    
    [JsonPropertyName("pending")]
    public ushort Pending { get; set; }
    
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
    
    [JsonPropertyName("contentMode")]
    public byte ContentMode { get; set; }
    
    [JsonPropertyName("LQI")]
    public byte LQI { get; set; }
    
    [JsonPropertyName("RSSI")]
    public sbyte RSSI { get; set; }
    
    [JsonPropertyName("temperature")]
    public sbyte Temperature { get; set; }
    
    [JsonPropertyName("batteryMv")]
    public ushort BatteryMv { get; set; }
    
    [JsonPropertyName("hwType")]
    public byte HardwareType { get; set; }
    
    [JsonPropertyName("wakeupReason")]
    public byte WakeupReason { get; set; }
    
    [JsonPropertyName("capabilities")]
    public byte Capabilities { get; set; }
    
    [JsonPropertyName("modecfgjson")]
    public string? ModeConfigJson { get; set; }
    
    [JsonPropertyName("isexternal")]
    public bool IsExternal { get; set; }
    
    [JsonPropertyName("apip")]
    public string? AccessPointIp { get; set; }
    
    [JsonPropertyName("rotate")]
    public byte Rotate { get; set; }
    
    [JsonPropertyName("lut")]
    public byte Lut { get; set; }
    
    [JsonPropertyName("invert")]
    public byte Invert { get; set; }
    
    [JsonPropertyName("updatecount")]
    public uint UpdateCount { get; set; }
    
    [JsonPropertyName("updatelast")]
    public uint UpdateLast { get; set; }
    
    [JsonPropertyName("ch")]
    public byte Channel { get; set; }
    
    [JsonPropertyName("ver")]
    public ushort Version { get; set; }
}