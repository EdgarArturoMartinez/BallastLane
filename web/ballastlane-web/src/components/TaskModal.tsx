import { useEffect, useRef, useState } from 'react';
import type { TaskItem, CreateTaskRequest, UpdateTaskRequest } from '../types/api';
import { TaskStatus } from '../types/api';

interface Props {
  task?: TaskItem | null;       // null / undefined → create mode
  onSave:  (data: CreateTaskRequest | UpdateTaskRequest) => void;
  onClose: () => void;
}

const STATUS_OPTIONS: { value: TaskStatus; label: string }[] = [
  { value: TaskStatus.Todo,       label: 'To Do'      },
  { value: TaskStatus.InProgress, label: 'In Progress'},
  { value: TaskStatus.Done,       label: 'Done'       },
];

function today(): string {
  return new Date().toISOString().slice(0, 10);
}

export default function TaskModal({ task, onSave, onClose }: Props) {
  const isEdit = !!task;
  const firstInputRef = useRef<HTMLInputElement>(null);

  const [form, setForm] = useState({
    title: task?.title ?? '',
    description: task?.description ?? '',
    status: task?.status ?? TaskStatus.Todo,
    dueDate: task?.dueDate ? task.dueDate.slice(0, 10) : today(),
  });
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    firstInputRef.current?.focus();
  }, []);

  // Close on Escape
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose();
    }
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [onClose]);

  function handleChange(
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>,
  ) {
    setError(null);
    setForm(f => ({ ...f, [e.target.name]: e.target.value }));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!form.title.trim()) {
      setError('Title is required');
      return;
    }

    const payload = {
      title: form.title.trim(),
      description: form.description.trim(),
      status: form.status,
      dueDate: form.dueDate || null,
    };
    onSave(payload);
  }

  return (
    // Back-drop
    <div className="modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="modal-card" onClick={e => e.stopPropagation()}>
        <header className="modal-header">
          <h2>{isEdit ? 'Edit task' : 'New task'}</h2>
          <button className="btn-icon" aria-label="Close" onClick={onClose}>✕</button>
        </header>

        <form onSubmit={handleSubmit} noValidate>
          {error && <p className="alert alert-error">{error}</p>}

          <div className="field">
            <label htmlFor="m-title">Title *</label>
            <input
              ref={firstInputRef}
              id="m-title"
              name="title"
              type="text"
              required
              value={form.title}
              onChange={handleChange}
            />
          </div>

          <div className="field">
            <label htmlFor="m-description">Description</label>
            <textarea
              id="m-description"
              name="description"
              rows={3}
              value={form.description}
              onChange={handleChange}
            />
          </div>

          <div className="field-row">
            <div className="field">
              <label htmlFor="m-status">Status</label>
              <select id="m-status" name="status" value={form.status} onChange={handleChange}>
                {STATUS_OPTIONS.map(o => (
                  <option key={o.value} value={o.value}>{o.label}</option>
                ))}
              </select>
            </div>

            <div className="field">
              <label htmlFor="m-dueDate">Due date</label>
              <input
                id="m-dueDate"
                name="dueDate"
                type="date"
                value={form.dueDate}
                onChange={handleChange}
              />
            </div>
          </div>

          <div className="modal-footer">
            <button className="btn btn-ghost" type="button" onClick={onClose}>Cancel</button>
            <button className="btn btn-primary" type="submit">
              {isEdit ? 'Save changes' : 'Create task'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
