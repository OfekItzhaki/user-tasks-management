import React from 'react';
import { Task, Priority } from '../types';

interface TaskListProps {
  tasks: Task[];
  onEdit: (task: Task) => void;
  onDelete: (id: number) => void;
  loading?: boolean;
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

export const TaskList: React.FC<TaskListProps> = ({ tasks, onEdit, onDelete, loading }) => {
  if (loading) {
    return <div className="loading">Loading tasks...</div>;
  }

  if (tasks.length === 0) {
    return <div className="empty-state">No tasks found. Create your first task!</div>;
  }

  return (
    <div className="task-list">
      {tasks.map((task) => (
        <div key={task.id} className="task-card">
          <div className="task-header">
            <h3 className="task-title">{task.title}</h3>
            <span
              className="priority-badge"
              style={{ backgroundColor: getPriorityColor(task.priority) }}
            >
              {getPriorityLabel(task.priority)}
            </span>
          </div>
          <p className="task-description">{task.description}</p>
          <div className="task-details">
            <div className="task-detail">
              <strong>Due Date:</strong> {new Date(task.dueDate).toLocaleDateString()}
            </div>
            <div className="task-detail">
              <strong>User:</strong> {task.user.fullName} ({task.user.email})
            </div>
            <div className="task-detail">
              <strong>Phone:</strong> {task.user.telephone}
            </div>
          </div>
          <div className="task-tags">
            {task.tags.map((tag) => (
              <span
                key={tag.id}
                className="tag-badge"
                style={{ backgroundColor: tag.color || '#6c757d' }}
              >
                {tag.name}
              </span>
            ))}
          </div>
          <div className="task-actions">
            <button onClick={() => onEdit(task)} className="btn btn-sm btn-primary">
              Edit
            </button>
            <button onClick={() => onDelete(task.id)} className="btn btn-sm btn-danger">
              Delete
            </button>
          </div>
        </div>
      ))}
    </div>
  );
};
