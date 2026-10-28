export const lerp = (a: number, b: number, t: number) => a + (b - a) * t;
export const clamp = (v: number, a: number, b: number) => Math.min(Math.max(v, a), b);
/** Godot's GD.RandRange for floats and ints. */
export const randf = (a: number, b: number) => a + Math.random() * (b - a);
export const randi = (a: number, b: number) => a + Math.floor(Math.random() * (b - a + 1));

/** Godot's ease(): c > 1 eases in, 0 < c < 1 eases out, c < 0 eases in-out. */
export function ease(x: number, c: number) {
  x = clamp(x, 0, 1);
  if (c > 0) return c < 1 ? 1 - Math.pow(1 - x, 1 / c) : Math.pow(x, c);
  if (c < 0) {
    return x < 0.5
      ? Math.pow(x * 2, -c) * 0.5
      : (1 - Math.pow(1 - (x - 0.5) * 2, -c)) * 0.5 + 0.5;
  }
  return 0;
}

/** A Godot animation value track: times, transitions, values. */
export function track(times: number[], transitions: number[], values: number[]) {
  return (t: number) => {
    if (t <= times[0]) return values[0];
    for (let i = 0; i < times.length - 1; i++) {
      if (t < times[i + 1]) {
        const k = (t - times[i]) / (times[i + 1] - times[i]);
        return lerp(values[i], values[i + 1], ease(k, transitions[i]));
      }
    }
    return values[values.length - 1];
  };
}

/** .NET's System.Random (the seeded, subtractive generator), for UniqueColorGenerator. */
class NetRandom {
  private seeds = new Array<number>(56).fill(0);
  private inext = 0;
  private inextp = 21;
  constructor(seed: number) {
    const MBIG = 2147483647;
    let mj = 161803398 - Math.abs(seed);
    this.seeds[55] = mj;
    let mk = 1;
    for (let i = 1; i < 55; i++) {
      const ii = (21 * i) % 55;
      this.seeds[ii] = mk;
      mk = mj - mk;
      if (mk < 0) mk += MBIG;
      mj = this.seeds[ii];
    }
    for (let k = 1; k < 5; k++)
      for (let i = 1; i < 56; i++) {
        this.seeds[i] -= this.seeds[1 + ((i + 30) % 55)];
        if (this.seeds[i] < 0) this.seeds[i] += MBIG;
      }
  }
  nextDouble() {
    const MBIG = 2147483647;
    if (++this.inext >= 56) this.inext = 1;
    if (++this.inextp >= 56) this.inextp = 1;
    let r = this.seeds[this.inext] - this.seeds[this.inextp];
    if (r === MBIG) r--;
    if (r < 0) r += MBIG;
    this.seeds[this.inext] = r;
    return r * (1 / MBIG);
  }
}

const DEFAULT_COLORS = ["#ff2a6d", "#8cff98", "#05b9e9", "#ffb237", "#07864a", "#f5f5f5"];

/** UniqueColorGenerator: six colours, then the k-th draw from Random(1337). */
export function playerColor(index: number) {
  if (index < DEFAULT_COLORS.length) return DEFAULT_COLORS[index];
  const random = new NetRandom(1337);
  let color = "";
  for (let k = DEFAULT_COLORS.length; k <= index; k++) {
    const hex = () =>
      Math.floor(random.nextDouble() * 255)
        .toString(16)
        .padStart(2, "0");
    color = `#${hex()}${hex()}${hex()}`;
  }
  return color;
}

const A = ["Mega", "Uber", "Ultra", "Super", "Hyper", "Mighty", "Giga", "Tera", "Epic", "Grand"];
const B = [
  "Red", "Sinister", "Wicked", "Violet", "Cruel", "Dreaded", "Malevolent", "Shadowy", "Gloomy",
  "Bleak", "Ominous", "Twisted", "Corrupt", "Ruthless", "Venomous", "Fiery", "Murky", "Midnight",
  "Savage", "Noxious", "Rancid", "Poisoned",
];
const C = [
  "Lemurs", "Baboons", "Capuchins", "Marmosets", "Tarsiers", "Macaques", "Gibbons", "Mandrills",
  "Chimpanzees", "Orangutans", "Gorillas", "AyeAyes", "Iguanas", "Komodos", "Cheetahs", "Ocelots",
  "Jaguars", "Pangolins", "Kinkajous", "Bushbabys", "Tamarins", "Sifakas", "Langurs", "Guerezsa",
];
const pick = <T>(list: T[]) => list[Math.floor(Math.random() * list.length)];
const usedNames = new Map<string, number>();

/** UniqueNameGenerator: optional A and B prefixes in random order, then C. No spaces. */
export function teamName() {
  let name = "";
  for (let attempt = 0; attempt < 100; attempt++) {
    const prefixes: string[] = [];
    if (Math.random() < 0.5) prefixes.push(pick(A));
    if (Math.random() < 0.5) prefixes.push(pick(B));
    if (Math.random() < 0.5) prefixes.reverse();
    name = prefixes.join("") + pick(C);
    if (!usedNames.has(name)) break;
  }
  const n = usedNames.get(name);
  usedNames.set(name, (n ?? -1) + 1);
  return n === undefined ? name : `${name} ${n + 1}`;
}

export const uid = () => crypto.randomUUID().replaceAll("-", "").slice(0, 12);

export function readableInteger(n: number) {
  return n.toLocaleString("en-US").replaceAll(",", " ");
}
