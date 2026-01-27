export enum Priority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

export interface User {
  id: number;
  fullName: string;
  telephone: string;
  email: string;
}

export interface Tag {
  id: number;
  name: string;
  color?: string;
}

export interface Task {
  id: number;
  title: string;
  description: string;
  dueDate: string;
  priority: Priority;
  user: User;
  tags: Tag[];
}

export interface CreateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: Priority;
  userId: number;
  tagIds: number[];
}

export interface UpdateTaskDto extends CreateTaskDto {
  id: number;
}
