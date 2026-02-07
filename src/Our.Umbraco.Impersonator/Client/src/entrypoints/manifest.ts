export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Impersonator Entrypoint",
    alias: "Impersonator.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.js"),
  },
];
