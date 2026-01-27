import React, { useState, useRef, useEffect } from 'react';
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
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      return () => {
        document.removeEventListener('mousedown', handleClickOutside);
      };
    }
  }, [isOpen]);

  const handleTagChange = (tagId: number) => {
    console.log('TagSelector: handleTagChange called', { tagId, multiple, currentSelected: selectedTagIds });
    if (multiple) {
      if (selectedTagIds.includes(tagId)) {
        const newIds = selectedTagIds.filter((id) => id !== tagId);
        console.log('TagSelector: Removing tag', { tagId, newIds });
        onChange(newIds);
      } else {
        const newIds = [...selectedTagIds, tagId];
        console.log('TagSelector: Adding tag', { tagId, newIds });
        onChange(newIds);
      }
    } else {
      onChange([tagId]);
      setIsOpen(false);
    }
  };

  const handleClear = () => {
    onChange([]);
  };

  const selectedTags = tags.filter(tag => selectedTagIds.includes(tag.id));
  const displayText = selectedTags.length > 0
    ? `${selectedTags.length} tag${selectedTags.length > 1 ? 's' : ''} selected`
    : 'Select tags...';

  return (
    <div className="relative" ref={dropdownRef}>
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Tags *
      </label>
      
      {/* Dropdown Button */}
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="w-full premium-input flex items-center justify-between text-left cursor-pointer"
      >
        <span className={selectedTags.length > 0 ? 'text-gray-900 dark:text-gray-100' : 'text-gray-500 dark:text-gray-400'}>
          {displayText}
        </span>
        <svg
          className={`w-5 h-5 text-gray-400 dark:text-gray-500 transition-transform ${isOpen ? 'rotate-180' : ''}`}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="absolute z-[9999] w-full mt-2 glass-card rounded-lg shadow-lg border border-white/20 dark:border-gray-700/30 max-h-64 overflow-y-auto">
          {/* Clear Button - Always visible inside dropdown */}
          <div className="p-2 border-b border-gray-200 dark:border-gray-700 sticky top-0 bg-white dark:bg-gray-900/70 backdrop-blur-sm">
            <button
              type="button"
              onClick={handleClear}
              disabled={selectedTagIds.length === 0}
              className="w-full px-3 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-transparent"
            >
              Clear All {selectedTagIds.length > 0 && `(${selectedTagIds.length})`}
            </button>
          </div>

          {/* Selectable Items List */}
          <div className="p-2">
            {tags.length === 0 ? (
              <div className="px-3 py-2 text-sm text-gray-500 dark:text-gray-400 text-center">
                No tags available
              </div>
            ) : (
              tags.map((tag) => {
                const isSelected = selectedTagIds.includes(tag.id);
                return (
                  <div
                    key={tag.id}
                    onClick={(e) => {
                      e.preventDefault();
                      e.stopPropagation();
                      handleTagChange(tag.id);
                    }}
                    onMouseDown={(e) => e.preventDefault()}
                    className={`flex items-center gap-3 px-3 py-2.5 rounded-lg cursor-pointer transition-all ${
                      isSelected
                        ? 'bg-gray-100 dark:bg-primary-900/20 border-2 border-gray-400 dark:border-primary-400'
                        : 'hover:bg-gray-50 dark:hover:bg-gray-800/50 border-2 border-transparent'
                    }`}
                  >
                    {/* Visual Selection Indicator */}
                    <div className={`flex-shrink-0 w-5 h-5 rounded border-2 flex items-center justify-center transition-all ${
                      isSelected
                        ? 'bg-gray-600 dark:bg-primary-500 border-gray-600 dark:border-primary-500'
                        : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800'
                    }`}>
                      {isSelected && (
                        <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                        </svg>
                      )}
                    </div>
                    <div className="flex items-center gap-2 flex-1">
                      {tag.color && (
                        <span
                          className="w-3 h-3 rounded-full"
                          style={{ backgroundColor: tag.color }}
                        />
                      )}
                      <span className={`text-sm font-medium ${
                        isSelected
                          ? 'text-gray-900 dark:text-primary-100'
                          : 'text-gray-700 dark:text-gray-300'
                      }`}>
                        {tag.name}
                      </span>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      )}

      {/* Selected Tags Display (below dropdown when closed) */}
      {selectedTags.length > 0 && !isOpen && (
        <div className="mt-2 flex flex-wrap gap-1.5">
          {selectedTags.map((tag) => (
            <span
              key={tag.id}
              className="inline-flex items-center gap-1.5 px-2 py-1 rounded-md text-xs font-medium bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700"
              style={tag.color ? {
                backgroundColor: `${tag.color}15`,
                color: tag.color,
                borderColor: `${tag.color}40`
              } : {}}
            >
              {tag.name}
              <button
                type="button"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  handleTagChange(tag.id);
                }}
                className="ml-1 hover:text-red-600 dark:hover:text-red-400 transition-colors"
                aria-label={`Remove ${tag.name}`}
              >
                <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </span>
          ))}
        </div>
      )}
    </div>
  );
};
