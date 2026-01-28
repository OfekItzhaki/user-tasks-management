import axios, { AxiosError } from 'axios';
import type { Task, CreateTaskDto, UpdateTaskDto, Tag, User, PagedResult, GetTasksParams } from '../types';
import { getFriendlyErrorMessage } from '../utils/errorHandler';

const API_BASE_URL = import.meta.env.VITE_API_URL || (import.meta.env.DEV ? '/api' : 'https://localhost:7000/api');

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  paramsSerializer: {
    indexes: null,
  },
});

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const friendlyMessage = getFriendlyErrorMessage(error, 'complete the request');
    console.error('API Error:', friendlyMessage, error);
    return Promise.reject(error);
  }
);

export const taskApi = {
  getAll: async (params?: GetTasksParams): Promise<PagedResult<Task>> => {
    const queryParams: any = { ...params };
    
    Object.keys(queryParams).forEach(key => {
      const value = queryParams[key];
      if (value === undefined || value === null) {
        delete queryParams[key];
      }
      if (Array.isArray(value) && value.length === 0) {
        delete queryParams[key];
      }
    });
    
    const response = await apiClient.get<PagedResult<Task>>('/tasks', { 
      params: queryParams,
    });
    return response.data;
  },

  getById: async (id: number): Promise<Task> => {
    const response = await apiClient.get<Task>(`/tasks/${id}`);
    return response.data;
  },

  create: async (task: CreateTaskDto): Promise<Task> => {
    const response = await apiClient.post<Task>('/tasks', task);
    return response.data;
  },

  update: async (task: UpdateTaskDto): Promise<Task> => {
    const response = await apiClient.put<Task>(`/tasks/${task.id}`, task);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/tasks/${id}`);
  },
};

export const tagApi = {
  getAll: async (): Promise<Tag[]> => {
    const response = await apiClient.get<Tag[]>('/tags');
    return response.data;
  },
};

export const userApi = {
  getAll: async (): Promise<User[]> => {
    const response = await apiClient.get<User[]>('/users');
    return response.data;
  },
};
