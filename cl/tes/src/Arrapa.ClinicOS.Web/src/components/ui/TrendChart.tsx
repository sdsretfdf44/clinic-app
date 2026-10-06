interface TrendPoint {
  date: string;
  total: number;
  completed: number;
}

interface TrendChartProps {
  points: TrendPoint[];
  height?: number;
  ariaLabel?: string;
  /** Show the y-axis peak value marker */
  showPeak?: boolean;
}

/**
 * Inline SVG dual-line trend chart with soft area fills. Zero external deps.
 * Blue = total scheduled, teal = completed.
 */
export function TrendChart({ points, height = 120, ariaLabel = 'Trend', showPeak = true }: TrendChartProps) {
  if (points.length === 0) {
    return <p className="text-body text-gray-500 dark:text-gray-400">No data yet.</p>;
  }

  const width = 800;
  const paddingX = 4;
  const paddingY = 12;
  const innerW = width - paddingX * 2;
  const innerH = height - paddingY * 2;

  const maxValue = Math.max(1, ...points.map((p) => p.total));
  const xStep = points.length > 1 ? innerW / (points.length - 1) : 0;

  const toXY = (value: number, i: number) => {
    const x = paddingX + i * xStep;
    const y = paddingY + innerH - (value / maxValue) * innerH;
    return { x, y };
  };

  const toPath = (values: number[]) => values.map((v, i) => {
    const { x, y } = toXY(v, i);
    return `${i === 0 ? 'M' : 'L'} ${x.toFixed(1)} ${y.toFixed(1)}`;
  }).join(' ');

  const toAreaPath = (values: number[]) => {
    if (values.length === 0) return '';
    const line = toPath(values);
    const last = toXY(values[values.length - 1]!, values.length - 1);
    const first = toXY(values[0]!, 0);
    return `${line} L ${last.x.toFixed(1)} ${height - paddingY} L ${first.x.toFixed(1)} ${height - paddingY} Z`;
  };

  const totals = points.map((p) => p.total);
  const completed = points.map((p) => p.completed);

  const firstDate = new Date(points[0]!.date).toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  const lastDate = new Date(points[points.length - 1]!.date).toLocaleDateString(undefined, { month: 'short', day: 'numeric' });

  return (
    <div>
      <svg
        role="img"
        aria-label={ariaLabel}
        viewBox={`0 0 ${width} ${height}`}
        className="w-full h-auto"
        preserveAspectRatio="none"
      >
        <defs>
          <linearGradient id="tc-total" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor="currentColor" className="text-brand-500" stopOpacity="0.16" />
            <stop offset="100%" stopColor="currentColor" className="text-brand-500" stopOpacity="0" />
          </linearGradient>
        </defs>
        <path d={toAreaPath(totals)} fill="url(#tc-total)" />
        <path
          d={toPath(totals)}
          fill="none"
          stroke="currentColor"
          strokeWidth={1.75}
          strokeLinecap="round"
          strokeLinejoin="round"
          className="text-brand-500"
        />
        <path
          d={toPath(completed)}
          fill="none"
          stroke="currentColor"
          strokeWidth={1.5}
          strokeDasharray="4 3"
          strokeLinecap="round"
          strokeLinejoin="round"
          className="text-teal-500"
        />
      </svg>
      <div className="flex items-center justify-between text-caption text-gray-500 dark:text-gray-400 mt-2">
        <span>{firstDate}</span>
        <span className="flex items-center gap-3">
          <LegendItem color="bg-brand-500" label="Scheduled" />
          <LegendItem color="bg-teal-500" label="Completed" />
          {showPeak && <span>Peak: <span className="tabular-nums font-medium text-gray-700 dark:text-gray-200">{maxValue}</span></span>}
        </span>
        <span>{lastDate}</span>
      </div>
    </div>
  );
}

function LegendItem({ color, label }: { color: string; label: string }) {
  return (
    <span className="inline-flex items-center gap-1.5">
      <span className={`inline-block h-1.5 w-3 rounded-full ${color}`} />
      {label}
    </span>
  );
}
