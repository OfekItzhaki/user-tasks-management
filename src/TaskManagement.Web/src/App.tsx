import React, { useEffect, useState } from 'react';
import { ErrorBoundary } from 'react-error-boundary';
import { useAppDispatch, useAppSelector } from './store/hooks';
import { 
  fetchTasks, 
  createTask, 
  updateTask, 
  deleteTask, 
  setSelectedTask, 
  setCurrentPage, 
  setItemsPerPage,
  setSearchTerm,
  setPriorityFilter,
  setPrioritiesFilter,
  setUserIdFilter,
  setTagIdFilter,
  setTagIdsFilter,
  setSortBy,
  setSortOrder,
  clearFilters
} from './store/taskSlice';
import { fetchTags } from './store/tagSlice';
import { fetchUsers } from './store/userSlice';
import { TaskList } from './components/TaskList';
import { TaskForm } from './components/TaskForm';
import { TaskFilters } from './components/TaskFilters';
import FloatingActionButton from './components/FloatingActionButton';
import ErrorFallback from './components/ErrorFallback';
import { CreateTaskDto, UpdateTaskDto, Task } from './types';

function App() {
  const dispatch = useAppDispatch();
  const { tasks, loading, error, selectedTask, pagination, filters } = useAppSelector((state) => state.tasks);
  const { tags } = useAppSelector((state) => state.tags);
  const { users } = useAppSelector((state) => state.users);
  const [showForm, setShowForm] = useState(false);
  const [editingTask, setEditingTask] = useState<Task | null>(null);
  const [darkMode, setDarkMode] = useState(() => {
    const stored = localStorage.getItem('darkMode');
    return stored ? stored === 'true' : false;
  });

  useEffect(() => {
    if (darkMode) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
    localStorage.setItem('darkMode', darkMode.toString());
  }, [darkMode]);

  useEffect(() => {
    dispatch(fetchTasks());
    dispatch(fetchTags());
    dispatch(fetchUsers());
  }, [dispatch]);

  // Refetch tasks when pagination or filters change
  useEffect(() => {
    dispatch(fetchTasks());
  }, [dispatch, pagination.currentPage, pagination.itemsPerPage, filters]);

  const handleCreateTask = async (data: CreateTaskDto) => {
    try {
      // Ensure createdByUserId is set
      if (!data.createdByUserId || data.createdByUserId === 0) {
        if (data.userIds && data.userIds.length > 0) {
          data.createdByUserId = data.userIds[0];
        } else {
          console.error('Cannot create task: No users selected');
          return;
        }
      }
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
    try {
      await dispatch(deleteTask(id)).unwrap();
      dispatch(fetchTasks());
    } catch (err) {
      console.error('Failed to delete task:', err);
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

  const handlePageChange = (page: number) => {
    dispatch(setCurrentPage(page));
  };

  const handleItemsPerPageChange = (itemsPerPage: number) => {
    dispatch(setItemsPerPage(itemsPerPage));
  };

  return (
    <ErrorBoundary FallbackComponent={ErrorFallback}>
      <div className="min-h-screen">
        <header className="glass-card border-b border-gray-200 dark:border-gray-700/30 sticky top-0 z-50 backdrop-blur-xl mb-6 bg-white dark:bg-gray-900/70">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between items-center h-16">
              <h1 className="text-2xl font-bold gradient-text">Task Management System</h1>
              <div className="flex items-center gap-3">
                <button
                  onClick={() => setDarkMode(!darkMode)}
                  className="p-2 glass-card rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/80 transition-colors"
                  aria-label="Toggle dark mode"
                >
                  {darkMode ? (
                    <svg className="w-5 h-5 text-gray-700 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                    </svg>
                  ) : (
                    <svg className="w-5 h-5 text-gray-700 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                    </svg>
                  )}
                </button>
              </div>
            </div>
          </div>
        </header>

        {error && (
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mb-4">
            <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
              <p className="text-red-800 dark:text-red-200">Error: {error}</p>
            </div>
          </div>
        )}

        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pb-24">
          {showForm ? (
            <div className="glass-card p-8 max-w-2xl mx-auto">
              <h2 className="premium-header-section mb-6">{editingTask ? 'Edit Task' : 'Create New Task'}</h2>
              <TaskForm
                onSubmit={editingTask ? handleUpdateTask : handleCreateTask}
                initialData={
                  editingTask
                    ? {
                        title: editingTask.title,
                        description: editingTask.description,
                        dueDate: editingTask.dueDate.split('T')[0],
                        priority: editingTask.priority,
                        createdByUserId: editingTask.createdByUserId,
                        userIds: editingTask.users.map((ut) => ut.user.id),
                        tagIds: editingTask.tags.map((t) => t.id),
                      }
                    : {
                        createdByUserId: users.length > 0 ? users[0].id : 0,
                      }
                }
                tags={tags}
                users={users.map(u => ({ id: u.id, fullName: u.fullName }))}
                onCancel={handleCancel}
              />
            </div>
          ) : (
            <>
              <TaskFilters
                searchTerm={filters.searchTerm || ''}
                priority={filters.priority}
                priorities={filters.priorities}
                userId={filters.userId}
                tagId={filters.tagId}
                tagIds={filters.tagIds}
                sortBy={filters.sortBy || 'createdAt'}
                sortOrder={filters.sortOrder || 'desc'}
                users={users}
                tags={tags}
                onSearchChange={(value) => dispatch(setSearchTerm(value))}
                onPriorityChange={(value) => dispatch(setPriorityFilter(value))}
                onPrioritiesChange={(value) => dispatch(setPrioritiesFilter(value))}
                onUserIdChange={(value) => dispatch(setUserIdFilter(value))}
                onTagIdChange={(value) => dispatch(setTagIdFilter(value))}
                onTagIdsChange={(value) => dispatch(setTagIdsFilter(value))}
                onSortByChange={(value) => dispatch(setSortBy(value))}
                onSortOrderChange={(value) => dispatch(setSortOrder(value))}
                onClearFilters={() => dispatch(clearFilters())}
              />
              <TaskList
                tasks={tasks}
                onEdit={handleEditTask}
                onDelete={handleDeleteTask}
                loading={loading}
                currentPage={pagination.currentPage}
                itemsPerPage={pagination.itemsPerPage}
                onPageChange={handlePageChange}
                onItemsPerPageChange={handleItemsPerPageChange}
              />
            </>
          )}
        </main>

        {!showForm && (
          <FloatingActionButton
            onClick={() => setShowForm(true)}
            ariaLabel="Create New Task"
          />
        )}
      </div>
    </ErrorBoundary>
  );
}

export default App;
