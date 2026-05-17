import { CalendarDays, CheckCircle2, ChevronDown, Clock3, Edit3, Plus, RefreshCw, Trash2 } from "lucide-react";
import { type CSSProperties, type FormEvent, useEffect, useMemo, useRef, useState } from "react";
import { useAppDispatch, useAppSelector } from "./app/hooks";
import {
  changeTaskStatus,
  clearFieldError,
  createTask,
  deleteTask,
  fetchTasks,
  setActiveFilter,
  updateTask
} from "./features/tasks/tasksSlice";
import type { TaskDetails, TaskItem, TaskStatus } from "./types/task";
import { statusLabels, taskStatuses } from "./types/task";

const emptyForm: TaskDetails = {
  title: "",
  description: "",
  deadline: ""
};

const filters: Array<TaskStatus | "All"> = ["All", ...taskStatuses];

export default function App() {
  const dispatch = useAppDispatch();
  const { activeFilter, error, fieldErrors, items, loading, saving } = useAppSelector((state) => state.tasks);
  const [form, setForm] = useState<TaskDetails>(emptyForm);
  const [clientFieldErrors, setClientFieldErrors] = useState<Record<string, string>>({});
  const [editingTaskId, setEditingTaskId] = useState<string | null>(null);

  useEffect(() => {
    dispatch(fetchTasks());
  }, [dispatch]);

  const visibleTasks = useMemo(() => {
    let filtered = items;
    
    if (activeFilter !== "All") {
      filtered = items.filter((task) => task.status === activeFilter);
    }

    return [...filtered].sort((a, b) => {
      if (!a.deadline) return 1;
      if (!b.deadline) return -1;
      return new Date(a.deadline).getTime() - new Date(b.deadline).getTime();
    });
  }, [activeFilter, items]);

  const totals = useMemo(() => {
    return {
      all: items.length,
      done: items.filter((task) => task.status === "Done").length,
      open: items.filter((task) => task.status !== "Done").length
    };
  }, [items]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const validationErrors = validateForm(form);

      if (Object.keys(validationErrors).length > 0) {
          setClientFieldErrors(validationErrors);
          return;
      }

    const details = normalizeForm(form);

    if (editingTaskId) {
      const result = await dispatch(updateTask({ id: editingTaskId, details }));
      if (updateTask.rejected.match(result)) {
        return;
      }

      setEditingTaskId(null);
    } else {
      const result = await dispatch(createTask(details));
      if (createTask.rejected.match(result)) {
        return;
      }
    }

    setForm(emptyForm);
    setClientFieldErrors({});
  }

  function startEditing(task: TaskItem) {
    setEditingTaskId(task.id);
    setForm({
      title: task.title,
      description: task.description ?? "",
      deadline: toDateInputValue(task.deadline)
    });
  }

  function cancelEditing() {
    setEditingTaskId(null);
    setForm(emptyForm);
    setClientFieldErrors({});
  }

  const titleError = clientFieldErrors.title ?? fieldErrors.title;
    const descriptionError = fieldErrors.description;
    const deadlineError = clientFieldErrors.deadline ?? (fieldErrors as Record<string, string>).deadline;

  return (
    <main className="app-shell">
      <section className="workspace" aria-label="ToDo workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">My Workspace</p>
            <h1>What's on the agenda today?</h1>
          </div>
          <button className="icon-button" type="button" onClick={() => dispatch(fetchTasks())} aria-label="Refresh tasks">
            <RefreshCw size={18} />
          </button>
        </header>

        <section className="summary-band" aria-label="Task summary">
          <div>
            <span>{totals.open}</span>
            <p>Active</p>
          </div>
          <div>
            <span>{totals.done}</span>
            <p>Done</p>
          </div>
          <div>
            <span>{totals.all}</span>
            <p>Total</p>
          </div>
        </section>

        {error ? <p className="alert">{error}</p> : null}

        <div className="content-grid">
          <form className="task-form" onSubmit={handleSubmit} noValidate>
            <div className="form-title">
              <Plus size={20} />
              <h2>{editingTaskId ? "Edit task" : "Create task"}</h2>
            </div>

            <label>
              <span>Title</span>
              <input
                className={titleError ? "invalid" : ""}
                value={form.title}
                onChange={(event) => {
                  setForm((current) => ({ ...current, title: event.target.value }));
                  if (clientFieldErrors.title) {
                    setClientFieldErrors({});
                  }

                  if (fieldErrors.title) {
                    dispatch(clearFieldError("title"));
                  }
                }}
                placeholder="What needs to be done?"
                maxLength={200}
                aria-invalid={Boolean(titleError)}
                aria-describedby={titleError ? "title-error" : undefined}
              />
              {titleError ? (
                <span className="field-error" id="title-error">
                  {titleError}
                </span>
              ) : null}
            </label>

            <label>
              <span>Description</span>
              <div className={`textarea-frame ${descriptionError ? "invalid" : ""}`}>
                <textarea
                  value={form.description ?? ""}
                  onChange={(event) => {
                    setForm((current) => ({ ...current, description: event.target.value }));
                    if (fieldErrors.description) {
                      dispatch(clearFieldError("description"));
                    }
                  }}
                  placeholder="Add any helpful notes, links, or context here..."
                  maxLength={1000}
                  aria-invalid={Boolean(descriptionError)}
                  aria-describedby={descriptionError ? "description-error" : undefined}
                />
              </div>
              {descriptionError ? (
                <span className="field-error" id="description-error">
                  {descriptionError}
                </span>
              ) : null}
            </label>

            <label>
              <span>Deadline</span>
              <div className="date-field">
                <CalendarDays size={18} />
                <input
                  type="date"
                  className={deadlineError ? "invalid" : ""}
                  value={form.deadline ?? ""}
                  onChange={(event) => {
                    setForm((current) => ({ ...current, deadline: event.target.value }));
                    if (clientFieldErrors.deadline) {
                      setClientFieldErrors((current) => {
                        const newErrors = { ...current };
                        delete newErrors.deadline;
                        return newErrors;
                      });
                    }
                  }}
                  aria-invalid={Boolean(deadlineError)}
                  aria-describedby={deadlineError ? "deadline-error" : undefined}
                />
              </div>
              {deadlineError ? (
                <span className="field-error" id="deadline-error" style={{ marginTop: "4px" }}>
                  {deadlineError}
                </span>
              ) : null}
            </label>

            <div className="form-actions">
              {editingTaskId ? (
                <button className="secondary-button" type="button" onClick={cancelEditing}>
                  Cancel
                </button>
              ) : null}
              <button className="primary-button" type="submit" disabled={saving}>
                {editingTaskId ? "Save changes" : "Add task"}
              </button>
            </div>
          </form>

          <section className="task-panel" aria-label="Tasks">
            <div className="panel-head">
              <div>
                <h2>Tasks</h2>
                <p>{loading ? "Loading tasks" : `${visibleTasks.length} shown`}</p>
              </div>
              <div
                className="segmented-control"
                style={{ "--active-index": filters.indexOf(activeFilter) } as CSSProperties}
                aria-label="Filter tasks"
              >
                {filters.map((filter) => (
                  <button
                    key={filter}
                    className={activeFilter === filter ? "active" : ""}
                    type="button"
                    onClick={() => dispatch(setActiveFilter(filter))}
                  >
                    {filter === "All" ? "All" : statusLabels[filter]}
                  </button>
                ))}
              </div>
            </div>

            <div className="task-list">
              {loading ? (
                <TaskSkeleton />
              ) : visibleTasks.length === 0 ? (
                <EmptyState />
              ) : (
                visibleTasks.map((task) => (
                  <TaskRow
                    key={task.id}
                    task={task}
                    onEdit={startEditing}
                    onDelete={(id) => dispatch(deleteTask(id))}
                    onStatusChange={(id, status) => dispatch(changeTaskStatus({ id, status }))}
                  />
                ))
              )}
            </div>
          </section>
        </div>
      </section>
    </main>
  );
}

function TaskRow({
  task,
  onDelete,
  onEdit,
  onStatusChange
}: {
  task: TaskItem;
  onDelete: (id: string) => void;
  onEdit: (task: TaskItem) => void;
  onStatusChange: (id: string, status: TaskStatus) => void;
}) {
  return (
    <article className={`task-row status-${task.status.toLowerCase()}`}>
      <div className="status-mark" aria-hidden="true">
        {task.status === "Done" ? <CheckCircle2 size={18} /> : <Clock3 size={18} />}
      </div>
      <div className="task-body">
              <div className="task-title-line">
                  <h3>{task.title}</h3>
                  <TaskStatusSelect
                      value={task.status}
                      onChange={(newStatus) => onStatusChange(task.id, newStatus)}
                      title={task.title}
                  />
              </div>
        {task.description ? <p>{task.description}</p> : null}
        <span className={`deadline ${isUrgentDeadline(task.deadline) && task.status !== "Done" ? "urgent" : ""}`}>
          <CalendarDays size={15} />
          {formatDeadline(task.deadline)}
        </span>
      </div>
      <div className="row-actions">
        <button className="icon-button" type="button" onClick={() => onEdit(task)} aria-label={`Edit ${task.title}`}>
          <Edit3 size={17} />
        </button>
        <button className="icon-button danger" type="button" onClick={() => onDelete(task.id)} aria-label={`Delete ${task.title}`}>
          <Trash2 size={17} />
        </button>
      </div>
    </article>
  );
}

function TaskSkeleton() {
  return (
    <>
      <div className="skeleton-row" />
      <div className="skeleton-row short" />
      <div className="skeleton-row" />
    </>
  );
}

function TaskStatusSelect({
    value,
    onChange,
    title
}: {
    value: TaskStatus;
    onChange: (status: TaskStatus) => void;
    title: string;
}) {
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    return (
        <div className="custom-select" ref={dropdownRef}>
            <button
                type="button"
                className="custom-select-trigger"
                onClick={() => setIsOpen(!isOpen)}
                aria-label={`Change status for ${title}`}
                aria-expanded={isOpen}
            >
                <span>{statusLabels[value]}</span>
                <ChevronDown size={16} className={`chevron ${isOpen ? "open" : ""}`} />
            </button>

            {isOpen && (
                <ul className="custom-select-menu">
                    {taskStatuses.map((status) => (
                        <li key={status}>
                            <button
                                type="button"
                                className={`custom-select-option ${status === value ? "selected" : ""}`}
                                onClick={() => {
                                    onChange(status);
                                    setIsOpen(false);
                                }}
                            >
                                {statusLabels[status]}
                            </button>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

function EmptyState() {
  return (
    <div className="empty-state">
      <CheckCircle2 size={28} />
      <h3>All clear!</h3>
      <p>You're fully caught up. Enjoy the break or add a new task.</p>
    </div>
  );
}

function normalizeForm(details: TaskDetails): TaskDetails {
  return {
    title: details.title.trim(),
    description: details.description?.trim() || null,
    deadline: details.deadline ? new Date(`${details.deadline}T12:00:00`).toISOString() : null
  };
}

function validateForm(details: TaskDetails) {
    const errors: Record<string, string> = {};

    if (!details.title.trim()) {
        errors.title = "Add a title before creating the task.";
    }

    if (details.deadline) {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const deadlineDate = new Date(`${details.deadline}T00:00:00`);

        if (deadlineDate < today) {
            errors.deadline = "Deadline cannot be in the past.";
        }
    }

    return errors;
}

function toDateInputValue(deadline?: string | null) {
  if (!deadline) {
    return "";
  }

  return deadline.slice(0, 10);
}

function formatDeadline(deadline?: string | null) {
  if (!deadline) {
    return "No deadline";
  }

  return new Intl.DateTimeFormat("en", {
    month: "short",
    day: "numeric",
    year: "numeric"
  }).format(new Date(deadline));
}

function isUrgentDeadline(deadline?: string | null) {
  if (!deadline) {
    return false;
  }

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  
  const dlDate = new Date(deadline);
  dlDate.setHours(0, 0, 0, 0);

  const diffTime = dlDate.getTime() - today.getTime();
  const diffDays = Math.round(diffTime / (1000 * 60 * 60 * 24));

  return diffDays <= 3;
}