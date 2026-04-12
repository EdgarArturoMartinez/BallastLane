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

export interface AuditEntry {
  id: string;
  entity: string; // e.g. 'Users' or 'Tasks'
  entityId: string;
  action: 'Create' | 'Update' | 'Delete' | string;
  userId?: string | null; // who performed the change (may be null for system)
  username?: string | null;
  oldValues?: Record<string, unknown> | null;
  newValues?: Record<string, unknown> | null;
  timestamp: string; // ISO string
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export interface AuditMeta {
  entities: string[];
  actions: string[];
  server?: {
    now: string; // ISO (UTC)
    localNow?: string; // ISO in server local tz
    timezone: string;
    offset: string; // formatted +HH:mm
    culture: string;
  };
}
