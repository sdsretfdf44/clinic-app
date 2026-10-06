import { useCallback, useId, useRef, useState } from 'react';
import type { ChangeEvent, DragEvent } from 'react';

import { cn } from '@/utils/cn';
import { Spinner } from './Spinner';

export interface FileUploadProps {
  onFileSelected: (file: File) => Promise<void> | void;
  accept?: string;
  maxBytes?: number;
  disabled?: boolean;
  label?: string;
  helperText?: string;
  className?: string;
}

/**
 * Drag-and-drop or click-to-select file input. Delegates the actual upload to the parent,
 * which is expected to call the backend and either resolve (success) or throw (failure).
 */
export function FileUpload({
  onFileSelected,
  accept,
  maxBytes,
  disabled,
  label = 'Upload a file',
  helperText,
  className,
}: FileUploadProps) {
  const inputId = useId();
  const inputRef = useRef<HTMLInputElement>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFile = useCallback(async (file: File) => {
    setError(null);

    if (maxBytes && file.size > maxBytes) {
      setError(`File exceeds the ${(maxBytes / (1024 * 1024)).toFixed(1)} MB limit.`);
      return;
    }

    try {
      setIsBusy(true);
      await onFileSelected(file);
    } catch (uploadError) {
      const message = uploadError instanceof Error ? uploadError.message : 'Upload failed.';
      setError(message);
    } finally {
      setIsBusy(false);
      if (inputRef.current) {
        inputRef.current.value = '';
      }
    }
  }, [maxBytes, onFileSelected]);

  const onChange = useCallback((event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      void handleFile(file);
    }
  }, [handleFile]);

  const onDrop = useCallback((event: DragEvent<HTMLLabelElement>) => {
    event.preventDefault();
    setIsDragging(false);
    if (disabled) return;

    const file = event.dataTransfer.files?.[0];
    if (file) {
      void handleFile(file);
    }
  }, [disabled, handleFile]);

  const onDragOver = useCallback((event: DragEvent<HTMLLabelElement>) => {
    event.preventDefault();
    if (!disabled) setIsDragging(true);
  }, [disabled]);

  const onDragLeave = useCallback(() => setIsDragging(false), []);

  return (
    <div className={className}>
      <label
        htmlFor={inputId}
        onDrop={onDrop}
        onDragOver={onDragOver}
        onDragLeave={onDragLeave}
        className={cn(
          'flex flex-col items-center justify-center gap-3 rounded-xl border-2 border-dashed px-6 py-8 cursor-pointer transition-colors',
          'border-gray-300 bg-gray-50 hover:bg-gray-100',
          'dark:border-gray-700 dark:bg-gray-900 dark:hover:bg-gray-800',
          isDragging && 'border-brand-500 bg-brand-50 dark:bg-brand-500/10',
          (disabled || isBusy) && 'opacity-60 cursor-not-allowed',
        )}
      >
        {isBusy ? (
          <Spinner className="h-6 w-6" />
        ) : (
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.5} className="h-8 w-8 text-brand-600" aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 16.5V9.75m0 0l3 3m-3-3l-3 3M6.75 19.5a4.5 4.5 0 01-1.41-8.775 5.25 5.25 0 0110.233-2.33 3 3 0 013.758 3.848A3.752 3.752 0 0118 19.5H6.75z" />
          </svg>
        )}
        <div className="text-center">
          <p className="text-body font-medium text-gray-900 dark:text-gray-100">{label}</p>
          <p className="text-caption text-gray-500 dark:text-gray-400 mt-1">
            {helperText ?? 'Click to browse or drag and drop'}
          </p>
        </div>
        <input
          ref={inputRef}
          id={inputId}
          type="file"
          className="sr-only"
          accept={accept}
          disabled={disabled || isBusy}
          onChange={onChange}
        />
      </label>
      {error && <p className="text-caption text-danger-600 mt-2">{error}</p>}
    </div>
  );
}
