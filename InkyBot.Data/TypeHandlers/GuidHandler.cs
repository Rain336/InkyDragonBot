using System.Data;
using Dapper;

namespace InkyBot.Data.TypeHandlers;

internal sealed class GuidHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value;
    }

    public override Guid Parse(object value)
    {
        return Guid.Parse((string)value);
    }
}