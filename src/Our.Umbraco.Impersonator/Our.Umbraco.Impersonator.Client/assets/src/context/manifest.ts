import { ManifestGlobalContext } from "@umbraco-cms/backoffice/extension-registry";

const contexts: Array<ManifestGlobalContext> = [
    {
        type: 'globalContext',
        alias: 'Our.Umbraco.Impersonator.Context',
        name: 'Impersonator Context',
        js: () => import('./impersonator.context.ts')
    }
]

export const manifests = [...contexts];