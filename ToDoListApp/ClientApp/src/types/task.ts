export type TaskStatus = "Todo" | "InProgress" | "Done";

export interface TaskItem {
  id: string;
  title: string;
  description?: string | null;
  status: TaskStatus;
  deadline?: string | null;
}

export interface TaskDetails {
  title: string;
  description?: string | null;
  deadline?: string | null;
}

export const taskStatuses: TaskStatus[] = ["Todo", "InProgress", "Done"];

export const statusLabels: Record<TaskStatus, string> = {
  Todo: "Todo",
  InProgress: "In progress",
  Done: "Done"
};
