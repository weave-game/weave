// Values from the Godot original (weave/Scripts, *.tscn). Keep them 1:1.

export const W = 1600;
export const H = 900;
export const TICK = 1 / 60;

export const CLEAR = "#111312";
export const OUTLINE = "#252525";
export const LINE_WIDTH = 6;
export const MENU_LINE_WIDTH = 14;

export const COUNTDOWN = 2;
export const INITIAL_COUNTDOWN = 4;

export const PLAYER_RADIUS = 3;
/** The collision circle (and the dot) sit 9 px ahead of the player's origin. */
export const HEAD_OFFSET = 9;
export const GOAL_RADIUS = 20.025;
export const UNLOCK_RADIUS = 44;

export const DRAW_TIME = 5;
export const GAP_TIME = 0.5;

export const POINTS_PER_SECOND = 25;
export const POINTS_PER_ROUND = 500;

export const initialSpeed = (n: number) => (n <= 2 ? 100 : n === 3 ? 75 : 50);
export const obstacleCount = (n: number) => (n === 1 ? 6 : n === 2 ? 5 : n === 3 ? 4 : 0);
export const acceleration = (n: number) =>
  n === 1 ? 6.66 : n === 2 ? 2.1 : n === 3 ? 2.05 : n === 4 ? 2.0 : 1.8;
export const turnAcceleration = (n: number) => (n <= 2 ? 1.5 : 1.25);

/** Keyboard pairs (left, right), as KeyboardEvent.code. */
export const KEY_PAIRS: [string, string][] = [
  ["ArrowLeft", "ArrowRight"],
  ["Digit1", "KeyQ"],
  ["KeyB", "KeyN"],
  ["KeyZ", "KeyX"],
  ["KeyL", "KeyO"],
  ["Digit5", "Digit6"],
];

export const keyLabel = (code: string) =>
  code === "ArrowLeft"
    ? "←"
    : code === "ArrowRight"
      ? "→"
      : code.replace(/^Key|^Digit/, "");

export const db = (v: number) => Math.pow(10, v / 20);
