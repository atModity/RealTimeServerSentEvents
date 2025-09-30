const source = new EventSource('http://localhost:5225/timeseries');

source.addEventListener('timeSeriesUpdate', e => {
    const { id, timeSeries } = JSON.parse(e.data);

    const li = document.createElement('li');
    li.classList.add('new', 'flex', 'justify-between', 'items-center');

    const timeSpan = document.createElement('span');
    timeSpan.classList.add('text-gray-500', 'text-sm');

    // Derive the latest timestamp across all series (if available)
    const latestPoints = Array.isArray(timeSeries)
        ? timeSeries
            .filter(Array.isArray)
            .map(series => series[series.length - 1])
            .filter(p => p && p.timestamp)
        : [];
    const latestDate = latestPoints.length
        ? latestPoints
            .map(p => new Date(p.timestamp))
            .reduce((max, d) => (d > max ? d : max), new Date(0))
        : null;
    timeSpan.textContent = latestDate ? latestDate.toLocaleTimeString() : '';

    // Show event id
    const idSpan = document.createElement('span');
    idSpan.classList.add('font-medium', 'text-gray-800');
    idSpan.textContent = `id: ${id ?? 'n/a'}`;

    // Show a compact summary of the latest values from each series
    const valuesSpan = document.createElement('span');
    valuesSpan.classList.add('font-bold', 'text-green-600');
    const valuesSummary = latestPoints.map((p, idx) => `s${idx + 1}:${p.value}`).join('  ');
    valuesSpan.textContent = valuesSummary || 'no data';

    // Append all elements to the list item
    li.appendChild(timeSpan);
    li.appendChild(idSpan);
    li.appendChild(valuesSpan);

    const list = document.getElementById('updates');
    list.prepend(li);

    // Remove highlight after a moment
    setTimeout(() => li.classList.remove('new'), 2000);
});

// ... existing code ...
source.onerror = err => console.error('EventSource failed:', err);
source.onmessage = e => console.log('Last event ID now:', source.lastEventId);