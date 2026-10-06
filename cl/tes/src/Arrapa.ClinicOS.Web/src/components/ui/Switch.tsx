import { cn } from '@/utils/cn';

interface SwitchProps {
  checked: boolean;
  onCheckedChange: (value: boolean) => void;
  disabled?: boolean;
  label?: string;
  id?: string;
  size?: 'sm' | 'md';
}

export function Switch({ checked, onCheckedChange, disabled, label, id, size = 'md' }: SwitchProps) {
  const dimensions =
    size === 'sm'
      ? { track: 'h-5 w-9', thumb: 'h-4 w-4', on: 'translate-x-4', off: 'translate-x-0.5' }
      : { track: 'h-6 w-11', thumb: 'h-5 w-5', on: 'translate-x-5', off: 'translate-x-0.5' };

  return (
    <label htmlFor={id} className="inline-flex items-center gap-3 cursor-pointer select-none">
      <button
        type="button"
        role="switch"
        aria-checked={checked}
        id={id}
        disabled={disabled}
        onClick={() => onCheckedChange(!checked)}
        className={cn(
          'relative inline-flex items-center rounded-full transition-colors duration-200 ease-out-quart',
          dimensions.track,
          checked ? 'bg-brand-600' : 'bg-gray-200 dark:bg-gray-700',
          disabled && 'opacity-50 cursor-not-allowed',
        )}
      >
        <span
          aria-hidden="true"
          className={cn(
            'inline-block transform rounded-full bg-white shadow-theme-sm transition-transform duration-200 ease-out-quart',
            dimensions.thumb,
            checked ? dimensions.on : dimensions.off,
          )}
        />
      </button>
      {label && <span className="text-body text-gray-700 dark:text-gray-200">{label}</span>}
    </label>
  );
}
