import { Button } from './Button';

interface PaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ pageNumber, pageSize, totalCount, totalPages, onPageChange }: PaginationProps) {
  if (totalCount === 0) {
    return null;
  }
  const start = (pageNumber - 1) * pageSize + 1;
  const end = Math.min(pageNumber * pageSize, totalCount);

  return (
    <div className="flex flex-col-reverse sm:flex-row items-center justify-between gap-3 mt-4">
      <p className="text-caption text-gray-500 dark:text-gray-400">
        Showing <span className="font-medium text-gray-700 dark:text-gray-200">{start.toLocaleString()}</span>
        {'–'}
        <span className="font-medium text-gray-700 dark:text-gray-200">{end.toLocaleString()}</span>
        {' of '}
        <span className="font-medium text-gray-700 dark:text-gray-200">{totalCount.toLocaleString()}</span>
      </p>
      <div className="flex items-center gap-2">
        <Button
          type="button"
          variant="secondary"
          size="sm"
          disabled={pageNumber <= 1}
          onClick={() => onPageChange(pageNumber - 1)}
          leftIcon={
            <svg viewBox="0 0 20 20" fill="currentColor" className="h-3.5 w-3.5">
              <path fillRule="evenodd" d="M12.79 5.23a.75.75 0 010 1.06L8.85 10l3.94 3.71a.75.75 0 11-1.02 1.1l-4.5-4.24a.75.75 0 010-1.1l4.5-4.24a.75.75 0 011.02 0z" clipRule="evenodd" />
            </svg>
          }
        >
          Previous
        </Button>
        <span className="text-caption text-gray-500 dark:text-gray-400 tabular-nums px-1">
          Page <span className="font-medium text-gray-700 dark:text-gray-200">{pageNumber}</span> of {totalPages || 1}
        </span>
        <Button
          type="button"
          variant="secondary"
          size="sm"
          disabled={pageNumber >= totalPages}
          onClick={() => onPageChange(pageNumber + 1)}
          rightIcon={
            <svg viewBox="0 0 20 20" fill="currentColor" className="h-3.5 w-3.5">
              <path fillRule="evenodd" d="M7.21 14.77a.75.75 0 010-1.06L11.15 10 7.21 6.29a.75.75 0 111.02-1.1l4.5 4.24a.75.75 0 010 1.1l-4.5 4.24a.75.75 0 01-1.02 0z" clipRule="evenodd" />
            </svg>
          }
        >
          Next
        </Button>
      </div>
    </div>
  );
}
