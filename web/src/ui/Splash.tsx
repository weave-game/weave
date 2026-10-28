import { useEffect, useRef } from "react";
import { onFrame } from "../loop";
import { session, type Snapshot } from "../session";

const img = import.meta.env.BASE_URL + "assets/img/splash-screen.jpg";

/** Menus/SplashScreen.tscn: the tutorial picture, "Press again to skip...", and the fade. */
export function Splash({ snap }: { snap: Snapshot }) {
  const fade = useRef<HTMLDivElement>(null);
  useEffect(
    () =>
      onFrame(() => {
        if (fade.current) fade.current.style.opacity = String(session.splashFade);
      }),
    [],
  );
  return (
    <div className="splash">
      <img src={img} alt="" style={{ left: -53, top: -48, width: 1718.9, height: 1022 }} />
      {snap.skipLabel && <div className="skip-label">Press again to skip...</div>}
      <div ref={fade} className="fade" />
    </div>
  );
}
