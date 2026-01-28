import React from 'react';
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
  const handlePriorityToggle = (priority: Priority) => {
    if (multiple) {
      if (selectedPriorities.includes(priority)) {
        const newPriorities = selectedPriorities.filter((p) => p !== priority);
        onChange(newPriorities);
      } else {
        const newPriorities = [...selectedPriorities, priority];
        onChange(newPriorities);
      }
    } else {
      onChange([priority]);
    }
  };

  return (
    <div className="relative">
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Priority *
      </label>
      
      {/* Chip-based selection - all priorities visible as clickable chips */}
      <div className="flex flex-wrap gap-2">
        {priorityOptions.map((option) => {
          const isSelected = selectedPriorities.includes(option.value);
          return (
            <button
              key={option.value}
              type="button"
              onClick={() => handlePriorityToggle(option.value)}
              className={`
                inline-flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm font-medium border-2
                transition-all duration-200 transform
                ${option.color}
                ${isSelected
                  ? 'ring-4 ring-offset-1 ring-blue-500 dark:ring-blue-400 shadow-xl scale-110 font-bold border-blue-600 dark:border-blue-500'
                  : 'hover:scale-105 hover:shadow-md border-transparent'
                }
              `}
            >
              {/* Checkmark icon for selected */}
              {isSelected && (
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={3}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                </svg>
              )}
              
              {/* Priority label */}
              <span className={isSelected ? 'font-semibold' : ''}>
                {option.label}
              </span>
            </button>
          );
        })}
      </div>
      
      {/* Selected count display */}
      {selectedPriorities.length > 0 && (
        <div className="mt-2 text-xs text-gray-600 dark:text-gray-400">
          {selectedPriorities.length} priorit{selectedPriorities.length > 1 ? 'ies' : 'y'} selected
        </div>
      )}
    </div>
  );
};
