import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import TaskModal from '../components/TaskModal';
import Header from '../components/Header';
import toast, { Toaster } from 'react-hot-toast';
import { tasksApi } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { TaskStatus } from '../types/api';
import type { TaskItem, CreateTaskRequest, UpdateTaskRequest } from '../types/api';

const COLUMNS: { key: TaskStatus; label: string; dot: string; bg: string }[] = [
  { key: TaskStatus.Todo,       label: 'To Do',       dot: 'bg-amber-400',  bg: 'bg-amber-50/60 dark:bg-amber-900/10 border-amber-200 dark:border-amber-800/40' },
  { key: TaskStatus.InProgress, label: 'In Progress', dot: 'bg-blue-500',   bg: 'bg-blue-50/60 dark:bg-blue-900/10 border-blue-200 dark:border-blue-800/40' },
  { key: TaskStatus.Done,       label: 'Done',        dot: 'bg-emerald-500', bg: 'bg-emerald-50/60 dark:bg-emerald-900/10 border-emerald-200 dark:border-emerald-800/40' },
];

const BADGE: Record<TaskStatus, string> = {
  [TaskStatus.Todo]:       'bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300',
  [TaskStatus.InProgress]: 'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300',
  [TaskStatus.Done]:       'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-300',
};

const COL_TEXT: Record<TaskStatus, string> = {
  [TaskStatus.Todo]:       'text-amber-600 dark:text-amber-400',
  [TaskStatus.InProgress]: 'text-blue-600 dark:text-blue-400',
  [TaskStatus.Done]:       'text-emerald-600 dark:text-emerald-400',
};

function isOverdue(dueDate: string | null): boolean {
  if (!dueDate) return false;
  return new Date(dueDate) < new Date();
}

export default function TasksPage() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const [tasks,     setTasks]     = useState<TaskItem[]>([]);
  const [loading,   setLoading]   = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editing,   setEditing]   = useState<TaskItem | null>(null);

  const loadTasks = useCallback(async () => {
    setLoading(true);
    try {
      const data = await tasksApi.list();
      setTasks(data);
    } catch (e: unknown) {
      toast.error(e instanceof Error ? e.message : 'Failed to load tasks');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadTasks(); }, [loadTasks]);

  async function handleCreate(data: CreateTaskRequest | UpdateTaskRequest) {
    try {
      await tasksApi.create(data as CreateTaskRequest);
      setShowModal(false);
      await loadTasks();
      toast.success('Task created successfully');
    } catch (e: unknown) {
      toast.error(e instanceof Error ? e.message : 'Failed to create task');
    }
  }

  async function handleUpdate(data: CreateTaskRequest | UpdateTaskRequest) {
    if (!editing) return;
    try {
      await tasksApi.update(editing.id, data as UpdateTaskRequest);
      setEditing(null);
      await loadTasks();
      toast.success('Task updated');
    } catch (e: unknown) {
      toast.error(e instanceof Error ? e.message : 'Failed to update task');
    }
  }

  async function handleDelete(id: string) {
    if (!confirm('Are you sure you want to delete this task?')) return;
    try {
      await tasksApi.delete(id);
      await loadTasks();
      toast('Task deleted', { icon: 'ðŸ—‘ï¸' });
    } catch (e: unknown) {
      toast.error(e instanceof Error ? e.message : 'Failed to delete task');
    }
  }

  function handleLogout() {
    logout();
    navigate('/login');
  }

  const totalDone = tasks.filter(t => t.status === TaskStatus.Done).length;
  const progress = tasks.length ? Math.round((totalDone / tasks.length) * 100) : 0;

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-950 text-gray-900 dark:text-gray-100">
      <Header />

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">

        {/* Page header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white">My Tasks</h1>
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
              {loading ? 'Loadingâ€¦' : `${tasks.length} task${tasks.length !== 1 ? 's' : ''} Â· ${totalDone} completed`}
            </p>
          </div>
          <button onClick={() => setShowModal(true)}
            className="inline-flex items-center gap-2 px-5 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-semibold rounded-xl transition-all duration-150 shadow-sm hover:shadow-indigo-200 dark:hover:shadow-indigo-900/40 self-start sm:self-auto">
            <svg className="w-4 h-4" viewBox="0 0 20 20" fill="currentColor">
              <path d="M10.75 4.75a.75.75 0 00-1.5 0v4.5h-4.5a.75.75 0 000 1.5h4.5v4.5a.75.75 0 001.5 0v-4.5h4.5a.75.75 0 000-1.5h-4.5v-4.5z"/>
            </svg>
            New task
          </button>
        </div>

        {/* Progress bar */}
        {!loading && tasks.length > 0 && (
          <div className="mb-8 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-600 dark:text-gray-400">Overall progress</span>
              <span className="text-sm font-bold text-indigo-600 dark:text-indigo-400">{progress}%</span>
            </div>
            <div className="h-2 bg-gray-100 dark:bg-gray-800 rounded-full overflow-hidden">
              <div className="h-full bg-gradient-to-r from-indigo-500 to-emerald-500 rounded-full transition-all duration-700"
                style={{ width: `${progress}%` }} />
            </div>
          </div>
        )}

        {/* Loading skeleton */}
        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {[...Array(3)].map((_, i) => (
              <div key={i} className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 p-4 animate-pulse">
                <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-20 mb-4" />
                {[...Array(2)].map((_, j) => (
                  <div key={j} className="mb-3 p-3 rounded-xl bg-gray-50 dark:bg-gray-800">
                    <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2" />
                    <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2" />
                  </div>
                ))}
              </div>
            ))}
          </div>
        ) : tasks.length === 0 ? (
          /* Empty state */
          <div className="text-center py-24">
            <div className="inline-flex items-center justify-center w-16 h-16 bg-indigo-50 dark:bg-indigo-900/30 rounded-2xl mb-4">
              <svg className="w-8 h-8 text-indigo-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
                <path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" strokeLinecap="round" strokeLinejoin="round"/>
              </svg>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-1">No tasks yet</h3>
            <p className="text-gray-500 dark:text-gray-400 mb-6 max-w-xs mx-auto text-sm">Create your first task and start tracking your progress.</p>
            <button onClick={() => setShowModal(true)}
              className="inline-flex items-center gap-2 px-5 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-semibold rounded-xl transition">
              <svg className="w-4 h-4" viewBox="0 0 20 20" fill="currentColor">
                <path d="M10.75 4.75a.75.75 0 00-1.5 0v4.5h-4.5a.75.75 0 000 1.5h4.5v4.5a.75.75 0 001.5 0v-4.5h4.5a.75.75 0 000-1.5h-4.5v-4.5z"/>
              </svg>
              Create first task
            </button>
          </div>
        ) : (
          /* Kanban columns */
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {COLUMNS.map(col => {
              const colTasks = tasks.filter(t => t.status === col.key);
              return (
                <div key={col.key} className={`rounded-2xl border p-4 ${col.bg}`}>
                  {/* Column header */}
                  <div className="flex items-center justify-between mb-4">
                    <div className="flex items-center gap-2">
                      <div className={`w-2.5 h-2.5 rounded-full ${col.dot}`} />
                      <span className={`text-sm font-semibold ${COL_TEXT[col.key]}`}>{col.label}</span>
                    </div>
                    <span className="text-xs font-bold text-gray-400 bg-white dark:bg-gray-800 px-2 py-0.5 rounded-full border border-gray-200 dark:border-gray-600">
                      {colTasks.length}
                    </span>
                  </div>

                  {/* Cards */}
                  <div className="space-y-3">
                    {colTasks.length === 0 ? (
                      <div className="text-center py-8 text-xs text-gray-400 dark:text-gray-500">No tasks here</div>
                    ) : colTasks.map(task => (
                      <div key={task.id}
                        className="group bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm hover:shadow-md transition-all duration-150 cursor-pointer"
                        onClick={() => setEditing(task)}>
                        <div className="flex items-start justify-between gap-2 mb-2">
                          <h3 className="text-sm font-semibold text-gray-900 dark:text-white leading-snug line-clamp-2 flex-1">{task.title}</h3>
                          <button
                            className="flex-shrink-0 opacity-0 group-hover:opacity-100 p-1 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/30 transition-all"
                            onClick={e => { e.stopPropagation(); handleDelete(task.id); }}
                            title="Delete task"
                            aria-label="Delete task">
                            <svg className="w-3.5 h-3.5" viewBox="0 0 20 20" fill="currentColor">
                              <path fillRule="evenodd" d="M8.75 1A2.75 2.75 0 006 3.75v.443c-.795.077-1.584.176-2.365.298a.75.75 0 10.23 1.482l.149-.022.841 10.518A2.75 2.75 0 007.596 19h4.807a2.75 2.75 0 002.742-2.53l.841-10.52.149.023a.75.75 0 00.23-1.482A41.03 41.03 0 0014 4.193V3.75A2.75 2.75 0 0011.25 1h-2.5zM10 4c.84 0 1.673.025 2.5.075V3.75c0-.69-.56-1.25-1.25-1.25h-2.5c-.69 0-1.25.56-1.25 1.25v.325C8.327 4.025 9.16 4 10 4zM8.58 7.72a.75.75 0 00-1.5.06l.3 7.5a.75.75 0 101.5-.06l-.3-7.5zm4.34.06a.75.75 0 10-1.5-.06l-.3 7.5a.75.75 0 101.5.06l.3-7.5z" clipRule="evenodd"/>
                            </svg>
                          </button>
                        </div>

                        {task.description && (
                          <p className="text-xs text-gray-500 dark:text-gray-400 mb-3 line-clamp-2 leading-relaxed">{task.description}</p>
                        )}

                        <div className="flex items-center gap-2 flex-wrap">
                          <span className={`inline-flex items-center text-xs font-medium px-2 py-0.5 rounded-full ${BADGE[task.status]}`}>
                            {col.label}
                          </span>
                          {task.dueDate && (
                            <span className={`inline-flex items-center gap-1 text-xs px-2 py-0.5 rounded-full font-medium ${
                              isOverdue(task.dueDate) && task.status !== TaskStatus.Done
                                ? 'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300'
                                : 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400'
                            }`}>
                              <svg className="w-3 h-3" viewBox="0 0 16 16" fill="currentColor">
                                <path d="M5.5 1a.5.5 0 01.5.5V2h4v-.5a.5.5 0 011 0V2A2 2 0 0113 4v8a2 2 0 01-2 2H5a2 2 0 01-2-2V4a2 2 0 012-2v-.5a.5.5 0 01.5-.5zM5 5a.5.5 0 000 1h6a.5.5 0 000-1H5z"/>
                              </svg>
                              {new Date(task.dueDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
                            </span>
                          )}
                        </div>
                      </div>
                    ))}

                    {/* Add task quick button per column */}
                    <button onClick={() => setShowModal(true)}
                      className="w-full flex items-center gap-2 px-3 py-2 rounded-xl text-xs text-gray-400 dark:text-gray-500 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-white dark:hover:bg-gray-900 border border-dashed border-gray-200 dark:border-gray-700 hover:border-indigo-300 dark:hover:border-indigo-700 transition-all">
                      <svg className="w-3.5 h-3.5" viewBox="0 0 20 20" fill="currentColor">
                        <path d="M10.75 4.75a.75.75 0 00-1.5 0v4.5h-4.5a.75.75 0 000 1.5h4.5v4.5a.75.75 0 001.5 0v-4.5h4.5a.75.75 0 000-1.5h-4.5v-4.5z"/>
                      </svg>
                      Add task
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Sign out (bottom mobile) */}
      <div className="sm:hidden flex justify-center py-4">
        <button onClick={handleLogout} className="text-xs text-gray-400 hover:text-red-500 transition-colors">Sign out</button>
      </div>

      {showModal && (
        <TaskModal onSave={handleCreate} onClose={() => setShowModal(false)} />
      )}
      {editing && (
        <TaskModal task={editing} onSave={handleUpdate} onClose={() => setEditing(null)} />
      )}

      <Toaster
        position="top-right"
        toastOptions={{
          className: 'text-sm font-medium',
          style: { borderRadius: '12px', padding: '12px 16px', boxShadow: '0 8px 24px rgba(0,0,0,0.12)' },
          success: { iconTheme: { primary: '#22c55e', secondary: '#fff' } },
          error:   { iconTheme: { primary: '#ef4444', secondary: '#fff' } },
        }}
      />
    </div>
  );
}

