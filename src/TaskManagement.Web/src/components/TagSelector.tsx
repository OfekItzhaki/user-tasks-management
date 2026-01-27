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
  const handleTagChange = (tagId: number) => {
    if (multiple) {
      if (selectedTagIds.includes(tagId)) {
        onChange(selectedTagIds.filter((id) => id !== tagId));
      } else {
        onChange([...selectedTagIds, tagId]);
      }
    } else {
      onChange([tagId]);
    }
  };

  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Tags</label>
      <div className="flex flex-wrap gap-2">
        {tags.map((tag) => (
          <label key={tag.id} className="flex items-center gap-2 cursor-pointer">
            <input
              type={multiple ? 'checkbox' : 'radio'}
              checked={selectedTagIds.includes(tag.id)}
              onChange={() => handleTagChange(tag.id)}
              className="w-4 h-4 text-primary-600 rounded focus:ring-primary-500"
            />
            <span
              className="px-3 py-1 rounded-lg text-sm text-white font-medium"
              style={{ backgroundColor: tag.color || '#6c757d' }}
            >
              {tag.name}
            </span>
          </label>
        ))}
      </div>
    </div>
  );
};
