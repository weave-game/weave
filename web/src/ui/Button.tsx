import { forwardRef, type ButtonHTMLAttributes } from "react";

const base = import.meta.env.BASE_URL + "assets/img/";

/** An icon in the current text colour (Godot's icon colours follow the button state). */
export function Icon({ name, size = 32, className = "" }: { name: string; size?: number; className?: string }) {
  const url = `url(${base}${name})`;
  return (
    <span
      className={`icon ${className}`}
      style={{ width: size, height: size, maskImage: url, WebkitMaskImage: url }}
      aria-hidden
    />
  );
}

/** Objects/KeyIcon.tscn: empty_key.png with the key's name on it. */
export function KeyIcon({ label, tinted = false }: { label: string; tinted?: boolean }) {
  return (
    <span className="key-icon">
      {tinted ? <Icon name="empty_key.png" size={38} /> : <img src={`${base}empty_key.png`} alt="" />}
      <span>{label}</span>
    </span>
  );
}

type Props = ButtonHTMLAttributes<HTMLButtonElement> & { icon?: string; label: string; accent?: boolean };

/** Themes/weave_theme.tres: Button and AccentedButton. */
export const Button = forwardRef<HTMLButtonElement, Props>(function Button(
  { icon, label, accent, className = "", ...props },
  ref,
) {
  return (
    <button
      ref={ref}
      className={`weave-button ${accent ? "accent" : ""} ${label ? "" : "collapsed"} ${className}`}
      {...props}
    >
      {icon && <Icon name={icon} />}
      {label && <span>{label}</span>}
    </button>
  );
});
