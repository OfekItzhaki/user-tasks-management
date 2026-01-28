import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { TaskForm } from '../components/TaskForm';
import { Priority, Tag } from '../types';

const mockTags: Tag[] = [
  { id: 1, name: 'Bug Fix', color: '#ff4500' },
  { id: 2, name: 'Feature', color: '#0000ff' },
];

const mockUsers = [
  { id: 1, fullName: 'John Doe' },
  { id: 2, fullName: 'Jane Smith' },
];

describe('TaskForm', () => {
  it('renders form fields correctly', () => {
    const onSubmit = vi.fn();
    
    render(
      <TaskForm
        onSubmit={onSubmit}
        tags={mockTags}
        users={mockUsers}
      />
    );

    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/due date/i)).toBeInTheDocument();
  });

  it('shows validation errors for required fields', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    
    render(
      <TaskForm
        onSubmit={onSubmit}
        tags={mockTags}
        users={mockUsers}
      />
    );

    const submitButton = screen.getByRole('button', { name: /save/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/title is required/i)).toBeInTheDocument();
    });
  });

  it('allows user to fill form fields', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    
    render(
      <TaskForm
        onSubmit={onSubmit}
        tags={mockTags}
        users={mockUsers}
      />
    );

    // Fill in text fields
    await user.type(screen.getByLabelText(/title/i), 'Test Task');
    await user.type(screen.getByLabelText(/description/i), 'Test Description');
    
    // Verify fields are filled
    expect(screen.getByDisplayValue('Test Task')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Test Description')).toBeInTheDocument();
    
    // Verify form controls are present
    expect(screen.getByLabelText(/due date/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/priority/i)).toBeInTheDocument();
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('Bug Fix')).toBeInTheDocument();
  });

  it('populates form with initial data when editing', () => {
    const onSubmit = vi.fn();
    const initialData = {
      title: 'Existing Task',
      description: 'Existing Description',
      dueDate: '2024-12-31',
      priority: Priority.High,
      userIds: [1],
      tagIds: [1],
    };

    render(
      <TaskForm
        onSubmit={onSubmit}
        initialData={initialData}
        tags={mockTags}
        users={mockUsers}
      />
    );

    expect(screen.getByDisplayValue('Existing Task')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Existing Description')).toBeInTheDocument();
  });
});
