import bodyParser from "body-parser";
import cors from "cors";
import express, { Request, Response } from "express";
import fs from "fs";
import util from "util";
import { ConfigManager } from "./config-manager";

const app = express();
const PORT = 3000;
app.use(cors());

type Score = {
  id: string;
  name: string;
  points: number;
};

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

/***************
 * CONTROLLERS *
 ***************/

app.get("/scores", async (_: Request, res: Response) /* NOSONAR */ => {
  let errorDetail = null;

  // Attempt to read scores
  try {
    const scores = await readScoresFromFile(configManager.getFilePath());
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
