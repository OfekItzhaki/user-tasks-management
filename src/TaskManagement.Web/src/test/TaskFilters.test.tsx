import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { TaskFilters } from '../components/TaskFilters';
import { Tag, User } from '../types';

const mockTags: Tag[] = [
  { id: 1, name: 'Bug Fix', color: '#ff4500' },
  { id: 2, name: 'Feature', color: '#0000ff' },
];

const mockUsers: User[] = [
  { id: 1, fullName: 'John Doe', telephone: '123-456-7890', email: 'john@example.com' },
  { id: 2, fullName: 'Jane Smith', telephone: '098-765-4321', email: 'jane@example.com' },
];

describe('TaskFilters', () => {
  const defaultProps = {
    searchTerm: '',
    priorities: undefined,
    userId: undefined,
    tagIds: undefined,
    sortBy: 'createdAt',
    sortOrder: 'desc' as const,
    users: mockUsers,
    tags: mockTags,
    onSearchChange: vi.fn(),
    onPrioritiesChange: vi.fn(),
    onUserIdChange: vi.fn(),
    onTagIdsChange: vi.fn(),
    onSortByChange: vi.fn(),
    onSortOrderChange: vi.fn(),
    onClearFilters: vi.fn(),
  };

  it('renders all filter controls', () => {
    render(<TaskFilters {...defaultProps} />);

    expect(screen.getByPlaceholderText(/search/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/sort by/i)).toBeInTheDocument();
  });

  it('calls onSearchChange when search input changes', async () => {
    const user = userEvent.setup();
    const onSearchChange = vi.fn();
    
    render(<TaskFilters {...defaultProps} onSearchChange={onSearchChange} />);

    const searchInput = screen.getByPlaceholderText(/search/i);
    await user.type(searchInput, 'test');

    expect(onSearchChange).toHaveBeenCalled();
  });

  it('calls onClearFilters when clear button is clicked', async () => {
    const user = userEvent.setup();
    const onClearFilters = vi.fn();
    
    render(<TaskFilters {...defaultProps} onClearFilters={onClearFilters} searchTerm="test" />);

    const clearButton = screen.getByRole('button', { name: /clear/i });
    await user.click(clearButton);

    expect(onClearFilters).toHaveBeenCalled();
  });
});
