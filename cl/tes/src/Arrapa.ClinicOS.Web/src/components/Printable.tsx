import { useCallback, type ReactNode } from 'react';
import { PrinterIcon } from '@heroicons/react/24/outline';

import { Button } from '@/components/ui/Button';
import { cn } from '@/utils/cn';

interface PrintableProps {
  children: ReactNode;
  className?: string;
  buttonLabel?: string;
}

/**
 * Wraps a section that should be the ONLY thing on the page when printed.
 * Everything outside `.u-printable` is hidden via the print stylesheet.
 * Provides a "Print" button in the top-right which is itself hidden on paper.
 */
export function Printable({ children, className, buttonLabel = 'Print' }: PrintableProps) {
  const onPrint = useCallback(() => window.print(), []);

  return (
    <div className={cn('u-printable relative', className)}>
      <div className="u-no-print flex justify-end mb-4">
        <Button variant="secondary" leftIcon={<PrinterIcon className="h-4 w-4" />} onClick={onPrint}>
          {buttonLabel}
        </Button>
      </div>
      {children}
    </div>
  );
}
