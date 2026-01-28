import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { CreateTaskDto, Priority, Tag } from '../types';
import { TagSelector } from './TagSelector';
import { PriorityDropdown } from './PriorityDropdown';
import { taskSchema } from '../schemas/taskSchema';

interface TaskFormProps {
  onSubmit: (data: CreateTaskDto) => void;
  initialData?: Partial<CreateTaskDto>;
  tags: Tag[];
  users: Array<{ id: number; fullName: string }>;
  onCancel?: () => void;
}

export const TaskForm: React.FC<TaskFormProps> = ({
  onSubmit,
  initialData,
  tags,
  users,
  onCancel,
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
  } = useForm<CreateTaskDto>({
    resolver: yupResolver(taskSchema),
    defaultValues: {
      title: initialData?.title || '',
      description: initialData?.description || '',
      dueDate: initialData?.dueDate 
        ? (typeof initialData.dueDate === 'string' 
            ? initialData.dueDate.split('T')[0] 
            : new Date(initialData.dueDate).toISOString().split('T')[0])
        : '',
      priority: initialData?.priority || Priority.Medium,
      createdByUserId: initialData?.createdByUserId || 0,
      userIds: initialData?.userIds || [],
      tagIds: initialData?.tagIds || [],
    },
  });

  const selectedTagIds = watch('tagIds');
  const selectedUserIds = watch('userIds');
  const selectedPriority = watch('priority');

  useEffect(() => {
    if (initialData?.tagIds) {
      setValue('tagIds', initialData.tagIds);
    }
    if (initialData?.priority) {
      setValue('priority', initialData.priority);
    }
  }, [initialData, setValue]);

  const handleTagChange = (tagIds: number[]) => {
    setValue('tagIds', tagIds, { shouldValidate: true });
  };

  const handlePriorityChange = (priority: Priority) => {
    setValue('priority', priority, { shouldValidate: true });
  };

  const onSubmitHandler = (data: CreateTaskDto) => {
    // Ensure createdByUserId is set if not already set
    if (!data.createdByUserId || data.createdByUserId === 0) {
      if (data.userIds && data.userIds.length > 0) {
        data.createdByUserId = data.userIds[0];
      } else {
        console.error('No users selected and no createdByUserId set');
        return;
      }
    }
    // Priority is already set from the form
    onSubmit(data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmitHandler)} className="space-y-6" style={{ position: 'relative', overflow: 'visible' }}>
      <div>
        <label htmlFor="title" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Title *
        </label>
        <input
          id="title"
          type="text"
          {...register('title')}
          className={`w-full premium-input ${errors.title ? 'border-red-500 focus:ring-red-500' : ''}`}
        />
        {errors.title && <span className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.title.message}</span>}
      </div>

      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Description *
        </label>
        <textarea
          id="description"
          {...register('description')}
          rows={4}
          className={`w-full premium-input ${errors.description ? 'border-red-500 focus:ring-red-500' : ''}`}
        />
        {errors.description && (
          <span className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.description.message}</span>
        )}
      </div>

      <div>
        <label htmlFor="dueDate" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Due Date *
        </label>
        <input
          id="dueDate"
          type="date"
          {...register('dueDate')}
          className={`w-full premium-input ${errors.dueDate ? 'border-red-500 focus:ring-red-500' : ''}`}
        />
        {errors.dueDate && <span className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.dueDate.message}</span>}
      </div>

      <PriorityDropdown
        selectedPriority={selectedPriority || Priority.Medium}
        onChange={handlePriorityChange}
        error={errors.priority?.message}
      />

      <div>
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Users *</label>
        <div className="glass-card p-4 max-h-48 overflow-y-auto">
          <div className="space-y-2">
            {users.map((user) => (
              <label key={user.id} className="flex items-center gap-2 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800/50 p-2 rounded-lg transition-colors">
                <input
                  type="checkbox"
                  checked={selectedUserIds.includes(user.id)}
                  onChange={() => {
                    const newIds = selectedUserIds.includes(user.id)
                      ? selectedUserIds.filter((id) => id !== user.id)
                      : [...selectedUserIds, user.id];
                    setValue('userIds', newIds, { shouldValidate: true });
                  }}
                  className="w-4 h-4 text-gray-600 dark:text-primary-600 rounded focus:ring-gray-500 dark:focus:ring-primary-500"
                />
                <span className="text-sm text-gray-700 dark:text-gray-300">{user.fullName}</span>
              </label>
            ))}
          </div>
        </div>
        {errors.userIds && <span className="mt-1 text-sm text-red-600 dark:text-red-400">{errors.userIds.message}</span>}
      </div>

      <div>
        <TagSelector
          tags={tags}
          selectedTagIds={selectedTagIds}
          onChange={handleTagChange}
          multiple={true}
        />
        {errors.tagIds && <span className="mt-1 text-sm text-red-600 dark:text-red-400">Please select at least one tag</span>}
      </div>

      <div className="flex gap-3 justify-end pt-4">
        {onCancel && (
          <button 
            type="button" 
            onClick={onCancel} 
            className="px-6 py-2 glass-card text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/80 transition-colors font-medium"
          >
            Cancel
          </button>
        )}
        <button 
          type="submit" 
          className="px-6 py-2 bg-gradient-to-r from-primary-600 to-purple-600 text-white rounded-lg hover:from-primary-700 hover:to-purple-700 transition-all duration-200 font-medium shadow-lg shadow-primary-500/30"
        >
          Save
        </button>
      </div>
    </form>
  );
};
