import type { ReactNode } from 'react';

import { cn } from '@/utils/cn';

export interface Column<T> {
  key: string;
  header: ReactNode;
  render: (row: T) => ReactNode;
  className?: string;
  align?: 'left' | 'right' | 'center';
  headerClassName?: string;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  rows: T[];
  keyExtractor: (row: T) => string;
  isLoading?: boolean;
  emptyState?: ReactNode;
  onRowClick?: (row: T) => void;
  rowClassName?: (row: T) => string | undefined;
  /** Number of skeleton rows to render when loading */
  skeletonRows?: number;
  /** Rounded container with border. Set false when embedding inside a card. */
  bordered?: boolean;
  /** Accessible label describing what the table lists (e.g., "Appointments this week"). */
  ariaLabel?: string;
  /** Row-click hint used for the row's aria-label / role when interactive. */
  rowActionLabel?: (row: T) => string;
}

export function DataTable<T>({
  columns,
  rows,
  keyExtractor,
  isLoading,
  emptyState,
  onRowClick,
  rowClassName,
  skeletonRows = 6,
  bordered = true,
  ariaLabel,
  rowActionLabel,
}: DataTableProps<T>) {
  const isInteractive = Boolean(onRowClick);

  return (
    <div
      className={cn(
        'w-full overflow-hidden bg-white dark:bg-gray-900',
        bordered && 'rounded-xl border border-gray-200/70 dark:border-gray-800 shadow-theme-xs',
      )}
    >
      <div className="overflow-x-auto u-scrollbar">
        <table
          className="min-w-full text-body"
          role="table"
          aria-label={ariaLabel}
          aria-busy={isLoading || undefined}
          aria-rowcount={isLoading ? undefined : rows.length}
        >
          <thead className="bg-gray-50/70 dark:bg-gray-800/40 sticky top-0 z-[1]">
            <tr className="border-b border-gray-200/70 dark:border-gray-800">
              {columns.map((c) => (
                <th
                  key={c.key}
                  scope="col"
                  className={cn(
                    'px-4 py-3 text-micro uppercase tracking-widest font-semibold text-gray-500 dark:text-gray-400',
                    c.align === 'right' ? 'text-right' : c.align === 'center' ? 'text-center' : 'text-left',
                    c.headerClassName,
                  )}
                >
                  {c.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {isLoading &&
              Array.from({ length: skeletonRows }).map((_, i) => (
                <tr key={`sk-${i}`} aria-hidden="true">
                  {columns.map((c) => (
                    <td key={c.key} className="px-4 py-3.5">
                      <div className="u-skeleton h-3 w-full max-w-[140px]" />
                    </td>
                  ))}
                </tr>
              ))}
            {!isLoading && rows.length === 0 && (
              <tr>
                <td
                  colSpan={columns.length}
                  className="px-4 py-14 text-center text-gray-500 dark:text-gray-400"
                >
                  {emptyState ?? 'Nothing to show yet.'}
                </td>
              </tr>
            )}
            {!isLoading &&
              rows.map((row) => {
                const activate = onRowClick ? () => onRowClick(row) : undefined;
                return (
                  <tr
                    key={keyExtractor(row)}
                    className={cn(
                      'text-gray-800 dark:text-gray-100 transition-colors duration-150',
                      isInteractive &&
                        'hover:bg-gray-50/70 dark:hover:bg-gray-800/50 cursor-pointer focus-within:bg-gray-50/70 dark:focus-within:bg-gray-800/50',
                      rowClassName?.(row),
                    )}
                    onClick={activate}
                    onKeyDown={
                      isInteractive
                        ? (event) => {
                            if (event.key === 'Enter' || event.key === ' ') {
                              event.preventDefault();
                              activate?.();
                            }
                          }
                        : undefined
                    }
                    tabIndex={isInteractive ? 0 : undefined}
                    role={isInteractive ? 'button' : undefined}
                    aria-label={isInteractive ? rowActionLabel?.(row) : undefined}
                  >
                    {columns.map((c) => (
                      <td
                        key={c.key}
                        className={cn(
                          'px-4 py-3.5 align-middle',
                          c.align === 'right' ? 'text-right' : c.align === 'center' ? 'text-center' : 'text-left',
                          c.className,
                        )}
                      >
                        {c.render(row)}
                      </td>
                    ))}
                  </tr>
                );
              })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
