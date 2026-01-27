import React, { useState, useRef, useEffect } from 'react';
import { Priority } from '../types';

interface PrioritySelectorProps {
  selectedPriorities: Priority[];
  onChange: (priorities: Priority[]) => void;
  multiple?: boolean;
}

const priorityOptions = [
  { value: Priority.Low, label: 'Low', color: 'bg-green-100 dark:bg-green-900/20 text-green-700 dark:text-green-400 border-green-200 dark:border-green-800' },
  { value: Priority.Medium, label: 'Medium', color: 'bg-yellow-100 dark:bg-yellow-900/20 text-yellow-700 dark:text-yellow-400 border-yellow-200 dark:border-yellow-800' },
  { value: Priority.High, label: 'High', color: 'bg-orange-100 dark:bg-orange-900/20 text-orange-700 dark:text-orange-400 border-orange-200 dark:border-orange-800' },
  { value: Priority.Critical, label: 'Critical', color: 'bg-red-100 dark:bg-red-900/20 text-red-700 dark:text-red-400 border-red-200 dark:border-red-800' },
];

export const PrioritySelector: React.FC<PrioritySelectorProps> = ({
  selectedPriorities,
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

  const handlePriorityChange = (priority: Priority) => {
    console.log('PrioritySelector: handlePriorityChange called', { priority, multiple, currentSelected: selectedPriorities });
    if (multiple) {
      if (selectedPriorities.includes(priority)) {
        const newPriorities = selectedPriorities.filter((p) => p !== priority);
        console.log('PrioritySelector: Removing priority', { priority, newPriorities });
        onChange(newPriorities);
      } else {
        const newPriorities = [...selectedPriorities, priority];
        console.log('PrioritySelector: Adding priority', { priority, newPriorities });
        onChange(newPriorities);
      }
    } else {
      onChange([priority]);
      setIsOpen(false);
    }
  };

  const handleClear = () => {
    onChange([]);
  };

  const selectedOptions = priorityOptions.filter(opt => selectedPriorities.includes(opt.value));
  const displayText = selectedOptions.length > 0
    ? `${selectedOptions.length} priorit${selectedOptions.length > 1 ? 'ies' : 'y'} selected`
    : 'Select priorit' + (multiple ? 'ies' : 'y') + '...';

  return (
    <div className="relative" ref={dropdownRef}>
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Priority *
      </label>
      
      {/* Dropdown Button */}
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="w-full premium-input flex items-center justify-between text-left cursor-pointer"
      >
        <span className={selectedOptions.length > 0 ? 'text-gray-900 dark:text-gray-100' : 'text-gray-500 dark:text-gray-400'}>
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
              disabled={selectedPriorities.length === 0}
              className="w-full px-3 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-transparent"
            >
              Clear All {selectedPriorities.length > 0 && `(${selectedPriorities.length})`}
            </button>
          </div>

          {/* Selectable Items List */}
          <div className="p-2">
            {priorityOptions.map((option) => {
              const isSelected = selectedPriorities.includes(option.value);
              return (
                <div
                  key={option.value}
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    handlePriorityChange(option.value);
                  }}
                  onMouseDown={(e) => e.preventDefault()}
                  className={`flex items-center gap-3 px-3 py-2.5 rounded-lg cursor-pointer transition-all ${
                    isSelected
                      ? 'bg-gray-100 dark:bg-primary-900/20 border-2 border-gray-400 dark:border-primary-400'
                      : 'hover:bg-gray-50 dark:hover:bg-gray-800/50 border-2 border-transparent'
                  }`}
                >
                  {/* Visual Selection Indicator (Checkbox-style for multiple) */}
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
                    <span className={`px-2 py-0.5 rounded-md text-xs font-medium border ${option.color} ${
                      isSelected ? 'ring-2 ring-primary-500 dark:ring-primary-400' : ''
                    }`}>
                      {option.label}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Selected Priorities Display (below dropdown when closed) */}
      {selectedOptions.length > 0 && !isOpen && (
        <div className="mt-2 flex flex-wrap gap-1.5">
          {selectedOptions.map((option) => (
            <span
              key={option.value}
              className={`inline-flex items-center px-2 py-1 rounded-md text-xs font-medium border ${option.color}`}
            >
              {option.label}
              <button
                type="button"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  handlePriorityChange(option.value);
                }}
                className="ml-1.5 hover:text-red-600 dark:hover:text-red-400 transition-colors"
                aria-label={`Remove ${option.label}`}
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
