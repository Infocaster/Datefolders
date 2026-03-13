import { defineConfig } from "vite";
export default defineConfig({
    build: {
        lib: {
            entry: [
                "nodenamesync/src/nodenamesync.ts",
            ],
            formats: ["es"],
        },
        outDir: "../wwwroot", // your web component will be saved in this location
        sourcemap: true,        
        emptyOutDir: true,
        rollupOptions: {
            external: [/^@umbraco/],
            output: {
                entryFileNames: () => {
                    return `[name]/[name].js`;
                },
                chunkFileNames: () => {
                    return `chunks/[name]-[hash].js`;
                },
            },
        },
    },
});
