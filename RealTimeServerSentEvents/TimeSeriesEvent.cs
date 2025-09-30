using Modity.TimeSeries;

namespace RealTimeServerSentEvents;

public record TimeSeriesEvent(string Id, IReadOnlyCollection<TimeSeries<decimal>> TimeSeries);