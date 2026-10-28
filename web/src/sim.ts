// The game itself at a fixed 60 ticks a second. A port of Main.cs, Player.cs, CurveSpawner.cs,
// Grid.cs, Goal.cs, Obstacle.cs, and ScoreDisplay.cs. Each tick makes a Frame, which view.ts draws.

import * as K from "./consts";
import { randf, randi } from "./util";

export type Setup = { players: { name: string; color: string }[] };

export type MapGoal = { x: number; y: number };
export type MapObstacle = { x: number; y: number; rot: number; sx: number; sy: number };

export type Ev =
  | { e: "map"; goals: MapGoal[]; obstacles: MapObstacle[] }
  | { e: "prep" }
  | { e: "unlock"; g: number }
  | { e: "reach"; g: number }
  | { e: "cleared"; shine: boolean }
  | { e: "obsKill"; o: number }
  | { e: "over"; x: number; y: number };

/** Flags per player and tick. */
export const SEGMENT = 1;
export const MOVING = 2;

export type Frame = {
  t: number;
  /** [x, y, rotation (rad), flags] per player. */
  p: number[];
  /** Round. */
  r: number;
  /** Score. */
  s: number;
  /** Whether the round label is counting ("ROUND n"). */
  l: 0 | 1;
  ev?: Ev[];
};

type Player = {
  x: number;
  y: number;
  rot: number;
  speed: number;
  /** Degrees a second. */
  turnSpeed: number;
  moving: boolean;
  turning: boolean;
  drawing: boolean;
  drawLeft: number;
  gapLeft: number;
  hasStarted: boolean;
  lastX: number;
  lastY: number;
};

type Segment = [number, number, number, number];

type Goal = MapGoal & { owner: number; locked: boolean; reached: boolean; gone: boolean };
type Obstacle = MapObstacle & { scale: [number, number]; target: [number, number]; done: boolean; dying: boolean; gone: boolean };

const ROWS = 10;
const COLS = 10;
const CELL_W = K.W / ROWS;
const CELL_H = K.H / COLS;

export type Input = (player: number) => { left: boolean; right: boolean };

export class Sim {
  tick = 0;
  private time = 0;
  private players: Player[] = [];
  private grid: Segment[][][] = [];
  private goals: Goal[] = [];
  private obstacles: Obstacle[] = [];
  private inside = new Set<string>();
  private acc = 0;
  private turnAcc = 0;
  private running = true;
  private round = 0;
  private completions = 0;
  private labelCounting = false;
  private score = 0;
  private scoreEnabled = false;
  private timers: { at: number; fn: () => void }[] = [];
  private delayTimer: { at: number; fn: () => void } | null = null;
  private events: Ev[] = [];
  /** Seconds since game over, to stop sending frames once nothing changes. */
  overFor = -1;

  constructor(
    n: number,
    private input: Input,
  ) {
    const positions = randomPositions(n);
    const speed = K.initialSpeed(n);
    for (let i = 0; i < n; i++) {
      const [x, y] = positions[i];
      this.players.push({
        x, y, rot: 0, speed, turnSpeed: 120, moving: false, turning: true, drawing: true,
        drawLeft: K.DRAW_TIME, gapLeft: 0, hasStarted: false, lastX: x, lastY: y,
      });
    }
    this.acc = K.acceleration(n);
    this.turnAcc = K.turnAcceleration(n);
    this.startPreparation();
  }

  get isOver() {
    return !this.running;
  }

  get finalScore() {
    return Math.floor(this.score);
  }

  get rounds() {
    return this.round;
  }

  step(): Frame {
    const dt = K.TICK;
    this.tick++;
    this.time += dt;

    // Main._PhysicsProcess: collisions, then wrapping.
    if (this.running) {
      for (const p of this.players) {
        if (!p.drawing) continue;
        if (this.hitsTrail(p)) {
          this.gameOver(p.x, p.y);
          break;
        }
      }
    }
    if (this.running) {
      for (const p of this.players) {
        if (p.x < 0) p.x = K.W;
        else if (p.x > K.W) p.x = 0;
        else if (p.y < 0) p.y = K.H;
        else if (p.y > K.H) p.y = 0;
      }
    }

    // Player._PhysicsProcess: turn, then move.
    this.players.forEach((p, i) => {
      if (p.turning) {
        const { left, right } = this.input(i);
        if (right) p.rot += (p.turnSpeed * dt * Math.PI) / 180;
        if (left) p.rot -= (p.turnSpeed * dt * Math.PI) / 180;
      }
      if (p.moving) {
        p.x += Math.sin(p.rot) * p.speed * dt;
        p.y -= Math.cos(p.rot) * p.speed * dt;
      }
    });

    // Main._Process: speed up. The decay runs once per player, as in the original.
    if (this.running) {
      for (const p of this.players) {
        this.acc -= 0.002 * this.acc * dt;
        this.turnAcc -= 0.002 * this.turnAcc * dt;
        p.speed += this.acc * dt;
        p.turnSpeed += this.turnAcc * dt;
      }
    }

    // CurveSpawner: only runs while the player moves, and its gap timers pause with it.
    const flags = this.players.map((p) => {
      let f = p.moving ? MOVING : 0;
      if (!p.moving) return f;
      if (p.hasStarted && p.drawing) {
        if (Math.hypot(p.x - p.lastX, p.y - p.lastY) <= 100) {
          this.addSegment([p.lastX, p.lastY, p.x, p.y]);
          f |= SEGMENT;
        }
      } else p.hasStarted = true;
      p.lastX = p.x;
      p.lastY = p.y;
      if (p.drawing) {
        p.drawLeft -= dt;
        if (p.drawLeft <= 0) {
          p.drawing = false;
          p.gapLeft = K.GAP_TIME;
        }
      } else {
        p.gapLeft -= dt;
        if (p.gapLeft <= 0) {
          p.drawing = true;
          p.drawLeft = K.DRAW_TIME;
        }
      }
      return f;
    });

    this.areas();
    this.growObstacles(dt);

    if (this.scoreEnabled) this.score += K.POINTS_PER_SECOND * dt;
    if (!this.running) this.overFor += dt;

    // Timers, in order.
    const due = this.timers.filter((timer) => timer.at <= this.time + 1e-9);
    this.timers = this.timers.filter((timer) => timer.at > this.time + 1e-9);
    for (const timer of due) timer.fn();

    const frame: Frame = {
      t: this.tick,
      p: this.players.flatMap((p, i) => [round2(p.x), round2(p.y), round4(p.rot), flags[i]]),
      r: this.round,
      s: Math.floor(this.score),
      l: this.labelCounting ? 1 : 0,
    };
    if (this.events.length) frame.ev = this.events.splice(0);
    return frame;
  }

  private later(seconds: number, fn: () => void) {
    const timer = { at: this.time + seconds, fn };
    this.timers.push(timer);
    return timer;
  }

  // --- Main.cs ---

  private startPreparation() {
    this.resetMap();
    this.players.forEach((p) => (p.moving = false));
    this.labelCounting = true;
    if (this.delayTimer) this.timers = this.timers.filter((t) => t !== this.delayTimer);
    this.delayTimer = this.later(this.round === 0 ? K.INITIAL_COUNTDOWN : K.COUNTDOWN, () =>
      this.startRound(),
    );
    this.scoreEnabled = false;
    if (this.round === 0) this.later(K.INITIAL_COUNTDOWN - K.COUNTDOWN, () => this.increaseRound());
    else this.increaseRound();
  }

  private increaseRound() {
    this.later(K.COUNTDOWN / 2, () => this.round++);
    this.events.push({ e: "prep" });
  }

  private startRound() {
    this.players.forEach((p) => (p.moving = true));
    this.labelCounting = false;
    this.scoreEnabled = true;
  }

  private reachedGoal() {
    if (++this.completions !== this.players.length) return;
    this.completions = 0;
    // ScoreDisplay.OnRoundComplete only counts while the score runs.
    const shine = this.scoreEnabled;
    if (shine) this.later(K.COUNTDOWN / 2, () => (this.score += K.POINTS_PER_ROUND));
    this.events.push({ e: "cleared", shine });
    this.startPreparation();
  }

  private gameOver(x: number, y: number) {
    this.running = false;
    this.overFor = 0;
    this.players.forEach((p) => {
      p.moving = false;
      p.turning = false;
    });
    this.scoreEnabled = false;
    this.events.push({ e: "over", x: round2(x), y: round2(y) });
  }

  private resetMap() {
    this.grid = Array.from({ length: ROWS }, () => Array.from({ length: COLS }, () => []));
    this.inside.clear();
    const playerPositions = this.players.map((p) => [p.x, p.y] as [number, number]);
    const goalPositions = randomPositions(this.players.length, playerPositions);
    this.goals = goalPositions.map(([x, y], owner) => ({
      x, y, owner, locked: true, reached: false, gone: false,
    }));
    // The original empties the goal list while spawning goals, so obstacles only avoid players.
    const obstaclePositions = randomPositions(K.obstacleCount(this.players.length), playerPositions);
    this.obstacles = obstaclePositions.map(([x, y]) => {
      const area = randi(15, 25);
      const side1 = Math.floor(randf(1, Math.sqrt(area)));
      const side2 = Math.floor(area / side1);
      return {
        x, y, rot: randi(0, 360), sx: side1, sy: side2,
        scale: [1, 1], target: [side1, side2], done: false, dying: false, gone: false,
      };
    });
    this.events.push({
      e: "map",
      goals: this.goals.map(({ x, y }) => ({ x: round2(x), y: round2(y) })),
      obstacles: this.obstacles.map(({ x, y, rot, sx, sy }) => ({ x: round2(x), y: round2(y), rot, sx, sy })),
    });
  }

  // --- Collisions ---

  private hitsTrail(p: Player) {
    // Grid.GetSegmentsFromPlayerPosition: cells touched by a radius-3 circle at the origin.
    const candidates = new Set<Segment>();
    for (let row = 0; row < ROWS; row++)
      for (let col = 0; col < COLS; col++) {
        const rx = col * CELL_W;
        const ry = row * CELL_H;
        const cx = Math.min(Math.max(p.x, rx), rx + CELL_W);
        const cy = Math.min(Math.max(p.y, ry), ry + CELL_H);
        if (Math.hypot(p.x - cx, p.y - cy) <= K.PLAYER_RADIUS)
          for (const s of this.grid[row][col]) candidates.add(s);
      }
    const [hx, hy] = head(p);
    const radius = K.PLAYER_RADIUS + K.LINE_WIDTH / 2;
    for (const s of candidates) if (segmentDistance(hx, hy, s) <= radius) return true;
    return false;
  }

  private addSegment(s: Segment) {
    const out = (x: number, y: number) => x < 0 || x > K.W || y < 0 || y > K.H;
    if (out(s[0], s[1]) || out(s[2], s[3])) return;
    const cell = (x: number, y: number) =>
      this.grid[Math.min(ROWS - 1, Math.floor(y / CELL_H))][Math.min(COLS - 1, Math.floor(x / CELL_W))];
    cell(s[0], s[1]).push(s);
    const b = cell(s[2], s[3]);
    if (b !== cell(s[0], s[1])) b.push(s);
  }

  /** Area2D BodyEntered and AreaEntered: they fire when an overlap starts. */
  private entered(key: string, overlapping: boolean) {
    if (!overlapping) {
      this.inside.delete(key);
      return false;
    }
    if (this.inside.has(key)) return false;
    this.inside.add(key);
    return true;
  }

  private areas() {
    this.players.forEach((p, pi) => {
      const [hx, hy] = head(p);
      this.goals.forEach((g, gi) => {
        if (g.gone) return;
        const d = Math.hypot(hx - g.x, hy - g.y);
        if (this.entered(`u${gi}:${pi}`, d <= K.UNLOCK_RADIUS + K.PLAYER_RADIUS)) {
          if (g.locked && g.owner !== pi) {
            g.locked = false;
            this.events.push({ e: "unlock", g: gi });
          }
        }
        if (this.entered(`c${gi}:${pi}`, d <= K.GOAL_RADIUS + K.PLAYER_RADIUS)) {
          if (!g.reached && !g.locked && g.owner === pi) {
            g.reached = true;
            this.later(0.86, () => (g.gone = true));
            this.events.push({ e: "reach", g: gi });
            this.reachedGoal();
          }
        }
      });
      this.obstacles.forEach((o, oi) => {
        if (o.gone) return;
        if (this.entered(`o${oi}:${pi}`, circleInObstacle(hx, hy, K.PLAYER_RADIUS, o)) && this.running)
          this.gameOver(p.x, p.y);
      });
    });
    // Obstacle.AreaEntered: a goal's areas make an obstacle shrink away.
    this.obstacles.forEach((o, oi) => {
      if (o.gone || o.dying) return;
      for (const g of this.goals) {
        if (g.gone) continue;
        if (circleInObstacle(g.x, g.y, K.UNLOCK_RADIUS, o)) {
          o.dying = true;
          o.target = [0, 0];
          o.done = false;
          this.events.push({ e: "obsKill", o: oi });
          break;
        }
      }
    });
  }

  private growObstacles(dt: number) {
    for (const o of this.obstacles) {
      if (o.done || o.gone) continue;
      o.scale = growScale(o.scale, o.target, dt);
      if (o.scale === o.target) {
        o.done = true;
        if (o.dying) o.gone = true;
      }
    }
  }
}

/** Obstacle._Process: lerp at 6/s, snap within 0.01. Returns `target` itself when done. */
export function growScale(scale: [number, number], target: [number, number], dt: number): [number, number] {
  const t = Math.min(1, dt * 6);
  const next: [number, number] = [scale[0] + (target[0] - scale[0]) * t, scale[1] + (target[1] - scale[1]) * t];
  return Math.hypot(next[0] - target[0], next[1] - target[1]) <= 0.01 ? target : next;
}

export function head(p: { x: number; y: number; rot: number }): [number, number] {
  return [p.x + Math.sin(p.rot) * K.HEAD_OFFSET, p.y - Math.cos(p.rot) * K.HEAD_OFFSET];
}

function circleInObstacle(cx: number, cy: number, r: number, o: Obstacle) {
  const a = (-o.rot * Math.PI) / 180;
  const dx = cx - o.x;
  const dy = cy - o.y;
  const lx = dx * Math.cos(a) - dy * Math.sin(a);
  const ly = dx * Math.sin(a) + dy * Math.cos(a);
  const hw = 10 * Math.abs(o.scale[0]);
  const hh = 10 * Math.abs(o.scale[1]);
  const nx = Math.min(Math.max(lx, -hw), hw);
  const ny = Math.min(Math.max(ly, -hh), hh);
  return Math.hypot(lx - nx, ly - ny) <= r;
}

function segmentDistance(px: number, py: number, [ax, ay, bx, by]: Segment) {
  const vx = bx - ax;
  const vy = by - ay;
  const len = vx * vx + vy * vy;
  const t = len === 0 ? 0 : Math.max(0, Math.min(1, ((px - ax) * vx + (py - ay) * vy) / len));
  return Math.hypot(px - (ax + vx * t), py - (ay + vy * t));
}

/** Main.GetRandomPositionsInView. */
function randomPositions(n: number, occupied: [number, number][] = []) {
  const positions: [number, number][] = [];
  for (let i = 0; i < n; i++) {
    let candidate: [number, number];
    let valid: boolean;
    let attempt = 0;
    do {
      attempt++;
      candidate = [randf(100, K.W - 100), randf(100, K.H - 100)];
      valid = [...occupied, ...positions].every(
        ([x, y]) => Math.hypot(x - candidate[0], y - candidate[1]) >= 250,
      );
    } while (!valid && attempt < 1000);
    positions.push(candidate);
  }
  return positions;
}

const round2 = (v: number) => Math.round(v * 100) / 100;
const round4 = (v: number) => Math.round(v * 10000) / 10000;
