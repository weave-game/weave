import { MongoScore, Score } from '../models';
import { ScoreRepository } from './score-repository';
import { MongoClient } from 'mongodb';

export class MongoScoreRepository implements ScoreRepository {
  async fetchAllScores(): Promise<Score[]> {
    const connectionString =
      process.env.CONNECTION_STRING ?? 'mongodb://localhost:27017';
    const client = new MongoClient(connectionString);

    try {
      await client.connect();
      const database = client.db('weave');
      const collection = database.collection<MongoScore>('scores');

      const rawScores = await collection.find({}).toArray();
      const scores: Score[] = rawScores.map((score) => {
        return {
          id: score.Id,
          name: score.Name,
          points: score.Points,
        };
      });
      return scores;
    } catch (error) {
      console.error('Error fetching scores:', error);
      return [];
    } finally {
      await client.close();
    }
  }
}
