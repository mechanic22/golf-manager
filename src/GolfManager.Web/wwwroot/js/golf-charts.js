window.initializeHoleTrendChart = (id, labelsJson, datasetsJson) => {
    const ele = document.getElementById(id);

    if (!ele) {
        return;
    }

    const labels = JSON.parse(labelsJson);
    const datasets = JSON.parse(datasetsJson);

    const existingChart = Chart.getChart(ele) || ele._chartInstance;
    if (existingChart) {
        existingChart.destroy();
    }

    const ctx = ele.getContext('2d');
    ele._chartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels,
            datasets: datasets
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {
                        label: (context) => `${context.dataset.label}: ${context.formattedValue}`
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        color: 'rgba(120,120,120,0.12)'
                    },
                    ticks: {
                        maxRotation: 0,
                        autoSkip: true
                    }
                },
                y: {
                    grid: {
                        color: 'rgba(120,120,120,0.12)'
                    },
                    ticks: {
                        callback: (value) => {
                            const v = parseFloat(value.toFixed(2));
                            return v > 0 ? `+${v}` : `${v}`;
                        }
                    }
                }
            }
        }
    });
};

window.initializeScoringProfileRadarChart = (id, labelsJson, datasetsJson) => {
    const ele = document.getElementById(id);

    if (!ele) {
        return;
    }

    const labels = JSON.parse(labelsJson);
    const datasets = JSON.parse(datasetsJson);
    const maxValue = datasets
        .flatMap(dataset => Array.isArray(dataset.data) ? dataset.data : [])
        .reduce((max, value) => Math.max(max, Number(value) || 0), 0);
    const dynamicMax = Math.min(100, Math.max(10, Math.ceil((maxValue + 5) / 5) * 5));

    const existingChart = Chart.getChart(ele) || ele._chartInstance;
    if (existingChart) {
        existingChart.destroy();
    }

    const ctx = ele.getContext('2d');
    ele._chartInstance = new Chart(ctx, {
        type: 'radar',
        data: {
            labels,
            datasets: datasets
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {
                        label: (context) => `${context.dataset.label}: ${context.formattedValue}%`
                    }
                }
            },
            scales: {
                r: {
                    beginAtZero: true,
                    suggestedMax: dynamicMax,
                    grid: {
                        color: 'rgba(120,120,120,0.15)',
                        circular: true
                    },
                    angleLines: {
                        color: 'rgba(120,120,120,0.15)'
                    },
                    pointLabels: {
                        font: {
                            size: 11,
                            weight: '600'
                        }
                    },
                    ticks: {
                        display: false,
                        backdropColor: 'transparent'
                    }
                }
            },
            elements: {
                line: {
                    tension: 0.2
                }
            }
        }
    });
};
