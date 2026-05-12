import { CalendarDays, CheckCircle2, Clock3, Edit3, Plus, RefreshCw, Trash2 } from "lucide-react";
import { type CSSProperties, type FormEvent, useEffect, useMemo, useState } from "react";
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
    if (activeFilter === "All") {
      return items;
    }

    return items.filter((task) => task.status === activeFilter);
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

    if (validationErrors.title) {
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

  return (
    <main className="app-shell">
      <section className="workspace" aria-label="ToDo workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">ToDo List</p>
            <h1>Plan the next useful thing.</h1>
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
                placeholder="Prepare frontend contract"
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
                  placeholder="Add the details that make this easy to resume."
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
                  value={form.deadline ?? ""}
                  onChange={(event) => setForm((current) => ({ ...current, deadline: event.target.value }))}
                />
              </div>
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
          <select
            value={task.status}
            onChange={(event) => onStatusChange(task.id, event.target.value as TaskStatus)}
            aria-label={`Change status for ${task.title}`}
          >
            {taskStatuses.map((status) => (
              <option key={status} value={status}>
                {statusLabels[status]}
              </option>
            ))}
          </select>
        </div>
        {task.description ? <p>{task.description}</p> : null}
        <span className="deadline">
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

function EmptyState() {
  return (
    <div className="empty-state">
      <CheckCircle2 size={28} />
      <h3>No tasks in this view</h3>
      <p>Create one or switch the filter.</p>
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
  if (!details.title.trim()) {
    return {
      title: "Add a title before creating the task."
    };
  }

  return {};
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
