import React, { useMemo, useState } from 'react';
import { Task, Priority } from '../types';
import Skeleton from './Skeleton';
import Pagination from './Pagination';
import { ConfirmDialog } from './ConfirmDialog';

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

const getPriorityColor = (priority: Priority): { bg: string; text: string; border: string } => {
  switch (priority) {
    case Priority.Low:
      return { bg: 'bg-green-50 dark:bg-green-900/20', text: 'text-green-700 dark:text-green-400', border: 'border-green-200 dark:border-green-800' };
    case Priority.Medium:
      return { bg: 'bg-yellow-50 dark:bg-yellow-900/20', text: 'text-yellow-700 dark:text-yellow-400', border: 'border-yellow-200 dark:border-yellow-800' };
    case Priority.High:
      return { bg: 'bg-orange-50 dark:bg-orange-900/20', text: 'text-orange-700 dark:text-orange-400', border: 'border-orange-200 dark:border-orange-800' };
    case Priority.Critical:
      return { bg: 'bg-red-50 dark:bg-red-900/20', text: 'text-red-700 dark:text-red-400', border: 'border-red-200 dark:border-red-800' };
    default:
      return { bg: 'bg-gray-50 dark:bg-gray-800', text: 'text-gray-700 dark:text-gray-400', border: 'border-gray-200 dark:border-gray-700' };
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
  const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; taskId: number | null; taskTitle: string }>({
    isOpen: false,
    taskId: null,
    taskTitle: '',
  });

  const paginatedTasks = useMemo(() => {
    if (!onPageChange) return tasks;
    const start = (currentPage - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    return tasks.slice(start, end);
  }, [tasks, currentPage, itemsPerPage, onPageChange]);

  const totalPages = onPageChange ? Math.ceil(tasks.length / itemsPerPage) : 1;

  const handleDeleteClick = (taskId: number, taskTitle: string) => {
    setDeleteConfirm({ isOpen: true, taskId, taskTitle });
  };

  const handleDeleteConfirm = () => {
    if (deleteConfirm.taskId !== null) {
      onDelete(deleteConfirm.taskId);
      setDeleteConfirm({ isOpen: false, taskId: null, taskTitle: '' });
    }
  };

  const handleDeleteCancel = () => {
    setDeleteConfirm({ isOpen: false, taskId: null, taskTitle: '' });
  };

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
      <ConfirmDialog
        isOpen={deleteConfirm.isOpen}
        title="Delete Task"
        message={`Are you sure you want to delete "${deleteConfirm.taskTitle}"? This action cannot be undone.`}
        confirmText="Delete"
        cancelText="Cancel"
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
        variant="danger"
      />

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {paginatedTasks.map((task) => {
          const priorityColors = getPriorityColor(task.priority);
          return (
          <div key={task.id} className="glass-card p-6 hover:shadow-lg transition-all duration-200 border border-gray-200/50 dark:border-gray-700/50">
            <div className="flex justify-between items-start mb-4">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 flex-1 pr-2">{task.title}</h3>
              <span
                className={`px-2.5 py-1 rounded-md text-xs font-medium border ${priorityColors.bg} ${priorityColors.text} ${priorityColors.border} whitespace-nowrap`}
              >
                {getPriorityLabel(task.priority)}
              </span>
            </div>
          <p className="text-gray-600 dark:text-gray-400 mb-4 line-clamp-3 text-sm leading-relaxed">{task.description}</p>
          <div className="space-y-2.5 mb-4">
            <div className="text-sm">
              <span className="text-gray-500 dark:text-gray-500 font-medium">Due Date</span>
              <div className="text-gray-900 dark:text-gray-100 mt-0.5">{new Date(task.dueDate).toLocaleDateString()}</div>
            </div>
            {task.users.length > 0 && (
              <div className="text-sm">
                <span className="text-gray-500 dark:text-gray-500 font-medium">Assigned</span>
                <div className="flex flex-wrap gap-1.5 mt-1.5">
                  {task.users.map((userTask, index) => (
                    <span 
                      key={index} 
                      className="px-2 py-0.5 rounded-md text-xs bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700"
                    >
                      {userTask.user.fullName}
                      <span className="text-gray-500 dark:text-gray-500 ml-1">
                        {userTask.role === 1 ? '(Owner)' : userTask.role === 2 ? '(Assignee)' : '(Watcher)'}
                      </span>
                    </span>
                  ))}
                </div>
              </div>
            )}
          </div>
          {task.tags.length > 0 && (
            <div className="flex flex-wrap gap-1.5 mb-4">
              {task.tags.map((tag) => (
                <span
                  key={tag.id}
                  className="px-2 py-0.5 rounded-md text-xs font-medium bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700"
                  style={tag.color ? { 
                    backgroundColor: `${tag.color}15`,
                    color: tag.color,
                    borderColor: `${tag.color}40`
                  } : {}}
                >
                  {tag.name}
                </span>
              ))}
            </div>
          )}
          <div className="flex gap-2 pt-4 border-t border-gray-200 dark:border-gray-700">
            <button 
              onClick={() => onEdit(task)} 
              className="flex-1 px-4 py-2 bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors text-sm font-medium border border-gray-200 dark:border-gray-700"
            >
              Edit
            </button>
            <button 
              onClick={() => handleDeleteClick(task.id, task.title)} 
              className="px-4 py-2 text-gray-500 dark:text-gray-400 hover:text-red-600 dark:hover:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors text-sm font-medium border border-transparent hover:border-red-200 dark:hover:border-red-800"
              title="Delete task"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </button>
          </div>
        </div>
        );
        })}
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
