import React, { useEffect, useState } from 'react';
import { useAppDispatch, useAppSelector } from './store/hooks';
import { fetchTasks, createTask, updateTask, deleteTask, setSelectedTask } from './store/taskSlice';
import { fetchTags } from './store/tagSlice';
import { TaskList } from './components/TaskList';
import { TaskForm } from './components/TaskForm';
import { CreateTaskDto, UpdateTaskDto, Task } from './types';
import './App.css';

// Mock users - in a real app, this would come from an API
const MOCK_USERS = [
  { id: 1, fullName: 'John Doe', email: 'john@example.com', telephone: '123-456-7890' },
  { id: 2, fullName: 'Jane Smith', email: 'jane@example.com', telephone: '098-765-4321' },
  { id: 3, fullName: 'Bob Johnson', email: 'bob@example.com', telephone: '555-123-4567' },
];

function App() {
  const dispatch = useAppDispatch();
  const { tasks, loading, error, selectedTask } = useAppSelector((state) => state.tasks);
  const { tags } = useAppSelector((state) => state.tags);
  const [showForm, setShowForm] = useState(false);
  const [editingTask, setEditingTask] = useState<Task | null>(null);

  useEffect(() => {
    dispatch(fetchTasks());
    dispatch(fetchTags());
  }, [dispatch]);

  const handleCreateTask = async (data: CreateTaskDto) => {
    try {
      await dispatch(createTask(data)).unwrap();
      setShowForm(false);
      dispatch(fetchTasks());
    } catch (err) {
      console.error('Failed to create task:', err);
    }
  };

  const handleUpdateTask = async (data: CreateTaskDto) => {
    if (!editingTask) return;
    try {
      const updateData: UpdateTaskDto = { ...data, id: editingTask.id };
      await dispatch(updateTask(updateData)).unwrap();
      setEditingTask(null);
      setShowForm(false);
      dispatch(fetchTasks());
    } catch (err) {
      console.error('Failed to update task:', err);
    }
  };

  const handleDeleteTask = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      try {
        await dispatch(deleteTask(id)).unwrap();
        dispatch(fetchTasks());
      } catch (err) {
        console.error('Failed to delete task:', err);
      }
    }
  };

  const handleEditTask = (task: Task) => {
    setEditingTask(task);
    setShowForm(true);
    dispatch(setSelectedTask(task));
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingTask(null);
    dispatch(setSelectedTask(null));
  };

  return (
    <div className="app">
      <header className="app-header">
        <h1>Task Management System</h1>
        {!showForm && (
          <button onClick={() => setShowForm(true)} className="btn btn-primary">
            Create New Task
          </button>
        )}
      </header>

      {error && (
        <div className="error-banner">
          <p>Error: {error}</p>
        </div>
      )}

      <main className="app-main">
        {showForm ? (
          <div className="form-container">
            <h2>{editingTask ? 'Edit Task' : 'Create New Task'}</h2>
            <TaskForm
              onSubmit={editingTask ? handleUpdateTask : handleCreateTask}
              initialData={
                editingTask
                  ? {
                      title: editingTask.title,
                      description: editingTask.description,
                      dueDate: editingTask.dueDate.split('T')[0],
                      priority: editingTask.priority,
                      userId: editingTask.user.id,
                      tagIds: editingTask.tags.map((t) => t.id),
                    }
                  : undefined
              }
              tags={tags}
              users={MOCK_USERS}
              onCancel={handleCancel}
            />
          </div>
        ) : (
          <TaskList
            tasks={tasks}
            onEdit={handleEditTask}
            onDelete={handleDeleteTask}
            loading={loading}
          />
        )}
      </main>
    </div>
  );
}

export default App;
