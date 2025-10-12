namespace Domain.Common;

public class TraceId
{
    private static readonly AsyncLocal<string> _currentTraceId = new AsyncLocal<string>();

    public static string Current
    {
        get => _currentTraceId.Value ?? Guid.NewGuid().ToString();
        set => _currentTraceId.Value = value;
    }

    public static void SetTraceId(string traceId)
    {
        if (!string.IsNullOrEmpty(traceId))
        {
            _currentTraceId.Value = traceId;
        }
    }

    public static void Clear()
    {
        _currentTraceId.Value = null;
    }
}