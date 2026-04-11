// ── Domain types mirroring the API DTOs ─────────────────────────────────

export const TaskStatus = {
  Todo:       'Todo',
  InProgress: 'InProgress',
  Done:       'Done',
} as const;
export type TaskStatus = typeof TaskStatus[keyof typeof TaskStatus];

export interface TaskItem {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string | null;
  ownerUserId: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  dueDate: string | null;
}

export interface UpdateTaskRequest {
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string | null;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
}

export interface UserDto {
  id: string;
  username: string;
  email: string;
  role: string;
}

export interface ApiError {
  status: number;
  error: string;
  correlationId?: string;
}
