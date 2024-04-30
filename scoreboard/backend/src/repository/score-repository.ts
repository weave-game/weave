import { Score } from '../models';

export interface ScoreRepository {
  /**
   * Fetches all scores from the repository.
   */
  fetchAllScores(): Promise<Score[]>;
}
