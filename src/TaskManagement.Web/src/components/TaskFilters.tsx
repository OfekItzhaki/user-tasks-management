import React from 'react';
import { Tag, User } from '../types';
import { PrioritySelector } from './PrioritySelector';
import { TagSelector } from './TagSelector';
import { useTaskFilters } from '../hooks/useTaskFilters';

interface TaskFiltersProps {
  searchTerm: string;
  priorities: number[] | undefined;
  userId: number | undefined;
  tagIds: number[] | undefined;
  sortBy: string;
  sortOrder: 'asc' | 'desc';
  users: User[];
  tags: Tag[];
  onSearchChange: (value: string) => void;
  onPrioritiesChange: (value: number[] | undefined) => void;
  onUserIdChange: (value: number | undefined) => void;
  onTagIdsChange: (value: number[] | undefined) => void;
  onSortByChange: (value: string) => void;
  onSortOrderChange: (value: 'asc' | 'desc') => void;
  onClearFilters: () => void;
}

export const TaskFilters: React.FC<TaskFiltersProps> = ({
  searchTerm,
  priorities,
  userId,
  tagIds,
  sortBy,
  sortOrder,
  users,
  tags,
  onSearchChange,
  onPrioritiesChange,
  onUserIdChange,
  onTagIdsChange,
  onSortByChange,
  onSortOrderChange,
  onClearFilters,
}) => {
  const { selectedPriorities, selectedTagIds } = useTaskFilters(priorities, tagIds);
  const hasActiveFilters = searchTerm || (priorities && priorities.length > 0) || userId || (tagIds && tagIds.length > 0);

  return (
    <div className="glass-card p-4 mb-6 space-y-4" style={{ position: 'relative', overflow: 'visible', zIndex: 100, isolation: 'isolate' }}>
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Filters & Search</h3>
        {hasActiveFilters && (
          <button
            onClick={onClearFilters}
            className="text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 underline"
          >
            Clear all
          </button>
        )}
      </div>

      <div>
        <label htmlFor="search" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Search
        </label>
        <div className="relative">
          <input
            id="search"
            type="text"
            value={searchTerm}
            onChange={(e) => onSearchChange(e.target.value)}
            placeholder="Search by title or description..."
            className="w-full premium-input pl-10"
          />
          <svg
            className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400 dark:text-gray-500"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <PrioritySelector
            selectedPriorities={selectedPriorities}
            onChange={(selected) => {
              const prioritiesAsNumbers = selected.length > 0 ? selected.map(p => p as number) : undefined;
              onPrioritiesChange(prioritiesAsNumbers);
            }}
            multiple={true}
          />
        </div>

        <div>
          <label htmlFor="user-filter" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Assigned User
          </label>
          <select
            id="user-filter"
            value={userId || ''}
            onChange={(e) => onUserIdChange(e.target.value ? Number(e.target.value) : undefined)}
            className="w-full premium-input border-2 border-gray-300 dark:border-gray-600"
          >
            <option value="">All Users</option>
            {users.map((user) => (
              <option key={user.id} value={user.id}>
                {user.fullName}
              </option>
            ))}
          </select>
        </div>

        <div>
          <TagSelector
            tags={tags}
            selectedTagIds={selectedTagIds}
            onChange={(selected) => {
              const valueToSet = selected.length > 0 ? selected : undefined;
              onTagIdsChange(valueToSet);
            }}
            multiple={true}
            variant="dropdown"
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-2 border-t border-gray-200 dark:border-gray-700">
        <div>
          <label htmlFor="sort-by" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Sort By
          </label>
          <select
            id="sort-by"
            value={sortBy}
            onChange={(e) => onSortByChange(e.target.value)}
            className="w-full premium-input border-2 border-gray-300 dark:border-gray-600"
          >
            <option value="createdAt">Created Date</option>
            <option value="title">Title</option>
            <option value="dueDate">Due Date</option>
            <option value="priority">Priority</option>
          </select>
        </div>

        <div>
          <label htmlFor="sort-order" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Order
          </label>
          <select
            id="sort-order"
            value={sortOrder}
            onChange={(e) => onSortOrderChange(e.target.value as 'asc' | 'desc')}
            className="w-full premium-input border-2 border-gray-300 dark:border-gray-600"
          >
            <option value="desc">Descending</option>
            <option value="asc">Ascending</option>
          </select>
        </div>
      </div>
    </div>
  );
};
