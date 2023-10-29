import { Score } from "../models";
import { ScoreRepository } from "./score-repository";
import fs from "fs";
import util from "util";

export class JsonScoreRepository implements ScoreRepository {
  async fetchAllScores(): Promise<Score[]> {
    const readFile = util.promisify(fs.readFile);

    try {
      const data = await readFile(filePath, "utf8");
      const jsonData = JSON.parse(data);
      const scores: Score[] = [];

      for (const key in jsonData) {
        const team = jsonData[key];
        scores.push({
          id: team.Id,
          name: team.Name,
          points: team.Points,
        });
      }

      return scores;
    } catch (error) {
      console.error(error);
      throw new Error(`Error reading JSON file`);
    }
  }
}
