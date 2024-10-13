import { ManifestEntityAction } from "@umbraco-cms/backoffice/extension-registry"
import ImpersonateAction from "./impersonate.action"

const entityAction: ManifestEntityAction = {
    type: 'entityAction',
    kind: 'default',
    alias: 'Our.Umbraco.Impersonator.EntityAction.User.Impersonate',
    name: 'Impersonate User Entity Action',
    forEntityTypes: ['user'],
    api: ImpersonateAction,
    meta: {
        icon: 'icon-check',
        label: 'Impersonate',
    }
};

export const manifests = [entityAction];