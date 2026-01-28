import React, { useState } from 'react';
import { Tag } from '../types';
import { useTagSelector } from '../hooks/useTagSelector';

interface TagSelectorProps {
  tags: Tag[];
  selectedTagIds: number[];
  onChange: (tagIds: number[]) => void;
  multiple?: boolean;
  variant?: 'chips' | 'dropdown';
}

export const TagSelector: React.FC<TagSelectorProps> = ({
  tags,
  selectedTagIds,
  onChange,
  multiple = true,
  variant = 'chips',
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useTagSelector(isOpen, () => setIsOpen(false));

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

  const getDisplayText = () => {
    if (selectedTagIds.length === 0) {
      return 'Select tags';
    }
    if (selectedTagIds.length === 1) {
      const tag = tags.find(t => t.id === selectedTagIds[0]);
      return tag ? tag.name : '1 tag selected';
    }
    return `${selectedTagIds.length} tags selected`;
  };

  if (variant === 'dropdown') {
    return (
      <div className="relative" ref={dropdownRef} style={{ zIndex: 1000 }}>
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Tags
        </label>
        <button
          type="button"
          onClick={() => setIsOpen(!isOpen)}
          disabled={tags.length === 0}
          className="w-full premium-input text-left flex items-center justify-between disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <span className={selectedTagIds.length > 0 ? 'text-gray-900 dark:text-gray-100' : 'text-gray-500 dark:text-gray-400'}>
            {getDisplayText()}
          </span>
          <svg
            className={`w-5 h-5 text-gray-400 transition-transform ${isOpen ? 'transform rotate-180' : ''}`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
          </svg>
        </button>
        {isOpen && tags.length > 0 && (
          <div className="absolute w-full mt-1 glass-card border-2 border-gray-300 dark:border-gray-600 rounded-lg shadow-lg max-h-60 overflow-auto z-[1001]">
            {tags.map((tag) => {
              const isSelected = selectedTagIds.includes(tag.id);
              return (
                <label
                  key={tag.id}
                  className="flex items-center gap-2 px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer"
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    handleTagToggle(tag.id);
                  }}
                >
                  <input
                    type="checkbox"
                    checked={isSelected}
                    onChange={(e) => {
                      e.preventDefault();
                      e.stopPropagation();
                      handleTagToggle(tag.id);
                    }}
                    onClick={(e) => {
                      e.preventDefault();
                      e.stopPropagation();
                      handleTagToggle(tag.id);
                    }}
                    className="w-4 h-4 text-gray-600 dark:text-primary-600 rounded focus:ring-gray-500 dark:focus:ring-primary-500 cursor-pointer"
                  />
                  <div className="flex items-center gap-2 flex-1 pointer-events-none">
                    {tag.color && (
                      <span
                        className="w-3 h-3 rounded-full flex-shrink-0"
                        style={{ backgroundColor: tag.color }}
                      />
                    )}
                    <span className="text-sm text-gray-700 dark:text-gray-300">{tag.name}</span>
                  </div>
                </label>
              );
            })}
          </div>
        )}
      </div>
    );
  }

  return (
    <div className="relative">
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Tags *
      </label>
      
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
                {isSelected && (
                  <svg className="w-4 h-4 text-gray-700 dark:text-primary-300" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={3}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                  </svg>
                )}
                
                {tag.color && (
                  <span
                    className="w-3 h-3 rounded-full"
                    style={{ backgroundColor: tag.color }}
                  />
                )}
                
                <span className={isSelected ? 'text-gray-900 dark:text-primary-100 font-semibold' : 'text-gray-700 dark:text-gray-300'}>
                  {tag.name}
                </span>
              </button>
            );
          })
        )}
      </div>
      
      {selectedTagIds.length > 0 && (
        <div className="mt-2 text-xs text-gray-600 dark:text-gray-400">
          {selectedTagIds.length} tag{selectedTagIds.length > 1 ? 's' : ''} selected
        </div>
      )}
    </div>
  );
};
