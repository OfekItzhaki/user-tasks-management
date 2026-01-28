import * as yup from 'yup';

export const taskSchema = yup.object({
  title: yup.string().required('Title is required').max(200, 'Title must not exceed 200 characters'),
  description: yup
    .string()
    .required('Description is required')
    .max(1000, 'Description must not exceed 1000 characters'),
  dueDate: yup
    .string()
    .required('Due date is required')
    .test('is-valid-date', 'Please enter a valid date', (value) => {
      if (!value) return false;
      const date = new Date(value);
      return !isNaN(date.getTime());
    })
    .test('is-future-date', 'Due date must be today or in the future', (value) => {
      if (!value) return false;
      const date = new Date(value);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      date.setHours(0, 0, 0, 0);
      return date >= today;
    }),
  priority: yup.number().required('Priority is required').oneOf([1, 2, 3, 4], 'Invalid priority').min(1, 'Priority is required'),
  createdByUserId: yup.number().required('Created by user is required').min(1, 'Please select a user'),
  userIds: yup.array().of(yup.number().required()).min(1, 'At least one user must be assigned').required('At least one user must be assigned'),
  tagIds: yup.array().of(yup.number().required()).required('At least one tag must be selected').min(1, 'At least one tag must be selected'),
});
