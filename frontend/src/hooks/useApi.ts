import { useState, useEffect, useCallback } from 'react';
import { getApiErrorMessage } from '@/lib/utils';

interface UseApiOptions<T> {
  queryFn: () => Promise<T>;
  enabled?: boolean;
  fallbackError?: string;
}

interface UseApiResult<T> {
  data: T | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

export function useApi<T>({ queryFn, enabled = true, fallbackError = 'Failed to load data' }: UseApiOptions<T>): UseApiResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(enabled);
  const [error, setError] = useState<string | null>(null);
  const [fetchCount, setFetchCount] = useState(0);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await queryFn();
      setData(result);
    } catch (err) {
      setError(getApiErrorMessage(err, fallbackError));
    } finally {
      setIsLoading(false);
    }
  }, [queryFn, fallbackError]);

  useEffect(() => {
    if (!enabled) {
      setIsLoading(false);
      return;
    }
    fetchData();
  }, [enabled, fetchCount, fetchData]);

  const refetch = useCallback(() => {
    setFetchCount((c) => c + 1);
  }, []);

  return { data, isLoading, error, refetch };
}
