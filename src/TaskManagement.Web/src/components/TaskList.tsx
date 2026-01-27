import React, { useMemo } from 'react';
import { Task, Priority } from '../types';
import Skeleton from './Skeleton';
import Pagination from './Pagination';

interface TaskListProps {
  tasks: Task[];
  onEdit: (task: Task) => void;
  onDelete: (id: number) => void;
  loading?: boolean;
  currentPage?: number;
  itemsPerPage?: number;
  onPageChange?: (page: number) => void;
  onItemsPerPageChange?: (itemsPerPage: number) => void;
}

const getPriorityColor = (priority: Priority): string => {
  switch (priority) {
    case Priority.Low:
      return '#28a745';
    case Priority.Medium:
      return '#ffc107';
    case Priority.High:
      return '#fd7e14';
    case Priority.Critical:
      return '#dc3545';
    default:
      return '#6c757d';
  }
};

const getPriorityLabel = (priority: Priority): string => {
  return Priority[priority] || 'Unknown';
};

export const TaskList: React.FC<TaskListProps> = ({ 
  tasks, 
  onEdit, 
  onDelete, 
  loading,
  currentPage = 1,
  itemsPerPage = 10,
  onPageChange,
  onItemsPerPageChange,
}) => {
  const paginatedTasks = useMemo(() => {
    if (!onPageChange) return tasks;
    const start = (currentPage - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    return tasks.slice(start, end);
  }, [tasks, currentPage, itemsPerPage, onPageChange]);

  const totalPages = onPageChange ? Math.ceil(tasks.length / itemsPerPage) : 1;

  if (loading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} className="h-48 w-full" />
        ))}
      </div>
    );
  }

  if (tasks.length === 0) {
    return (
      <div className="text-center py-12 glass-card p-8">
        <p className="text-gray-600 dark:text-gray-400 text-lg">No tasks found. Create your first task!</p>
      </div>
    );
  }

  return (
    <>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {paginatedTasks.map((task) => (
        <div key={task.id} className="glass-card p-6 hover:shadow-lg transition-shadow duration-200">
          <div className="flex justify-between items-start mb-4">
            <h3 className="text-xl font-semibold text-gray-900 dark:text-gray-100 flex-1">{task.title}</h3>
            <span
              className="px-3 py-1 rounded-full text-xs font-bold text-white ml-2"
              style={{ backgroundColor: getPriorityColor(task.priority) }}
            >
              {getPriorityLabel(task.priority)}
            </span>
          </div>
          <p className="text-gray-600 dark:text-gray-400 mb-4 line-clamp-3">{task.description}</p>
          <div className="space-y-2 mb-4">
            <div className="text-sm">
              <strong className="text-gray-700 dark:text-gray-300">Due Date:</strong>{' '}
              <span className="text-gray-600 dark:text-gray-400">{new Date(task.dueDate).toLocaleDateString()}</span>
            </div>
            <div className="text-sm">
              <strong className="text-gray-700 dark:text-gray-300">Assigned Users:</strong>
              <div className="flex flex-wrap gap-2 mt-1">
                {task.users.map((userTask, index) => (
                  <span 
                    key={index} 
                    className="px-2 py-1 rounded-lg text-xs bg-primary-500/20 text-primary-700 dark:text-primary-300"
                  >
                    {userTask.user.fullName} ({userTask.role === 1 ? 'Owner' : userTask.role === 2 ? 'Assignee' : 'Watcher'})
                  </span>
                ))}
              </div>
            </div>
          </div>
          <div className="flex flex-wrap gap-2 mb-4">
            {task.tags.map((tag) => (
              <span
                key={tag.id}
                className="px-2 py-1 rounded-lg text-xs text-white"
                style={{ backgroundColor: tag.color || '#6c757d' }}
              >
                {tag.name}
              </span>
            ))}
          </div>
          <div className="flex gap-2 pt-4 border-t border-gray-200 dark:border-gray-700">
            <button 
              onClick={() => onEdit(task)} 
              className="flex-1 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors text-sm font-medium"
            >
              Edit
            </button>
            <button 
              onClick={() => onDelete(task.id)} 
              className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors text-sm font-medium"
            >
              Delete
            </button>
          </div>
        </div>
        ))}
      </div>

      {onPageChange && tasks.length > 0 && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={onPageChange}
          itemsPerPage={itemsPerPage}
          totalItems={tasks.length}
          onItemsPerPageChange={onItemsPerPageChange}
        />
      )}
    </>
  );
};
