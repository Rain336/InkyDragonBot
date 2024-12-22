using System.Data;
using System.Net.NetworkInformation;
using Dapper;

namespace InkyBot.Data.TypeHandlers;

public class PhysicalAddressHandler : SqlMapper.TypeHandler<PhysicalAddress>
{
    public override void SetValue(IDbDataParameter parameter, PhysicalAddress? value)
    {
        parameter.Value = value?.GetAddressBytes();
    }

    public override PhysicalAddress? Parse(object? value)
    {
        if (value is null)
        {
            return null;
        }

        return new PhysicalAddress((byte[])value);
    }
}