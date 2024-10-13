import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

// load up the manifests here.
import { manifests as contextManifests } from './context/manifest.ts';
import { manifests as entityActionManifests } from './actions/entity/manifest.ts';

const manifests: Array<ManifestTypes> = [
    ...contextManifests,
    ...entityActionManifests
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
    // register them here. 
    extensionRegistry.registerMany(manifests);
};
