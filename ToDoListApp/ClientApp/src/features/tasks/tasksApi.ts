import type { TaskDetails, TaskItem, TaskStatus } from "../../types/task";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "";

export interface ApiErrorPayload {
  message: string;
  fieldErrors: Record<string, string>;
  status?: number;
}

class ApiError extends Error {
  constructor(public readonly payload: ApiErrorPayload) {
    super(payload.message);
  }
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  let response: Response;

  try {
    response = await fetch(`${apiBaseUrl}${path}`, {
      headers: {
        "Content-Type": "application/json",
        ...options?.headers
      },
      ...options
    });
  } catch {
    throw new ApiError({
      message: "Could not reach the backend. Check that the API is running.",
      fieldErrors: {}
    });
  }

  if (!response.ok) {
    throw new ApiError(await readError(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

async function readError(response: Response): Promise<ApiErrorPayload> {
  try {
    const problem = await response.json();
    const fieldErrors = readFieldErrors(problem?.errors);

    if (Object.keys(fieldErrors).length > 0) {
      return {
        message: "Some fields need attention.",
        fieldErrors,
        status: response.status
      };
    }

    if (problem?.errors) {
      const firstError = Object.values(problem.errors).flat()[0];
      if (typeof firstError === "string") {
        return {
          message: firstError,
          fieldErrors: {},
          status: response.status
        };
      }
    }

    if (typeof problem?.detail === "string") {
      return {
        message: problem.detail,
        fieldErrors: {},
        status: response.status
      };
    }
  } catch {
    return {
      message: "Request failed. Please try again.",
      fieldErrors: {},
      status: response.status
    };
  }

  return {
    message: `Request failed with status ${response.status}.`,
    fieldErrors: {},
    status: response.status
  };
}

function readFieldErrors(errors: unknown): Record<string, string> {
  if (!errors || typeof errors !== "object") {
    return {};
  }

  return Object.entries(errors).reduce<Record<string, string>>((result, [field, messages]) => {
    if (Array.isArray(messages) && typeof messages[0] === "string") {
      result[field.toLowerCase()] = messages[0];
    }

    return result;
  }, {});
}

export function toApiErrorPayload(error: unknown): ApiErrorPayload {
  if (error instanceof ApiError) {
    return error.payload;
  }

  if (error instanceof Error) {
    return {
      message: error.message,
      fieldErrors: {}
    };
  }

  return {
    message: "Something went wrong. Please try again.",
    fieldErrors: {}
  };
}

export const tasksApi = {
  getTasks: () => request<TaskItem[]>("/api/tasks"),
  createTask: (details: TaskDetails) =>
    request<string>("/api/tasks", {
      method: "POST",
      body: JSON.stringify(details)
    }),
  updateTask: (id: string, details: TaskDetails) =>
    request<void>(`/api/tasks/${id}`, {
      method: "PUT",
      body: JSON.stringify(details)
    }),
  changeStatus: (id: string, status: TaskStatus) =>
    request<void>(`/api/tasks/${id}/status`, {
      method: "PATCH",
      body: JSON.stringify({ status })
    }),
  deleteTask: (id: string) =>
    request<void>(`/api/tasks/${id}`, {
      method: "DELETE"
    })
};
