/**
 * Centralised API client.
 *
 * Pattern: Facade — single entry point for all HTTP calls; components never
 *   call fetch() directly.
 * Security: JWT injected from in-memory store (not localStorage) to reduce
 *   XSS exposure.  Token held in module-level variable — cleared on tab close.
 */
import type {
  TaskItem, CreateTaskRequest, UpdateTaskRequest,
  LoginRequest, RegisterRequest, LoginResponse, UserDto,
} from '../types/api';

const BASE = '/api';

// ── Token store (in-memory — not persisted to localStorage) ───────────────
let _token: string | null = null;

export const tokenStore = {
  set: (t: string) => { _token = t; },
  clear: () => { _token = null; },
  get: () => _token,
};

// ── Core fetch wrapper ─────────────────────────────────────────────────────
async function request<T>(
  method: string,
  path: string,
  body?: unknown,
): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };

  if (_token) headers['Authorization'] = `Bearer ${_token}`;

  const res = await fetch(`${BASE}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: res.statusText }));
    throw Object.assign(new Error(err.error ?? 'API error'), { status: res.status, ...err });
  }

  // 204 No Content — return empty
  if (res.status === 204) return undefined as unknown as T;

  return res.json() as Promise<T>;
}

// ── Auth API ──────────────────────────────────────────────────────────────
export const authApi = {
  login:    (data: LoginRequest)    => request<LoginResponse>('POST', '/auth/login', data),
  register: (data: RegisterRequest) => request<UserDto>('POST', '/auth/register', data),
  ping:     ()                      => request<{ message: string }>('GET', '/auth/ping'),
};

// ── Tasks API ─────────────────────────────────────────────────────────────
export const tasksApi = {
  list:      ()                                     => request<TaskItem[]>('GET', '/tasks'),
  getById:   (id: string)                           => request<TaskItem>('GET', `/tasks/${id}`),
  create:    (data: CreateTaskRequest)              => request<TaskItem>('POST', '/tasks', data),
  update:    (id: string, data: UpdateTaskRequest)  => request<TaskItem>('PUT', `/tasks/${id}`, data),
  delete:    (id: string)                           => request<void>('DELETE', `/tasks/${id}`),
};

// ── Users API ─────────────────────────────────────────────────────────────
export const usersApi = {
  me: () => request<UserDto>('GET', '/users/me'),
};
