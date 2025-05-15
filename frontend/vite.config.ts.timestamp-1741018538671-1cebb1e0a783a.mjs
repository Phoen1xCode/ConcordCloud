// vite.config.ts
import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "file:///Users/lan/Project/WebstormProject/FileCodeBoxFronted/node_modules/.pnpm/vite@5.4.7_@types+node@20.16.7/node_modules/vite/dist/node/index.js";
import vue from "file:///Users/lan/Project/WebstormProject/FileCodeBoxFronted/node_modules/.pnpm/@vitejs+plugin-vue@5.1.4_vite@5.4.7+vue@3.5.8/node_modules/@vitejs/plugin-vue/dist/index.mjs";
import vueJsx from "file:///Users/lan/Project/WebstormProject/FileCodeBoxFronted/node_modules/.pnpm/@vitejs+plugin-vue-jsx@4.0.1_vite@5.4.7+vue@3.5.8/node_modules/@vitejs/plugin-vue-jsx/dist/index.mjs";
import vueDevTools from "file:///Users/lan/Project/WebstormProject/FileCodeBoxFronted/node_modules/.pnpm/vite-plugin-vue-devtools@7.4.6_vite@5.4.7+vue@3.5.8/node_modules/vite-plugin-vue-devtools/dist/vite.mjs";
import tailwindcss from "file:///Users/lan/Project/WebstormProject/FileCodeBoxFronted/node_modules/.pnpm/tailwindcss@3.4.13/node_modules/tailwindcss/lib/index.js";
import autoprefixer from "file:///Users/lan/Project/WebstormProject/FileCodeBoxFronted/node_modules/.pnpm/autoprefixer@10.4.20_postcss@8.4.47/node_modules/autoprefixer/lib/autoprefixer.js";
var __vite_injected_original_import_meta_url = "file:///Users/lan/Project/WebstormProject/FileCodeBoxFronted/vite.config.ts";
var vite_config_default = defineConfig({
  plugins: [vue(), vueJsx(), vueDevTools()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", __vite_injected_original_import_meta_url))
    }
  },
  css: {
    postcss: {
      plugins: [tailwindcss, autoprefixer]
    }
  }
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCIvVXNlcnMvbGFuL1Byb2plY3QvV2Vic3Rvcm1Qcm9qZWN0L0ZpbGVDb2RlQm94RnJvbnRlZFwiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9maWxlbmFtZSA9IFwiL1VzZXJzL2xhbi9Qcm9qZWN0L1dlYnN0b3JtUHJvamVjdC9GaWxlQ29kZUJveEZyb250ZWQvdml0ZS5jb25maWcudHNcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfaW1wb3J0X21ldGFfdXJsID0gXCJmaWxlOi8vL1VzZXJzL2xhbi9Qcm9qZWN0L1dlYnN0b3JtUHJvamVjdC9GaWxlQ29kZUJveEZyb250ZWQvdml0ZS5jb25maWcudHNcIjtpbXBvcnQgeyBmaWxlVVJMVG9QYXRoLCBVUkwgfSBmcm9tICdub2RlOnVybCdcblxuaW1wb3J0IHsgZGVmaW5lQ29uZmlnIH0gZnJvbSAndml0ZSdcbmltcG9ydCB2dWUgZnJvbSAnQHZpdGVqcy9wbHVnaW4tdnVlJ1xuaW1wb3J0IHZ1ZUpzeCBmcm9tICdAdml0ZWpzL3BsdWdpbi12dWUtanN4J1xuaW1wb3J0IHZ1ZURldlRvb2xzIGZyb20gJ3ZpdGUtcGx1Z2luLXZ1ZS1kZXZ0b29scydcbmltcG9ydCB0YWlsd2luZGNzcyBmcm9tICd0YWlsd2luZGNzcydcbmltcG9ydCBhdXRvcHJlZml4ZXIgZnJvbSAnYXV0b3ByZWZpeGVyJ1xuZXhwb3J0IGRlZmF1bHQgZGVmaW5lQ29uZmlnKHtcbiAgcGx1Z2luczogW3Z1ZSgpLCB2dWVKc3goKSwgdnVlRGV2VG9vbHMoKV0sXG4gIHJlc29sdmU6IHtcbiAgICBhbGlhczoge1xuICAgICAgJ0AnOiBmaWxlVVJMVG9QYXRoKG5ldyBVUkwoJy4vc3JjJywgaW1wb3J0Lm1ldGEudXJsKSlcbiAgICB9XG4gIH0sXG4gIGNzczoge1xuICAgIHBvc3Rjc3M6IHtcbiAgICAgIHBsdWdpbnM6IFt0YWlsd2luZGNzcywgYXV0b3ByZWZpeGVyXVxuICAgIH1cbiAgfVxufSlcbiJdLAogICJtYXBwaW5ncyI6ICI7QUFBaVYsU0FBUyxlQUFlLFdBQVc7QUFFcFgsU0FBUyxvQkFBb0I7QUFDN0IsT0FBTyxTQUFTO0FBQ2hCLE9BQU8sWUFBWTtBQUNuQixPQUFPLGlCQUFpQjtBQUN4QixPQUFPLGlCQUFpQjtBQUN4QixPQUFPLGtCQUFrQjtBQVB5TCxJQUFNLDJDQUEyQztBQVFuUSxJQUFPLHNCQUFRLGFBQWE7QUFBQSxFQUMxQixTQUFTLENBQUMsSUFBSSxHQUFHLE9BQU8sR0FBRyxZQUFZLENBQUM7QUFBQSxFQUN4QyxTQUFTO0FBQUEsSUFDUCxPQUFPO0FBQUEsTUFDTCxLQUFLLGNBQWMsSUFBSSxJQUFJLFNBQVMsd0NBQWUsQ0FBQztBQUFBLElBQ3REO0FBQUEsRUFDRjtBQUFBLEVBQ0EsS0FBSztBQUFBLElBQ0gsU0FBUztBQUFBLE1BQ1AsU0FBUyxDQUFDLGFBQWEsWUFBWTtBQUFBLElBQ3JDO0FBQUEsRUFDRjtBQUNGLENBQUM7IiwKICAibmFtZXMiOiBbXQp9Cg==
