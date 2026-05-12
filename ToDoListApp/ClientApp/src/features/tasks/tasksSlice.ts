import { createAsyncThunk, createSlice, isAnyOf } from "@reduxjs/toolkit";
import { tasksApi, toApiErrorPayload, type ApiErrorPayload } from "./tasksApi";
import type { TaskDetails, TaskItem, TaskStatus } from "../../types/task";

type FieldErrors = Record<string, string>;

interface TasksState {
  items: TaskItem[];
  loading: boolean;
  saving: boolean;
  error: string | null;
  fieldErrors: FieldErrors;
  activeFilter: TaskStatus | "All";
}

const initialState: TasksState = {
  items: [],
  loading: false,
  saving: false,
  error: null,
  fieldErrors: {},
  activeFilter: "All"
};

export const fetchTasks = createAsyncThunk<TaskItem[], void, { rejectValue: ApiErrorPayload }>(
  "tasks/fetchTasks",
  async (_, { rejectWithValue }) => {
    try {
      return await tasksApi.getTasks();
    } catch (error) {
      return rejectWithValue(toApiErrorPayload(error));
    }
  }
);

export const createTask = createAsyncThunk<TaskItem[], TaskDetails, { rejectValue: ApiErrorPayload }>(
  "tasks/createTask",
  async (details, { rejectWithValue }) => {
    try {
      await tasksApi.createTask(details);
      return await tasksApi.getTasks();
    } catch (error) {
      return rejectWithValue(toApiErrorPayload(error));
    }
  }
);

export const updateTask = createAsyncThunk<
  TaskItem[],
  { id: string; details: TaskDetails },
  { rejectValue: ApiErrorPayload }
>("tasks/updateTask", async ({ id, details }, { rejectWithValue }) => {
  try {
    await tasksApi.updateTask(id, details);
    return await tasksApi.getTasks();
  } catch (error) {
    return rejectWithValue(toApiErrorPayload(error));
  }
});

export const changeTaskStatus = createAsyncThunk<
  TaskItem[],
  { id: string; status: TaskStatus },
  { rejectValue: ApiErrorPayload }
>("tasks/changeTaskStatus", async ({ id, status }, { rejectWithValue }) => {
  try {
    await tasksApi.changeStatus(id, status);
    return await tasksApi.getTasks();
  } catch (error) {
    return rejectWithValue(toApiErrorPayload(error));
  }
});

export const deleteTask = createAsyncThunk<TaskItem[], string, { rejectValue: ApiErrorPayload }>(
  "tasks/deleteTask",
  async (id, { rejectWithValue }) => {
    try {
      await tasksApi.deleteTask(id);
      return await tasksApi.getTasks();
    } catch (error) {
      return rejectWithValue(toApiErrorPayload(error));
    }
  }
);

const mutationThunks = [createTask, updateTask, changeTaskStatus, deleteTask] as const;

const tasksSlice = createSlice({
  name: "tasks",
  initialState,
  reducers: {
    setActiveFilter(state, action: { payload: TasksState["activeFilter"] }) {
      state.activeFilter = action.payload;
    },
    clearError(state) {
      state.error = null;
      state.fieldErrors = {};
    },
    clearFieldError(state, action: { payload: string }) {
      delete state.fieldErrors[action.payload.toLowerCase()];
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchTasks.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchTasks.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
      })
      .addCase(fetchTasks.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload?.message ?? action.error.message ?? "Could not load tasks.";
      })
      .addMatcher(isAnyOf(...mutationThunks.map((thunk) => thunk.pending)), (state) => {
        state.saving = true;
        state.error = null;
        state.fieldErrors = {};
      })
      .addMatcher(isAnyOf(...mutationThunks.map((thunk) => thunk.fulfilled)), (state, action) => {
        state.saving = false;
        state.items = action.payload;
      })
      .addMatcher(isAnyOf(...mutationThunks.map((thunk) => thunk.rejected)), (state, action) => {
        const payload = action.payload as ApiErrorPayload | undefined;
        const fieldErrors = payload?.fieldErrors ?? {};

        state.saving = false;
        state.fieldErrors = fieldErrors;
        state.error = Object.keys(fieldErrors).length > 0
          ? null
          : payload?.message ?? action.error.message ?? "Could not save task.";
      });
  }
});

export const { clearError, clearFieldError, setActiveFilter } = tasksSlice.actions;
export default tasksSlice.reducer;
