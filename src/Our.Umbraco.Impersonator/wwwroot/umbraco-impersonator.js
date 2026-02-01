const a = [
  {
    type: "userProfileApp",
    alias: "impersonator",
    name: "Impersonator",
    js: () => import("./impersonator-app-NoLMn81o.js"),
    weight: -1,
    meta: {
      label: "Impersonator",
      pathname: "impersonator"
    }
  }
], t = [
  {
    name: "Your Package Name Entrypoint",
    alias: "YourPackageName.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-BiAltGAM.js")
  }
], e = [
  ...a,
  ...t
];
export {
  e as manifests
};
//# sourceMappingURL=umbraco-impersonator.js.map
