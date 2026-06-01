import { useState } from 'react';
import { toast } from 'sonner';
import { getApiErrorMessage } from '@/lib/utils';

interface UseFormSubmitOptions<T = unknown> {
  successMessage?: string;
  successMessageFn?: (result: T) => string;
  errorMessage?: string;
  onSuccess?: (result: T) => void;
  onError?: (error: unknown) => void;
}

export function useFormSubmit(defaultOptions: UseFormSubmitOptions = {}) {
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async <T>(fn: () => Promise<T>, submitOptions?: UseFormSubmitOptions<T>) => {
    const opts = { ...defaultOptions, ...submitOptions };
    setIsSubmitting(true);
    try {
      const result = await fn();
      const successMsg = opts.successMessageFn
        ? opts.successMessageFn(result)
        : opts.successMessage;
      if (successMsg) {
        toast.success(successMsg);
      }
      opts.onSuccess?.(result);
      return result;
    } catch (error) {
      const fallback = opts.errorMessage || 'Something went wrong. Please try again.';
      const message = getApiErrorMessage(error, fallback);
      toast.error(message);
      opts.onError?.(error);
      throw error;
    } finally {
      setIsSubmitting(false);
    }
  };

  return { isSubmitting, handleSubmit };
}
