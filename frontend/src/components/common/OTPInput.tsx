import type { FieldError, UseFormRegisterReturn } from 'react-hook-form';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';

interface OTPInputProps {
  label?: string;
  htmlFor?: string;
  value?: string;
  onChange?: (value: string) => void;
  error?: FieldError;
  disabled?: boolean;
  register?: UseFormRegisterReturn;
  className?: string;
}

export function OTPInput({
  label = 'Verification Code',
  htmlFor = 'otpCode',
  value,
  onChange,
  error,
  disabled,
  register,
  className,
}: OTPInputProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const filtered = e.target.value.replace(/\D/g, '');
    onChange?.(filtered);
  };

  return (
    <div className={cn('space-y-2', className)}>
      <Label htmlFor={htmlFor}>{label}</Label>
      <Input
        id={htmlFor}
        type="text"
        inputMode="numeric"
        maxLength={6}
        placeholder="000000"
        value={value}
        onChange={onChange ? handleChange : undefined}
        disabled={disabled}
        className="text-center text-2xl tracking-[0.5em] font-mono"
        {...(register ? register : {})}
      />
      {error && <p className="text-red-500 text-xs">{error.message}</p>}
    </div>
  );
}
