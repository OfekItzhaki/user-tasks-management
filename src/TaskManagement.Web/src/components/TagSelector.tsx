import React from 'react';
import { Tag } from '../types';

interface TagSelectorProps {
  tags: Tag[];
  selectedTagIds: number[];
  onChange: (tagIds: number[]) => void;
  multiple?: boolean;
}

export const TagSelector: React.FC<TagSelectorProps> = ({
  tags,
  selectedTagIds,
  onChange,
  multiple = true,
}) => {
  const handleTagToggle = (tagId: number) => {
    if (multiple) {
      if (selectedTagIds.includes(tagId)) {
        const newIds = selectedTagIds.filter((id) => id !== tagId);
        onChange(newIds);
      } else {
        const newIds = [...selectedTagIds, tagId];
        onChange(newIds);
      }
    } else {
      onChange([tagId]);
    }
  };

  return (
    <div className="relative">
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Tags *
      </label>
      
      {/* Chip-based selection - all tags visible as clickable chips */}
      <div className="flex flex-wrap gap-2">
        {tags.length === 0 ? (
          <div className="text-sm text-gray-500 dark:text-gray-400">
            No tags available
          </div>
        ) : (
          tags.map((tag) => {
            const isSelected = selectedTagIds.includes(tag.id);
            return (
              <button
                key={tag.id}
                type="button"
                onClick={() => handleTagToggle(tag.id)}
                className={`
                  inline-flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm font-medium
                  transition-all duration-200 transform
                  ${isSelected
                    ? 'bg-blue-200 dark:bg-blue-900/40 border-2 border-blue-500 dark:border-blue-400 ring-2 ring-blue-300 dark:ring-blue-500/50 scale-105 shadow-lg font-semibold'
                    : 'bg-gray-100 dark:bg-gray-800 border-2 border-gray-300 dark:border-gray-600 hover:bg-gray-200 dark:hover:bg-gray-700 hover:scale-102'
                  }
                `}
              >
                {/* Checkmark icon for selected */}
                {isSelected && (
                  <svg className="w-4 h-4 text-gray-700 dark:text-primary-300" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={3}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                  </svg>
                )}
                
                {/* Tag color dot */}
                {tag.color && (
                  <span
                    className="w-3 h-3 rounded-full"
                    style={{ backgroundColor: tag.color }}
                  />
                )}
                
                {/* Tag name */}
                <span className={isSelected ? 'text-gray-900 dark:text-primary-100 font-semibold' : 'text-gray-700 dark:text-gray-300'}>
                  {tag.name}
                </span>
              </button>
            );
          })
        )}
      </div>
      
      {/* Selected count display */}
      {selectedTagIds.length > 0 && (
        <div className="mt-2 text-xs text-gray-600 dark:text-gray-400">
          {selectedTagIds.length} tag{selectedTagIds.length > 1 ? 's' : ''} selected
        </div>
      )}
    </div>
  );
};
