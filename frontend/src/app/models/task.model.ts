export interface Task {
  id: number;
  title: string;
  isCompleted: boolean;
}

export interface CreateTask {
  title: string;
}

export interface UpdateTask {
  title: string;
  isCompleted: boolean;
}
