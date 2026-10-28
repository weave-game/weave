import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// Relative paths, so the build works from any folder or host.
export default defineConfig({ base: "./", plugins: [react()], build: { assetsInlineLimit: 0 } });
