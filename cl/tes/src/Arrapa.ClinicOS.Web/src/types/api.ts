export interface ApiError {
  code: string;
  message: string;
  type: string;
  details?: Record<string, string[]>;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message?: string | null;
  error?: ApiError | null;
  correlationId?: string | null;
  timestamp: string;
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
