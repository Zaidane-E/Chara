export interface Habit {
  id: number;
  title: string;
  isActive: boolean;
  sortOrder: number;
  createdAt: string;
  updatedAt: string;
  isCompletedToday: boolean;
  lastCompletedAt: string | null;
  currentStreak: number;
  totalCompletions: number;
  completionRate: number;
}

export interface CreateHabit {
  title: string;
}

export interface UpdateHabit {
  title: string;
  isActive: boolean;
}

export interface HabitCompletion {
  id: number;
  habitId: number;
  completedAt: string;
  completedDate: string;
}

export interface HabitStats {
  habitId: number;
  habitTitle: string;
  totalCompletions: number;
  currentStreak: number;
  longestStreak: number;
  completionRateLastMonth: number;
  completionHistory: DailyCompletion[];
}

export interface DailyCompletion {
  date: string;
  completed: boolean;
}

export type HabitSortOption = 'default' | 'completionRate' | 'streak' | 'completionStatus';

export interface ReorderHabits {
  habitIds: number[];
}

// Accountability types
export interface Penalty {
  id: number;
  description: string;
  createdAt: string;
}

export interface Reward {
  id: number;
  description: string;
  createdAt: string;
}

export interface AccountabilitySettings {
  goalPercentage: number;
  penalties: Penalty[];
  rewards: Reward[];
}

export interface AccountabilityLog {
  id: number;
  date: string;
  completionRate: number;
  goalMet: boolean;
  penaltyApplied: boolean;
  rewardClaimed: boolean;
  appliedPenaltyId?: number;
  claimedRewardId?: number;
}
