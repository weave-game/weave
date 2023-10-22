import * as fs from "fs";

interface TeamScore {
  team: string;
  score: number;
}

const teamNames = [
  "Alpha",
  "Bravo",
  "Charlie",
  "Delta",
  "Echo",
  "Foxtrot",
  "Golf",
  "Hotel",
  "India",
  "Juliet",
];

function getRandomTeamName(): string {
  const randomIndex = Math.floor(Math.random() * teamNames.length);
  return teamNames[randomIndex];
}

function generateData(numberOfRecords: number): TeamScore[] {
  const data: TeamScore[] = [];
  for (let i = 0; i < numberOfRecords; i++) {
    const teamScore: TeamScore = {
      team: getRandomTeamName(),
      score: Math.floor(Math.random() * 100001),
    };
    data.push(teamScore);
  }
  return data;
}

function writeCSV(filePath: string, data: TeamScore[]): void {
  const writeStream = fs.createWriteStream(filePath);
  writeStream.write("team,score\n"); // Write the header

  for (const record of data) {
    writeStream.write(`"${record.team}",${record.score}\n`);
  }

  writeStream.end();
}

const numberOfRecords = 500;
const data = generateData(numberOfRecords);
const filePath = "scores.csv";

writeCSV(filePath, data);

console.log(`Data written to ${filePath}`);
