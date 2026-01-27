import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { Task, CreateTaskDto, UpdateTaskDto, GetTasksParams } from '../types';
import { taskApi } from '../services/api';

interface TaskState {
  tasks: Task[];
  loading: boolean;
  error: string | null;
  selectedTask: Task | null;
  pagination: {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
  };
  filters: {
    searchTerm?: string;
    priority?: number; // Single priority (backward compatibility)
    priorities?: number[]; // Multiple priorities
    userId?: number;
    tagId?: number; // Single tag (backward compatibility)
    tagIds?: number[]; // Multiple tags
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
  };
}

const initialState: TaskState = {
  tasks: [],
  loading: false,
  error: null,
  selectedTask: null,
  pagination: {
    currentPage: 1,
    itemsPerPage: 10,
    totalItems: 0,
    totalPages: 0,
  },
  filters: {
    searchTerm: undefined,
    priority: undefined,
    priorities: undefined,
    userId: undefined,
    tagId: undefined,
    tagIds: undefined,
    sortBy: 'createdAt',
    sortOrder: 'desc',
  },
};

export const fetchTasks = createAsyncThunk(
  'tasks/fetchAll',
  async (params?: GetTasksParams, { getState }) => {
    const state = getState() as { tasks: TaskState };
    const { pagination, filters } = state.tasks;
    
    // Validate and sanitize parameters
    const page = Math.max(1, params?.page ?? pagination.currentPage);
    const pageSize = Math.max(1, Math.min(1000, params?.pageSize ?? pagination.itemsPerPage));
    const searchTerm = params?.searchTerm?.trim() || filters.searchTerm?.trim() || undefined;
    
    // Filter out invalid IDs (negative or zero)
    const validPriorities = params?.priorities?.filter(p => p > 0 && p <= 4) || 
                           (filters.priorities && filters.priorities.length > 0 
                            ? filters.priorities.filter(p => p > 0 && p <= 4) 
                            : undefined);
    const validTagIds = params?.tagIds?.filter(id => id > 0) || 
                       filters.tagIds?.filter(id => id > 0);
    const validUserId = (params?.userId ?? filters.userId) && (params?.userId ?? filters.userId)! > 0 
                       ? (params?.userId ?? filters.userId) 
                       : undefined;
    const validTagId = (params?.tagId ?? filters.tagId) && (params?.tagId ?? filters.tagId)! > 0 
                      ? (params?.tagId ?? filters.tagId) 
                      : undefined;
    const validPriority = (params?.priority ?? filters.priority) && (params?.priority ?? filters.priority)! > 0 && (params?.priority ?? filters.priority)! <= 4
                         ? (params?.priority ?? filters.priority) 
                         : undefined;
    
    const queryParams: GetTasksParams = {
      page,
      pageSize,
      searchTerm: searchTerm || undefined,
      priority: validPriority,
      priorities: validPriorities && validPriorities.length > 0 ? validPriorities : undefined,
      userId: validUserId,
      tagId: validTagId,
      tagIds: validTagIds && validTagIds.length > 0 ? validTagIds : undefined,
      sortBy: params?.sortBy?.trim() || filters.sortBy?.trim() || 'createdAt',
      sortOrder: params?.sortOrder || filters.sortOrder || 'desc',
    };
    
    return await taskApi.getAll(queryParams);
  }
);

export const fetchTaskById = createAsyncThunk('tasks/fetchById', async (id: number) => {
  return await taskApi.getById(id);
});

export const createTask = createAsyncThunk('tasks/create', async (task: CreateTaskDto) => {
  return await taskApi.create(task);
});

export const updateTask = createAsyncThunk('tasks/update', async (task: UpdateTaskDto) => {
  return await taskApi.update(task);
});

export const deleteTask = createAsyncThunk('tasks/delete', async (id: number) => {
  await taskApi.delete(id);
  return id;
});

const taskSlice = createSlice({
  name: 'tasks',
  initialState,
  reducers: {
    setSelectedTask: (state, action: PayloadAction<Task | null>) => {
      state.selectedTask = action.payload;
    },
    clearError: (state) => {
      state.error = null;
    },
    setCurrentPage: (state, action: PayloadAction<number>) => {
      // Validate: page must be >= 1
      const page = Math.max(1, action.payload);
      state.pagination.currentPage = page;
    },
    setItemsPerPage: (state, action: PayloadAction<number>) => {
      // Validate: pageSize must be >= 1 and <= 1000
      const pageSize = Math.max(1, Math.min(1000, action.payload));
      state.pagination.itemsPerPage = pageSize;
      state.pagination.currentPage = 1; // Reset to first page when changing items per page
    },
    setSearchTerm: (state, action: PayloadAction<string | undefined>) => {
      state.filters.searchTerm = action.payload;
      state.pagination.currentPage = 1; // Reset to first page when searching
    },
    setPriorityFilter: (state, action: PayloadAction<number | undefined>) => {
      state.filters.priority = action.payload;
      state.filters.priorities = undefined; // Clear multiple when setting single
      state.pagination.currentPage = 1;
    },
    setPrioritiesFilter: (state, action: PayloadAction<number[] | undefined>) => {
      state.filters.priorities = action.payload;
      state.filters.priority = undefined; // Clear single when setting multiple
      state.pagination.currentPage = 1;
    },
    setUserIdFilter: (state, action: PayloadAction<number | undefined>) => {
      state.filters.userId = action.payload;
      state.pagination.currentPage = 1;
    },
    setTagIdFilter: (state, action: PayloadAction<number | undefined>) => {
      state.filters.tagId = action.payload;
      state.filters.tagIds = undefined; // Clear multiple when setting single
      state.pagination.currentPage = 1;
    },
    setTagIdsFilter: (state, action: PayloadAction<number[] | undefined>) => {
      state.filters.tagIds = action.payload;
      state.filters.tagId = undefined; // Clear single when setting multiple
      state.pagination.currentPage = 1;
    },
    setSortBy: (state, action: PayloadAction<string>) => {
      state.filters.sortBy = action.payload;
    },
    setSortOrder: (state, action: PayloadAction<'asc' | 'desc'>) => {
      state.filters.sortOrder = action.payload;
    },
    clearFilters: (state) => {
      state.filters = {
        searchTerm: undefined,
        priority: undefined,
        priorities: undefined,
        userId: undefined,
        tagId: undefined,
        tagIds: undefined,
        sortBy: 'createdAt',
        sortOrder: 'desc',
      };
      state.pagination.currentPage = 1;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch all tasks
      .addCase(fetchTasks.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchTasks.fulfilled, (state, action) => {
        state.loading = false;
        state.tasks = action.payload.items;
        state.pagination.totalItems = action.payload.totalCount;
        state.pagination.totalPages = action.payload.totalPages;
        state.pagination.currentPage = action.payload.page;
        state.pagination.itemsPerPage = action.payload.pageSize;
      })
      .addCase(fetchTasks.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch tasks';
      })
      // Fetch task by ID
      .addCase(fetchTaskById.fulfilled, (state, action) => {
        state.selectedTask = action.payload;
      })
      // Create task
      .addCase(createTask.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createTask.fulfilled, (state, action) => {
        state.loading = false;
        state.tasks.push(action.payload);
      })
      .addCase(createTask.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to create task';
      })
      // Update task
      .addCase(updateTask.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateTask.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.tasks.findIndex((t) => t.id === action.payload.id);
        if (index !== -1) {
          state.tasks[index] = action.payload;
        }
        if (state.selectedTask?.id === action.payload.id) {
          state.selectedTask = action.payload;
        }
      })
      .addCase(updateTask.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to update task';
      })
      // Delete task
      .addCase(deleteTask.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteTask.fulfilled, (state, action) => {
        state.loading = false;
        state.tasks = state.tasks.filter((t) => t.id !== action.payload);
        if (state.selectedTask?.id === action.payload) {
          state.selectedTask = null;
        }
      })
      .addCase(deleteTask.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to delete task';
      });
  },
});

export const { 
  setSelectedTask, 
  clearError, 
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
} = taskSlice.actions;
export default taskSlice.reducer;
