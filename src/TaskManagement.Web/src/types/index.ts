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

export enum UserTaskRole {
  Owner = 1,
  Assignee = 2,
  Watcher = 3
}

export interface UserTask {
  user: User;
  role: UserTaskRole;
  assignedAt: string;
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
  createdByUserId: number;
  users: UserTask[];
  tags: Tag[];
}

export interface CreateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: Priority;
  createdByUserId: number;
  userIds: number[];
  tagIds: number[];
}

export interface UpdateTaskDto extends CreateTaskDto {
  id: number;
}
