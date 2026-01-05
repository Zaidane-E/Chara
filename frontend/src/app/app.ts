import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TaskService } from './services/task.service';
import { Task } from './models/task.model';

@Component({
  selector: 'app-root',
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly taskService = inject(TaskService);

  tasks = signal<Task[]>([]);
  newTaskTitle = signal('');
  editingTask = signal<Task | null>(null);
  editTitle = signal('');
  loading = signal(false);
  error = signal<string | null>(null);

  completedCount = computed(() => this.tasks().filter(t => t.isCompleted).length);
  pendingCount = computed(() => this.tasks().filter(t => !t.isCompleted).length);

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading.set(true);
    this.error.set(null);
    this.taskService.getTasks().subscribe({
      next: (tasks) => {
        this.tasks.set(tasks);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load tasks. Make sure the backend is running.');
        this.loading.set(false);
        console.error(err);
      }
    });
  }

  addTask(): void {
    const title = this.newTaskTitle().trim();
    if (!title) return;

    this.taskService.createTask({ title }).subscribe({
      next: (task) => {
        this.tasks.update(tasks => [...tasks, task]);
        this.newTaskTitle.set('');
      },
      error: (err) => {
        this.error.set('Failed to create task.');
        console.error(err);
      }
    });
  }

  toggleComplete(task: Task): void {
    this.taskService.updateTask(task.id, {
      title: task.title,
      isCompleted: !task.isCompleted
    }).subscribe({
      next: (updatedTask) => {
        this.tasks.update(tasks =>
          tasks.map(t => t.id === updatedTask.id ? updatedTask : t)
        );
      },
      error: (err) => {
        this.error.set('Failed to update task.');
        console.error(err);
      }
    });
  }

  startEdit(task: Task): void {
    this.editingTask.set(task);
    this.editTitle.set(task.title);
  }

  saveEdit(): void {
    const task = this.editingTask();
    if (!task) return;

    const title = this.editTitle().trim();
    if (!title) return;

    this.taskService.updateTask(task.id, {
      title,
      isCompleted: task.isCompleted
    }).subscribe({
      next: (updatedTask) => {
        this.tasks.update(tasks =>
          tasks.map(t => t.id === updatedTask.id ? updatedTask : t)
        );
        this.cancelEdit();
      },
      error: (err) => {
        this.error.set('Failed to update task.');
        console.error(err);
      }
    });
  }

  cancelEdit(): void {
    this.editingTask.set(null);
    this.editTitle.set('');
  }

  deleteTask(id: number): void {
    this.taskService.deleteTask(id).subscribe({
      next: () => {
        this.tasks.update(tasks => tasks.filter(t => t.id !== id));
      },
      error: (err) => {
        this.error.set('Failed to delete task.');
        console.error(err);
      }
    });
  }
}
