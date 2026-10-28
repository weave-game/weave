// Draws the game from its frames: the world (players, trails, goals, obstacles) through the
// camera, then the score and round labels, plus the animations and sounds that go with them.

import * as K from "./consts";
import { FONT_BOLD, FONT_DEFAULT, FONT_DISPLAY, SFX, image, setMusicPitch, tint } from "./assets";
import { MOVING, SEGMENT, growScale, type Ev, type Frame, type Setup } from "./sim";
import { lerp, readableInteger, track } from "./util";

type PlayerView = {
  name: string;
  color: string;
  x: number;
  y: number;
  rot: number;
  px: number;
  py: number;
  prot: number;
  flags: number;
  scale: number;
  desired: number;
  settled: boolean;
  segments: number[];
};

type GoalView = {
  x: number;
  y: number;
  locked: boolean;
  reached: boolean;
  removeIn: number;
  particlesAge: number;
  particles: { delay: number; angle: number; size: number }[];
};

type ObstacleView = {
  x: number;
  y: number;
  rot: number;
  scale: [number, number];
  target: [number, number];
  gone: boolean;
};

const roundScale = track([0, 1, 1.1, 1.3, 2], [0.3, 0.33, 3, 7, 1], [0.5, 0.75, 1, 0.95, 1]);
const roundAlpha = track([0, 0.9, 1.5, 2], [2, 3, 3, 1], [0, 1, 1, 0]);
const shineScale = track([0, 0.2, 1, 1.1, 1.3, 2], [3, 0.333, 0.33, 3, 7, 7], [0.4, 0.6, 0.9, 1, 0.9, 0.4]);
const endScale = track([0, 0.5, 0.9, 1.6], [0.333, 4, 0.1, 1], [0.4, 0.35, 0.95, 1]);
const endY = track([0, 1], [0.2, 7], [0, 80]);
const endLabelAlpha = track([0.7, 1.1, 1.6], [2, 0.3, 1], [0, 0.541176, 1]);

export class GameView {
  players: PlayerView[];
  private goals: GoalView[] = [];
  private obstacles: ObstacleView[] = [];
  private age = 0;
  private round = 0;
  private score = 0;
  private labelCounting = false;
  private roundAnim = -1;
  private shineAnim = -1;
  private endAnim = -1;
  private arc = 0;
  over = false;
  private camera = { x: 800, y: 450, zoom: 1, rot: 0, strength: 3, tx: 800, ty: 450, tzoom: 1, trot: 0, resetIn: -1 };
  private trails: HTMLCanvasElement | null = null;
  private trailsDrawn = new Map<number, number>();
  lastTick = 0;

  constructor(setup: Setup) {
    this.players = setup.players.map((p) => ({
      ...p,
      x: 0, y: 0, rot: 0, px: 0, py: 0, prot: 0, flags: 0,
      scale: 0, desired: 2, settled: false, segments: [],
    }));
  }

  /** Applies one tick. Frames must arrive in order. */
  apply(frame: Frame) {
    const continuous = frame.t === this.lastTick + 1;
    for (const ev of frame.ev ?? []) this.event(ev);
    this.players.forEach((p, i) => {
      const [x, y, rot, flags] = frame.p.slice(i * 4, i * 4 + 4);
      p.px = p.x;
      p.py = p.y;
      p.prot = p.rot;
      if (flags & SEGMENT && continuous) p.segments.push(p.x, p.y, x, y);
      p.x = x;
      p.y = y;
      p.rot = rot;
      p.flags = flags;
    });
    this.round = frame.r;
    this.score = frame.s;
    this.labelCounting = frame.l === 1;
    this.lastTick = frame.t;
  }

  private event(ev: Ev) {
    switch (ev.e) {
      case "map":
        this.goals = ev.goals.map((g) => newGoal(g.x, g.y));
        this.obstacles = ev.obstacles.map((o) => ({
          x: o.x, y: o.y, rot: o.rot, scale: [1, 1], target: [o.sx, o.sy], gone: false,
        }));
        this.players.forEach((p) => (p.segments = []));
        this.trailsDrawn.clear();
        this.trails?.getContext("2d")!.clearRect(0, 0, this.trails.width, this.trails.height);
        break;
      case "prep":
        this.roundAnim = 0;
        break;
      case "unlock": {
        const g = this.goals[ev.g];
        if (!g) break;
        g.locked = false;
        g.particlesAge = 0;
        SFX.unlock();
        break;
      }
      case "reach": {
        const g = this.goals[ev.g];
        if (!g) break;
        g.reached = true;
        g.removeIn = 0.86;
        SFX.ding();
        break;
      }
      case "cleared":
        SFX.complete();
        if (ev.shine) this.shineAnim = 0;
        break;
      case "obsKill": {
        const o = this.obstacles[ev.o];
        if (o) o.target = [0, 0];
        break;
      }
      case "over":
        this.over = true;
        this.endAnim = 0;
        this.shineAnim = -1;
        Object.assign(this.camera, { tx: ev.x, ty: ev.y, trot: 0.3, tzoom: 2, resetIn: 3 });
        setMusicPitch(0.5);
        SFX.explosion();
        break;
    }
  }

  update(dt: number) {
    this.age += dt;
    for (const p of this.players) {
      if (this.age >= K.INITIAL_COUNTDOWN - K.COUNTDOWN && p.desired === 2) {
        p.desired = 1;
        p.settled = false;
      }
      if (!p.settled) {
        p.scale = lerp(p.scale, p.desired, Math.min(1, dt * 4));
        if (Math.abs(p.scale - p.desired) * Math.SQRT2 < 0.01) p.settled = true;
      }
    }
    this.arc += 0.5 * dt;
    for (const g of this.goals) {
      if (g.particlesAge >= 0) g.particlesAge += dt;
      if (g.removeIn > 0) g.removeIn = Math.max(0, g.removeIn - dt);
    }
    for (const o of this.obstacles) {
      if (o.gone || o.scale === o.target) continue;
      o.scale = growScale(o.scale, o.target, dt);
      if (o.scale === o.target && o.target[0] === 0) o.gone = true;
    }
    if (this.roundAnim >= 0) this.roundAnim += dt;
    if (this.shineAnim >= 0) this.shineAnim = this.shineAnim + dt > 2 ? -1 : this.shineAnim + dt;
    if (this.endAnim >= 0) this.endAnim += dt;

    const c = this.camera;
    if (c.resetIn > 0) {
      c.resetIn -= dt;
      // Camera.Reset with a lerp strength of 0: it creeps back over a minute or so. Kept.
      if (c.resetIn <= 0) Object.assign(c, { strength: 0, tx: 800, ty: 450, tzoom: 1, trot: 0 });
    }
    const k = dt * c.strength;
    c.x = lerp(c.x, c.tx, k);
    c.y = lerp(c.y, c.ty, k);
    c.zoom = lerp(c.zoom, c.tzoom, k);
    c.rot = lerp(c.rot, c.trot, k);
    c.strength = lerp(c.strength, 3, dt * 0.01);
  }

  /** `alpha` blends from the previous tick to the latest one. */
  render(ctx: CanvasRenderingContext2D, unit: number, alpha = 1) {
    const { width, height } = ctx.canvas;
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.fillStyle = K.CLEAR;
    ctx.fillRect(0, 0, width, height);

    const c = this.camera;
    const identity =
      Math.abs(c.zoom - 1) < 1e-4 && Math.abs(c.rot) < 1e-4 && Math.abs(c.x - 800) < 0.01 && Math.abs(c.y - 450) < 0.01;
    const world = () => {
      ctx.setTransform(unit, 0, 0, unit, 0, 0);
      ctx.translate(800, 450);
      ctx.scale(c.zoom, c.zoom);
      ctx.rotate(-c.rot);
      ctx.translate(-c.x, -c.y);
    };
    world();

    const positions = this.players.map((p) => {
      const jump = Math.hypot(p.x - p.px, p.y - p.py) > 100;
      return jump || alpha >= 1
        ? { x: p.x, y: p.y, rot: p.rot }
        : { x: lerp(p.px, p.x, alpha), y: lerp(p.py, p.y, alpha), rot: lerp(p.prot, p.rot, alpha) };
    });

    // Players first, as in the scene tree; goals, obstacles, and lines are added after them.
    this.players.forEach((p, i) => this.drawPlayer(ctx, p, positions[i]));
    for (const g of this.goals) this.drawGoal(ctx, g);
    for (const o of this.obstacles) {
      if (o.gone) continue;
      ctx.save();
      ctx.translate(o.x, o.y);
      ctx.rotate((o.rot * Math.PI) / 180);
      ctx.scale(o.scale[0], o.scale[1]);
      ctx.drawImage(image("cube.svg"), -10, -10, 20, 20);
      ctx.restore();
    }

    if (identity) {
      this.drawTrailCache(ctx, unit);
      world();
    } else {
      ctx.lineWidth = K.LINE_WIDTH;
      ctx.lineCap = "butt";
      ctx.lineJoin = "round";
      for (const p of this.players) {
        ctx.strokeStyle = p.color;
        ctx.beginPath();
        tracePath(ctx, p.segments, 0);
        ctx.stroke();
      }
    }

    // Outline, just outside the screen. It shows when the camera moves.
    ctx.strokeStyle = K.OUTLINE;
    ctx.lineWidth = K.LINE_WIDTH;
    ctx.strokeRect(-3, -3, K.W + 6, K.H + 6);

    // Player names (z_index 999), upright.
    this.players.forEach((p, i) => {
      if (p.scale < 0.01) return;
      const { x, y } = positions[i];
      ctx.save();
      ctx.translate(x, y);
      ctx.scale(p.scale, p.scale);
      outlinedText(ctx, p.name, 17, -29, `16px ${FONT_BOLD}`, "#fff", "#000", 8);
      ctx.restore();
    });

    // Canvas layers: not moved by the camera.
    ctx.setTransform(unit, 0, 0, unit, 0, 0);
    this.drawScore(ctx);
    this.drawRoundLabel(ctx);
  }

  private drawPlayer(ctx: CanvasRenderingContext2D, p: PlayerView, at: { x: number; y: number; rot: number }) {
    if (p.scale > 0.001) {
      ctx.save();
      ctx.translate(at.x, at.y);
      ctx.rotate(at.rot);
      ctx.scale(p.scale, p.scale);
      ctx.fillStyle = p.color;
      ctx.beginPath();
      ctx.ellipse(0, -9, 3.03, 3.12, 0, 0, Math.PI * 2);
      ctx.fill();
      if (!(p.flags & MOVING)) {
        const size = 14.8;
        ctx.drawImage(tint("arrow.svg", p.color, 64), -size / 2, -13 - size / 2, size, size);
      }
      ctx.restore();
    }
    // Edge indicators: where the player will come back in.
    const dx = Math.min(at.x, K.W - at.x);
    const dy = Math.min(at.y, K.H - at.y);
    const indicator = (x: number, y: number, d: number) => {
      const r = Math.pow(Math.max(0, (100 - d) / 100), 2) * 0.01 * 1024;
      if (r <= 0.05) return;
      ctx.fillStyle = p.color;
      ctx.beginPath();
      ctx.arc(x, y, r, 0, Math.PI * 2);
      ctx.fill();
    };
    indicator(((Math.round(at.x / K.W) + 1) % 2) * K.W, at.y, dx);
    indicator(at.x, ((Math.round(at.y / K.H) + 1) % 2) * K.H, dy);
  }

  private drawGoal(ctx: CanvasRenderingContext2D, g: GoalView) {
    if (g.removeIn === 0) return;
    const owner = this.players[this.goals.indexOf(g)];
    if (!owner) return;
    ctx.save();
    ctx.translate(g.x, g.y);
    if (g.locked) {
      ctx.strokeStyle = owner.color;
      ctx.lineWidth = 8;
      ctx.beginPath();
      ctx.arc(0, 0, K.UNLOCK_RADIUS - 8, this.arc, this.arc + Math.PI * 2);
      ctx.stroke();
    } else {
      ctx.fillStyle = g.reached ? "#000" : owner.color;
      ctx.beginPath();
      ctx.arc(0.1, 0.1, 19.9, 0, Math.PI * 2);
      ctx.fill();
    }
    if (g.particlesAge >= 0 && g.particlesAge < 1.3) {
      ctx.fillStyle = "#fff";
      for (const particle of g.particles) {
        const age = g.particlesAge - particle.delay;
        if (age < 0 || age > 1) continue;
        const r = 335.23 * age;
        const a = particle.angle + Math.PI * 4 * age;
        ctx.globalAlpha = 1 - age;
        const s = particle.size;
        ctx.fillRect(Math.cos(a) * r - s / 2, Math.sin(a) * r - s / 2, s, s);
      }
      ctx.globalAlpha = 1;
    }
    if (g.locked) ctx.drawImage(tint("lock.svg", owner.color, 64), -16.5, -4 - 16.5, 33, 33);
    outlinedText(ctx, owner.name, 0, 0, `16px ${FONT_DEFAULT}`, "#fff", "#000", 10);
    ctx.restore();
  }

  private drawTrailCache(ctx: CanvasRenderingContext2D, unit: number) {
    const { width, height } = ctx.canvas;
    if (!this.trails || this.trails.width !== width || this.trails.height !== height) {
      this.trails ??= document.createElement("canvas");
      this.trails.width = width;
      this.trails.height = height;
      this.trailsDrawn.clear();
    }
    const tctx = this.trails.getContext("2d")!;
    tctx.setTransform(unit, 0, 0, unit, 0, 0);
    tctx.lineWidth = K.LINE_WIDTH;
    tctx.lineCap = "butt";
    tctx.lineJoin = "round";
    this.players.forEach((p, i) => {
      const from = this.trailsDrawn.get(i) ?? 0;
      const s = p.segments;
      if (from >= s.length) return;
      tctx.strokeStyle = p.color;
      tctx.beginPath();
      // Start one segment back so the joint with the last stroke is round, not a seam.
      tracePath(tctx, s, Math.max(0, from - 4));
      tctx.stroke();
      this.trailsDrawn.set(i, s.length);
    });
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.drawImage(this.trails, 0, 0);
  }

  private drawScore(ctx: CanvasRenderingContext2D) {
    let scale = 0.4;
    let y = 0;
    let labelAlpha = 0;
    if (this.endAnim >= 0) {
      scale = endScale(this.endAnim);
      y = endY(this.endAnim);
      labelAlpha = this.endAnim >= 0.7 ? endLabelAlpha(this.endAnim) : 0;
    }
    let fill = "#fff";
    if (this.endAnim < 0 && this.shineAnim >= 0) {
      scale = shineScale(this.shineAnim);
      const g = Math.round(255 * shineGreen(this.shineAnim));
      const b = Math.round(255 * shineBlue(this.shineAnim));
      fill = `rgb(255 ${g} ${b})`;
    }
    outlinedText(ctx, readableInteger(this.score), 800, y + 70 * scale, `${100 * scale}px ${FONT_DISPLAY}`, fill, K.CLEAR, 16 * scale);
    if (labelAlpha > 0) {
      ctx.globalAlpha = labelAlpha;
      outlinedText(ctx, "SCORE", 800, 93, `36px ${FONT_DISPLAY}`, "#fff", K.CLEAR, 16);
      ctx.globalAlpha = 1;
    }
  }

  private drawRoundLabel(ctx: CanvasRenderingContext2D) {
    if (this.roundAnim < 0) return;
    const alpha = roundAlpha(this.roundAnim);
    if (alpha <= 0.001 || !this.labelCounting) return;
    const scale = roundScale(this.roundAnim);
    ctx.globalAlpha = alpha;
    outlinedText(ctx, `ROUND ${this.round}`, 800, 450, `${64 * scale}px ${FONT_DISPLAY}`, "#fff", K.CLEAR, 16 * scale);
    ctx.globalAlpha = 1;
  }
}

/** Segments that touch become one polyline, so joints have no seams. */
function tracePath(ctx: CanvasRenderingContext2D, s: number[], from: number) {
  let x = NaN;
  let y = NaN;
  for (let i = from; i < s.length; i += 4) {
    if (s[i] !== x || s[i + 1] !== y) ctx.moveTo(s[i], s[i + 1]);
    ctx.lineTo(s[i + 2], s[i + 3]);
    x = s[i + 2];
    y = s[i + 3];
  }
}

const shineGreen = track([0, 1, 1.1, 1.5], [1, 1, 1, 1], [1, 1, 0.901961, 1]);
const shineBlue = track([0, 1, 1.1, 1.5], [1, 1, 1, 1], [1, 1, 0.294118, 1]);

function newGoal(x: number, y: number): GoalView {
  return {
    x, y, locked: true, reached: false, removeIn: -1, particlesAge: -1,
    particles: Array.from({ length: 77 }, () => ({
      delay: Math.random() * 0.21,
      angle: Math.random() * Math.PI * 2,
      size: 1.1 + Math.random() * 0.4,
    })),
  };
}

export function outlinedText(
  ctx: CanvasRenderingContext2D,
  text: string,
  x: number,
  y: number,
  font: string,
  fill: string,
  outline: string,
  outlineWidth: number,
) {
  ctx.font = font;
  ctx.textAlign = "center";
  ctx.textBaseline = "middle";
  if (outlineWidth > 0) {
    ctx.lineJoin = "round";
    ctx.strokeStyle = outline;
    ctx.lineWidth = outlineWidth;
    ctx.strokeText(text, x, y);
  }
  ctx.fillStyle = fill;
  ctx.fillText(text, x, y);
}

/**
 * Godot's glow (Glowifier.tscn): blurred copies of the screen at 1/8 and 1/32 size, screened
 * over it.
 */
export class Glow {
  private levels: HTMLCanvasElement[] = [];

  apply(ctx: CanvasRenderingContext2D, strength = 1) {
    const { width, height } = ctx.canvas;
    let source: HTMLCanvasElement = ctx.canvas;
    let w = width;
    let h = height;
    let i = 0;
    const out: HTMLCanvasElement[] = [];
    while (w > 60) {
      w = Math.max(1, Math.round(w / 2));
      h = Math.max(1, Math.round(h / 2));
      const level = (this.levels[i] ??= document.createElement("canvas"));
      if (level.width !== w || level.height !== h) {
        level.width = w;
        level.height = h;
      }
      const lctx = level.getContext("2d")!;
      lctx.imageSmoothingQuality = "high";
      lctx.clearRect(0, 0, w, h);
      lctx.globalCompositeOperation = "source-over";
      lctx.drawImage(source, 0, 0, w, h);
      if (i === 0) {
        // Godot's HDR threshold: only what is brighter than the background glows.
        lctx.globalCompositeOperation = "difference";
        lctx.fillStyle = K.CLEAR;
        lctx.fillRect(0, 0, w, h);
        lctx.globalCompositeOperation = "source-over";
      }
      out.push(level);
      source = level;
      i++;
    }
    const pick = (fraction: number) => out[Math.min(out.length - 1, Math.round(Math.log2(width / (width * fraction))) - 1)];
    ctx.save();
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.globalCompositeOperation = "screen";
    ctx.imageSmoothingQuality = "high";
    const layers: [HTMLCanvasElement, number][] = [
      [pick(1 / 16), 0.55 * strength],
      [pick(1 / 64), 0.5 * strength],
    ];
    for (const [level, alpha] of layers) {
      ctx.globalAlpha = alpha;
      ctx.drawImage(level, 0, 0, width, height);
    }
    ctx.restore();
  }
}
