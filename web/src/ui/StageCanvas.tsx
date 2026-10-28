import { useEffect, useRef } from "react";
import { H, W } from "../consts";
import { onFrame } from "../loop";

/** A full-stage canvas at device resolution. `draw` gets the pixels per stage unit. */
export function StageCanvas({
  scale,
  draw,
}: {
  scale: number;
  draw: (ctx: CanvasRenderingContext2D, unit: number, dt: number) => void;
}) {
  const ref = useRef<HTMLCanvasElement>(null);
  const drawRef = useRef(draw);
  drawRef.current = draw;

  useEffect(() => {
    const canvas = ref.current!;
    const ctx = canvas.getContext("2d")!;
    return onFrame((dt) => {
      const unit = Math.min(2.5, scale * devicePixelRatio);
      const width = Math.round(W * unit);
      const height = Math.round(H * unit);
      if (canvas.width !== width || canvas.height !== height) {
        canvas.width = width;
        canvas.height = height;
      }
      drawRef.current(ctx, unit, dt);
    });
  }, [scale]);

  return <canvas ref={ref} className="stage-canvas" />;
}
