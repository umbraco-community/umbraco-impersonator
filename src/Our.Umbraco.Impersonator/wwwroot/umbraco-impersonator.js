const t = [
  {
    type: "userProfileApp",
    alias: "impersonator",
    name: "Impersonator",
    js: () => import("./impersonator-app-CVvnv8an.js"),
    weight: -1,
    meta: {
      label: "Impersonator",
      pathname: "impersonator"
    }
  }
], o = [
  {
    name: "Impersonator Entrypoint",
    alias: "Impersonator.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-CsNspbZf.js")
  }
], e = [
  ...t,
  ...o
];
export {
  e as manifests
};
//# sourceMappingURL=umbraco-impersonator.js.map
