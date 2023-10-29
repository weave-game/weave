import bodyParser from 'body-parser';
import cors from 'cors';
import express, { Request, Response } from 'express';
import 'dotenv/config';
import { ConfigManager } from './config-manager';
import { Score, ScoreOrigin, ScoreResponse } from './models';
import { JsonScoreRepository } from './repository/json-score-repository';
import { MongoScoreRepository } from './repository/mongo-score-repository';
import { ScoreRepository } from './repository/score-repository';

const app = express();
const PORT = 3000;
app.use(cors());

const jsonParser = bodyParser.json();
let cachedScores: Score[] = [];
let lastSuccessfulReadTimestamp: string | null = null;

// Config
const configManager = ConfigManager.getInstance();

const jsonScoreRepository: ScoreRepository = new JsonScoreRepository();
const mongoScoreRepository: ScoreRepository = new MongoScoreRepository();

async function fetchAllScores(): Promise<Score[]> {
  const scoreOrigin = configManager.getScoreOrigin();

  if (scoreOrigin === 'mongo') {
    return mongoScoreRepository.fetchAllScores();
  }

  if (scoreOrigin === 'json') {
    return jsonScoreRepository.fetchAllScores();
  }

  return [];
}

/***************
 * CONTROLLERS *
 ***************/

app.get('/scores', async (_: Request, res: Response) /* NOSONAR */ => {
  let errorDetail = {};

  try {
    const scores = await fetchAllScores();
    cachedScores = scores;
    lastSuccessfulReadTimestamp = new Date().toISOString();
  } catch (error) {
    errorDetail = {
      message: 'Failed syncing with the CSV file. Using cached scores.',
    };
  }

  const scoreOrigin = configManager.getScoreOrigin();
  let scoreOriginMessage: string = 'Not configured';

  if (scoreOrigin === 'mongo') {
    scoreOriginMessage = 'MongoDB';
  }

  if (scoreOrigin === 'json') {
    scoreOriginMessage =
      "JSON, reading from: '" + configManager.getFilePath() + "'";
  }

  const response: ScoreResponse = {
    timestamp: lastSuccessfulReadTimestamp,
    scores: cachedScores,
    error: errorDetail,
    scoreOrigin: scoreOriginMessage,
  };

  // Return new scores or cached scores
  res.json(response);
});

app.get('/settings/file-path', (_: Request, res: Response) => {
  res.json({
    filePath: configManager.getFilePath(),
  });
});

app.put('/settings/file-path', jsonParser, (req: Request, res: Response) => {
  const newFilePath = req.body.filePath;

  if (typeof newFilePath === 'string') {
    configManager.setFilePath(newFilePath);

    res.json({
      filePath: configManager.getFilePath(),
    });
  } else {
    res.status(400).json({
      message: 'File path must be a string',
    });
  }
});

/*********
 * START *
 *********/

// Default to JSON if not configured
if (!configManager.getScoreOrigin()) {
  configManager.setScoreOrigin(ScoreOrigin.Json);
}

app.listen(PORT, () => {
  console.log(`Server is running on http://localhost:${PORT}`);
});
