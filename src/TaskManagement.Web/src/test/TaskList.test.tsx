import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { TaskList } from '../components/TaskList';
import { Task, Priority } from '../types';

const mockTasks: Task[] = [
  {
    id: 1,
    title: 'Test Task 1',
    description: 'Description 1',
    dueDate: '2024-12-31T00:00:00',
    priority: Priority.High,
    createdByUserId: 1,
    users: [],
    tags: [],
  },
  {
    id: 2,
    title: 'Test Task 2',
    description: 'Description 2',
    dueDate: '2024-12-30T00:00:00',
    priority: Priority.Low,
    createdByUserId: 1,
    users: [],
    tags: [],
  },
];

describe('TaskList', () => {
  it('renders tasks correctly', () => {
    const onEdit = vi.fn();
    const onDelete = vi.fn();

    render(
      <TaskList
        tasks={mockTasks}
        onEdit={onEdit}
        onDelete={onDelete}
      />
    );

    expect(screen.getByText('Test Task 1')).toBeInTheDocument();
    expect(screen.getByText('Test Task 2')).toBeInTheDocument();
  });

  it('calls onEdit when edit button is clicked', async () => {
    const user = userEvent.setup();
    const onEdit = vi.fn();
    const onDelete = vi.fn();

    render(
      <TaskList
        tasks={mockTasks}
        onEdit={onEdit}
        onDelete={onDelete}
      />
    );

    const editButtons = screen.getAllByRole('button', { name: /edit/i });
    await user.click(editButtons[0]);

    expect(onEdit).toHaveBeenCalledWith(mockTasks[0]);
  });

  it('calls onDelete when delete button is clicked and confirmed', async () => {
    const user = userEvent.setup();
    const onEdit = vi.fn();
    const onDelete = vi.fn();

    render(
      <TaskList
        tasks={mockTasks}
        onEdit={onEdit}
        onDelete={onDelete}
      />
    );

    // Find and click the delete button (trash icon) for the first task
    // The delete button has title="Delete task" but no accessible name, so we use getAllByTitle
    const deleteButtons = screen.getAllByTitle('Delete task');
    await user.click(deleteButtons[0]);

    // Wait for confirm dialog and click the confirm button
    const confirmButton = await screen.findByRole('button', { name: /^delete$/i });
    await user.click(confirmButton);

    expect(onDelete).toHaveBeenCalledWith(mockTasks[0].id);
  });

  it('shows loading skeleton when loading', () => {
    const onEdit = vi.fn();
    const onDelete = vi.fn();

    render(
      <TaskList
        tasks={[]}
        onEdit={onEdit}
        onDelete={onDelete}
        loading={true}
      />
    );

    const skeletons = screen.getAllByTestId('skeleton');
    expect(skeletons.length).toBeGreaterThan(0);
  });
});
