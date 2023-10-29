/**
 * Entity model for the score collection in MongoDB.
 */
export interface MongoScore {
  Id: string;
  Name: string;
  Players: number;
  Rounds: number;
  Points: number;
}

export interface Score {
  id: string;
  name: string;
  points: number;
}

/**
 * The response the server sends back to the client.
 */
export interface ScoreResponse {
  timestamp: string | null;
  scores: Score[];
  error: any;

  /**
   * A message indicating where the scores were fetched from. For example, mongo or json.
   */
  scoreOrigin: string;
}

export enum ScoreOrigin {
  Mongo = "mongo",
  Json = "json",
}
