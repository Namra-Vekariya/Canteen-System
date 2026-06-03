import { useState, useEffect, useCallback, useRef } from 'react';
import { getApiErrorMessage } from '@/lib/utils';

interface UseApiOptions<T> {
  /** The async function that fetches your data. May be an inline arrow function — the hook handles it safely. */
  queryFn: () => Promise<T>;
  /** When false, the hook will not fetch (useful when a required id/param is not yet available). Defaults to true. */
  enabled?: boolean;
  /** Fallback error message if the API doesn't provide one. */
  fallbackError?: string;
}

interface UseApiResult<T> {
  data: T | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

/**
 * Generic GET-style data-fetching hook.
 *
 * Returns { data, isLoading, error, refetch }. Safe to call with an inline
 * queryFn — internal ref pattern avoids the classic "new function ref on
 * every render" infinite-loop footgun.
 */
export function useApi<T>({
  queryFn,
  enabled = true,
  fallbackError = 'Failed to load data',
}: UseApiOptions<T>): UseApiResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fetchCount, setFetchCount] = useState(0);

  const queryFnRef = useRef(queryFn);
  useEffect(() => {
    queryFnRef.current = queryFn;
  }, [queryFn]);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await queryFnRef.current();
      setData(result);
    } catch (err) {
      setError(getApiErrorMessage(err, fallbackError));
    } finally {
      setIsLoading(false);
    }
  }, [fallbackError]);

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
