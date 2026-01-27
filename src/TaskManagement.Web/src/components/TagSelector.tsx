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
    <div className="tag-selector">
      <label className="form-label">Tags</label>
      <div className="tag-options">
        {tags.map((tag) => (
          <label key={tag.id} className="tag-option">
            <input
              type={multiple ? 'checkbox' : 'radio'}
              checked={selectedTagIds.includes(tag.id)}
              onChange={() => handleTagChange(tag.id)}
            />
            <span
              className="tag-badge"
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
