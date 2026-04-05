export const manifests: Array<UmbExtensionManifest> = [
    {
        type: "userProfileApp",
        alias: "impersonator",
        name: "Impersonator",
        js: () => import("./impersonator-app.ts"),
        weight: -1,
        meta: {
            "label": "Impersonator",
            "pathname": "impersonator"
        }
    }
];
