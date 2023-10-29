import fs from "fs";

export class ConfigManager {
  private readonly configFile = "config.json";

  public getFilePath(): string {
    return this.getConfigValue("filePath") ?? "";
  }

  public setFilePath(newFilePath: string): void {
    this.setConfigValue("filePath", newFilePath);
  }

  public getScoreOrigin() {
    this.getConfigValue("scoreOrigin");
  }

  public setScoreOrigin(newScoreOrigin: string): void {
    this.setConfigValue("scoreOrigin", newScoreOrigin);
  }

  /****************
   * COMMON LOGIC *
   ****************/

  private makeConfigFile(): void {
    fs.writeFileSync(this.configFile, JSON.stringify({}));
  }

  private getConfigValue(key: string): string | null {
    if (!fs.existsSync(this.configFile)) {
      this.makeConfigFile();
    }

    const configData = fs.readFileSync(this.configFile, "utf8");
    const config = JSON.parse(configData);

    try {
      return config[key];
    } catch (error) {
      return null;
    }
  }

  private setConfigValue(key: string, value: string): void {
    if (!fs.existsSync(this.configFile)) {
      this.makeConfigFile();
    }

    const configData = fs.readFileSync(this.configFile, "utf8");
    const config = JSON.parse(configData);

    config[key] = value;

    fs.writeFileSync(this.configFile, JSON.stringify(config));
  }
}
