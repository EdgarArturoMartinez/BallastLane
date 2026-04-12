import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import TaskModal from '../components/TaskModal';
import { tasksApi } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { TaskStatus } from '../types/api';
import type { TaskItem, CreateTaskRequest, UpdateTaskRequest } from '../types/api';

const STATUS_LABEL: Record<TaskStatus, string> = {
  [TaskStatus.Todo]:       'To Do',
  [TaskStatus.InProgress]: 'In Progress',
  [TaskStatus.Done]:       'Done',
};

const STATUS_CLASS: Record<TaskStatus, string> = {
  [TaskStatus.Todo]:       'badge badge-todo',
  [TaskStatus.InProgress]: 'badge badge-inprogress',
  [TaskStatus.Done]:       'badge badge-done',
};

export default function TasksPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const [tasks,    setTasks]    = useState<TaskItem[]>([]);
  const [loading,  setLoading]  = useState(true);
  const [apiError, setApiError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [editing,   setEditing]  = useState<TaskItem | null>(null);

  // ── Load tasks ─────────────────────────────────────────────────────────
  const loadTasks = useCallback(async () => {
    setLoading(true);
    setApiError(null);
    try {
      const data = await tasksApi.list();
      setTasks(data);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Failed to load tasks';
      setApiError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadTasks(); }, [loadTasks]);

  // ── Create ─────────────────────────────────────────────────────────────
  async function handleCreate(data: CreateTaskRequest | UpdateTaskRequest) {
    await tasksApi.create(data as CreateTaskRequest);
    setShowModal(false);
    loadTasks();
  }

  // ── Update ─────────────────────────────────────────────────────────────
  async function handleUpdate(data: CreateTaskRequest | UpdateTaskRequest) {
    if (!editing) return;
    await tasksApi.update(editing.id, data as UpdateTaskRequest);
    setEditing(null);
    loadTasks();
  }

  // ── Delete ─────────────────────────────────────────────────────────────
  async function handleDelete(id: string) {
    if (!confirm('Delete this task?')) return;
    await tasksApi.delete(id);
    loadTasks();
  }

  // ── Logout ─────────────────────────────────────────────────────────────
  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <div className="page-wrapper">
      <header className="page-header">
        <h1 className="page-title">My Tasks</h1>
        <div className="header-actions">
          <span className="user-label">👤 {user?.username}</span>
          <button className="btn btn-ghost btn-sm" onClick={handleLogout}>Sign out</button>
        </div>
      </header>

      <div className="page-content">
        {apiError && <p className="alert alert-error">{apiError}</p>}

        <div className="toolbar">
          <button className="btn btn-primary" onClick={() => setShowModal(true)}>
            + New task
          </button>
        </div>

        {loading ? (
          <p className="loading-text">Loading…</p>
        ) : tasks.length === 0 ? (
          <p className="empty-text">No tasks yet. Create one!</p>
        ) : (
          <div className="task-grid">
            {tasks.map(task => (
              <div key={task.id} className="task-card">
                <div className="task-card-body">
                  <span className={STATUS_CLASS[task.status]}>
                    {STATUS_LABEL[task.status]}
                  </span>
                  <h3 className="task-title">{task.title}</h3>
                  {task.description && (
                    <p className="task-desc">{task.description}</p>
                  )}
                  {task.dueDate && (
                    <p className="task-due">
                      Due: {new Date(task.dueDate).toLocaleDateString()}
                    </p>
                  )}
                </div>
                <div className="task-card-footer">
                  <button
                    className="btn btn-ghost btn-sm"
                    onClick={() => setEditing(task)}
                  >
                    Edit
                  </button>
                  <button
                    className="btn btn-danger btn-sm"
                    onClick={() => handleDelete(task.id)}
                  >
                    Delete
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {showModal && (
        <TaskModal
          onSave={handleCreate}
          onClose={() => setShowModal(false)}
        />
      )}

      {editing && (
        <TaskModal
          task={editing}
          onSave={handleUpdate}
          onClose={() => setEditing(null)}
        />
      )}
    </div>
  );
}
