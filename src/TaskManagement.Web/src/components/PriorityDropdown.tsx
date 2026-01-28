import React from 'react';
import { Priority } from '../types';

interface PriorityDropdownProps {
  selectedPriority: Priority;
  onChange: (priority: Priority) => void;
  error?: string;
}

const priorityOptions = [
  { value: Priority.Low, label: 'Low' },
  { value: Priority.Medium, label: 'Medium' },
  { value: Priority.High, label: 'High' },
  { value: Priority.Critical, label: 'Critical' },
];

export const PriorityDropdown: React.FC<PriorityDropdownProps> = ({
  selectedPriority,
  onChange,
  error,
}) => {
  return (
    <div>
      <label htmlFor="priority" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Priority *
      </label>
      <select
        id="priority"
        value={selectedPriority}
        onChange={(e) => onChange(Number(e.target.value) as Priority)}
        className={`w-full premium-input border-2 ${error ? 'border-red-500 focus:ring-red-500' : 'border-gray-300 dark:border-gray-600'}`}
      >
        {priorityOptions.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {error && <span className="mt-1 text-sm text-red-600 dark:text-red-400">{error}</span>}
    </div>
  );
};
