import express, { Request, Response } from "express";
import cors from "cors";
import fs from "fs";
import csv from "csv-parser";

const app = express();
const PORT = 3000;
app.use(cors());

type Score = {
  name: string;
  score: number;
};

let cachedScores: Score[] = [];
let lastSuccessfulReadTimestamp: string | null = null;

/**
 * Reads scores from a CSV file and returns an array of Score objects.
 * @returns A promise that resolves with an array of Score objects.
 */
const readScoresFromFile = async (): Promise<Score[]> => {
  const scores: Score[] = [];

  return new Promise<Score[]>((resolve, reject) => {
    fs.createReadStream("scores.csv")
      .pipe(csv())
      .on("data", (data) => {
        const teamName = data["team"];
        const teamScore = data["score"];

        if (teamName && typeof teamScore !== "undefined") {
          scores.push({
            name: teamName,
            score: parseInt(teamScore, 10),
          });
        } else {
          reject(new Error("Badly formatted CSV data"));
        }
      })
      .on("end", () => resolve(scores))
      .on("error", (error) => reject(error));
  });
};

/***************
 * CONTROLLERS *
 ***************/

app.get("/scores", async (_: Request, res: Response) => {
  let errorDetail = null;

  try {
    const scores = await readScoresFromFile();
    cachedScores = scores;
    lastSuccessfulReadTimestamp = new Date().toISOString();
  } catch (error) {
    errorDetail = {
      message: "Failed syncing with the CSV file. Using cached scores.",
    };
  }

  res.json({
    timestamp: lastSuccessfulReadTimestamp,
    scores: cachedScores,
    error: errorDetail,
  });
});

/*********
 * START *
 *********/

app.listen(PORT, () => {
  console.log(`Server is running on http://localhost:${PORT}`);
});
