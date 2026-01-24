import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { GuestHabitService } from '../../services/guest-habit.service';
import { AccountabilitySettings, AccountabilityLog, Penalty, Reward } from '../../models/habit.model';

@Component({
  selector: 'app-habit-accountability',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './habit-accountability.component.html',
  styleUrl: './habit-accountability.component.css'
})
export class HabitAccountabilityComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly guestHabitService = inject(GuestHabitService);

  settings = signal<AccountabilitySettings>({ goalPercentage: 80, penalties: [], rewards: [] });
  todayLog = signal<AccountabilityLog | null>(null);
  historyLogs = signal<AccountabilityLog[]>([]);

  newPenalty = signal('');
  newReward = signal('');
  goalInput = signal(80);

  randomPenalty = signal<Penalty | null>(null);
  randomReward = signal<Reward | null>(null);

  loading = signal(false);
  error = signal<string | null>(null);

  isGuest = this.authService.isGuest;

  dailyStatus = computed(() => {
    return this.guestHabitService.getDailyCompletionRate();
  });

  goalMet = computed(() => {
    const status = this.dailyStatus();
    const goal = this.settings().goalPercentage;
    return status.percentage >= goal;
  });

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.loading.set(true);

    const settings = this.guestHabitService.getAccountabilitySettings();
    this.settings.set(settings);
    this.goalInput.set(settings.goalPercentage);

    // Update today's log with current status
    const status = this.guestHabitService.getDailyCompletionRate();
    const goalMet = status.percentage >= settings.goalPercentage;
    const log = this.guestHabitService.logAccountability(status.percentage, goalMet);
    this.todayLog.set(log);

    // Load history
    this.historyLogs.set(this.guestHabitService.getAccountabilityLog(7));

    this.loading.set(false);
  }

  saveGoal(): void {
    const goal = Math.max(0, Math.min(100, this.goalInput()));
    this.guestHabitService.setGoalPercentage(goal);
    this.settings.update(s => ({ ...s, goalPercentage: goal }));

    // Recalculate goal met status
    const status = this.dailyStatus();
    const goalMet = status.percentage >= goal;
    const log = this.guestHabitService.logAccountability(status.percentage, goalMet);
    this.todayLog.set(log);
    this.historyLogs.set(this.guestHabitService.getAccountabilityLog(7));
  }

  onGoalInputChange(value: string): void {
    const num = parseInt(value, 10);
    if (!isNaN(num)) {
      this.goalInput.set(num);
    }
  }

  validateGoalInput(): void {
    const current = this.goalInput();
    this.goalInput.set(Math.max(0, Math.min(100, current)));
  }

  addPenalty(): void {
    const description = this.newPenalty().trim();
    if (!description) return;

    const penalty = this.guestHabitService.addPenalty(description);
    this.settings.update(s => ({ ...s, penalties: [...s.penalties, penalty] }));
    this.newPenalty.set('');
  }

  removePenalty(id: number): void {
    this.guestHabitService.removePenalty(id);
    this.settings.update(s => ({ ...s, penalties: s.penalties.filter(p => p.id !== id) }));
    if (this.randomPenalty()?.id === id) {
      this.randomPenalty.set(null);
    }
  }

  pickRandomPenalty(): void {
    const penalties = this.settings().penalties;
    if (penalties.length < 2) return;
    const randomIndex = Math.floor(Math.random() * penalties.length);
    this.randomPenalty.set(penalties[randomIndex]);
  }

  addReward(): void {
    const description = this.newReward().trim();
    if (!description) return;

    const reward = this.guestHabitService.addReward(description);
    this.settings.update(s => ({ ...s, rewards: [...s.rewards, reward] }));
    this.newReward.set('');
  }

  removeReward(id: number): void {
    this.guestHabitService.removeReward(id);
    this.settings.update(s => ({ ...s, rewards: s.rewards.filter(r => r.id !== id) }));
    if (this.randomReward()?.id === id) {
      this.randomReward.set(null);
    }
  }

  pickRandomReward(): void {
    const rewards = this.settings().rewards;
    if (rewards.length < 2) return;
    const randomIndex = Math.floor(Math.random() * rewards.length);
    this.randomReward.set(rewards[randomIndex]);
  }

  applyPenalty(): void {
    const penalties = this.settings().penalties;
    if (penalties.length === 0) return;

    // Apply first penalty by default
    this.guestHabitService.applyPenalty(penalties[0].id);
    this.todayLog.set(this.guestHabitService.getTodayLog());
    this.historyLogs.set(this.guestHabitService.getAccountabilityLog(7));
  }

  claimReward(): void {
    const rewards = this.settings().rewards;
    if (rewards.length === 0) return;

    // Claim first reward by default
    this.guestHabitService.claimReward(rewards[0].id);
    this.todayLog.set(this.guestHabitService.getTodayLog());
    this.historyLogs.set(this.guestHabitService.getAccountabilityLog(7));
  }

  cancelPenalty(): void {
    this.guestHabitService.cancelPenalty();
    this.todayLog.set(this.guestHabitService.getTodayLog());
    this.historyLogs.set(this.guestHabitService.getAccountabilityLog(7));
  }

  cancelReward(): void {
    this.guestHabitService.cancelReward();
    this.todayLog.set(this.guestHabitService.getTodayLog());
    this.historyLogs.set(this.guestHabitService.getAccountabilityLog(7));
  }

  getPenaltyDescription(id: number | undefined): string {
    if (!id) return '-';
    const penalty = this.settings().penalties.find(p => p.id === id);
    return penalty?.description || '-';
  }

  getRewardDescription(id: number | undefined): string {
    if (!id) return '-';
    const reward = this.settings().rewards.find(r => r.id === id);
    return reward?.description || '-';
  }

  formatDate(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
