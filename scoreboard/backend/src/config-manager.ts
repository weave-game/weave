import fs from "fs";

export class ConfigManager {
  private readonly configFile = "config.json";

  public getFilePath(): string {
    if (!fs.existsSync(this.configFile)) {
      this.makeConfigFile();
    }

    const configData = fs.readFileSync(this.configFile, "utf8");
    const config = JSON.parse(configData);
    return config.filePath;
  }

  public setFilePath(newFilePath: string): void {
    fs.writeFileSync(
      this.configFile,
      JSON.stringify({ filePath: newFilePath })
    );
  }

  private makeConfigFile(): void {
    fs.writeFileSync(
      this.configFile,
      JSON.stringify({ filePath: "placeholder" })
    );
  }
}
