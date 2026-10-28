// Firefly.cs: a 30-point line that follows a Curve2D, waits 3–13 s between runs, and drifts
// between speeds of 5 and 17 px a frame.

import { MENU_LINE_WIDTH } from "./consts";
import { lerp } from "./util";

/** Curve2D `_data.points` from StartScreen.tscn: (in.x, in.y, out.x, out.y, x, y) per point. */
export const CURVES = [
  [-31.4964, 160.259, 31.4964, -160.259, 61, 1120, -215.644, 11.8823, 215.644, -11.8823, 414, 669, -95.3672, 37.9185, 95.3672, -37.9185, 768, 467, 0, 0, 0, 0, 989, 453, -63.6776, -4.61465, 63.6776, 4.61465, 1187, 503, -21.5988, 27.5571, 21.5988, -27.5571, 1388, 381, 0, 0, 0, 0, 1519.38, 291.628, -59.7617, -74.0741, 59.7617, 74.0741, 1707, 248, 241.777, -9.6594, -241.777, 9.6594, 1258, 1025],
  [0, 0, 0, 0, 1001, -113, -187.887, 294.078, 187.887, -294.078, 259, 114, -313.992, 71.5364, 313.992, -71.5364, 725, 757, -204.445, 131.908, 204.445, -131.908, 614, 174, 157.939, -396.605, -157.939, 396.605, 1762, 536, 0, 0, 0, 0, 374, 1089],
  [-228.356, 119.356, 228.356, -119.356, 525, 1044, -57.2315, 184.683, 57.2315, -184.683, 362, 521, -136.364, -55.4701, 136.364, 55.4701, 697, -59, -152.661, 1.16349, 152.661, -1.16349, 1079, 357, -232.382, -79.1561, 232.382, 79.1561, 1556, 90, 215.148, -61.2672, -215.148, 61.2672, 1250, 789, 241.777, -9.6594, -241.777, 9.6594, -299, 683],
  [-228.356, 119.356, 228.356, -119.356, -182, 0, -155.544, -61.8105, 155.544, 61.8105, 344, 285, -136.364, -55.4701, 136.364, 55.4701, 844, 174, -45.8669, 150.054, 45.8669, -150.054, 1320, 257, -125.521, -20.7385, 125.521, 20.7385, 1591, 80, 68.3913, -202.324, -68.3913, 202.324, 1562, 714, 282.114, 11.3986, -282.114, -11.3986, 549, 1011, 0, 0, 0, 0, 39, 606, 241.777, -9.6594, -241.777, 9.6594, -270, 531],
];

const COLORS = ["#ff2a6d", "#8cff98", "#05b9e9", "#ffb237"];
const POINTS = 30;
const SPACING = 5;

/** Bakes the curve to points about 1 px apart, with their distance along the curve. */
function bake(data: number[]) {
  const points: { x: number; y: number }[] = [];
  for (let i = 0; i + 11 < data.length; i += 6) {
    const p0 = { x: data[i + 4], y: data[i + 5] };
    const c0 = { x: p0.x + data[i + 2], y: p0.y + data[i + 3] };
    const p1 = { x: data[i + 10], y: data[i + 11] };
    const c1 = { x: p1.x + data[i + 6], y: p1.y + data[i + 7] };
    for (let k = 0; k < 400; k++) {
      const t = k / 400;
      const u = 1 - t;
      points.push({
        x: u * u * u * p0.x + 3 * u * u * t * c0.x + 3 * u * t * t * c1.x + t * t * t * p1.x,
        y: u * u * u * p0.y + 3 * u * u * t * c0.y + 3 * u * t * t * c1.y + t * t * t * p1.y,
      });
    }
  }
  points.push({ x: data[data.length - 2], y: data[data.length - 1] });
  const lengths = [0];
  for (let i = 1; i < points.length; i++)
    lengths.push(lengths[i - 1] + Math.hypot(points[i].x - points[i - 1].x, points[i].y - points[i - 1].y));
  return { points, lengths, total: lengths[lengths.length - 1] };
}

export class Firefly {
  private path;
  private progress = 0;
  private lastProgress = 0;
  private speed = 0;
  private goal = 0;
  private wait = 0;
  private first = true;
  private line: { x: number; y: number }[] = [];

  constructor(data: number[], readonly color: string) {
    this.path = bake(data);
  }

  private at(progress: number) {
    const { points, lengths } = this.path;
    let lo = 0;
    let hi = lengths.length - 1;
    while (lo < hi) {
      const mid = (lo + hi) >> 1;
      if (lengths[mid] < progress) lo = mid + 1;
      else hi = mid;
    }
    return points[lo];
  }

  update(dt: number) {
    if (this.wait > 0) {
      this.wait -= dt;
      return;
    }
    const head = this.at(this.progress);
    if (this.lastProgress > this.progress || this.first) {
      this.line = Array.from({ length: POINTS }, () => ({ ...head }));
      this.wait = Math.random() * 10 + 3;
      this.first = false;
    }
    if (Math.abs(this.speed - this.goal) < 10e-5) this.goal = Math.random() * 12 + 5;
    if (Math.hypot(this.line[0].x - head.x, this.line[0].y - head.y) >= SPACING) {
      this.line.pop();
      this.line.unshift({ ...head });
    }
    this.lastProgress = this.progress;
    this.speed = lerp(this.speed, this.goal, 0.3 * dt);
    // PathFollow2D loops: progress wraps, which starts the wait above.
    this.progress = (this.progress + this.speed * dt * 60) % this.path.total;
  }

  draw(ctx: CanvasRenderingContext2D) {
    if (!this.line.length) return;
    ctx.strokeStyle = this.color;
    ctx.lineWidth = MENU_LINE_WIDTH;
    ctx.lineJoin = "round";
    ctx.lineCap = "butt";
    ctx.beginPath();
    this.line.forEach((p, i) => (i ? ctx.lineTo(p.x, p.y) : ctx.moveTo(p.x, p.y)));
    ctx.stroke();
  }
}

export const fireflies = CURVES.map((curve, i) => new Firefly(curve, COLORS[i]));
