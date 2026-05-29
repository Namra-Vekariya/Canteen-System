import type { FieldError, UseFormRegisterReturn } from 'react-hook-form';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';

interface InputFieldProps {
  label: string;
  htmlFor?: string;
  type?: string;
  placeholder?: string;
  error?: FieldError;
  register: UseFormRegisterReturn;
  disabled?: boolean;
  readOnly?: boolean;
  inputClassName?: string;
}

export function InputField({
  label,
  htmlFor,
  type = 'text',
  placeholder,
  error,
  register,
  disabled,
  readOnly,
  inputClassName,
}: InputFieldProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor={htmlFor}>{label}</Label>
      <Input
        id={htmlFor}
        type={type}
        placeholder={placeholder}
        disabled={disabled}
        readOnly={readOnly}
        className={cn(readOnly && 'bg-gray-100 text-gray-500 cursor-not-allowed', inputClassName)}
        {...register}
      />
      {error && <p className="text-red-500 text-xs">{error.message}</p>}
    </div>
  );
}
