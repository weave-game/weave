import { useEffect, useState, type RefObject } from "react";
import { H, W } from "../consts";

/**
 * The 1600×900 stage is scaled to fit its box (Godot's canvas_items stretch, aspect keep).
 * Without a box, it fits the window.
 */
export function useStageScale(box?: RefObject<HTMLElement | null>) {
  const [scale, setScale] = useState(() => Math.min(innerWidth / W, innerHeight / H));
  useEffect(() => {
    const measure = () => {
      const rect = box?.current?.getBoundingClientRect() ?? { width: innerWidth, height: innerHeight };
      if (rect.width && rect.height) setScale(Math.min(rect.width / W, rect.height / H));
    };
    measure();
    const observer = new ResizeObserver(measure);
    if (box?.current) observer.observe(box.current);
    addEventListener("resize", measure);
    return () => {
      observer.disconnect();
      removeEventListener("resize", measure);
    };
  }, [box]);
  return scale;
}
