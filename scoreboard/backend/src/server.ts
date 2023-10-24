import bodyParser from "body-parser";
import cors from "cors";
import express, { Request, Response } from "express";
import fs from "fs";
import util from "util";
import { ConfigManager } from "./config-manager";
import { MongoClient } from 'mongodb';

const app = express();
const PORT = 3000;
app.use(cors());

type Score = {
  id: string;
  name: string;
  points: number;
};

interface ScoreNew {
  Id: string;
  Name: string;
  Players: number;
  Rounds: number;
  Points: number;
}

const jsonParser = bodyParser.json();
let cachedScores: Score[] = [];
let lastSuccessfulReadTimestamp: string | null = null;

// Config
const configManager = new ConfigManager();

/**
 * Reads scores from a JSON file and returns an array of Score objects.
 * @returns A promise that resolves with an array of Score objects.
 */
const readScoresFromFile = async (filePath: string): Promise<Score[]> => {
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
};

async function fetchAllScores(): Promise<Score[]> {
  const connectionString = 'mongodb+srv://erik:shyanne@weave-db.zurbpnp.mongodb.net/';
  const client = new MongoClient(connectionString);

  try {
    await client.connect();
    const database = client.db('weave');
    const collection = database.collection<ScoreNew>('scores');

    const rawScores = await collection.find({}).toArray();
    const scores: Score[] = rawScores.map(score => {
      return {
        name: score.Name,
        score: score.Points
      };
    });
    return scores
  } catch (error) {
    console.error('Error fetching scores:', error);
    return []
  } finally {
    await client.close();
  }
}

/***************
 * CONTROLLERS *
 ***************/

app.get("/scores", async (_: Request, res: Response) /* NOSONAR */ => {
  let errorDetail = {}

  try {
    const scores = await fetchAllScores();
    cachedScores = scores;
    lastSuccessfulReadTimestamp = new Date().toISOString();
  } catch (error) {
    errorDetail = {
      message: "Failed syncing with the CSV file. Using cached scores.",
    };
  }

  // Return new scores or cached scores
  res.json({
    timestamp: lastSuccessfulReadTimestamp,
    scores: cachedScores,
    error: errorDetail,
    filePath: configManager.getFilePath(),
  });
});

app.get("/settings/file-path", (_: Request, res: Response) => {
  res.json({
    filePath: configManager.getFilePath(),
  });
});

app.put("/settings/file-path", jsonParser, (req: Request, res: Response) => {
  const newFilePath = req.body.filePath;

  if (typeof newFilePath === "string") {
    configManager.setFilePath(newFilePath);

    res.json({
      filePath: configManager.getFilePath(),
    });
  } else {
    res.status(400).json({
      message: "File path must be a string",
    });
  }
});

/*********
 * START *
 *********/

app.listen(PORT, () => {
  console.log(`Server is running on http://localhost:${PORT}`);
});
