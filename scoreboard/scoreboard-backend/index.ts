import bodyParser from "body-parser";
import cors from "cors";
import express, { Request, Response } from "express";
import fs from "fs";
import util from "util";

const app = express();
const PORT = 3000;
app.use(cors());

type Score = {
  name: string;
  score: number;
};

const jsonParser = bodyParser.json();
let filePath = "scores.csv";
let cachedScores: Score[] = [];
let lastSuccessfulReadTimestamp: string | null = null;

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
        name: team.Name,
        score: team.Value,
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
    const scores = await readScoresFromFile(filePath);
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
    filePath,
  });
});

app.get("/settings/file-path", (_: Request, res: Response) => {
  res.json({
    filePath,
  });
});

app.put("/settings/file-path", jsonParser, (req: Request, res: Response) => {
  const newFilePath = req.body.filePath;

  if (typeof newFilePath === "string") {
    filePath = newFilePath;
    res.json({
      filePath,
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
