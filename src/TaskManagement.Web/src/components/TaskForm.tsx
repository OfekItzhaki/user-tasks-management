import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { CreateTaskDto, Priority, Tag } from '../types';
import { TagSelector } from './TagSelector';

interface TaskFormProps {
  onSubmit: (data: CreateTaskDto) => void;
  initialData?: Partial<CreateTaskDto>;
  tags: Tag[];
  users: Array<{ id: number; fullName: string }>;
  onCancel?: () => void;
}

const schema = yup.object({
  title: yup.string().required('Title is required').max(200, 'Title must not exceed 200 characters'),
  description: yup
    .string()
    .required('Description is required')
    .max(1000, 'Description must not exceed 1000 characters'),
  dueDate: yup
    .date()
    .required('Due date is required')
    .min(new Date().setHours(0, 0, 0, 0), 'Due date must be today or in the future')
    .typeError('Please enter a valid date'),
  priority: yup.number().required('Priority is required').oneOf([1, 2, 3, 4], 'Invalid priority'),
  userId: yup.number().required('User is required').min(1, 'Please select a user'),
  tagIds: yup.array().of(yup.number()).required(),
});

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
    resolver: yupResolver(schema),
    defaultValues: {
      title: initialData?.title || '',
      description: initialData?.description || '',
      dueDate: initialData?.dueDate || '',
      priority: initialData?.priority || Priority.Medium,
      userId: initialData?.userId || 0,
      tagIds: initialData?.tagIds || [],
    },
  });

  const selectedTagIds = watch('tagIds');

  useEffect(() => {
    if (initialData?.tagIds) {
      setValue('tagIds', initialData.tagIds);
    }
  }, [initialData, setValue]);

  const handleTagChange = (tagIds: number[]) => {
    setValue('tagIds', tagIds, { shouldValidate: true });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="task-form">
      <div className="form-group">
        <label htmlFor="title" className="form-label">
          Title *
        </label>
        <input
          id="title"
          type="text"
          {...register('title')}
          className={`form-input ${errors.title ? 'error' : ''}`}
        />
        {errors.title && <span className="error-message">{errors.title.message}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="description" className="form-label">
          Description *
        </label>
        <textarea
          id="description"
          {...register('description')}
          rows={4}
          className={`form-input ${errors.description ? 'error' : ''}`}
        />
        {errors.description && (
          <span className="error-message">{errors.description.message}</span>
        )}
      </div>

      <div className="form-group">
        <label htmlFor="dueDate" className="form-label">
          Due Date *
        </label>
        <input
          id="dueDate"
          type="date"
          {...register('dueDate')}
          className={`form-input ${errors.dueDate ? 'error' : ''}`}
        />
        {errors.dueDate && <span className="error-message">{errors.dueDate.message}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="priority" className="form-label">
          Priority *
        </label>
        <select
          id="priority"
          {...register('priority', { valueAsNumber: true })}
          className={`form-input ${errors.priority ? 'error' : ''}`}
        >
          <option value={Priority.Low}>Low</option>
          <option value={Priority.Medium}>Medium</option>
          <option value={Priority.High}>High</option>
          <option value={Priority.Critical}>Critical</option>
        </select>
        {errors.priority && <span className="error-message">{errors.priority.message}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="userId" className="form-label">
          User *
        </label>
        <select
          id="userId"
          {...register('userId', { valueAsNumber: true })}
          className={`form-input ${errors.userId ? 'error' : ''}`}
        >
          <option value={0}>Select a user</option>
          {users.map((user) => (
            <option key={user.id} value={user.id}>
              {user.fullName}
            </option>
          ))}
        </select>
        {errors.userId && <span className="error-message">{errors.userId.message}</span>}
      </div>

      <div className="form-group">
        <TagSelector
          tags={tags}
          selectedTagIds={selectedTagIds}
          onChange={handleTagChange}
          multiple={true}
        />
        {errors.tagIds && <span className="error-message">Please select at least one tag</span>}
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary">
          Save
        </button>
        {onCancel && (
          <button type="button" onClick={onCancel} className="btn btn-secondary">
            Cancel
          </button>
        )}
      </div>
    </form>
  );
};
