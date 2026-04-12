import { useEffect, useRef, useState } from 'react';
import type { TaskItem, CreateTaskRequest, UpdateTaskRequest } from '../types/api';
import { TaskStatus } from '../types/api';

interface Props {
  task?: TaskItem | null;
  onSave:  (data: CreateTaskRequest | UpdateTaskRequest) => void;
  onClose: () => void;
}

const STATUS_OPTIONS: { value: TaskStatus; label: string; color: string; dot: string }[] = [
  { value: TaskStatus.Todo,       label: 'To Do',       color: 'text-amber-700 bg-amber-50 border-amber-200',   dot: 'bg-amber-400' },
  { value: TaskStatus.InProgress, label: 'In Progress', color: 'text-blue-700 bg-blue-50 border-blue-200',     dot: 'bg-blue-500' },
  { value: TaskStatus.Done,       label: 'Done',        color: 'text-emerald-700 bg-emerald-50 border-emerald-200', dot: 'bg-emerald-500' },
];

function today(): string {
  return new Date().toISOString().slice(0, 10);
}

export default function TaskModal({ task, onSave, onClose }: Props) {
  const isEdit = !!task;
  const firstInputRef = useRef<HTMLInputElement>(null);
  const [visible, setVisible] = useState(false);

  const [form, setForm] = useState({
    title:       task?.title       ?? '',
    description: task?.description ?? '',
    status:      task?.status      ?? TaskStatus.Todo,
    dueDate:     task?.dueDate ? task.dueDate.slice(0, 10) : today(),
  });
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  // Animate in
  useEffect(() => {
    const t = requestAnimationFrame(() => setVisible(true));
    return () => cancelAnimationFrame(t);
  }, []);

  useEffect(() => { firstInputRef.current?.focus(); }, []);

  // Close on Escape
  useEffect(() => {
    // Intentionally do not close modal on Escape to require explicit Cancel/Close
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function handleClose() {
    setVisible(false);
    setTimeout(onClose, 200); // wait for fade-out
  }

  function handleChange(e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) {
    setError(null);
    setForm(f => ({ ...f, [e.target.name]: e.target.value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!form.title.trim()) { setError('Title is required'); return; }
    setSaving(true);
    try {
      await onSave({
        title:       form.title.trim(),
        description: form.description.trim(),
        status:      form.status,
        dueDate:     form.dueDate || null,
      });
    } finally {
      setSaving(false);
    }
  }

  return (
    <div
      className={`fixed inset-0 z-50 flex items-end sm:items-center justify-center p-0 sm:p-4 transition-all duration-200 ${visible ? 'bg-black/50 backdrop-blur-sm' : 'bg-transparent'}`}
      role="dialog"
      aria-modal="true"
      aria-label={isEdit ? 'Edit task' : 'New task'}>

      <div
        className={`w-full sm:max-w-lg bg-white dark:bg-gray-900 sm:rounded-2xl rounded-t-2xl shadow-2xl border border-gray-200 dark:border-gray-700 transition-all duration-200 ${visible ? 'translate-y-0 opacity-100 scale-100' : 'translate-y-8 opacity-0 sm:scale-95'}`}
        onClick={e => e.stopPropagation()}>

        {/* Header */}
        <div className="flex items-center justify-between px-6 pt-6 pb-0">
          <div className="flex items-center gap-3">
            <div className={`w-8 h-8 rounded-xl flex items-center justify-center ${isEdit ? 'bg-indigo-50 dark:bg-indigo-900/40' : 'bg-emerald-50 dark:bg-emerald-900/40'}`}>
              {isEdit
                ? <svg className="w-4 h-4 text-indigo-600 dark:text-indigo-400" viewBox="0 0 20 20" fill="currentColor"><path d="M2.695 14.763l-1.262 3.154a.5.5 0 00.65.65l3.155-1.262a4 4 0 001.343-.885L17.5 5.5a2.121 2.121 0 00-3-3L3.58 13.42a4 4 0 00-.885 1.343z"/></svg>
                : <svg className="w-4 h-4 text-emerald-600 dark:text-emerald-400" viewBox="0 0 20 20" fill="currentColor"><path d="M10.75 4.75a.75.75 0 00-1.5 0v4.5h-4.5a.75.75 0 000 1.5h4.5v4.5a.75.75 0 001.5 0v-4.5h4.5a.75.75 0 000-1.5h-4.5v-4.5z"/></svg>
              }
            </div>
            <h2 className="text-base font-bold text-gray-900 dark:text-white">{isEdit ? 'Edit task' : 'New task'}</h2>
          </div>
          <button onClick={handleClose}
            className="p-2 rounded-xl text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            aria-label="Close">
            <svg className="w-4 h-4" viewBox="0 0 20 20" fill="currentColor">
              <path d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z"/>
            </svg>
          </button>
        </div>

        {/* Divider */}
        <div className="h-px bg-gray-100 dark:bg-gray-800 mx-6 mt-4 mb-5" />

        {/* Form */}
        <form onSubmit={handleSubmit} noValidate className="px-6 pb-6 space-y-4">
          {error && (
            <div className="flex items-center gap-2.5 bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700/50 text-red-700 dark:text-red-300 rounded-xl px-4 py-3 text-sm">
              <svg className="w-4 h-4 flex-shrink-0" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z" clipRule="evenodd"/>
              </svg>
              {error}
            </div>
          )}

          {/* Title */}
          <div>
            <label htmlFor="m-title" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
              Title <span className="text-red-500">*</span>
            </label>
            <input
              ref={firstInputRef} id="m-title" name="title" type="text" required
              value={form.title} onChange={handleChange}
              placeholder="What needs to be done?"
              className="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition text-sm"
            />
          </div>

          {/* Description */}
          <div>
            <label htmlFor="m-description" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">Description</label>
            <textarea
              id="m-description" name="description" rows={3}
              value={form.description} onChange={handleChange}
              placeholder="Add more details…"
              className="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition text-sm resize-none"
            />
          </div>

          {/* Status + Due Date row */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="m-status" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">Status</label>
              {isEdit ? (
                <div className="space-y-2">
                  {STATUS_OPTIONS.map(opt => (
                    <label key={opt.value}
                      className={`flex items-center gap-2.5 px-3 py-2 rounded-xl border text-sm font-medium cursor-pointer transition-all ${
                        form.status === opt.value
                          ? `${opt.color} ring-2 ring-offset-1 ring-indigo-400`
                          : 'border-gray-200 dark:border-gray-700 text-gray-600 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800'
                      }`}>
                      <input type="radio" name="status" value={opt.value} checked={form.status === opt.value} onChange={handleChange} className="sr-only"/>
                      <div className={`w-2 h-2 rounded-full ${opt.dot}`} />
                      {opt.label}
                    </label>
                  ))}
                </div>
              ) : (
                <select id="m-status" name="status" value={form.status} onChange={handleChange}
                  className="w-full px-3 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 transition text-sm">
                  {STATUS_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                </select>
              )}
            </div>

            <div>
              <label htmlFor="m-dueDate" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">Due date</label>
              <input
                id="m-dueDate" name="dueDate" type="date"
                value={form.dueDate} onChange={handleChange}
                className="w-full px-3 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 transition text-sm"
              />
            </div>
          </div>

          {/* Footer actions */}
          <div className="flex items-center justify-end gap-3 pt-2">
            <button type="button" onClick={handleClose}
              className="px-4 py-2.5 rounded-xl text-sm font-medium text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors border border-gray-200 dark:border-gray-700">
              Cancel
            </button>
            <button type="submit" disabled={saving}
              className="inline-flex items-center gap-2 px-5 py-2.5 bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-semibold rounded-xl transition-all shadow-sm">
              {saving
                ? <><svg className="animate-spin w-4 h-4" viewBox="0 0 24 24" fill="none"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"/></svg>Saving…</>
                : isEdit ? 'Save changes' : 'Create task'
              }
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
