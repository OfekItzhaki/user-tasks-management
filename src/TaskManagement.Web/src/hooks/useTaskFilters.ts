import { useMemo } from 'react';
import { Priority } from '../types';

export const useTaskFilters = (priorities?: number[], tagIds?: number[]) => {
  const selectedPriorities = useMemo(() => {
    if (priorities && priorities.length > 0) {
      return priorities.map(p => p as Priority);
    }
    return [];
  }, [priorities]);

  const selectedTagIds = useMemo(() => {
    if (tagIds && tagIds.length > 0) {
      return tagIds;
    }
    return [];
  }, [tagIds]);

  const hasActiveFilters = useMemo(() => {
    return (priorities && priorities.length > 0) || (tagIds && tagIds.length > 0);
  }, [priorities, tagIds]);

  return { selectedPriorities, selectedTagIds, hasActiveFilters };
};
