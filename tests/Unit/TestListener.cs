using System.Diagnostics.Metrics;

namespace Unit;

internal sealed class TestListener : IDisposable
{
    private readonly MeterListener _listener = new();
    private readonly Dictionary<string, long> _counters = [];
    public IReadOnlyDictionary<string, long> Counters => _counters;

    public TestListener(params string[] instrumentNames)
    {
        _listener.InstrumentPublished = (inst, listener) =>
        {
            if (instrumentNames.Contains(inst.Name))
            {
                listener.EnableMeasurementEvents(inst);
            }
        };
        _listener.SetMeasurementEventCallback<long>((inst, measurement, tags, state) =>
        {
            if (_counters.TryGetValue(inst.Name, out var cur))
            {
                _counters[inst.Name] = cur + measurement;
            }
            else
            {
                _counters[inst.Name] = measurement;
            }
        });
        _listener.Start();
    }

    public void Dispose() => _listener.Dispose();

    public void RecordObservableInstruments() => _listener.RecordObservableInstruments();
}
