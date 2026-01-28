import { useMemo } from 'react';

export const useArrayKey = (arr: number[] | undefined): string => {
  return useMemo(() => {
    const array = arr || [];
    const sorted = [...array].sort((a, b) => a - b);
    return sorted.join(',');
  }, [arr]);
};
