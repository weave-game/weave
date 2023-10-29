import fs from 'fs';

/**
 * `ConfigManager` is responsible for managing a configuration file named "config.json".
 * It provides methods to read from and write specific configurations to this file.
 */
export class ConfigManager {
  private static instance: ConfigManager | null = null;
  private readonly configFile = 'config.json';

  private constructor() {}

  public static getInstance(): ConfigManager {
    if (!ConfigManager.instance) {
      ConfigManager.instance = new ConfigManager();
    }

    return ConfigManager.instance;
  }

  /**
   * Fetches the file path from the configuration.
   *
   * @returns {string} - The file path from the configuration or an empty string if not found.
   */
  public getFilePath(): string {
    return this.getConfigValue('filePath') ?? '';
  }

  /**
   * Updates or sets the file path in the configuration.
   *
   * @param {string} newFilePath - The new file path to be set.
   */
  public setFilePath(newFilePath: string): void {
    this.setConfigValue('filePath', newFilePath);
  }

  /**
   * Fetches the score origin from the configuration.
   *
   * @returns {string | null} - The score origin from the configuration or null if not found.
   */
  public getScoreOrigin() {
    return this.getConfigValue('scoreOrigin');
  }

  /**
   * Updates or sets the score origin in the configuration.
   *
   * @param {string} newScoreOrigin - The new score origin to be set.
   */
  public setScoreOrigin(newScoreOrigin: string): void {
    this.setConfigValue('scoreOrigin', newScoreOrigin);
  }

  /****************
   * COMMON LOGIC *
   ****************/

  /**
   * Creates the configuration file with an empty JSON object if it doesn't exist.
   */
  private makeConfigFile(): void {
    fs.writeFileSync(this.configFile, JSON.stringify({}));
  }

  /**
   * Fetches a configuration value from the configuration file based on a key.
   *
   * @param {string} key - The key of the configuration value to fetch.
   * @returns {string | null} - The configuration value or null if not found or on error.
   */
  private getConfigValue(key: string): string | null {
    if (!fs.existsSync(this.configFile)) {
      this.makeConfigFile();
    }

    const configData = fs.readFileSync(this.configFile, 'utf8');
    const config = JSON.parse(configData);

    try {
      return config[key];
    } catch (error) {
      return null;
    }
  }

  /**
   * Sets a configuration value in the configuration file.
   *
   * @param {string} key - The key of the configuration value to set.
   * @param {string} value - The value to set for the provided key.
   */
  private setConfigValue(key: string, value: string): void {
    if (!fs.existsSync(this.configFile)) {
      this.makeConfigFile();
    }

    const configData = fs.readFileSync(this.configFile, 'utf8');
    const config = JSON.parse(configData);

    config[key] = value;

    fs.writeFileSync(this.configFile, JSON.stringify(config));
  }
}
